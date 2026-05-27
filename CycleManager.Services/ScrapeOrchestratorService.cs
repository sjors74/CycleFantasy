using CycleManager.Services.Interfaces;
using Hangfire;

namespace CycleManager.Services
{
    public class ScrapeOrchestratorService : IScrapeOrchestratorService
    {
        private readonly IScraperService _scraperService;
        private readonly IScoreService _scoreService;

        public ScrapeOrchestratorService(
            IScraperService scraperService,
            IScoreService scoreService)
        {
            _scraperService = scraperService;
            _scoreService = scoreService;
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task RunStageScrapeAsync(
            int eventId,
            string eventName,
            int stageId,
            int stageNumber,
            int year)
        {
            await _scraperService.RunAsync(
                eventId,
                eventName,
                stageNumber,
                year);

            await _scoreService.UpdateScoresForStageAsync(
                eventId,
                stageId);
        }
    }
}
