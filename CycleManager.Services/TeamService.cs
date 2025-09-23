using CycleManager.Services.Interfaces;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            return await _teamRepository.GetTeamById(id);
        }

        public async Task<IEnumerable<SelectListItem>> GetTeamsAsSelectList(int selectedId = 0)
        {
            var teams = await _teamRepository.GetAll();
            return teams.Select(t => new SelectListItem
            {
                Value = t.TeamId.ToString(),
                Text = t.TeamName,
                Selected = (t.TeamId == selectedId)
            });
        }
    }
}
