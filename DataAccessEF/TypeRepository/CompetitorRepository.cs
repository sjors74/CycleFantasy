using Domain.Context;
using Domain.Interfaces;
using Domain.Models;

namespace DataAccessEF.TypeRepository
{
    class CompetitorRepository : GenericRepository<Competitor>, ICompetitorRepository
    {
        public CompetitorRepository(DatabaseContext context) : base(context) { }
    }
}
