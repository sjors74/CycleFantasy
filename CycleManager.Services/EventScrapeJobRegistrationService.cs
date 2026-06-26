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
                        "*/5 * * * *",
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
