using Domain.Interfaces;
using Domain.Models;

namespace CycleManager.Domain.Interfaces
{
    public interface IGameCompetitorEventPickRepository : IGenericRepository<GameCompetitorEventPick>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        IQueryable<GameCompetitorEventPick> GetCompetitorEventPicksByEventId(int eventId);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        Task<IEnumerable<GameCompetitorEventPick>> GetCompetitorEventPicksById(int id);

        Task CreateGamePicksAsync(List<GameCompetitorEventPick> picks);

        Task RemovePickFromEvent(int id);

        Task DeleteGameCompetitorEventAsync(int id);
    }
}
