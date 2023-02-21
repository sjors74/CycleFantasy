using Domain.Dto;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface IResultsRepository : IGenericRepository<ResultDto>
    {
        Task<IEnumerable<ResultDto>> GetResultsByStageId(int stageId);
    }
}
