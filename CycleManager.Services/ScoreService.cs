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

            var existingTotals = await _context.DeelnemerScores
                .ToDictionaryAsync(s => s.GameCompetitorEventId);

            var newStagePickScores = new List<DeelnemerStagePickScore>();
            var newPickTotals = new List<DeelnemerPickScore>();
            var newStageScores = new List<DeelnemerStageScore>();
            var newTotals = new List<DeelnemerScore>();


            foreach (var deelnemer in deelnemers)
            {
                int newStageTotalForDeelnemer = 0;

                foreach(var pick in deelnemer.Renners)
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
                        // eerste keer dat we deze pick tegenkomen; TotalScore = newPickScore
                        newPickTotals.Add(new DeelnemerPickScore
                        {
                            Id = Guid.NewGuid(),
                            GameCompetitorEventPickId = pick.Id,
                            TotalScore = newPickScore,
                            LastUpdate = DateTime.UtcNow
                        });
                    }
                }

                existingStageScores.TryGetValue(deelnemer.Id, out var prevStageScoreEntry);
                int prevStageScore = prevStageScoreEntry?.Score ?? 0;
                int participantDelta = newStageTotalForDeelnemer - prevStageScore;

                // update of insert DeelnemerStageScore (snapshot)
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

                if (existingTotals.TryGetValue(deelnemer.Id, out var totalEntry))
                {
                    if (participantDelta != 0)
                    {
                        totalEntry.TotalScore += participantDelta;
                    }

                    totalEntry.LaatsteStageScore = newStageTotalForDeelnemer;
                    totalEntry.LaatsteStageId = stageId;
                    totalEntry.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    newTotals.Add(new DeelnemerScore
                    {
                        Id = Guid.NewGuid(),
                        GameCompetitorEventId = deelnemer.Id,
                        TotalScore = newStageTotalForDeelnemer,
                        LaatsteStageScore = newStageTotalForDeelnemer,
                        LaatsteStageId = stageId,
                        LastUpdated = DateTime.UtcNow
                    });
                }
            }

            if (newStagePickScores.Any()) _context.DeelnemerStagePickScores.AddRange(newStagePickScores);
            if (newPickTotals.Any()) _context.DeelnemerPickScores.AddRange(newPickTotals);
            if (newStageScores.Any()) _context.DeelnemerStageScores.AddRange(newStageScores);
            if (newTotals.Any()) _context.DeelnemerScores.AddRange(newTotals);

            await _context.SaveChangesAsync();
        }
    }
}
