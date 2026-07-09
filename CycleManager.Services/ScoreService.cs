using CycleManager.Domain.Enums;
using CycleManager.Domain.Models;
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
            // --- 1. DEELNEMERS ---
            var deelnemers = await _context.GameCompetitorsEvent
                .Include(d => d.Renners)
                .Where(d => d.EventId == eventId)
                .ToListAsync();

            // --- 2. NORMAL RESULTS  ---
            var resultsLookup = await _context.Results
                .Where(r => r.StageId == stageId)
                .Include(r => r.ConfigurationItem)
                .GroupBy(r => r.CompetitorInEventId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.First().ConfigurationItem.Score
                );

            // --- 3. SPECIAL RESULTS ---
            var specialResults = await _context.SpecialResults
                .Where(r => r.StageId == stageId)
                .Include(r => r.Special)
                .ToListAsync();

            // --- 4. EXISTING DATA ---
            var existingStagePickScores = await _context.DeelnemerStagePickScores
                .Where(s => s.StageId == stageId)
                .ToDictionaryAsync(s => s.GameCompetitorEventPickId);

            var existingStagePickSpecialScores = await _context.DeelnemerStagePickSpecialScores
                .Where(s => s.StageId == stageId)
                .ToDictionaryAsync(x => new
                {
                    x.GameCompetitorEventPickId,
                    x.QuestionType
                });

            var existingPickTotals = await _context.DeelnemerPickScores
                .ToDictionaryAsync(p => p.GameCompetitorEventPickId);

            var existingStageScores = await _context.DeelnemerStageScores
                .Where(s => s.StageId == stageId)
                .ToDictionaryAsync(s => s.GameCompetitorEventId);

            var existingTotals = await _context.DeelnemerScores
                .GroupBy(s => s.GameCompetitorEventId)
                .Select(g => g.OrderByDescending(x => x.LastUpdated).First())
                .ToDictionaryAsync(s => s.GameCompetitorEventId);

            // --- 5. NEW RECORDS ---
            var newStagePickScores = new List<DeelnemerStagePickScore>();
            var newStagePickSpecialScores = new List<DeelnemerStagePickSpecialScore>();
            var newPickTotals = new List<DeelnemerPickScore>();
            var newStageScores = new List<DeelnemerStageScore>();
            var newTotals = new List<DeelnemerScore>();

            var stageSnapshotByParticipant = new Dictionary<int, int>();

            // --- 6. MAIN LOOP ---
            foreach (var deelnemer in deelnemers)
            {
                int stageTotalForDeelnemer = 0;

                foreach (var pick in deelnemer.Renners)
                {
                    int normalScore =
                        resultsLookup.TryGetValue(pick.CompetitorsInEventId, out var n)
                            ? n
                            : 0;

                    int specialScore = 0;

                    var specialsForPick = specialResults
                        .Where(x => x.CompetitorInEventId == pick.CompetitorsInEventId)
                        .ToList();

                    foreach(var special in specialsForPick)
                    {
                        var key = new
                        {
                            GameCompetitorEventPickId = pick.Id,
                            QuestionType = special.Special!.Question
                        };

                        int score = special.Special.Score;

                        specialScore += score;

                        if (existingStagePickSpecialScores.TryGetValue(key, out var existing))
                        {
                            existing.Score = score;
                            existing.LastUpdated = DateTime.UtcNow;
                        }
                        else
                        {
                            newStagePickSpecialScores.Add(new DeelnemerStagePickSpecialScore
                            {
                                Id = Guid.NewGuid(),
                                GameCompetitorEventPickId = pick.Id,
                                StageId = stageId,
                                QuestionType = special.Special.Question,
                                Score = score,
                                LastUpdated = DateTime.UtcNow
                            });
                        }
                    }

                    int totalPickScore = normalScore + specialScore;

                    stageTotalForDeelnemer += totalPickScore;

                    // =============================
                    // NORMAL STAGE SCORE
                    // =============================

                    int previousNormalScore = 0;

                    if (existingStagePickScores.TryGetValue(pick.Id, out var existingNormal))
                    {
                        previousNormalScore = existingNormal.Score;

                        existingNormal.Score = normalScore;
                        existingNormal.LastUpdated = DateTime.UtcNow;
                    }
                    else
                    {
                        newStagePickScores.Add(new DeelnemerStagePickScore
                        {
                            Id = Guid.NewGuid(),
                            GameCompetitorEventPickId = pick.Id,
                            StageId = stageId,
                            Score = normalScore,
                            LastUpdated = DateTime.UtcNow
                        });
                    }

                    // =============================
                    // UPDATE PICK TOTAL
                    // =============================

                    int newPickTotal = normalScore + specialScore;

                    if (existingPickTotals.TryGetValue(pick.Id, out var pickTotal))
                    {
                        pickTotal.TotalScore = newPickTotal;
                        pickTotal.LastUpdate = DateTime.UtcNow;
                    }
                    else
                    {
                        newPickTotals.Add(new DeelnemerPickScore
                        {
                            Id = Guid.NewGuid(),
                            GameCompetitorEventPickId = pick.Id,
                            TotalScore = newPickTotal,
                            LastUpdate = DateTime.UtcNow
                        });
                    }
                }

                // --- STAGE SNAPSHOT ---
                stageSnapshotByParticipant[deelnemer.Id] = stageTotalForDeelnemer;

                if (existingStageScores.TryGetValue(deelnemer.Id, out var stageScore))
                {
                    stageScore.Score = stageTotalForDeelnemer;
                    stageScore.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    newStageScores.Add(new DeelnemerStageScore
                    {
                        Id = Guid.NewGuid(),
                        GameCompetitorEventId = deelnemer.Id,
                        StageId = stageId,
                        Score = stageTotalForDeelnemer,
                        LastUpdated = DateTime.UtcNow
                    });
                }
            }

            // --- 7. SAVE STAGE DATA ---
            if (newStagePickScores.Any())
                _context.DeelnemerStagePickScores.AddRange(newStagePickScores);

            if (newStagePickSpecialScores.Any())
                _context.DeelnemerStagePickSpecialScores.AddRange(newStagePickSpecialScores);

            if (newPickTotals.Any())
                _context.DeelnemerPickScores.AddRange(newPickTotals);

            if (newStageScores.Any())
                _context.DeelnemerStageScores.AddRange(newStageScores);

            await _context.SaveChangesAsync();

            // --- 8. REBUILD TOTALS ---
            var allPickIds = deelnemers.SelectMany(d => d.Renners.Select(r => r.Id)).ToList();

            var pickTotalsFromDb = await _context.DeelnemerPickScores
                .Where(p => allPickIds.Contains(p.GameCompetitorEventPickId))
                .ToListAsync();

            var pickTotalMap = pickTotalsFromDb
                .ToDictionary(x => x.GameCompetitorEventPickId, x => x.TotalScore);

            foreach (var deelnemer in deelnemers)
            {
                var pickIds = deelnemer.Renners.Select(r => r.Id);

                int computedTotal = pickIds.Sum(pid =>
                    pickTotalMap.TryGetValue(pid, out var val) ? val : 0);

                int lastStageScore = stageSnapshotByParticipant.TryGetValue(deelnemer.Id, out var ss)
                    ? ss
                    : 0;

                if (existingTotals.TryGetValue(deelnemer.Id, out var total))
                {
                    total.TotalScore = computedTotal;
                    total.LaatsteStageScore = lastStageScore;
                    total.LaatsteStageId = stageId;
                    total.LastUpdated = DateTime.UtcNow;
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

            if (newTotals.Any())
                _context.DeelnemerScores.AddRange(newTotals);

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

                var toRemoveStagePickSpecialScores =  _context.DeelnemerStagePickSpecialScores.Where(s => allPickIds.Contains(s.GameCompetitorEventPickId));
                _context.DeelnemerStagePickSpecialScores.RemoveRange(toRemoveStagePickSpecialScores);

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

                var specialResults = await _context.SpecialResults
                    .Where(r => r.StageId == stage.Id)
                    .Include(r => r.Special)
                    .ToListAsync();

                var specialsLookup = specialResults.ToLookup(x => x.CompetitorInEventId);

                foreach (var gce in participants)
                {
                    int stageTotalForGce = 0;

                    foreach (var pick in gce.Renners)
                    {
                        int normalScore = resultByCompetitor.TryGetValue(pick.CompetitorsInEventId,out var sc) ? sc : 0;

                        int specialScore = 0;

                        foreach (var special in specialsLookup[pick.CompetitorsInEventId])
                        {
                            _context.DeelnemerStagePickSpecialScores.Add(
                                new DeelnemerStagePickSpecialScore
                                {
                                    Id = Guid.NewGuid(),
                                    GameCompetitorEventPickId = pick.Id,
                                    StageId = stage.Id,
                                    QuestionType = special.Special.Question,
                                    Score = special.Special.Score,
                                    LastUpdated = DateTime.UtcNow
                                });

                            specialScore += special.Special.Score;
                        }

                        // stage snapshot for each pick
                        _context.DeelnemerStagePickScores.Add(new DeelnemerStagePickScore
                        {
                            Id = Guid.NewGuid(),
                            GameCompetitorEventPickId = pick.Id,
                            StageId = stage.Id,
                            Score = normalScore,
                            LastUpdated = DateTime.UtcNow
                        });

                        // accumulate pick total
                        if (!pickTotals.ContainsKey(pick.Id)) pickTotals[pick.Id] = 0;
                        pickTotals[pick.Id] += normalScore + specialScore;

                        stageTotalForGce += normalScore + specialScore;
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
