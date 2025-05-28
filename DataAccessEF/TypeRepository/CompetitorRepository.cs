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
            .Include(c => c.Team)
            .Include(c => c.Country);
            return competitors;
        }

        public async Task<Competitor> GetById(int competitorId)
        {
            var competitor = await context.Competitors
                .Include(c => c.Team)
                .Include(c => c.Country)
                .Where(c => c.CompetitorId.Equals(competitorId))
                .FirstOrDefaultAsync();
            return competitor;
        }

        public async Task<IEnumerable<Competitor>> GetByTeamId(int teamId)
        {
            var competitors = await context.Competitors
                .Include(c => c.Team)
                .Include(c => c.Country)
                .Where(c => c.TeamId.Equals(teamId)).ToListAsync();
            return  competitors;
        }

        public async Task<int> GetCompetitorsByCountry(int countryId)
        {
            var numberOfCompetitors = await context.Competitors
                .Include(c => c.Team)
                .Include(c => c.Country)
                .Where(c => c.CountryId.Equals(countryId)).CountAsync();
            return numberOfCompetitors;
        }
    }
}
