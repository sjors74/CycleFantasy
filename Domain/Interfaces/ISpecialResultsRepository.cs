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
    }
}
