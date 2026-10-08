namespace CycleManager.Services.Interfaces
{
    public interface IDelayService
    {
        Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default);
    }
}
