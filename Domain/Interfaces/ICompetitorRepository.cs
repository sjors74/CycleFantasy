using Domain.Models;

namespace Domain.Interfaces
{
    public interface ICompetitorRepository : IGenericRepository<Competitor> 
    {
        IQueryable<Competitor> GetAllCompetitors();
        Task<IEnumerable<Competitor>> GetByTeamId(int teamId);
    }
}
