using CycleManager.Services.Interfaces;
using Domain.Interfaces;

namespace CycleManager.Services
{
    public class CompetitorService : ICompetitorService
    {
        private readonly ICompetitorRepository _competitorRepository;
        public CompetitorService(ICompetitorRepository competitorRepository) 
        {
            _competitorRepository = competitorRepository;
        }

        public async Task<int> GetCompetitorsByCountry(int id)
        {
            return await _competitorRepository.GetCompetitorsByCountry(id);
        }
    }
}
