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

        public async Task<List<CompetitorDto>> GetAllCompetitors(int seasonYearId)
        {
            var competitors = await context.Competitors
                .Where(c => c.CompetitorInTeams.Any(cit => cit.TeamYear.SeasonYearId == seasonYearId))
                .Select(c => new CompetitorDto
                {
                    CompetitorId = c.CompetitorId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    PcsName = c.PcsName,
                    ScraperName = c.PcsScraperName,
                    CountryShort = c.Country.CountryNameShort,

                    Teams = c.CompetitorInTeams
                        .Where(cit => cit.TeamYear.SeasonYearId == seasonYearId)
                        .Select(cit => new CompetitorInTeamDto
                        {
                            CompetitorInTeamId = cit.Id,
                            TeamId = cit.TeamYear.TeamId,
                            TeamYearId = cit.TeamYearId,
                            TeamName = cit.TeamYear.Name,
                            SeasonYearId = cit.TeamYear.SeasonYearId,
                            Year = cit.TeamYear.SeasonYear.Year,
                            IsNationalChampion = cit.IsNationalChampion
                        })
                        .ToList()
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
                        .ThenInclude(t => t.TeamYear)
                            .ThenInclude(ty => ty.Team)
                .Include(c => c.CompetitorInTeams)
                    .ThenInclude(cit => cit.TeamYear)
                        .ThenInclude(ty => ty.SeasonYear)
                .FirstOrDefaultAsync(c => c.CompetitorId == competitorId);

            return competitor;
        }

        public async Task<IEnumerable<CompetitorInTeamDto>> GetByTeamId(int teamId)
        {
            var competitors = await context.CompetitorInTeams
                .Include(cit => cit.Competitor)
                    .ThenInclude(c => c.Country)
                .Include(cit => cit.TeamYear)
                    .ThenInclude(ty => ty.SeasonYear)
                .Where(cit => cit.TeamYear.TeamId == teamId)
                .Select(cit => new CompetitorInTeamDto
                {

                    CompetitorInTeamId = cit.Id,
                    FirstName = cit.Competitor.FirstName,
                    LastName = cit.Competitor.LastName,
                    CompetitorName = cit.Competitor.CompetitorName,
                    TeamId = cit.TeamYear.TeamId,
                    TeamYearId = cit.TeamYearId,
                    TeamName = cit.TeamYear.Name,
                    SeasonYearId = cit.TeamYear.SeasonYearId,
                    Year = cit.TeamYear.SeasonYear.Year
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

        public async Task<List<SeasonYearDto>> GetAvailableSeasonYears()
        {
            return await context.SeasonYears
                .OrderByDescending(sy => sy.Year)
                .Select(sy => new SeasonYearDto
                {
                    SeasonYearId = sy.SeasonYearId,
                    Year = sy.Year,
                    Active = sy.Active
                })
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
                if (!dto.SelectedTeamYearId.HasValue)
                    throw new InvalidOperationException("Er is geen team geselecteerd.");

                var competitor = await context.Competitors
                    .Include(c => c.CompetitorInTeams)
                        .ThenInclude(cit => cit.TeamYear)
                    .FirstOrDefaultAsync(c => c.CompetitorId == dto.CompetitorId);

                if (competitor == null)
                    throw new KeyNotFoundException($"Competitor met ID {dto.CompetitorId} niet gevonden.");

                // Algemene gegevens bijwerken
                competitor.FirstName = dto.FirstName;
                competitor.LastName = dto.LastName;
                competitor.PcsName = dto.PcsName;
                competitor.PcsScraperName = dto.PcsScraperName;
                competitor.CyclingFlashScraperName = dto.CyclingFlashScraperName;
                competitor.CyclingFlashLastScraped = dto.CyclingFlahsLastScraped;
                competitor.CountryId = dto.CountryId;

                // Zoek de ploegkoppeling voor het geselecteerde seizoen
                var existingCit = competitor.CompetitorInTeams
                    .FirstOrDefault(cit =>
                        cit.TeamYear.SeasonYearId == dto.SelectedSeasonYearId);

                if (existingCit == null)
                {
                    // Nog geen ploeg in dit seizoen
                    context.CompetitorInTeams.Add(new CompetitorInTeam
                    {
                        CompetitorId = competitor.CompetitorId,
                        TeamYearId = dto.SelectedTeamYearId.Value
                    });
                }
                else
                {
                    // Ploeg wijzigen
                    existingCit.TeamYearId = dto.SelectedTeamYearId.Value;
                }

                await context.SaveChangesAsync();
            }
            catch (KeyNotFoundException)
            {
                throw;
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
            await context.SaveChangesAsync();
        }

        public async Task<List<CompetitorInTeam>> GetCompetitorInTeamsByIdsAsync(List<int> ids)
        {
            return await context.CompetitorInTeams
                .Where(cit => ids.Contains(cit.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<CompetitorInTeamDto>> GetByTeamAndSeason(int teamId, int seasonYearId)
        {
            return await context.CompetitorInTeams
                .Include(cit => cit.Competitor)
                    .ThenInclude(c => c.Country)
                .Include(cit => cit.TeamYear)
                    .ThenInclude(ty => ty.SeasonYear)
                .Where(cit =>
                    cit.TeamYear.TeamId == teamId &&
                    cit.TeamYear.SeasonYearId == seasonYearId)
                .Select(cit => new CompetitorInTeamDto
                {
                    CompetitorInTeamId = cit.Id,
                    FirstName = cit.Competitor.FirstName,
                    LastName = cit.Competitor.LastName,
                    CompetitorName = cit.Competitor.CompetitorName,
                    TeamId = cit.TeamYear.TeamId,
                    TeamYearId = cit.TeamYearId,
                    TeamName = cit.TeamYear.Name,
                    SeasonYearId = cit.TeamYear.SeasonYearId,
                    Year = cit.TeamYear.SeasonYear.Year,
                    IsNationalChampion = cit.IsNationalChampion
                })
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
