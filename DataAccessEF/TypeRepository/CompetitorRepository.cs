using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class CompetitorRepository : GenericRepository<Competitor>, ICompetitorRepository
    {
        public CompetitorRepository(ApplicationDbContext context) : base(context) 
        { 
        }

        public IQueryable<Competitor> GetAllCompetitors()
        {
            var competitors = context.Competitors
                .Include(c => c.Country)
                .Include(c => c.CompetitorInTeams)
                    .ThenInclude(cit => cit.Team);

            return competitors;
        }

        public async Task<Competitor> GetById(int competitorId)
        {
            var competitor = await context.Competitors
                .Include(c => c.Country)
                .Include(c => c.CompetitorInTeams)
                    .ThenInclude(cit => cit.Team)
                .FirstOrDefaultAsync(c => c.CompetitorId == competitorId);

            return competitor;
        }

        public async Task<IEnumerable<Competitor>> GetByTeamId(int teamId)
        {
            var competitors = await context.Competitors
                .Include(c => c.Country)
                .Include(c => c.CompetitorInTeams)
                    .ThenInclude(cit => cit.Team)
                .Where(c => c.CompetitorInTeams.Any(cit => cit.TeamId == teamId))
                .ToListAsync();

            return competitors;
        }

        public async Task<int> GetCompetitorsByCountry(int countryId)
        {
            var numberOfCompetitors = await context.Competitors
                .Where(c => c.CountryId == countryId)
                .CountAsync();
            return numberOfCompetitors;
        }
    }
}
