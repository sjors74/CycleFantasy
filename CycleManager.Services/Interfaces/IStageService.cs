using Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface IStageService
    {
        Task<IEnumerable<Stage>> GetStagesByEventId(int eventId);
    }
}
