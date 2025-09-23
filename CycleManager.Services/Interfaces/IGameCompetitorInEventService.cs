using CycleManager.Domain.Dto;
using Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface IGameCompetitorInEventService
    {
        /// <summary>
        /// Get a game competitor in an event by it's id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<GameCompetitorEvent?> GetGameCompetitorEventById(int id);
        
        /// <summary>
        /// Get a list of all gamecompetitors in an event
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        Task<IEnumerable<GameCompetitorEvent>> GetAllCompetitorsInEvent(int eventId);
        
        /// <summary>
        /// Create a new game competitor for an event and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Create(GameCompetitorEvent entity);
        
        /// <summary>
        /// Update a game competitor for an event and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task UpdateAsync(DeelnemerEditDto entity);
        
        /// <summary>
        /// Delete a game competitor for an event and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Delete(GameCompetitorEvent entity);

        /// <summary>
        /// Get a list of all gamecompetitors in an event and its picks (competitors for this event)
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        IQueryable<GameCompetitorEventPick> GetPicks(int eventId);

        /// <summary>
        /// Get all picks for a gamecompetitor in event
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IEnumerable<GameCompetitorEventPick>> GetAllPicks(int id);

        Task<int> GetNumberOfPicks(int eventId, int gameCompetitorId);

        Task<IEnumerable<CompetitorsInEvent>> GetCompetitors(int id, int number);

        Task<CompetitorsInEvent> GetCompetitorInEventById(int id);

        Task<IEnumerable<int>> GetAllPicksAsCompetitorIds(int id);

        Task<GameCompetitorEvent> CreateGameCompetitorEventAsync(DeelnemerCreateDto dto);
    }
}
