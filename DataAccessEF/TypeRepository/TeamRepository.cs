using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class TeamRepository : GenericRepository<Team>, ITeamRepository
    {
        public TeamRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Team>> GetAll()
        {
            var teams = await context.Teams
                .Include(c => c.Country)
                .ToListAsync();
            return teams;
        }

        public async Task<Team> GetTeamById(int id)
        {
            var team = await context.Teams
                .Include(c => c.Country)
                .Where(c => c.TeamId == id)
                .FirstOrDefaultAsync();

            return team;
        }
    }
}
