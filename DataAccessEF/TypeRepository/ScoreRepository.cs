using CycleManager.Domain.Dto;
using DataAccessEF.Migrations;
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

        public async Task<List<DeelnemerDto>> GetPoolRankingForStage(int eventId, int stageId)
        {
            var stage = await context.Stages
                .AsNoTracking()
                .Where(s => s.Id == stageId && s.EventId == eventId)
                .Select(s => new { s.StageOrder })
                .FirstOrDefaultAsync();

            if (stage == null)
                throw new ArgumentException($"Stage met id {stageId} bestaat niet voor event {eventId}");

            var targetStageOrder = stage.StageOrder;


            var result = await context.DeelnemerScores
                .AsNoTracking()
                .Where(ds => 
                    ds.Stage.StageOrder <= targetStageOrder &&  
                    ds.Stage.EventId == eventId)
                .GroupBy(ds => new
                {
                    ds.GameCompetitorEventId,
                    FirstName = ds.GameCompetitorEvent.User != null ? ds.GameCompetitorEvent.User.FirstName : "",
                    LastName = ds.GameCompetitorEvent.User != null ? ds.GameCompetitorEvent.User.LastName : "",
                    GameCompetitorName = ds.GameCompetitorEvent.TeamName
                })
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
