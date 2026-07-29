using CycleManager.Domain.Dto;
using CycleManager.Domain.Interfaces;
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
        private readonly ISeasonYearRepository _seasonYearRepository;
        private readonly IRatingRepository _ratingRepository;

        public CompetitorService(
                ICompetitorRepository competitorRepository, 
                ICompetitorInTeamRepository competitorInTeamRepository, 
                ITeamRepository teamRepository, 
                ICountryRepository countryRepository,
                ISeasonYearRepository seasonYearRepository,
                IRatingRepository ratingRepository)
        {
            _competitorRepository = competitorRepository;
            _competitorInTeamRepository = competitorInTeamRepository;
            _teamRepository = teamRepository;
            _countryRepository = countryRepository;
            _ratingRepository = ratingRepository;
            _seasonYearRepository = seasonYearRepository;
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
        public async Task<List<CompetitorDto>> GetAllCompetitors(int seasonYearId)
        {
            return await _competitorRepository.GetAllCompetitors(seasonYearId);
        }

        public Task<List<SeasonYearDto>> GetAvailableYears()
        {
            return _competitorRepository.GetAvailableSeasonYears();
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

        public async Task<bool> CheckCompetitorInTeam(int competitorId, int teamYearId)
        {
            return await _competitorInTeamRepository.CheckCompetitorInTeam(competitorId, teamYearId);
        }

        public IQueryable<Competitor> GetCompetitorsByTerm(string term)
        {
            return _competitorRepository.GetCompetitorsByTerm(term);
        }

        public async Task UpdateCompetitorWithTeam(CompetitorEditDto dto)
        {
            var competitor = await _competitorRepository.GetById(dto.CompetitorId);
            if (competitor == null) throw new Exception("Competitor not found");

            competitor.FirstName = dto.FirstName;
            competitor.LastName = dto.LastName;
            competitor.PcsName = dto.PcsName ?? string.Empty;
            competitor.ScraperName = dto.ScraperName ?? string.Empty;
            competitor.CountryId = dto.CountryId;

            foreach (var teamDto in dto.CompetitorInTeams)
            {
                var team = competitor.CompetitorInTeams
                    .FirstOrDefault(t => t.Id == teamDto.CompetitorInTeamId);

                if (team != null)
                {
                    team.IsNationalChampion = teamDto.IsNationalChampion;
                }
            }

            await _competitorRepository.UpdateCompetitorAsync(competitor);

        }
        public async Task<CompetitorEditDto> GetCompetitorForEdit(int competitorId)
        {
            var competitor = await _competitorRepository.GetById(competitorId);

            if (competitor == null)
                return null;

            var availableYears = await _seasonYearRepository.GetAllAsync();
            var countries = await _countryRepository.GetAll();
            var categories = await _ratingRepository.GetRatingCategories();
            var ratings = await _ratingRepository.GetRatingsByCompetitorId(competitorId);

            return new CompetitorEditDto
            {
                CompetitorId = competitor.CompetitorId,
                FirstName = competitor.FirstName,
                LastName = competitor.LastName,
                PcsName = competitor.PcsName,
                ScraperName = competitor.ScraperName,
                CountryId = competitor.CountryId,

                Countries = countries
                    .Select(c => new CountryDto
                    {
                        Id = c.CountryId,
                        CountryNameLong = c.CountryNameLong,
                        CountryNameShort = c.CountryNameShort
                    })
                    .ToList(),
                AvailableYears = availableYears
                    .Select(y => new SeasonYearDto
                    {
                        SeasonYearId = y.SeasonYearId,
                        Year = y.Year,
                        Active = y.Active
                    })
                    .OrderByDescending(y => y.Year)
                    .ToList(),
                
                RatingCategories = categories
                    .Select(r => new RatingCategoryDto
                    {
                        RatingCategoryId = r.RatingCategoryId,
                        Name = r.Name,
                        IsActive = r.IsActive,
                        Color = r.Color,
                        DisplayOrder = r.DisplayOrder
                    })
                    .OrderBy(r => r.Name),

                Ratings = ratings
                    .Select(r => new CompetitorRatingDto
                    {
                        RatingCategoryId = r.RatingCategoryId,
                        Rating = r.Rating
                    })
                    .ToList(),
                CompetitorInTeams = competitor.CompetitorInTeams
                .Select(cit => new CompetitorInTeamDto
                {
                    CompetitorInTeamId = cit.Id,
                    TeamYearId = cit.TeamYearId,
                    SeasonYearId = cit.TeamYear.SeasonYearId,
                    Year = cit.TeamYear.Year,
                    TeamName = cit.TeamYear.Team.CurrentTeamName,
                   IsNationalChampion = cit.IsNationalChampion
                })
                .ToList()
            };
        }

        public async Task<List<CompetitorInTeam>> GetCompetitorInTeamsByIdsAsync(List<int> ids)
        {
            return await _competitorRepository.GetCompetitorInTeamsByIdsAsync(ids);
        }
        public async Task<IEnumerable<CompetitorInTeamDto>> GetByTeamAndSeason(int teamId, int seasonYearId)
        {
            return await _competitorRepository.GetByTeamAndSeason(teamId, seasonYearId);
        }

    }
}
