using Domain.Models;

namespace Domain.Interfaces
{
    public interface ITeamRepository : IGenericRepository<Team>
    {
        Task<IEnumerable<Team>> GetAll();

        Task<Team> GetTeamById(int id);
    }
}
