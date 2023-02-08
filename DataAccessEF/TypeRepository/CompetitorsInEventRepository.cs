using Domain.Context;
using Domain.Interfaces;
using Domain.Models;

namespace DataAccessEF.TypeRepository
{
    class CompetitorsInEventRepository : GenericRepository<CompetitorsInEvent>, ICompetitorsInEventRepository
    {
        public CompetitorsInEventRepository(DatabaseContext context) : base(context) { }
    }
}
