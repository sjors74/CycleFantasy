namespace CycleManager.Services.Interfaces
{
    public interface IScoreService
    {
        Task UpdateScoresForStageAsync(int eventId, int stageId);

    }
}
