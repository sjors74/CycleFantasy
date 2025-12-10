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
        Task<CompetitorScoreDto?> GetCompetitorResultsByEventId(int eventId, int competitorInEventId);

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
        Task<IEnumerable<ResultDto>> GetResultsByEventId(int eventId, bool onlyTop15 = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stageId"></param>
        /// <returns></returns>
        Task<List<EtappeUitslagDto>> GetEtappeUitslag(int stageId);

        /// <summary>
        /// Get pool ranking for a given event and stage number
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="stageId"></param>
        /// <returns></returns>
        Task<List<DeelnemerDto>> GetPoolRankingForStage(int eventId, int stageId);

        Task<List<DeelnemerStageScore>> GetScoresByEventIdAsync(int eventId);

        Task<List<DeelnemerScore>> GetTotalScoresByEventIdAsync(int eventId);

        //methodes for manager
        Task<Stage?> GetStageByIdAsync(int stageId);
        Task<List<Result>> GetResultsByStageAsync(int stageId);
        Task<List<CompetitorsInEvent>> GetCompetitorsInEventAsync(int eventId);
        Task<List<ConfigurationItem>> GetConfigurationItemsByConfigAsync(int configId);
        Task AddResultsAsync(IEnumerable<Result> results);
        Task<Result?> GetResultByIdAsync(int id);
        Task DeleteResultAsync(Result result);
        Task<bool> ResultExistsAsync(int id);
        string GetCompetitorFullName(int competitorId);
        Task RecalculateEventScoresAsync(int eventId);
    }
}
