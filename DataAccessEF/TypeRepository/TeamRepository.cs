using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class TeamRepository : GenericRepository<Team>, ITeamRepository
    {
        public TeamRepository(ApplicationDbContext context) : base(context) { }

        public async Task<int> CountUnprocessedScrapedCompetitors()
        {
            return await context.ScrapedCompetitors.CountAsync(s => s.ProcessedAt == null);
        }

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

        public async Task<Team> GetTeamForCurrentYear(int id, int year)
        {
            var team = await context.Teams
                .Include(t => t.Country)
                .Include(t => t.CompetitorInTeams)
                    .ThenInclude(cit => cit.Competitor)
                        .ThenInclude(c => c.Country)
                .Include(t => t.TeamYears)
                .FirstOrDefaultAsync(t => 
                    t.TeamId == id &&
                    t.TeamYears.Any(ty => ty.Year == year));
            return team;
            
        }

        public async Task<IEnumerable<Team>> GetTeamsForEvent(int eventId)
        {
            var teams = await context.Teams
                .Include(t => t.EventTeams)
                .Where(t => t.EventTeams.Any(t => t.EventId == eventId))
                .ToListAsync();

            return teams;
        }

        public async Task<bool> HasUnprocessedScrapedCompetitors()
        {
            return await context.ScrapedCompetitors.AnyAsync(t => t.ProcessedAt == null);
        }

    }
}
