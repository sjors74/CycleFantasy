using CycleManager.Services.Interfaces;
using Domain.Interfaces;
using Domain.Models;

namespace CycleManager.Services
{
    public class StageService : IStageService
    {
        private IStageRepository _stageRepository;
        public StageService(IStageRepository stageRepository)
        {
            _stageRepository = stageRepository;
        }

        public Task<IEnumerable<Stage>> GetStagesByEventId(int eventId)
        {
            return _stageRepository.GetByEventId(eventId);
        }
    }
}
