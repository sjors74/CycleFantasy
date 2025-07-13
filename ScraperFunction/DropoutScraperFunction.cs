using System;
using CycleManager.Services.Interfaces;
using CycleManager.Services;
using DataAccessEF.Migrations;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ScraperFunction;

public class DropoutScraperFunction
{
    private readonly ILogger _logger;
    private readonly ScraperService _scraper;
    //private readonly IStageService _stageService;

    public DropoutScraperFunction(ILoggerFactory loggerFactory, ScraperService scraper)
    {
        _logger = loggerFactory.CreateLogger<DropoutScraperFunction>();
        _scraper = scraper;
    }

    [Function("DropoutScraperFunction")]
    public async Task RunDropoutScraper([TimerTrigger("0 0 14-23 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation($"Dropout scraper gestart op: {DateTime.Now}");

        // Config ophalen zoals in de andere functie
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

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

        await _scraper.RunDropoutsAsync(eventId, eventName, eventYear); // nieuwe methode
        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
        }

    }

}