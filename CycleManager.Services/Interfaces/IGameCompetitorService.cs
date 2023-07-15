using Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface IGameCompetitorService
    {
        /// <summary>
        /// Get a gamecompetitor by its id
        /// </summary>s
        /// <param name="id"></param>
        /// <returns></returns>
        Task<GameCompetitor> GetGameCompetitorById(int id);

        /// <summary>
        /// Get a list of all gamecompetitors in an event
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<GameCompetitor>> GetAllGameCompetitors();
        
        /// <summary>
        /// Create a new gamecompetitor and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Create(GameCompetitor entity);

        /// <summary>
        /// Update a gamecompetitor and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Update(GameCompetitor entity);

        /// <summary>
        /// Remove a gamecompetitor and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Delete(GameCompetitor entity);
    }
}
