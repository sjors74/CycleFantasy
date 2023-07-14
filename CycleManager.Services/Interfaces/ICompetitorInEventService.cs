using Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface ICompetitorInEventService
    {
       
        /// <summary>
        /// Get a competitor for an event by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<CompetitorsInEvent> GetCompetitorById(int id);
        
        /// <summary>
        /// Get all competitors for an event
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        Task<IEnumerable<Competitor>> GetCompetitors(int eventId);

        /// <summary>
        /// Create one or more new competitors for an event and save them
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Create(List<CompetitorsInEvent> entities);

        /// <summary>
        /// Update and save a competitor for an event
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task Update(CompetitorsInEvent entity);

        /// <summary>
        /// Remove and save a competitor for an event
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Delete(CompetitorsInEvent entity);

        /// <summary>
        /// Get a competitor in an event by id
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="competitorId"></param>
        /// <returns></returns>
        Task<CompetitorsInEvent> GetCompetitorsInEventByIds(int eventId, int competitorId);
    }
}
