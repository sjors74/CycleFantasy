using CycleManager.Domain.Dto;
using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class ResultsRepository : GenericRepository<Result>, IResultsRepository
    {
        public ResultsRepository(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<IEnumerable<Result>> GetResultsByEventId(int eventId)
        {
            var results = await context.Results
                .Include(c => c.CompetitorInEvent)
                    .ThenInclude(ci => ci.Competitor)
                        .ThenInclude(t => t.Team)
                .Include(s => s.Stage)
                .Include(r => r.ConfigurationItem)
                .Where(r => r.Stage.EventId == eventId)
                .OrderBy(r => r.ConfigurationItem.Position)
                .ToListAsync();

            return results;
        }

        public async Task<CompetitorScoreDto?> GetCompetitorResultsByEventId(int eventId, int competitorId)
        {
            var results = await context.Results
                .Where(r => r.Stage != null && r.CompetitorInEvent != null && r.Stage.EventId == eventId && r.CompetitorInEvent.Id == competitorId)
                .GroupBy(r => r.CompetitorInEventId)
                .Select(g => new CompetitorScoreDto
                {
                    CompetitorInEventId = g.Key,
                    TotalScore = g.Sum(r => r.ConfigurationItem.Score)
                }).FirstOrDefaultAsync();
            return results;
        }

        public async Task<int> GetCompetitorLatestScore(int eventId, int competitorInEventId)
        {
            var configItems = await context.ConfigurationItems.ToListAsync();
            var configDict = configItems.ToDictionary(ci => ci.Id, ci => ci.Score);
            int laatsteStageId = await context.Stages
                                        .Where(s => s.EventId == eventId)
                                        .OrderByDescending(s => s.Id)
                                        .Select(s => s.Id)
                                        .FirstOrDefaultAsync();

            var results = await context.Results
                .Where(r => r.StageId == laatsteStageId && r.CompetitorInEventId == competitorInEventId)
                .ToListAsync();

            int score = results.Sum(r => configDict.TryGetValue(r.ConfigurationItemId, out var s) ? s : 0);
            return score;
        }

        public async Task<int> GetResultsByStageId(int stageId)
        {
            var results = await context.Results
                                .Include(s => s.Stage)
                                .Where(s => s.StageId == stageId)
                                .ToListAsync();
            return results.Count;
        }

        public async Task<List<EtappeUitslagDto>?> GetEtappeUitslag(int stageId)
        {
            var stage = await context.Stages
                .Include(s => s.Event)
                    .ThenInclude(e => e.Configuration)
                .FirstOrDefaultAsync(s => s.Id == stageId);

            if (stage == null)
            {
                return null;
            };

            var configItems = await context.ConfigurationItems
                .AsNoTracking()
                .Where(ci => ci.ConfigurationId == stage.Event.ConfigurationId)
                .OrderBy(ci => ci.Position)
                .ToListAsync();

            var results = await context.Results
                .AsNoTracking()
                .Where(r => r.StageId == stageId)
                .Include(r => r.CompetitorInEvent)
                    .ThenInclude(cie => cie.Competitor)
                        .ThenInclude(t => t.Team)
                .Include(r => r.ConfigurationItem)
                .ToListAsync();

            var top15 = configItems.Select(ci =>
            {
                var result = results.FirstOrDefault(r => r.ConfigurationItem.Position == ci.Position);
                if (result == null || result.CompetitorInEvent?.Competitor == null)
                    return null;

                var competitor = result.CompetitorInEvent.Competitor;
                return new EtappeUitslagDto
                {
                    Positie = ci.Position,
                    CompetitorName = $"{competitor.FirstName} {competitor.LastName}",
                    TeamName = competitor.Team?.TeamName,
                    Score = ci.Score
                };
            })
            .Where(r => r != null)
            .ToList();

            return top15;
        }
    }
}
