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
        IQueryable<Competitor> GetAllCompetitors();

        /// <summary>
        /// Create a new competitor and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Create(Competitor entity);

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
    }
}
