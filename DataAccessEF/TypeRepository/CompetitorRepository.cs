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

        public async Task<IEnumerable<Competitor>> GetByTeamId(int teamId)
        {
            var competitors = await context.Competitors.Where(c => c.TeamId.Equals(teamId)).ToListAsync();
            return  competitors;
        }
    }
}
