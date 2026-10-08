using CycleManager.Services.Interfaces;

namespace CycleManager.Services
{
    public class DelayService : IDelayService
    {
        public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
        {
            return Task.Delay(delay, cancellationToken);
        }
    }
}
