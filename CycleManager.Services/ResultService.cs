using CycleManager.Domain.Dto;
using CycleManager.Services.Interfaces;
using Domain.Interfaces;
using Domain.Models;

namespace CycleManager.Services
{
    public class ResultService : IResultService
    {
        private readonly IResultsRepository _resultsRepository;
        private readonly ISpecialResultsRepository _specialResultsRepository;
        private readonly IScoreRepository _scoreRepository;
        public ResultService(IResultsRepository resultsRepository, ISpecialResultsRepository specialResultsRepository, IScoreRepository scoreRepository)
        {
            _resultsRepository = resultsRepository;
            _specialResultsRepository = specialResultsRepository;
            _scoreRepository = scoreRepository;
        }

        /// <summary>
        /// Get a list of all results for an event
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<CompetitorRankingDto>> GetResultsByEventId(int eventId, bool onlyTop15 = false)
        {
            var results = await _resultsRepository.GetResultsByEventId(eventId);
            var specialResults = await _specialResultsRepository.GetByEventId(eventId);

            var specialLookup = specialResults.GroupBy(s => s.CompetitorInEventId).ToDictionary(g => g.Key, g => g.Sum(x => x.Special.Score));

            var groupedList = results
                .GroupBy(g => g.CompetitorInEventId)
                .Select(c =>
                {
                    var first = c.First();
                    var competitor = first?.CompetitorInEvent?.CompetitorInTeam.Competitor;
                    var team = competitor?.CompetitorInTeams.FirstOrDefault()?.Team;

                    var normalPoints = c.Sum(a => a.ConfigurationItem?.Score ?? 0);
                    var specialPoints = specialLookup.GetValueOrDefault(first?.CompetitorInEventId ?? 0, 0);

                    return new CompetitorRankingDto
                    {
                        CompetitorName = competitor?.CompetitorName ?? "onbekend",
                        EventId = first?.Stage?.EventId ?? 0,
                        CompetitorInEventId = first?.CompetitorInEventId ?? 0,
                        Points = c.Sum(a => a.ConfigurationItem?.Score ?? 0),
                        CompetitorTeam = team?.CurrentTeamName ?? "onbekend",
                        NormalPoints = normalPoints,
                        SpecialPoints = specialPoints,
                    };
            })
                .OrderByDescending(c => c.TotalPoints)
                .ThenBy(c => c.CompetitorName)
                .ToList();

            if (onlyTop15 && groupedList.Any())
            {
                var top15 = groupedList.Take(15).ToList();
                int minScoreInTop15 = top15.LastOrDefault()?.TotalPoints ?? 0;

                var extendedTop15 = groupedList
                    .Skip(15)
                    .TakeWhile(x => x.TotalPoints == minScoreInTop15)
                    .ToList();

                groupedList = top15.Concat(extendedTop15).ToList();
            }

            int rank = 1;
            int actualRank = -1;
            int? previousScore = null;

            foreach(var item in groupedList)
            {
                if(previousScore != item.TotalPoints)
                {
                    actualRank = rank;
                }

                item.Position = actualRank;
                previousScore = item.TotalPoints;
                rank++;
            }

            return groupedList;
        }


        /// <summary>
        /// Get a list of all results for an event and competitorId
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="competitorInEventId">id van renner in een pool</param>
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

        public Task<List<DeelnemerDto>> GetPoolRankingForStage(int eventId, int stageId)
        {
            return _scoreRepository.GetPoolRankingForStage(eventId, stageId);
        }

        public async Task<List<DeelnemerStageScore>> GetScoresByEventIdAsync(int eventId)
        {
            return await _scoreRepository.GetScoresByEventIdAsync(eventId);
        }


        //Methodes voor manager

        public async Task<Stage?> GetStageByIdAsync(int stageId)
        {
            return await _resultsRepository.GetStageByIdAsync(stageId);
        }

        public async Task<List<Result>> GetResultsByStageAsync(int stageId)
        {
            return await _resultsRepository.GetResultsByStageAsync(stageId);
        }

        //TODO: maak een StageResultDto-object met Results en SpecialResults, zodat je in 1 call alle resultaten van een etappe kan ophalen.
        public async Task<List<SpecialResult>> GetSpecialResultsByStageAsync(int stageId)
        {
            return await _specialResultsRepository.GetByStageAsync(stageId);
        }

        public async Task<List<CompetitorsInEvent>> GetCompetitorsInEventAsync(int eventId)
        {
            return await _resultsRepository.GetCompetitorsInEventAsync(eventId);
        }

        public async Task<List<ConfigurationItem>> GetConfigurationItemsByConfigAsync(int configId)
        {
            return await _resultsRepository.GetConfigurationItemsByConfigAsync(configId);
        }

        public async Task<List<ConfigurationItemSpecial>> GetConfigurationItemSpecialsAsync(int configId)
        {
            return await _specialResultsRepository.GetConfigurationItemsByConfigAsync(configId);
        }

        public async Task AddResultsAsync(IEnumerable<Result> results)
        {
            await _resultsRepository.AddResultsAsync(results);
        }

        public async Task AddSpecialResultsAsync(IEnumerable<SpecialResult> specialResults)
        {
            await _specialResultsRepository.AddResultsAsync(specialResults);
        }

        public async Task<Result?> GetResultByIdAsync(int id)
        {
            return await _resultsRepository.GetResultByIdAsync(id);
        }

        public async Task DeleteResultAsync(Result result)
        {
            await _resultsRepository.DeleteResultAsync(result);
        }

        public async Task<bool> ResultExistsAsync(int id)
        {
            return await _resultsRepository.ResultExistsAsync(id);
        }

        public string GetCompetitorFullName(int competitorId)
        {
            return _resultsRepository.GetCompetitorFullName(competitorId);
        }

        /// <summary>
        /// Recalculate all scores for an event based on current ConfigurationItems.
        /// Updates Results, DeelnemerScores, and DeelnemerPickScores.
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public Task RecalculateEventScoresAsync(int eventId)
        {
            return _resultsRepository.RecalculateEventScoresAsync(eventId);
        }

        public Task<List<DeelnemerScore>> GetTotalScoresByEventIdAsync(int eventId)
        {
            return _resultsRepository.GetTotalScoresByEventIdAsync(eventId);
        }

        public Task<List<PickDetailDto>> GetPickDetailsAsync(int eventId, int competitorInEventId)
        {
            return _resultsRepository.GetPickDetailsAsync(eventId, competitorInEventId);
        }

        public Task<List<CompetitorScoreDto>> GetCompetitorResultsForEvent(int eventId)
        {
            return _resultsRepository.GetCompetitorResultsForEvent(eventId);
        }

        public Task<SpecialResult?> GetSpecialResultByIdAsync(int id)
        {
            return _specialResultsRepository.GetByIdAsync(id);
        }

        public Task DeleteSpecialResultAsync(int id)
        {
            return _specialResultsRepository.DeleteAsync(id);
        }
    }
}
