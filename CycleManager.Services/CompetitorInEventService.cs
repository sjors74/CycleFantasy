using CycleManager.Services.Interfaces;
using Domain.Interfaces;
using Domain.Models;

namespace CycleManager.Services
{
    public class CompetitorInEventService : ICompetitorInEventService
    {
        private ICompetitorsInEventRepository _competitorsInEventRepository;
        public CompetitorInEventService(ICompetitorsInEventRepository competitorsInEventRepository) 
        {
            _competitorsInEventRepository = competitorsInEventRepository;
        }

        /// <summary>
        /// Create one or more competitors for an event and save them.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public async Task Create(List<CompetitorsInEvent> entities)
        {
            _competitorsInEventRepository.AddRange(entities);
            await _competitorsInEventRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Delete a competitor in an event
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Delete(CompetitorsInEvent entity)
        {
            _competitorsInEventRepository.Remove(entity);
            await _competitorsInEventRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Get a competitor in an event by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CompetitorsInEvent> GetCompetitorById(int id)
        {
            return await _competitorsInEventRepository.GetById(id);
        }

        /// <summary>
        /// Get all competitors in an event by eventId
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Competitor>> GetCompetitors(int eventId)
        {
            return await _competitorsInEventRepository.GetCompetitors(eventId);
        }

        /// <summary>
        /// Get a competitor in an event by eventId and comepetitorId
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="competitorId"></param>
        /// <returns></returns>
        public async Task<CompetitorsInEvent> GetCompetitorsInEventByIds(int eventId, int competitorId)
        {
            return await _competitorsInEventRepository.GetCompetitorsInEventByIds(eventId, competitorId);
        }

        /// <summary>
        /// Update a competitor in an event
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Update(CompetitorsInEvent entity)
        {
            _competitorsInEventRepository.Update(entity);
            await _competitorsInEventRepository.SaveChangesAsync();
        }
    }
}
