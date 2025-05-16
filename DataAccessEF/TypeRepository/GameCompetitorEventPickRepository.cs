using CycleManager.Domain.Interfaces;
using Domain.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class GameCompetitorEventPickRepository : GenericRepository<GameCompetitorEventPick>, IGameCompetitorEventPickRepository
    {
        public GameCompetitorEventPickRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public IQueryable<GameCompetitorEventPick> GetCompetitorEventPicksByEventId(int eventId)
        {
            return context.GameCompetitorEventPicks
                .Include(c => c.CompetitorsInEvent).ThenInclude(a => a.Competitor)
                .Include(g => g.GameCompetitorEvent).ThenInclude(b => b.GameCompetitor)
                .Where(c => c.CompetitorsInEvent.EventId.Equals(eventId));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IEnumerable<GameCompetitorEventPick>> GetCompetitorEventPicksById(int id)
        {
            return await context.GameCompetitorEventPicks
                 .Include(g => g.GameCompetitorEvent).ThenInclude(b => b.GameCompetitor)
                 .Where(c => c.GameCompetitorEvent.Id.Equals(id))
                 .OrderBy(c => c.CompetitorsInEvent.EventNumber)
                 .ToListAsync();
        }
    }
}
