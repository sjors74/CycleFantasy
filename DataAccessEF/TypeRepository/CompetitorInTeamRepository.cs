using CycleManager.Domain.Models;
using Domain.Context;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class CompetitorInTeamRepository : GenericRepository<CompetitorInTeam>, ICompetitorInTeamRepository
    {
        public CompetitorInTeamRepository(ApplicationDbContext context) : base(context) 
        { 

        }

        public async Task<bool> CheckCompetitorInTeam(int competitorId, int teamId, int year)
        {
            return await context.CompetitorInTeams
                .AnyAsync(cit => cit.CompetitorId == competitorId
                && cit.TeamId == teamId
                && cit.Year == year);
        }
    }
}
