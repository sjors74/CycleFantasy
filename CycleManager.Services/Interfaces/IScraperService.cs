using CycleManager.Domain.Dto;

namespace CycleManager.Services.Interfaces
{
    public interface IScraperService
    {
        Task RunAsync(int eventId, string eventName, int year, int stageNumber);
        Task RunDropoutsAsync(int eventId, string eventName, int year);
        Task RunCompetitorsAsync(int teamId, int year);
        Task ImportScrapedCompetitorsAsync();
        Task SyncStartlistAsync(int eventId, List<ScrapedStartlistEntry> scrapedEntries);

        Task RefreshStartlistAsync(int eventId);
    }
}
