using Domain.Models;

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
        Task<IEnumerable<Team>> GetAll();
    }
}
