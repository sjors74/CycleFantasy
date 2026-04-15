using CycleManager.Services.Interfaces;
using Domain.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Services
{
    public class ScoreService : IScoreService
    {
        private readonly ApplicationDbContext _context;

        public ScoreService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task UpdateScoresForStageAsync(int eventId, int stageId)
        {

            var deelnemers = await _context.GameCompetitorsEvent
                .Include(d => d.Renners)
                .Where(d => d.EventId == eventId)
                .ToListAsync();

            var resultsLookup = await _context.Results
                .Where(r => r.StageId == stageId)
                .Include(r => r.ConfigurationItem)
                .ToDictionaryAsync(
                    r => r.CompetitorInEventId,
                    r => r.ConfigurationItem.Score
                );

            var existingStagePickScores = await _context.DeelnemerStagePickScores
                .Where(s => s.StageId == stageId)
                .ToDictionaryAsync(s => s.GameCompetitorEventPickId);

            var existingPickTotals = await _context.DeelnemerPickScores
                    .ToDictionaryAsync(p => p.GameCompetitorEventPickId);

            var existingStageScores = await _context.DeelnemerStageScores
                .Where(s => s.StageId == stageId)
                .ToDictionaryAsync(s => s.GameCompetitorEventId);

            // pick most recent total record per participant as baseline if needed
            var existingTotals = await _context.DeelnemerScores
                .GroupBy(s => s.GameCompetitorEventId)
                .Select(g => g.OrderByDescending(s => s.LastUpdated).First())
                .ToDictionaryAsync(s => s.GameCompetitorEventId);

            var newStagePickScores = new List<DeelnemerStagePickScore>();
            var newPickTotals = new List<DeelnemerPickScore>();
            var newStageScores = new List<DeelnemerStageScore>();
            var newTotals = new List<DeelnemerScore>();

            // keep a map of stage snapshot totals per participant for LaatsteStageScore
            var stageSnapshotByParticipant = new Dictionary<int, int>();

            foreach (var deelnemer in deelnemers)
            {
                int newStageTotalForDeelnemer = 0;

                foreach (var pick in deelnemer.Renners)
                {
                    int newPickScore = resultsLookup.TryGetValue(pick.CompetitorsInEventId, out var s) ? s : 0;
                    newStageTotalForDeelnemer += newPickScore;

                    existingStagePickScores.TryGetValue(pick.Id, out var prevStagePickEntry);
                    int prevPickStageScore = prevStagePickEntry?.Score ?? 0;

                    int pickDelta = newPickScore - prevPickStageScore;

                    if (prevStagePickEntry != null)
                    {
                        prevStagePickEntry.Score = newPickScore;
                        prevStagePickEntry.LastUpdated = DateTime.UtcNow;
                    }
                    else
                    {
                        newStagePickScores.Add(new DeelnemerStagePickScore
                        {
                            Id = Guid.NewGuid(),
                            GameCompetitorEventPickId = pick.Id,
                            StageId = stageId,
                            Score = newPickScore,
                            LastUpdated = DateTime.UtcNow
                        });
                    }

                    if (existingPickTotals.TryGetValue(pick.Id, out var pickTotalEntry))
                    {
                        if (pickDelta != 0)
                        {
                            pickTotalEntry.TotalScore += pickDelta;
                            pickTotalEntry.LastUpdate = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        // first time we see this pick -> insert total record
                        newPickTotals.Add(new DeelnemerPickScore
                        {
                            Id = Guid.NewGuid(),
                            GameCompetitorEventPickId = pick.Id,
                            TotalScore = newPickScore,
                            LastUpdate = DateTime.UtcNow
                        });
                    }
                }

                // snapshot participant stage score for later use
                stageSnapshotByParticipant[deelnemer.Id] = newStageTotalForDeelnemer;

                // update or add stage snapshot record
                existingStageScores.TryGetValue(deelnemer.Id, out var prevStageScoreEntry);
                if (prevStageScoreEntry != null)
                {
                    prevStageScoreEntry.Score = newStageTotalForDeelnemer;
                    prevStageScoreEntry.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    newStageScores.Add(new DeelnemerStageScore
                    {
                        Id = Guid.NewGuid(),
                        GameCompetitorEventId = deelnemer.Id,
                        StageId = stageId,
                        Score = newStageTotalForDeelnemer,
                        LastUpdated = DateTime.UtcNow
                    });
                }
            }

            if (newStagePickScores.Any()) _context.DeelnemerStagePickScores.AddRange(newStagePickScores);
            if (newPickTotals.Any()) _context.DeelnemerPickScores.AddRange(newPickTotals);
            if (newStageScores.Any()) _context.DeelnemerStageScores.AddRange(newStageScores);

            // flush changes so DB reflects updated pick totals / snapshots
            await _context.SaveChangesAsync();

            // rebuild a map of pickId -> totalScore using DB content (including newly inserted rows)
            var allPickIds = deelnemers.SelectMany(d => d.Renners.Select(r => r.Id)).ToList();
            var pickTotalsFromDb = await _context.DeelnemerPickScores
                .Where(dps => allPickIds.Contains(dps.GameCompetitorEventPickId))
                .ToListAsync();

            var pickTotalMap = pickTotalsFromDb.ToDictionary(dps => dps.GameCompetitorEventPickId, dps => dps.TotalScore);

            // Now compute and persist participant totals based on the sum of their pick totals
            foreach (var deelnemer in deelnemers)
            {
                var picks = deelnemer.Renners.Select(r => r.Id).ToList();
                int computedTotal = picks.Sum(pid => pickTotalMap.TryGetValue(pid, out var val) ? val : 0);
                int lastStageScore = stageSnapshotByParticipant.TryGetValue(deelnemer.Id, out var ss) ? ss : 0;

                if (existingTotals.TryGetValue(deelnemer.Id, out var totalEntry))
                {
                    // set authoritative total (recompute) rather than apply deltas
                    totalEntry.TotalScore = computedTotal;
                    totalEntry.LaatsteStageScore = lastStageScore;
                    totalEntry.LaatsteStageId = stageId;
                    totalEntry.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    newTotals.Add(new DeelnemerScore
                    {
                        Id = Guid.NewGuid(),
                        GameCompetitorEventId = deelnemer.Id,
                        TotalScore = computedTotal,
                        LaatsteStageScore = lastStageScore,
                        LaatsteStageId = stageId,
                        LastUpdated = DateTime.UtcNow
                    });
                }
            }

            if (newTotals.Any()) _context.DeelnemerScores.AddRange(newTotals);

            await _context.SaveChangesAsync();
        }

        // RecalculateEventScoresAsync remains unchanged (keeps authoritative rebuild)
        public async Task RecalculateEventScoresAsync(int eventId)
        {
            // Rebuild all aggregated score tables for an event from Results.
            // Use a transaction to avoid partial state.
            await using var tx = await _context.Database.BeginTransactionAsync();

            // load event participants, picks and stages
            var ev = await _context.Events
                .Include(e => e.GameCompetitorEvents)
                    .ThenInclude(g => g.Renners)
                .Include(e => e.Stages)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (ev == null)
            {
                await tx.DisposeAsync();
                return;
            }

            var participants = ev.GameCompetitorEvents.ToList();
            var participantIds = participants.Select(p => p.Id).ToList();
            var allPickIds = participants.SelectMany(p => p.Renners.Select(r => r.Id)).ToList();
            var stageIds = ev.Stages.Select(s => s.Id).ToList();

            // Remove existing aggregates for this event (safely)
            if (allPickIds.Any())
            {
                var toRemoveStagePickScores = _context.DeelnemerStagePickScores.Where(d => allPickIds.Contains(d.GameCompetitorEventPickId));
                _context.DeelnemerStagePickScores.RemoveRange(toRemoveStagePickScores);

                var toRemovePickTotals = _context.DeelnemerPickScores.Where(d => allPickIds.Contains(d.GameCompetitorEventPickId));
                _context.DeelnemerPickScores.RemoveRange(toRemovePickTotals);
            }

            if (stageIds.Any())
            {
                var toRemoveStageScores = _context.DeelnemerStageScores.Where(s => stageIds.Contains(s.StageId));
                _context.DeelnemerStageScores.RemoveRange(toRemoveStageScores);
            }

            if (participantIds.Any())
            {
                var toRemoveTotals = _context.DeelnemerScores.Where(ds => participantIds.Contains(ds.GameCompetitorEventId));
                _context.DeelnemerScores.RemoveRange(toRemoveTotals);
            }

            await _context.SaveChangesAsync();

            // Prepare accumulators
            var pickTotals = new Dictionary<int, int>();         // pickId -> total
            var participantTotals = participantIds.ToDictionary(id => id, id => 0); // participantId -> total
            var lastStageScoreByParticipant = new Dictionary<int, int>(); // participantId -> last stage snapshot

            // Iterate stages in order and compute
            var stagesOrdered = ev.Stages.OrderBy(s => s.StageOrder).ToList();
            foreach (var stage in stagesOrdered)
            {
                // load results for stage with configuration item
                var results = await _context.Results
                    .Where(r => r.StageId == stage.Id)
                    .Include(r => r.ConfigurationItem)
                    .ToListAsync();

                var resultByCompetitor = results
                    .Where(r => r.ConfigurationItem != null)
                    .ToDictionary(r => r.CompetitorInEventId, r => r.ConfigurationItem.Score);

                foreach (var gce in participants)
                {
                    int stageTotalForGce = 0;

                    foreach (var pick in gce.Renners)
                    {
                        int pickScore = resultByCompetitor.TryGetValue(pick.CompetitorsInEventId, out var sc) ? sc : 0;

                        // stage snapshot for each pick
                        _context.DeelnemerStagePickScores.Add(new DeelnemerStagePickScore
                        {
                            Id = Guid.NewGuid(),
                            GameCompetitorEventPickId = pick.Id,
                            StageId = stage.Id,
                            Score = pickScore,
                            LastUpdated = DateTime.UtcNow
                        });

                        // accumulate pick total
                        if (!pickTotals.ContainsKey(pick.Id)) pickTotals[pick.Id] = 0;
                        pickTotals[pick.Id] += pickScore;

                        stageTotalForGce += pickScore;
                    }

                    // store stage snapshot for participant
                    _context.DeelnemerStageScores.Add(new DeelnemerStageScore
                    {
                        Id = Guid.NewGuid(),
                        GameCompetitorEventId = gce.Id,
                        StageId = stage.Id,
                        Score = stageTotalForGce,
                        LastUpdated = DateTime.UtcNow
                    });

                    participantTotals[gce.Id] += stageTotalForGce;
                    lastStageScoreByParticipant[gce.Id] = stageTotalForGce;
                }
                await _context.SaveChangesAsync();
            }

            // persist pick totals
            foreach (var kv in pickTotals)
            {
                _context.DeelnemerPickScores.Add(new DeelnemerPickScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventPickId = kv.Key,
                    TotalScore = kv.Value,
                    LastUpdate = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync();

            // persist participant totals
            foreach (var gceId in participantTotals.Keys)
            {
                _context.DeelnemerScores.Add(new DeelnemerScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventId = gceId,
                    TotalScore = participantTotals[gceId],
                    LaatsteStageScore = lastStageScoreByParticipant.ContainsKey(gceId) ? lastStageScoreByParticipant[gceId] : 0,
                    LaatsteStageId = (int)(stagesOrdered.LastOrDefault()?.Id),
                    LastUpdated = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }
    }
}
