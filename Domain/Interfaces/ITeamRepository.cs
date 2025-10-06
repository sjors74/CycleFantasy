using Domain.Models;

namespace Domain.Interfaces
{
    public interface ITeamRepository : IGenericRepository<Team>
    {
        Task<IEnumerable<Team>> GetAllTeams();

        Task<Team> GetTeamById(int id);

        Task<Team> GetTeamForCurrentYear(int id, int year);

        Task<IEnumerable<Team>> GetTeamsForEvent(int eventId);
    }
}
