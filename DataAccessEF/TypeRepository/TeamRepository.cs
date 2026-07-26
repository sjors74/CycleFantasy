using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
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
                .Include(t => t.Country)
                .Include(t => t.TeamYears)
                    .ThenInclude(ty => ty.SeasonYear)
                .Include(t => t.TeamYears)
                    .ThenInclude(ty => ty.CompetitorInTeams)
                        .ThenInclude(cit => cit.Competitor)
                            .ThenInclude(c => c.Country)
                .ToListAsync();

            return teams;
        }

        public async Task<List<TeamYearDto>> GetTeamYears(int seasonYearId)
        {
            return await context.TeamYear
                .Where(ty => ty.SeasonYearId == seasonYearId)
                .OrderBy(ty => ty.Name)
                 .Select(ty => new TeamYearDto
                 {
                     TeamYearId = ty.TeamYearId,
                     Name = ty.Name
                 })
                .ToListAsync();
        }

        public async Task<Team> GetTeamById(int id)
        {
            var team = await context.Teams
                .Include(t => t.Country)
                .Include(t => t.TeamYears)
                    .ThenInclude(ty => ty.SeasonYear)
                .FirstOrDefaultAsync(t => t.TeamId == id);

            return team;
        }

        public async Task<Team> GetTeamForCurrentYear(int id, int year)
        {
            var team = await context.Teams
                .Include(t => t.Country)
                .Include(t => t.TeamYears)
                    .ThenInclude(ty => ty.SeasonYear)
                .Include(t => t.TeamYears)
                    .ThenInclude(ty => ty.CompetitorInTeams)
                        .ThenInclude(cit => cit.Competitor)
                            .ThenInclude(c => c.Country)
                .FirstOrDefaultAsync(t =>
                    t.TeamId == id &&
                    t.TeamYears.Any(ty => ty.SeasonYear.Year == year));

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

        public async Task<TeamYear?> GetTeamYearByIdAsync(int teamYearId)
        {
            return await context.TeamYear
                .Include(ty => ty.Team)
                .Include(ty => ty.SeasonYear)
                .FirstOrDefaultAsync(ty => ty.TeamYearId == teamYearId);
        }

        public async Task<bool> HasUnprocessedScrapedCompetitors()
        {
            return await context.ScrapedCompetitors.AnyAsync(t => t.ProcessedAt == null);
        }

        public async Task<TeamYearDto?> GetByTeamAndSeasonAsync(int teamId, int seasonYearId)
        {
            return await context.TeamYear
                .Where(ty => ty.TeamId == teamId &&
                         ty.SeasonYearId == seasonYearId)
                .Select(ty => new TeamYearDto
                {
                    TeamYearId = ty.TeamYearId,
                    Name = ty.Name,

                })
            .FirstOrDefaultAsync();
        }
    }
}
