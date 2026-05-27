using CycleManager.Services.Interfaces;
using Domain.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CycleManager.Services
{
    public class DropoutOrchestratorService : IDropoutOrchestratorService
    {
        private readonly ApplicationDbContext _db;
        private readonly IScraperService _scraperService;
        private readonly ILogger<DropoutOrchestratorService> _logger;

        public DropoutOrchestratorService(
            ApplicationDbContext db,
            IScraperService scraperService,
            ILogger<DropoutOrchestratorService> logger)
        {
            _db = db;
            _scraperService = scraperService;
            _logger = logger;
        }

        public async Task RunDailyDropoutScrapeAsync(int eventId, string eventName, int year)
        {
            var activeEvents = await _db.Events
                .Where(e => e.IsActive)
                .ToListAsync();

            if (!activeEvents.Any())
            {
                _logger.LogInformation("Geen actieve events voor dropout scrape.");
                return;
            }

            foreach (var e in activeEvents)
            {
                _logger.LogInformation(
                    "Dropout scrape gestart voor event {EventId}",
                    e.EventId);

                try
                {
                    await _scraperService.RunDropoutsAsync(
                        eventId, eventName, year);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Dropout scrape mislukt voor event {EventId}",
                        e.EventId);
                }
            }
        }
    }
}
