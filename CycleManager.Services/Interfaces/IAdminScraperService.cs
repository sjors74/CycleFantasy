using Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface IAdminScraperService
    {
        Task<Stage?> GetStageByIdAsync(int stageId);
        Task<Team?> GetTeamByIdAsync(int teamId);
        Task ImportScrapedCompetitorsAsync();
    }
}
