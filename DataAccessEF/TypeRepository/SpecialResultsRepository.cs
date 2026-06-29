using CycleManager.Domain.Dto;
using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccessEF.TypeRepository
{
    public class SpecialResultsRepository : GenericRepository<SpecialResult>, ISpecialResultsRepository
    {
        public SpecialResultsRepository(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<IEnumerable<SpecialResult>> GetByEventId(int eventId)
        {
            var results = await context.SpecialResults
                .Include(c => c.CompetitorInEvent)
                    .ThenInclude(c => c.CompetitorInTeam)
                        .ThenInclude(cit => cit.Team)
                 .Include(c => c.CompetitorInEvent)
                    .ThenInclude(c => c.CompetitorInTeam)
                        .ThenInclude(c => c.Competitor)
                .Include(s => s.Stage)
                .Include(r => r.Special)
                .Where(r => r.Stage != null && r.Stage.EventId == eventId)
                .OrderBy(r => r.Special != null ? r.Special.Question : default)
                .ToListAsync();
            return results;
        }

        public async Task<List<SpecialResult>> GetByStageAsync(int stageId)
        {
            var results = await context.SpecialResults
                .Include(c => c.CompetitorInEvent)
                    .ThenInclude(c => c.CompetitorInTeam)
                        .ThenInclude(cit => cit.Team)
                 .Include(c => c.CompetitorInEvent)
                    .ThenInclude(c => c.CompetitorInTeam)
                        .ThenInclude(c => c.Competitor)
                .Include(s => s.Stage)
                .Include(r => r.Special)
                .Where(r => r.Stage != null && r.Stage.Id == stageId)
                .OrderBy(r => r.Special != null ? r.Special.Question : default)
                .ToListAsync();
            return results;
        }
    }
}
