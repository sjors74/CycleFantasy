using CycleManager.Services.Interfaces;
using Domain.Interfaces;
using Domain.Models;

namespace CycleManager.Services
{
    public class AdminScraperService : IAdminScraperService
    {
        private readonly IStageRepository _stageRepository;
        private readonly ITeamRepository _teamRepository;

        public AdminScraperService(IStageRepository stageRepository, ITeamRepository teamRepository)
        {
            _stageRepository = stageRepository;
            _teamRepository = teamRepository;
        }

        public async Task<Stage?> GetStageByIdAsync(int stageId)
        {
            return await _stageRepository.GetStageById(stageId);
        }

        public async Task<Team?> GetTeamByIdAsync(int teamId)
        {
            return await _teamRepository.GetTeamById(teamId);
        }

        public async Task ImportScrapedCompetitorsAsync()
        {
            //TODO : implement?
            await Task.CompletedTask;
        }
    }
}
