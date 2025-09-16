using CycleManager.Domain.Dto;
using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DataAccessEF.TypeRepository
{
    public class ScoreRepository : GenericRepository<DeelnemerScore>, IScoreRepository
    {
        public ScoreRepository(ApplicationDbContext context) : base(context)
        {

        }
        public async Task<List<DeelnemerScore>> GetScoresByEventIdAsync(int eventId)
        {
            return await context.DeelnemerScores
                .Where(s => s.Stage.EventId == eventId)
                .ToListAsync();
        }

        public async Task<List<DeelnemerDto>> GetPoolRankingForStage(int eventId, string stageNumber)
        {
            if (!int.TryParse(stageNumber, out int targetStageNumber))
            {
                throw new ArgumentException("StageNumber is geen geldig getal", nameof(stageNumber));
            }
            var result = await context.DeelnemerScores
                .Where(ds => Convert.ToInt32(ds.Stage.StageName) <= targetStageNumber &&  ds.Stage.EventId == eventId)
                .GroupBy(ds => new
                {
                    ds.GameCompetitorEventId,
                    FirstName = ds.GameCompetitorEvent.User != null ? ds.GameCompetitorEvent.User.FirstName : "",
                    LastName = ds.GameCompetitorEvent.User != null ? ds.GameCompetitorEvent.User.LastName : "",
                    GameCompetitorName = ds.GameCompetitorEvent.TeamName
                })
                //TODO: laatste score?
                //TODO: renners?
                .Select(g => new DeelnemerDto
                {
                    DeelnemerNaam = g.Key.FirstName + " " + g.Key.LastName,
                    PoolNaam = g.Key.GameCompetitorName,
                    Id = g.Key.GameCompetitorEventId,
                    EventId = eventId,
                    Punten = g.Sum(x => x.TotalScore)
                })
                .OrderByDescending(x => x.Punten)
                .ToListAsync();

            return result;
        }
    }
}
