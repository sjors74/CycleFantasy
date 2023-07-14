using CycleManager.Services.Interfaces;
using Domain.Interfaces;
using Domain.Models;

namespace CycleManager.Services
{
    public class CompetitorService : ICompetitorService
    {
        private readonly ICompetitorRepository _competitorRepository;
        public CompetitorService(ICompetitorRepository competitorRepository) 
        {
            _competitorRepository = competitorRepository;
        }
    
        /// <summary>
        /// Create a new competitor and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Create(Competitor entity)
        {
            _competitorRepository.Add(entity);
            await _competitorRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Delete a competitor and save it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Delete(Competitor entity)
        {
            _competitorRepository.Remove(entity);
            await _competitorRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Get all competitors
        /// </summary>
        /// <returns></returns>
        public IQueryable<Competitor> GetAllCompetitors()
        {
            return _competitorRepository.GetAllCompetitors();
        } 

        /// <summary>
        /// Get a competitor by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<Competitor> GetCompetitorById(int id)
        {
            return _competitorRepository.GetById(id);
        }

        /// <summary>
        /// Get number of competitors by country Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<int> GetCompetitorsByCountry(int id)
        {
            return await _competitorRepository.GetCompetitorsByCountry(id);
        }

        /// <summary>
        /// Update and save a competitor
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Update(Competitor entity)
        {
            _competitorRepository.Update(entity);
            await _competitorRepository.SaveChangesAsync();
        }
    }
}
