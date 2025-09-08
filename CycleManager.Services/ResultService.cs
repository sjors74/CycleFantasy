using CycleManager.Domain.Dto;
using CycleManager.Services.Interfaces;
using Domain.Interfaces;

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
        public async Task<IEnumerable<ResultDto>> GetResultsByEventId(int eventId)
        {
            var dto = new List<ResultDto>();
            var results = await _resultsRepository.GetResultsByEventId(eventId);
            var groupedList = results
                .GroupBy(g => g.CompetitorInEventId)
                .Select(c =>
                {
                    var first = c.FirstOrDefault();
                    return new ResultDto
                    {
                        CompetitorName = first?.CompetitorInEvent?.Competitor?.CompetitorName ?? "onbekend",
                        EventId = first?.Stage?.EventId ?? 0,
                        CompetitorInEventId = first?.CompetitorInEventId ?? 0,
                        Points = c.Sum(a => a.ConfigurationItem?.Score ?? 0),
                        CompetitorTeam = first?.CompetitorInEvent?.Competitor?.Team?.TeamName ?? "onbekend"
                    };
            })
                .OrderByDescending(c => c.Points)
                .ThenBy(c => c.CompetitorName)
                .ToList();

            var top15 = groupedList.Take(15).ToList();
            int minScoreInTop15 = top15.LastOrDefault()?.Points ?? 0;

            var extendedTop15 = groupedList
                .Skip(15)
                .TakeWhile(x => x.Points == minScoreInTop15)
                .ToList();

            var finalTop = top15.Concat(extendedTop15).ToList();

            int rank = 1;
            int actualRank = -1;
            int? previousScore = null;

            foreach(var item in finalTop)
            {
                if(previousScore != item.Points)
                {
                    actualRank = rank;
                }

                item.Position = actualRank;
                previousScore = item.Points;
                rank++;
            }

            dto = finalTop;
            return dto;
        }

        /// <summary>
        /// Get a list of all results for an event and competitorId
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public async Task<CompetitorScoreDto?> GetCompetitorResultsByEventId(int eventId, int competitorInEventId)
        {
            return await _resultsRepository.GetCompetitorResultsByEventId(eventId, competitorInEventId);
        }

        /// <summary>
        /// Get result in latest stage for competitor
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="competitorInEventId"></param>
        /// <returns></returns>
        public async Task<int> GetCompetitorScoreByEventAndStageIdAsync(int eventId, int competitorInEventId)
        {
            return await _resultsRepository.GetCompetitorLatestScore(eventId, competitorInEventId);
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
