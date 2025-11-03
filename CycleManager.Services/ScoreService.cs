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

            var resultsForStage = await _context.Results
                .Where(r => r.StageId == stageId)
                .Include(r => r.ConfigurationItem)
                .ToListAsync();

            var existingPickScores = await _context.DeelnemerPickScores
                .Where(s => s.StageId == stageId)
                .ToListAsync();

            var existingTotalScores = await _context.DeelnemerScores
                .Where(s => s.StageId == stageId)
                .ToListAsync();

            var resultsLookup = resultsForStage
                .GroupBy(r => r.CompetitorInEventId)
                .ToDictionary(g => g.Key, g => g.First().ConfigurationItem.Score);

            var newPickScores = new List<DeelnemerPickScore>();
            var newTotalScores = new List<DeelnemerScore>();


            foreach (var deelnemer in deelnemers)
            {
                int totalScore = 0;

                foreach (var pick in deelnemer.Renners)
                {
                    int pickScore = resultsLookup.TryGetValue(pick.CompetitorsInEventId, out var score)
                         ? score : 0;
                                        
                    var existingPick = existingPickScores
                        .FirstOrDefault(s => s.GameCompetitorEventPickId == pick.Id);

                    if (existingPick != null)
                    {
                        existingPick.Score = pickScore;
                        existingPick.LastUpdate = DateTime.UtcNow;
                    }
                    else
                    {
                        newPickScores.Add(new DeelnemerPickScore
                        {
                            Id = Guid.NewGuid(),
                            GameCompetitorEventPickId = pick.Id,
                            StageId = stageId,
                            Score = pickScore,
                            LastUpdate = DateTime.UtcNow
                        });
                    }

                    totalScore += pickScore;
                }


                var existingTotal = existingTotalScores
                    .FirstOrDefault(s => s.GameCompetitorEventId == deelnemer.Id);

                if(existingTotal != null)
                {
                    existingTotal.TotalScore = totalScore;
                    existingTotal.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    newTotalScores.Add(new DeelnemerScore
                    {
                        Id = Guid.NewGuid(),
                        GameCompetitorEventId = deelnemer.Id,
                        StageId = stageId,
                        TotalScore = totalScore,
                        LastUpdated = DateTime.UtcNow
                    });
                }
            }

            if(newPickScores.Any())
                _context.DeelnemerPickScores.AddRange(newPickScores);
            
            if(newTotalScores.Any())
                _context.DeelnemerScores.AddRange(newTotalScores);

            await _context.SaveChangesAsync();
        }
    }
}
