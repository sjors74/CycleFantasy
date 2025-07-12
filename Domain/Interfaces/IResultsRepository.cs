using CycleManager.Domain.Dto;
using Domain.Dto;
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
        /// Get a list of all results for a competitor for an event
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="competitorId"></param>
        /// <returns></returns>
        Task<CompetitorScoreDto?> GetCompetitorResultsByEventId(int eventId, int competitorId);

        /// <summary>
        /// Get the latest (stage) score for an competitor in an event
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="competitorInEventId"></param>
        /// <returns></returns>
        Task<int> GetCompetitorLatestScore(int eventId, int competitorInEventId);
        
        /// <summary>
        /// Get the number of results for a stage
        /// </summary>
        /// <param name="stageId"></param>
        /// <returns></returns>
        Task<int> GetResultsByStageId(int stageId);
        Task<List<EtappeUitslagDto>?> GetEtappeUitslag(int stageId);
    }
}
