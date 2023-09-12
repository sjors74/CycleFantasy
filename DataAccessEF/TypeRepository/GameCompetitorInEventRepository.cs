using CycleManager.Domain.Interfaces;
using Domain.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class GameCompetitorInEventRepository : GenericRepository<GameCompetitorEvent>, IGameCompetitorInEventRepository
    {
        public GameCompetitorInEventRepository(DatabaseContext context) : base(context) 
        {
        }

        public async Task<IEnumerable<GameCompetitorEvent>> GetAllGameCompetitorsInEventByEventId(int eventId)
        {
            var gameCompetitorsInEvent = context.GameCompetitorsEvent.Where(c => c.EventId.Equals(eventId));
            return await gameCompetitorsInEvent.ToListAsync();
        }
    }
}
