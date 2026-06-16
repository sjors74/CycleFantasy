using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using Domain.Context;
using Domain.Dto;
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

        public async Task<List<CompetitorDto>> GetAllCompetitors(int year)
        {
            var competitors = await context.Competitors
                .Where(c => c.CompetitorInTeams.Any(cit => cit.Year == year))
                .Select(c => new CompetitorDto
                {
                    CompetitorId = c.CompetitorId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    PcsName = c.PcsName,
                    ScraperName = c.ScraperName,
                    CountryShort = c.Country.CountryNameShort,
                    Teams = c.CompetitorInTeams
                        .Where(cit => cit.Year == year)
                        .Select(cit => new CompetitorInTeamDto
                        {
                            CompetitorInTeamId = cit.Id,
                            TeamId = cit.TeamId,
                            TeamName = cit.Team.CurrentTeamName,
                            Year = cit.Year,
                            IsNationalChampion = cit.IsNationalChampion
                        })
                        .ToList() // mag blijven voor materialisatie
                })
                .AsNoTracking()
                .ToListAsync();

            return competitors;
        }

        public async Task<Competitor> GetById(int competitorId)
        {
            var competitor = await context.Competitors
                .Include(c => c.Country)
                .Include(c => c.CompetitorInTeams)
                    .ThenInclude(cit => cit.Team)
                        .ThenInclude(t => t.TeamYears)
                .Include(c => c.CompetitorInTeams)
                    .ThenInclude(cit => cit.TeamYear)
                .FirstOrDefaultAsync(c => c.CompetitorId == competitorId);

            return competitor;
        }

        public async Task<IEnumerable<CompetitorInTeamDto>> GetByTeamId(int teamId, int year)
        {
            var competitors = await context.CompetitorInTeams
                .Include(cit => cit.Competitor)
                    .ThenInclude(c => c.Country)
                .Include(cit => cit.Team)
                .Where(cit => cit.TeamId == teamId && cit.Year == year)
                .Select(cit => new CompetitorInTeamDto
                {

                    CompetitorInTeamId = cit.Id,
                    FirstName = cit.Competitor.FirstName,
                    LastName = cit.Competitor.LastName,
                    CompetitorName = cit.Competitor.CompetitorName,
                    TeamName = cit.Team.CurrentTeamName,
                    TeamId = cit.TeamId,
                    Year = cit.Year
                })
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

        public async Task UpdateCompetitorWithTeam(CompetitorEditDto dto)
        {
            try
            {
                var competitor = await context.Competitors
                    .Include(c => c.CompetitorInTeams)
                    .FirstOrDefaultAsync(c => c.CompetitorId == dto.CompetitorId);

                if (competitor == null)
                    throw new KeyNotFoundException($"Competitor met ID {dto.CompetitorId} niet gevonden.");

                competitor.FirstName = dto.FirstName;
                competitor.LastName = dto.LastName;
                competitor.PcsName = dto.PcsName;
                competitor.ScraperName = dto.ScraperName;
                competitor.CountryId = dto.CountryId;

                var existingCit = competitor.CompetitorInTeams
                    .FirstOrDefault(cit => cit.Year == dto.SelectedYear);

                if (existingCit == null)
                {
                    context.CompetitorInTeams.Add(new CompetitorInTeam
                    {
                        CompetitorId = competitor.CompetitorId,
                        TeamId = dto.SelectedTeamId,
                        Year = dto.SelectedYear
                    });
                }
                else
                {
                    existingCit.TeamId = dto.SelectedTeamId;
                }

                await context.SaveChangesAsync();
            }
            catch (KeyNotFoundException)
            {
                throw; // laat de KeyNotFoundException door
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Fout bij updaten van renner met team.", ex);
            }
        }

        public async Task<Competitor?> GetByIdWithTeamsAsync(int id)
        {
            return await context.Competitors
                    .Include(c => c.CompetitorInTeams)
                    .FirstOrDefaultAsync(c => c.CompetitorId == id);
        }

        public async Task UpdateCompetitorAsync(Competitor competitor)
        {
            var existingCompetitor = await context.Competitors
                .Include(c => c.CompetitorInTeams)
                .FirstOrDefaultAsync(c => c.CompetitorId == competitor.CompetitorId);

            if (existingCompetitor != null)
            {
                foreach (var updatedCit in competitor.CompetitorInTeams)
                {
                    var existingCit = existingCompetitor.CompetitorInTeams
                        .FirstOrDefault(c => c.Id == updatedCit.Id);

                    if (existingCit == null)
                    {
                        existingCompetitor.CompetitorInTeams.Add(updatedCit);
                    }
                    else
                    {
                        context.Entry(existingCit).CurrentValues.SetValues(updatedCit);
                    }
                }
            }
            await context.SaveChangesAsync();
        }

        public async Task<List<CompetitorInTeam>> GetCompetitorInTeamsByIdsAsync(List<int> ids)
        {
            return await context.CompetitorInTeams
                .Where(cit => ids.Contains(cit.Id))
                .ToListAsync();
        }
    }
}
