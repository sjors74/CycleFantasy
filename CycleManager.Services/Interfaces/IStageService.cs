using Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface IStageService
    {
        /// <summary>
        /// Get al ist of all stages in an event
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        Task<IEnumerable<Stage>> GetStagesByEventId(int eventId);
    }
}
