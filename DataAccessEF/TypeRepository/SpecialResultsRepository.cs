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

        public async Task<SpecialResult?> GetByIdAsync(int id)
        {
            return await context.SpecialResults
                .Include(s => s.Special)
                .Include(s => s.CompetitorInEvent)
                    .ThenInclude(c => c.CompetitorInTeam)
                        .ThenInclude(ct => ct.Competitor)
                .Include(s => s.Stage)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await context.SpecialResults.FindAsync(id);
            if (entity == null)
                return;

            context.SpecialResults.Remove(entity);
            await context.SaveChangesAsync();
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

        public async Task<List<ConfigurationItemSpecial>> GetConfigurationItemsByConfigAsync(int configId)
        {
            return await context.ConfigurationItemSpecials
                .AsNoTracking()
                .Where(ci => ci.ConfigurationId == configId)
                .ToListAsync();
        }

        public async Task AddResultsAsync(IEnumerable<SpecialResult> specialResults)
        {
            context.SpecialResults.AddRange(specialResults);
            await context.SaveChangesAsync();

        }
    }
}
