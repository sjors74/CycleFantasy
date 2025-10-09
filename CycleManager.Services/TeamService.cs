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

        public async Task Add(Team entity)
        {
            _teamRepository.Add(entity);
            await _teamRepository.SaveChangesAsync();
        }

        public async Task Delete(Team entity)
        {
            _teamRepository.Remove(entity);
            await _teamRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Get all teams
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IEnumerable<Team>> GetAllTeams()
        {
            return await _teamRepository.GetAllTeams();
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

        public async Task<Team> GetTeamForCurrentYear(int id, int year)
        {
            return await _teamRepository.GetTeamForCurrentYear(id, year);
        }

        public async Task<IEnumerable<SelectListItem>> GetTeamsAsSelectList(int selectedId = 0)
        {
            var teams = await _teamRepository.GetAll();
            return teams.Select(t => new SelectListItem
            {
                Value = t.TeamId.ToString(),
                Text = t.CurrentTeamName,
                Selected = (t.TeamId == selectedId)
            });
        }

        public async Task<IEnumerable<Team>> GetTeamsForEvent(int eventId)
        {
            return await _teamRepository.GetTeamsForEvent(eventId);
        }

        public async Task Update(Team entity)
        {
            _teamRepository.Update(entity);
            await _teamRepository.SaveChangesAsync();
        }
    }
}
