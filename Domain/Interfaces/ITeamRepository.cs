using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface ITeamRepository : IGenericRepository<Team>
    {
        Task<IEnumerable<Team>> GetAllTeams();

        Task<Team> GetTeamById(int id);

        Task<TeamYear?> GetTeamYearByIdAsync(int teamYearId);

        Task<Team> GetTeamForCurrentYear(int id, int year);

        Task<IEnumerable<Team>> GetTeamsForEvent(int eventId);

        Task<bool> HasUnprocessedScrapedCompetitors();

        Task<int> CountUnprocessedScrapedCompetitors();

        Task<List<TeamYearDto>> GetTeamYears(int seasonYearId);

        Task<TeamYearDto?> GetByTeamAndSeasonAsync(int teamId, int seasonYearId);
    }
}
