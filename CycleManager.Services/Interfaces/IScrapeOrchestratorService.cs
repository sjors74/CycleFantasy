namespace CycleManager.Services.Interfaces
{
    public interface IScrapeOrchestratorService
    {
        Task RunStageScrapeAsync(
            int eventId,
            string eventName,
            int stageId,
            int stageNumber,
            int year);

        Task RefreshStartlistAsync(int eventId);

    }
}
