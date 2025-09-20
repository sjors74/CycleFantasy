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
                .Include(ct => ct.Country)
                .Include(t => t.CompetitorInTeams) // koppelingen tussen team en competitors
                    .ThenInclude(cit => cit.Competitor) // de competitors zelf
                        .ThenInclude(c => c.Country) // optioneel: land van de competitor
                .ToListAsync();

            return teams;
        }

        public async Task<Team> GetTeamById(int id)
        {
            var team = await context.Teams
                .Include(t => t.Country) // land van het team
                .Include(t => t.CompetitorInTeams) // koppelingen tussen team en competitors
                    .ThenInclude(cit => cit.Competitor) // de competitors zelf
                        .ThenInclude(c => c.Country) // optioneel: land van de competitor
                .FirstOrDefaultAsync(t => t.TeamId == id);

            return team;
        }
    }
}
