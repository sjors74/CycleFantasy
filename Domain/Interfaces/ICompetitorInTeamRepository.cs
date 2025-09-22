using CycleManager.Domain.Models;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface ICompetitorInTeamRepository : IGenericRepository<CompetitorInTeam> 
    {
        Task<bool> CheckCompetitorInTeam(int competitorId, int teamId, int year);
    }
}
