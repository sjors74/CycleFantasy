using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using Domain.Dto;
using Domain.Interfaces;
using Domain.Models;

namespace CycleManager.Services
{
    public class CompetitorService : ICompetitorService
    {
        private readonly ICompetitorRepository _competitorRepository;
        private readonly ICompetitorInTeamRepository _competitorInTeamRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly ICountryRepository _countryRepository;
        public CompetitorService(ICompetitorRepository competitorRepository, 
                ICompetitorInTeamRepository competitorInTeamRepository, 
                ITeamRepository teamRepository, ICountryRepository countryRepository)
        {
            _competitorRepository = competitorRepository;
            _competitorInTeamRepository = competitorInTeamRepository;
            _teamRepository = teamRepository;
            _countryRepository = countryRepository;
        }

        /// <summary>
        /// Create a new competitor and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Create(Competitor entity)
        {
            _competitorRepository.Add(entity);
            await _competitorRepository.SaveChangesAsync();
        }

        public async Task CreateCompetitorInTeam(CompetitorInTeam entity)
        {
            _competitorInTeamRepository.Add(entity);
            await _competitorInTeamRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Delete a competitor and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Delete(Competitor entity)
        {
            _competitorRepository.Remove(entity);
            await _competitorRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Get all competitors
        /// </summary>
        /// <returns></returns>
        public async Task<List<CompetitorDto>> GetAllCompetitors(int year)
        {
            return await _competitorRepository.GetAllCompetitors(year);
        }

        public Task<List<int>> GetAvailableYears()
        {
            return _competitorRepository.GetAvailableYears();
        }

        /// <summary>
        /// Get a list of competttors for a team
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<CompetitorInTeamDto>> GetByTeamId(int teamId)
        {
            return await _competitorRepository.GetByTeamId(teamId);
        }

        /// <summary>
        /// Get a competitor by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<Competitor> GetCompetitorById(int id)
        {
            return _competitorRepository.GetById(id);
        }

        /// <summary>
        /// Get number of competitors by country Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<int> GetCompetitorsByCountry(int id)
        {
            return await _competitorRepository.GetCompetitorsByCountry(id);
        }

        /// <summary>
        /// Update and save a competitor
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Update(Competitor entity)
        {
            _competitorRepository.Update(entity);
            await _competitorRepository.SaveChangesAsync();
        }

        public async Task<Competitor?> GetCompetitorByName(string firstName, string lastName, int countryId )
        {
            return await _competitorRepository.GetCompetitorByName(firstName, lastName, countryId);
        }

        public async Task<bool> CheckCompetitorInTeam(int competitorId, int teamId, int year)
        {
            return await _competitorInTeamRepository.CheckCompetitorInTeam(competitorId, teamId, year);
        }

        public IQueryable<Competitor> GetCompetitorsByTerm(string term)
        {
            return _competitorRepository.GetCompetitorsByTerm(term);
        }

        public async Task UpdateCompetitorWithTeam(CompetitorEditDto dto)
        {
            //TODO: nieuwe methode met Teams?
            var competitor = await _competitorRepository.GetById(dto.CompetitorId);
            if (competitor == null) throw new Exception("Competitor not found");

            competitor.FirstName = dto.FirstName;
            competitor.LastName = dto.LastName;
            competitor.PcsName = dto.PcsName;
            competitor.CountryId = dto.CountryId;

            var competitorInTeam = competitor.CompetitorInTeams
                .FirstOrDefault(cit => cit.TeamId == dto.SelectedTeamId && cit.Year == dto.SelectedYear);

            if(competitorInTeam == null)
            {
                competitorInTeam = new CompetitorInTeam
                {
                    CompetitorId = competitor.CompetitorId,
                    TeamId = dto.SelectedTeamId,
                    Year = dto.SelectedYear
                };
                competitor.CompetitorInTeams.Add(competitorInTeam);
            }
            competitorInTeam.IsNationalChampion = dto.IsNationalChampion;

            _competitorRepository.Update(competitor);
            await _competitorRepository.SaveChangesAsync();
        }

        public async Task<CompetitorEditDto> GetCompetitorForEdit(int competitorId)
        {
            var competitor = await _competitorRepository.GetById(competitorId);
            if (competitor == null) return null;

            var teams = await _teamRepository.GetAll();
            var countries =  await _countryRepository.GetAll();

            var years = Enumerable.Range(DateTime.Now.Year - 3, 7);

            return new CompetitorEditDto
            {
                CompetitorId = competitor.CompetitorId,
                FirstName = competitor.FirstName,
                LastName = competitor.LastName,
                PcsName = competitor.PcsName,
                CountryId = competitor.CountryId,
                SelectedTeamId = competitor.CompetitorInTeams.FirstOrDefault()?.TeamId ?? 0,
                SelectedYear = competitor.CompetitorInTeams?.FirstOrDefault()?.Year ?? DateTime.Now.Year,
                IsNationalChampion = competitor.CompetitorInTeams.FirstOrDefault().IsNationalChampion,
                AvailableYears = years,
                Teams = teams.Select(t => new TeamDto { Id = t.TeamId, Naam = t.TeamName, Renners = new List<CompetitorDto>() }),
                Countries = countries.Select(c => new CountryDto { Id = c.CountryId, CountryNameLong = c.CountryNameLong, CountryNameShort = c.CountryNameShort })
            };
        }
    }
}
