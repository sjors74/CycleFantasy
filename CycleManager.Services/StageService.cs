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

        public Task<int> GetStageNumberForDateAsync(DateTime date, int eventId)
        {
            return _stageRepository.GetStageNumber(date, eventId);
        }

        public Task<int> GetStageResults(int stageNumber, int eventId)
        {
            return _stageRepository.GetStagesResults(stageNumber, eventId);
        }
    }
}
