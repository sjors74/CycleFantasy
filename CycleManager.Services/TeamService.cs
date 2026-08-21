using CycleManager.Domain.Dto;
using CycleManager.Domain.Interfaces;
using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CycleManager.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly ISeasonYearRepository _seasonYearRepository;

        public TeamService(ITeamRepository teamRepository, ISeasonYearRepository seasonYearRepository)
        {
            _teamRepository = teamRepository;
            _seasonYearRepository = seasonYearRepository;
        }

        public async Task Add(Team team)
        {
            var seasonYears = await _seasonYearRepository.GetAllAsync();

            foreach (var seasonYear in seasonYears)
            {
                team.TeamYears.Add(new TeamYear
                {
                    SeasonYearId = seasonYear.SeasonYearId,
                    Year = seasonYear.Year,
                    Name = team.CurrentTeamName
                });
            }

            _teamRepository.Add(team);
            await _teamRepository.SaveChangesAsync();
        }

        public async Task<int> CountUnprocessedScrapedCompetitors()
        {
            return await _teamRepository.CountUnprocessedScrapedCompetitors();
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

        public async Task<List<TeamYearDto>> GetTeamYears(int seasonYearId)
        {
            return await _teamRepository.GetTeamYears(seasonYearId);
        }

        public async Task<bool> HasUnprocessedScrapedTeams()
        {
            return await _teamRepository.HasUnprocessedScrapedCompetitors();
        }

        public async Task Update(Team entity)
        {
            _teamRepository.Update(entity);
            await _teamRepository.SaveChangesAsync();
        }

        public async Task<TeamYearDto?> GetByTeamAndSeasonAsync(int teamId, int seasonYearId)
        {
            return await _teamRepository.GetByTeamAndSeasonAsync(teamId, seasonYearId);
        }

        public async Task<TeamYear?> GetTeamYearById(int teamYearId)
        {
            var teamYear = await _teamRepository.GetTeamYearByIdAsync(teamYearId);
            if (teamYear == null)
            {
                return null;
            }

            return teamYear;
        }
    }
}
