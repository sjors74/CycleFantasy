using CycleManager.Domain.Dto;
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
        /// Get a list of all results for a competitor in an event
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="competitorId"></param>
        /// <returns></returns>
        Task<CompetitorScoreDto?> GetCompetitorResultsByEventId(int eventId, int competitorId);

        /// <summary>
        /// Get latest (stage) score for a competitor in an event
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="competitorInEventId"></param>
        /// <returns></returns>
        Task<int> GetCompetitorScoreByEventAndStageIdAsync(int eventId, int competitorInEventId);
        
        /// <summary>
        /// Get a list of all results for an event
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        Task<IEnumerable<ResultDto>> GetResultsByEventId(int eventId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stageId"></param>
        /// <returns></returns>
        Task<List<EtappeUitslagDto>> GetEtappeUitslag(int stageId);
    }
}
