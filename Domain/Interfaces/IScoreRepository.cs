using CycleManager.Domain.Dto;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface IScoreRepository
    {
        Task<List<DeelnemerScore>> GetScoresByEventIdAsync(int eventId);

        Task<List<DeelnemerDto>> GetPoolRankingForStage(int eventId, int stageId);
    }
}
