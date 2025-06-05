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
            var picks = context.GameCompetitorEventPicks
                .Include(c => c.CompetitorsInEvent)
                    .ThenInclude(a => a.Competitor)
                .Include(g => g.GameCompetitorEvent)
                    .ThenInclude(b => b.User)
                .Where(c => c.CompetitorsInEvent.EventId.Equals(eventId));
            return picks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IEnumerable<GameCompetitorEventPick>> GetCompetitorEventPicksById(int id)
        {
            var picks =  await context.GameCompetitorEventPicks
                 .Include(g => g.GameCompetitorEvent)
                    .ThenInclude(b => b.User)
                 .Include(c => c.CompetitorsInEvent)
                    .ThenInclude(c => c.Competitor)
                         .ThenInclude(c => c.Team)
                 .Include(c => c.CompetitorsInEvent)
                    .ThenInclude(c => c.Competitor)
                         .ThenInclude(c => c.Country)
                 .Include(c => c.CompetitorsInEvent)
                    .ThenInclude(e => e.Event)
                 .Where(c => c.GameCompetitorEvent.Id.Equals(id))
                 .OrderBy(c => c.CompetitorsInEvent.EventNumber)
                 .ToListAsync();
            return picks;
        }

        public async Task CreateGamePicksAsync(List<GameCompetitorEventPick> picks)
        {
            await context.GameCompetitorEventPicks.AddRangeAsync(picks);
            try
            {
                var changes = await context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Er is een fout opgetreden: {ex.Message}");
            }
        }
    }
}
