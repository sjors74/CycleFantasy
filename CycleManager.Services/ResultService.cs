using CycleManager.Services.Interfaces;
using Domain.Interfaces;

namespace CycleManager.Services
{
    public class ResultService : IResultService
    {
        private readonly IResultsRepository _resultsRepository;
        public ResultService(IResultsRepository resultsRepository)
        {
            _resultsRepository = resultsRepository;
        }

        public Task<int> GetResultsByStageId(int stageId)
        {
            return _resultsRepository.GetResultsByStageId(stageId);
        }
    }
}
