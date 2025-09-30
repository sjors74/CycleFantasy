using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using Domain.Dto;
using Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface ICompetitorService
    {
        /// <summary>
        /// Get the number of competitors by country id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<int> GetCompetitorsByCountry(int id);

        /// <summary>
        /// Get a competitor by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Competitor> GetCompetitorById(int id);
        
        /// <summary>
        /// Get all competitors
        /// </summary>
        /// <returns></returns>
        Task<List<CompetitorDto>> GetAllCompetitors(int year);

        /// <summary>
        /// Create a new competitor and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Create(Competitor entity);

        /// <summary>
        /// Create a new competitorInTeam and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task CreateCompetitorInTeam(CompetitorInTeam entity);
        /// <summary>
        /// Update and save a competitor
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Update(Competitor entity);

        /// <summary>
        /// Remove and save a competitor
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Delete(Competitor entity);

        /// <summary>
        /// Get all competitors for a team
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        Task<IEnumerable<CompetitorInTeamDto>> GetByTeamId(int teamId, int year);
        Task<List<int>> GetAvailableYears();

        Task<Competitor?> GetCompetitorByName(string firstName, string lastName, int countryId);
        Task<bool> CheckCompetitorInTeam(int competitorId, int teamId, int year);
        IQueryable<Competitor> GetCompetitorsByTerm(string term);
        Task UpdateCompetitorWithTeam(CompetitorEditDto dto);
        Task<CompetitorEditDto> GetCompetitorForEdit(int competitorId);
    }
}
