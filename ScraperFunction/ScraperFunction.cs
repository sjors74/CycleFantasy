using System;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ScraperFunction
{
    public class ScraperFunction
    {
        private readonly ILogger _logger;
        private readonly ScraperService _scraper;
        private readonly ScoreService _scoreService;
        private readonly IStageService _stageService;

        public ScraperFunction(ILoggerFactory loggerFactory, ScraperService scraper, IStageService stageService, ScoreService scoreService)
        {
            _logger = loggerFactory.CreateLogger<ScraperFunction>();
            _scraper = scraper;
            _stageService = stageService;
            _scoreService = scoreService;
        }

        [Function("ScraperFunction")]
        public async Task Run([TimerTrigger("0 */15 16-18 * * *")] TimerInfo myTimer, ExecutionContext context)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            DateTime today = DateTime.UtcNow.Date;

            if (!int.TryParse(config["Scraper:EventId"], out int eventId))
            {
                _logger.LogError("Scraper:EventId ontbreekt of is ongeldig.");
                return;
            }

            string? eventName = config["Scraper:EventName"];
            if (string.IsNullOrWhiteSpace(eventName))
            {
                _logger.LogError("Scraper:EventName ontbreekt of is leeg.");
                return;
            }

            if (!int.TryParse(config["Scraper:Year"], out int eventYear))
            {
                _logger.LogError("Scraper:Year ontbreekt of is ongeldig.");
                return;
            }
            var stageNumber = await _stageService.GetStageNumberForDateAsync(today, eventId);
            var stage = await _stageService.GetStage(stageNumber, eventId);

            if (stage != null && stage.NoScore == false)
            {
                var count = await _stageService.GetStageResults(stageNumber, eventId);

                if (count >= 15)
                {
                    _logger.LogInformation($"Al {count} resultaten voor {today:yyyy-MM-dd}. Scrape wordt overgeslagen.");
                    return;
                }

                await _scraper.RunAsync(eventId, eventName, eventYear, stageNumber);

                if (myTimer.ScheduleStatus is not null)
                {
                    _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
                }

                if (stage.Id > 0)
                    await _scoreService.UpdateScoresForStageAsync(eventId, stage.Id);
            }
            else
            {
                _logger.LogInformation($"Geen etappe gevonden óf er worden geen resultaten verwacht voor deze etappe.");
            }
        }
    }
}
