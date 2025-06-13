using CycleManager.Domain.Interfaces;
using Domain.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class GameCompetitorInEventRepository : GenericRepository<GameCompetitorEvent>, IGameCompetitorInEventRepository
    {
        public GameCompetitorInEventRepository(ApplicationDbContext context) : base(context) 
        {
        }

        public async Task<GameCompetitorEvent> CreateGameCompetitorEventAsync(GameCompetitorEvent gameCompetitorEvent)
        {
            await context.GameCompetitorsEvent.AddAsync(gameCompetitorEvent);
            await context.SaveChangesAsync();
            return gameCompetitorEvent;
        }

        public async Task<IEnumerable<GameCompetitorEvent>> GetAllGameCompetitorsInEventByEventId(int eventId)
        {
            var gameCompetitorsInEvent = context.GameCompetitorsEvent
                .Include(gce => gce.User)
                .Where(c => c.EventId.Equals(eventId));
            return await gameCompetitorsInEvent.ToListAsync();
        }

        public async Task<List<Event>> GetEventsByUserId(string userId)
        {
            var events = await context.Events
                .Where(e => e.GameCompetitorEvents.Any(gce => gce.UserId == userId))
                .Include(e => e.GameCompetitorEvents.Where(gce => gce.UserId == userId))
                    .ThenInclude(gce => gce.User)
                .Include(e => e.GameCompetitorEvents.Where(gce => gce.UserId == userId))
                    .ThenInclude(gce => gce.Renners)
                        .ThenInclude(p => p.CompetitorsInEvent)
                            .ThenInclude(ci => ci.Competitor)
                                .ThenInclude(c => c.Team)
                .Include(e => e.GameCompetitorEvents.Where(gce => gce.UserId == userId))
                    .ThenInclude(gce => gce.Renners)
                        .ThenInclude(p => p.CompetitorsInEvent)
                            .ThenInclude(ci => ci.Competitor)
                                .ThenInclude(c => c.Country)
                .Include(e => e.GameCompetitorEvents.Where(gce => gce.UserId == userId))
                    .ThenInclude(gce => gce.Renners)
                        .ThenInclude(p => p.CompetitorsInEvent)
                            .ThenInclude(ci => ci.Event)
                .ToListAsync();



            return events;
        }

    }
}
