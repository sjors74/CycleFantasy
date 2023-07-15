using CycleManager.Domain.Interfaces;
using CycleManager.Services.Interfaces;
using Domain.Models;

namespace CycleManager.Services
{
    public class GameCompetitorService : IGameCompetitorService
    {
        private readonly IGameCompetitorRepository _repo;
        public GameCompetitorService(IGameCompetitorRepository repo) 
        {
            _repo = repo;
        }

        /// <summary>
        /// Create a new gamecompetitor and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Create(GameCompetitor entity)
        {
            _repo.Add(entity);
            await _repo.SaveChangesAsync(); 
        }

        /// <summary>
        /// Delete a gamecompetitor and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Delete(GameCompetitor entity)
        {
            _repo.Remove(entity);
            await _repo.SaveChangesAsync();
        }

        /// <summary>
        /// Get a list of all gamecompetitors for an event
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<GameCompetitor>> GetAllGameCompetitors()
        {
            return await _repo.GetAllGameCompetitorsInEvent();
        }

        /// <summary>
        /// Get a gamecompetitor by it's id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<GameCompetitor> GetGameCompetitorById(int id)
        {
            return await _repo.GetById(id);
        }

        /// <summary>
        /// Update a game competitor and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Update(GameCompetitor entity)
        {
            _repo.Update(entity);
            await _repo.SaveChangesAsync();
        }
    }
}
