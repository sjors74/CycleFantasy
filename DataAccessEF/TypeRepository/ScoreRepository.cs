using CycleManager.Domain.Dto;
using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class ScoreRepository : GenericRepository<DeelnemerScore>, IScoreRepository
    {
        public ScoreRepository(ApplicationDbContext context) : base(context)
        {

        }
        public async Task<List<DeelnemerStageScore>> GetScoresByEventIdAsync(int eventId)
        {
            // alle stageIds van het event ophalen
            var stageIds = await context.Stages
                .Where(s => s.EventId == eventId)
                .Select(s => s.Id)
                .ToListAsync();

            // stage scores ophalen
            return await context.DeelnemerStageScores
                .Where(s => stageIds.Contains(s.StageId))
                .ToListAsync();
        }

        public async Task<List<DeelnemerDto>> GetPoolRankingForStage(int eventId, int stageId)
        {
            // 1. Haal de target stage order op
            var targetStage = await context.Stages
                .AsNoTracking()
                .Where(s => s.Id == stageId && s.EventId == eventId)
                .Select(s => new { s.StageOrder })
                .FirstOrDefaultAsync();

            if (targetStage == null)
                throw new ArgumentException($"Stage {stageId} bestaat niet voor event {eventId}");

            var targetStageOrder = targetStage.StageOrder;

            // 2. Haal alle stages tot en met target stage
            var stageIds = await context.Stages
                .Where(s => s.EventId == eventId && s.StageOrder <= targetStageOrder)
                .Select(s => s.Id)
                .ToListAsync();

            // 3. Haal alle deelnemers van dit event
            var deelnemers = await context.GameCompetitorsEvent
                .AsNoTracking()
                .Where(gce => gce.EventId == eventId)
                .Select(gce => new
                {
                    gce.Id,
                    gce.TeamName,
                    UserFirstName = gce.User != null ? gce.User.FirstName : "",
                    UserLastName = gce.User != null ? gce.User.LastName : ""
                })
                .ToListAsync();

            var deelnemerIds = deelnemers.Select(d => d.Id).ToList();

            // 4. Haal scores van alle stages tot target stage
            var stageScores = await context.DeelnemerStageScores
                .AsNoTracking()
                .Where(ds => stageIds.Contains(ds.StageId) && deelnemerIds.Contains(ds.GameCompetitorEventId))
                .ToListAsync();

            // 5. Groepeer per deelnemer en som scores
            var result = stageScores
                .GroupBy(ds => ds.GameCompetitorEventId)
                .Select(g =>
                {
                    var d = deelnemers.First(d => d.Id == g.Key);
                    return new DeelnemerDto
                    {
                        Id = d.Id,
                        EventId = eventId,
                        DeelnemerNaam = $"{d.UserFirstName} {d.UserLastName}".Trim(),
                        PoolNaam = d.TeamName,
                        Punten = g.Sum(x => x.Score)
                    };
                })
                .OrderByDescending(x => x.Punten)
                .ToList();

            return result;
        }

    }
}
