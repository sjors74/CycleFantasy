using Domain.Models;

namespace CycleManager.Services
{
    public interface IEventService
    {
        Task<IEnumerable<Event>> GetAllEvents();

        Task<Event> GetEventById(int id);

        /// <summary>
        /// Add and save a new event
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Create (Event entity);

        /// <summary>
        /// Update and save an event
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Update(Event entity);

        /// <summary>
        /// Remove and save an event
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Delete(Event entity);
    }
}
