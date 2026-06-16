using MimeKit.Tnef;

namespace CycleManager.Services.Interfaces
{
    public interface IDropoutOrchestratorService
    {
        Task RunDailyDropoutScrapeAsync(int eventId, string eventName, int year);
    }
}
