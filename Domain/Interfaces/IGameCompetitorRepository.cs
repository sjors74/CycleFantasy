using Domain.Interfaces;
using Domain.Models;

namespace CycleManager.Domain.Interfaces
{
    public interface IGameCompetitorRepository : IGenericRepository<GameCompetitor>
    {
        /// <summary>
        /// Get all game competitors for an event
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        Task<IEnumerable<GameCompetitor>> GetAllGameCompetitorsInEvent();
    }
}
