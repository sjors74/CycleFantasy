using Domain.Models;

namespace Domain.Interfaces
{
    public interface ITeamRepository : IGenericRepository<Team>
    {
        Task<IEnumerable<Team>> GetAllTeams();

        Task<Team> GetTeamById(int id);
    }
}
