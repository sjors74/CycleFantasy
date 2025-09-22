using CycleManager.Domain.Models;
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

        public async Task<List<Competitor>> GetAllCompetitors(int year)
        {
            var competitors = await context.Competitors
                .Include(c => c.CompetitorInTeams) // <-- zo heb je de teams van de renner
                    .ThenInclude(cit => cit.Team)   // inclusief Team
                .Include(c => c.Country)            // land van de renner
                .AsNoTracking()
                .Where(c => c.CompetitorInTeams.Any(cit => cit.Year == year))
                .ToListAsync();

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

        public async Task<List<int>> GetAvailableYears()
        {
            return await context.CompetitorInTeams
                .Select(cit => cit.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
        }

        public async Task<Competitor?> GetCompetitorByName(string firstName, string lastName, int countryId)
        {
            var competitors = await context.Competitors
                .FirstOrDefaultAsync(c =>
                    c.FirstName == firstName &&
                    c.LastName == lastName &&
                    c.CountryId == countryId);

            return competitors;
        }

        public IQueryable<Competitor> GetCompetitorsByTerm(string term)
        {
            var competitors = context.Competitors
                .Where(c => c.FirstName.Contains(term) || c.LastName.Contains(term))
                .OrderBy(c => c.LastName)
                .Take(20); // limiteren voor performance
            return competitors;
        }
    }
}
