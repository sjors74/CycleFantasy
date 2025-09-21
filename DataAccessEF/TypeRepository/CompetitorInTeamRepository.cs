using CycleManager.Domain.Models;
using Domain.Context;
using Domain.Interfaces;

namespace DataAccessEF.TypeRepository
{
    public class CompetitorInTeamRepository : GenericRepository<CompetitorInTeam>, ICompetitorInTeamRepository
    {
        public CompetitorInTeamRepository(ApplicationDbContext context) : base(context) 
        { 
        }
    }
}
