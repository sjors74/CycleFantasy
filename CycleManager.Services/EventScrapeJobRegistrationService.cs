using CycleManager.Domain.Interfaces;
using CycleManager.Services.Interfaces;
using Domain.Context;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Services
{
    public class EventScrapeJobRegistrationService : IScrapeScheduleService
    {
        private readonly ApplicationDbContext _db;

        public EventScrapeJobRegistrationService(ApplicationDbContext db)
        {
            _db = db;
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

            var events = await _db.Events
                        .Where(e => e.IsActive)
                        .ToListAsync();

            foreach (var e in events)
            {
                RecurringJob.RemoveIfExists($"event-scraper-{e.EventId}");

                RecurringJob.AddOrUpdate<IEventScrapeSchedulerService>(
                    $"event-scraper-{e.EventId}",
                    x => x.RunEventScrapeAsync(e.EventId),
                    "*/5 * * * *",
                    new RecurringJobOptions
                    {
                        TimeZone = timezone
                    });

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
        }
    }
}
