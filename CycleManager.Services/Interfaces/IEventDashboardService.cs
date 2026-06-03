using CycleManager.Domain.Dto;

namespace CycleManager.Services.Interfaces
{
    public interface IEventDashboardService
    {
        Task<EventDashboardDto> GetDashboardAsync(string userId);
    }
}
