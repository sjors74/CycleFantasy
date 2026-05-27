using CycleManager.Domain.Interfaces;
using CycleManager.Services.Interfaces;
using Domain.Context;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Services
{
    public class ScrapeScheduleService : IScrapeScheduleService
    {
        private readonly ApplicationDbContext _db;

        public ScrapeScheduleService(ApplicationDbContext db)
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
            }
        }
    }
}
