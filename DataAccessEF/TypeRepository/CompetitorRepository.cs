using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class CompetitorRepository : GenericRepository<Competitor>, ICompetitorRepository
    {
        public CompetitorRepository(DatabaseContext context) : base(context) 
        { 
        }

        public IQueryable<Competitor> GetAllCompetitors()
        {
            return context.Set<Competitor>();
        }

        public async Task<IEnumerable<Competitor>> GetByTeamId(int teamId)
        {
            var competitors = await context.Competitors.Where(c => c.TeamId.Equals(teamId)).ToListAsync();
            return  competitors;
        }

        public async Task<int> GetCompetitorsByCountry(int countryId)
        {
            var numberOfCompetitors = await context.Competitors.Where(c => c.CountryId.Equals(countryId)).CountAsync();
            return numberOfCompetitors;
        }
    }
}
