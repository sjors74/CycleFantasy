using CycleManager.Domain.Dto;
using Domain.Interfaces;
using Domain.Models;

namespace CycleManager.Domain.Interfaces
{
    public interface IGameCompetitorInEventRepository : IGenericRepository<GameCompetitorEvent>
    {
        Task<IEnumerable<GameCompetitorEvent>> GetAllGameCompetitorsInEventByEventId(int eventId);
        Task<List<Event>> GetEventsByUserId(string userId);
        Task<GameCompetitorEvent> CreateGameCompetitorEventAsync(DeelnemerCreateDto dto);
        Task<GameCompetitorEvent> GetyCompetitorWithPicksById(int id);
        Task<GameCompetitorEvent?> GetGameCompetitorInEventById(int id);
    }
}
