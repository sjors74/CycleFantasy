using Domain.Models;

namespace Domain.Interfaces
{
    public interface IStageRepository : IGenericRepository<Stage>
    {
        Task<IEnumerable<Stage>> GetByEventId(int eventId);
        Task<int> GetStageNumber(DateTime date, int eventId);
        Task<int> GetStagesResults(int stageNumber, int eventId);
    }
}
