using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class TeamRepository : GenericRepository<Team>, ITeamRepository
    {
        public TeamRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Team>> GetAllTeams()
        {
            var teams = await context.Teams
                .Include(ct => ct.Country)
                .Include(t => t.CompetitorInTeams)
                    .ThenInclude(cit => cit.Competitor)
                        .ThenInclude(c => c.Country)
                .Include(t => t.TeamYears)
                .ToListAsync();

            return teams;
        }

        public async Task<Team> GetTeamById(int id)
        {
            var team = await context.Teams
                .Include(t => t.Country)
                .Include(t => t.CompetitorInTeams)
                    .ThenInclude(cit => cit.Competitor)
                        .ThenInclude(c => c.Country)
                .Include(t => t.TeamYears)
                .FirstOrDefaultAsync(t => t.TeamId == id);

            return team;
        }
    }
}
