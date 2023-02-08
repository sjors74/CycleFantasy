using Domain.Context;
using Domain.Interfaces;
using Domain.Models;

namespace DataAccessEF.TypeRepository
{
    class TeamRepository : GenericRepository<Team>, ITeamRepository
    {
        public TeamRepository(DatabaseContext context) : base(context) { }
    }
}
