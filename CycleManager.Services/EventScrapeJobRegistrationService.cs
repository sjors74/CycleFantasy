using CycleManager.Domain.Interfaces;
using CycleManager.Services.Interfaces;
using Domain.Context;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CycleManager.Services
{
    public class EventScrapeJobRegistrationService : IScrapeScheduleService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<EventScrapeJobRegistrationService> _logger;

        public EventScrapeJobRegistrationService(ApplicationDbContext db, ILogger<EventScrapeJobRegistrationService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task RegisterSchedulesAsync()
        {
            TimeZoneInfo timezone;

            if (OperatingSystem.IsWindows())
            {
                timezone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            }
            else
            {
                timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam");
            }

            var today = DateTime.UtcNow.Date;

            var events = await _db.Events.ToListAsync();

            foreach (var e in events)
            {
                if (e.IsActive && e.StartDate <= today && e.EndDate >= today.AddDays(-1))
                {
                    RecurringJob.RemoveIfExists($"event-scraper-{e.EventId}");

                    RecurringJob.AddOrUpdate<IEventScrapeSchedulerService>(
                        $"event-scraper-{e.EventId}",
                        x => x.RunEventScrapeAsync(e.EventId),
                        "*/5 16-17 * * *",
                        new RecurringJobOptions
                        {
                            TimeZone = timezone
                        });
                }
                else
                {
                    RecurringJob.RemoveIfExists($"event-scraper-{e.EventId}");
                }

                if (e.IsActive && e.StartDate <= today && e.EndDate >= today)
                {
                    if (string.IsNullOrWhiteSpace(e.EventCode))
                    {
                        _logger.LogWarning(
                            "Event {EventId} is active but has no EventCode. Dropout scrape not scheduled.",
                            e.EventId);

                        continue;
                    }

                    RecurringJob.RemoveIfExists($"event-dropout-{e.EventId}");

                    RecurringJob.AddOrUpdate<IDropoutOrchestratorService>(
                        $"event-dropout-{e.EventId}",
                        x => x.RunDailyDropoutScrapeAsync(
                            e.EventId,
                            e.EventCode,
                            e.EventYear),
                        "0 14,16,18 * * *",
                        new RecurringJobOptions
                        {
                            TimeZone = timezone
                        });
                }
                else
                {
                    RecurringJob.RemoveIfExists($"event-dropout-{e.EventId}");
                }

                if (e.StartDate is DateTime startDate &&
                    today >= startDate.AddDays(-14) &&
                    today <= startDate.AddDays(2))
                {
                    RecurringJob.RemoveIfExists(
                        $"event-startlist-{e.EventId}");

                    RecurringJob.AddOrUpdate<IEventScrapeSchedulerService>(
                        $"event-startlist-{e.EventId}",
                        x => x.RunStartlistSyncAsync(e.EventId),
                        "0 6 * * *",
                        new RecurringJobOptions
                        {
                            TimeZone = timezone
                        });
                }
                else
                {
                    RecurringJob.RemoveIfExists($"event-startlist-{e.EventId}");
                }
            }
        }
    }
}
