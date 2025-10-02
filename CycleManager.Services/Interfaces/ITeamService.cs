using Domain.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CycleManager.Services.Interfaces
{
    public interface ITeamService
    {
        /// <summary>
        /// Get a team by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Team> GetTeamById(int id);

        /// <summary>
        /// Get all teams
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Team>> GetAllTeams();

        Task<IEnumerable<SelectListItem>> GetTeamsAsSelectList(int selectedId = 0);

        Task Delete(Team entity);

        Task Update(Team entity);

        Task Add(Team entity);

        Task<IEnumerable<Team>> GetTeamsForEvent(int eventId);
    }
}
