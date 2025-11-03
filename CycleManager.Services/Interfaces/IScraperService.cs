namespace CycleManager.Services.Interfaces
{
    public interface IScraperService
    {
        Task RunAsync(int eventId, string eventName, int year, int stageNumber);
        Task RunDropoutsAsync(int eventId, string eventName, int year);
        Task RunCompetitorsAsync(int teamId, int year);
        Task ImportScrapedCompetitorsAsync();
    }
}
