using Domain.Interfaces;
using Domain.Models;

namespace CycleManager.Domain.Interfaces
{
    public interface IGameCompetitorInEventRepository : IGenericRepository<GameCompetitorEvent>
    {
        Task<IEnumerable<GameCompetitorEvent>> GetAllGameCompetitorsInEventByEventId(int eventId);
    }
}
