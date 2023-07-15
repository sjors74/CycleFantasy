using Domain.Models;

namespace Domain.Interfaces
{
    public interface IResultsRepository : IGenericRepository<Result>
    {
        /// <summary>
        /// Get a list of results for an event
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        Task<IEnumerable<Result>> GetResultsByEventId(int eventId);

        /// <summary>
        /// Get the number of results for a stage
        /// </summary>
        /// <param name="stageId"></param>
        /// <returns></returns>
        Task<int> GetResultsByStageId(int stageId);
    }
}
