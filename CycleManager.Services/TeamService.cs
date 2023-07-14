using CycleManager.Services.Interfaces;
using Domain.Interfaces;
using Domain.Models;

namespace CycleManager.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;
        public TeamService(ITeamRepository teamRepository) 
        {
            _teamRepository = teamRepository;
        }

        /// <summary>
        /// Get all teams
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IEnumerable<Team>> GetAll()
        {
            return await _teamRepository.GetAll();
        }

        /// <summary>
        /// Get a team by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Team> GetTeamById(int id)
        {
            return await _teamRepository.GetById(id);
        }
    }
}
