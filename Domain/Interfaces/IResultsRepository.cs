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
        Task<CompetitorScoreDto?> GetCompetitorResultsByEventId(int eventId, int competitorInEventId);

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

        //methodes voor manager
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

        Task<List<DeelnemerScore>> GetTotalScoresByEventIdAsync(int eventId);

        Task<List<PickDetailDto>> GetPickDetailsAsync(int eventId, int gameCompetitorEventId);
    }
}
