using CycleManager.Domain.Dto;
using CycleManager.Services.Interfaces;
using Domain.Interfaces;
using Domain.Models;

namespace CycleManager.Services
{
    public class ResultService : IResultService
    {
        private readonly IResultsRepository _resultsRepository;
        public ResultService(IResultsRepository resultsRepository)
        {
            _resultsRepository = resultsRepository;
        }

        /// <summary>
        /// Get a list of all results for an event
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public Task<IEnumerable<Result>> GetResultsByEventId(int eventId)
        {
            return _resultsRepository.GetResultsByEventId(eventId);
        }

        /// <summary>
        /// Get a list of all results for an event and competitorId
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public async Task<CompetitorScoreDto?> GetCompetitorResultsByEventId(int eventId, int competitorId)
        {
            return await _resultsRepository.GetCompetitorResultsByEventId(eventId, competitorId);
        }


        /// <summary>
        ///  Get number of results for a given stage
        /// </summary>
        /// <param name="stageId"></param>
        /// <returns></returns>
        public Task<int> GetResultsByStageId(int stageId)
        {
            return _resultsRepository.GetResultsByStageId(stageId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stageId"></param>
        /// <returns></returns>
        public Task<List<EtappeUitslagDto>> GetEtappeUitslag(int stageId)
        {
            return _resultsRepository.GetEtappeUitslag(stageId);
        }
    }
}
