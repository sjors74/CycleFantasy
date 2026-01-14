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

        public Task<int> GetStageIdFromStageNumber(int stageNumber, int eventId)
        {
            return _stageRepository.GetStageId(stageNumber, eventId);
        }

        public Task<int> GetStageResults(int stageNumber, int eventId)
        {
            return _stageRepository.GetStagesResults(stageNumber, eventId);
        }

        public Task<Stage> GetStage(int stageNumber, int eventId)
        {
            return _stageRepository.GetStage(stageNumber, eventId);
        }

        public async Task AddStage(Stage stage)
        {
            _stageRepository.Add(stage);
            await _stageRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteStage(int id)
        {
            var entity = await _stageRepository.GetById(id);
            if (entity != null)
            {
                _stageRepository.Remove(entity);
                await _stageRepository.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public Task<Stage> GetStageById(int id)
        {
            return _stageRepository.GetStageById(id);
        }

        public async Task UpdateStage(Stage stage)
        {
            _stageRepository.Update(stage);
            await _stageRepository.SaveChangesAsync();
        }
    }
}
