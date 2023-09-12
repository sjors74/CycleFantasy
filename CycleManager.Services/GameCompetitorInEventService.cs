using CycleManager.Domain.Interfaces;
using CycleManager.Services.Interfaces;
using Domain.Models;

namespace CycleManager.Services
{
    public class GameCompetitorInEventService : IGameCompetitorInEventService
    {
        private readonly IGameCompetitorInEventRepository _repo;
        private readonly IGameCompetitorEventPickRepository _pickRepository;

        public GameCompetitorInEventService(IGameCompetitorInEventRepository repo, IGameCompetitorEventPickRepository pickRepository)
        {
            _repo = repo;
            _pickRepository = pickRepository;
        }

        /// <summary>
        /// Create a new gamecompetitor for an event and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Create(GameCompetitorEvent entity)
        {
            _repo.Add(entity);
            await _repo.SaveChangesAsync();
        }

        /// <summary>
        /// Delete a gamecompetitor for an event and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Delete(GameCompetitorEvent entity)
        {
            _repo.Remove(entity);
            await _repo.SaveChangesAsync();
        }

        /// <summary>
        /// Get a list of all game competitors for an event by event id
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<GameCompetitorEvent>> GetAllCompetitorsInEvent(int eventId)
        {
            return await _repo.GetAllGameCompetitorsInEventByEventId(eventId);
        }

        /// <summary>
        /// Get a gamecompettor for an event by it's id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<GameCompetitorEvent> GetCompetitorEventById(int id)
        {
            return await _repo.GetById(id);
        }


        /// <summary>
        /// Get a list of all gamecompetitors in an event and its picks (competitors for this event)
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public IQueryable<GameCompetitorEventPick> GetPicks(int eventId)
        {
            return _pickRepository.GetCompetitorEventPicksByEventId(eventId);

        }

        /// <summary>
        /// Get all picks for a gamecompetitor in event
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<GameCompetitorEventPick> GetPicks(int eventId, int id)
        {
            return _pickRepository.GetCompetitorEventPicksById(eventId, id);
        }

        /// <summary>
        /// Get the number of picks for a gamecompetitor
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="gameCompetitorId"></param>
        /// <returns></returns>
        public int GetNumberOfPicks(int eventId, int gameCompetitorId)
        {
            var picks = GetPicks(eventId, gameCompetitorId);
            return picks.Count();
        }

        /// <summary>
        /// Update a game competitor for an event and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Update(GameCompetitorEvent entity)
        {
            _repo.Update(entity);
            await _repo.SaveChangesAsync();
        }
    }
}
