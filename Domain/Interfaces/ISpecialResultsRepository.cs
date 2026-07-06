using CycleManager.Domain.Dto;
using Domain.Dto;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface ISpecialResultsRepository : IGenericRepository<SpecialResult>
    {
        /// <summary>
        /// Get a list of results for an event
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        Task<IEnumerable<SpecialResult>> GetByEventId(int eventId);

        /// <summary>
        /// Get a list of results for a stage
        /// </summary>
        /// <param name="stageId"></param>
        /// <returns></returns>
        Task<List<SpecialResult>> GetByStageAsync(int stageId);

        /// <summary>
        /// Get a special result by its ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<SpecialResult?> GetByIdAsync(int id);

        /// <summary>   
        /// Delete a special result by its ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteAsync(int id);

        /// <summary>
        /// Get a list of al special configuration items for a configuration
        /// </summary>
        /// <param name="configId"></param>
        /// <returns></returns>
        Task<List<ConfigurationItemSpecial>> GetConfigurationItemsByConfigAsync(int configId);

        /// <summary>
        /// Add a list of special results to the collection
        /// </summary>
        /// <param name="specialResults"></param>
        /// <returns></returns>
        Task AddResultsAsync(IEnumerable<SpecialResult> specialResults);
    }
}
