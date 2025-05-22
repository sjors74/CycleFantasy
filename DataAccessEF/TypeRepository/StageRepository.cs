using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class StageRepository : GenericRepository<Stage>, IStageRepository
    {
        public StageRepository(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<IEnumerable<Stage>> GetByEventId(int eventId)
        {
            var stages = await context.Stages
                .Include(e => e.Event)
                .Where(s => s.EventId.Equals(eventId))
                .OrderBy(s => s.StageOrder)
                .ToListAsync();
            return stages;
        }
    }
}
