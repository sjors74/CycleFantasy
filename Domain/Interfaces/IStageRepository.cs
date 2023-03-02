using Domain.Models;

namespace Domain.Interfaces
{
    public interface IStageRepository : IGenericRepository<Stage>
    {
        Task<IEnumerable<Stage>> GetByEventId(int eventId);
    }
}
