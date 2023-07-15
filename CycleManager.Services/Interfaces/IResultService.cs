using Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface IResultService
    {
        /// <summary>
        /// Get number of results for a given stage
        /// </summary>
        /// <param name="stageId"></param>
        /// <returns></returns>
        Task<int> GetResultsByStageId(int stageId);

        /// <summary>
        /// Get a list of all results for an event
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        Task<IEnumerable<Result>> GetResultsByEventId(int eventId);
    }
}
