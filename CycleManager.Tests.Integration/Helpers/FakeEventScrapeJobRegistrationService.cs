using CycleManager.Services.Interfaces;

namespace CycleManager.Tests.Integration.Helpers
{
    public class FakeEventScrapeJobRegistrationService : IScrapeScheduleService
    {
        public Task RegisterSchedulesAsync()
        {
            return Task.CompletedTask;
        }
    }
}
