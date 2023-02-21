using Domain.Models;

namespace Domain.Interfaces
{
    public interface ICompetitorRepository : IGenericRepository<Competitor> 
    {
        Task<IEnumerable<Competitor>> GetByTeamId(int teamId);
    }
}
