using CycleManager.Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface IScrapeScheduleService
    {
        Task RegisterSchedulesAsync();
    }
}
