using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class EventRepository : GenericRepository<Event>, IEventRepository
    {
        public EventRepository(DatabaseContext context) : base(context) 
        { 
        }

        public async Task<IEnumerable<Event>> GetActiveEvents()
        {
            var eventList = await context.Events.Where(e => e.IsActive.Equals(true)).ToListAsync();
            return eventList;   
        }

        public async Task<IEnumerable<Event>> GetAllEvents()
        {
            var eventList = await context.Events.OrderByDescending(e => e.EventYear).ThenBy(e => e.StartDate).ToListAsync();
            return eventList;
        }

        public async Task<Event> GetEventById(int id)
        {
            var e = await context.Events.Where(e => e.EventId.Equals(id)).FirstOrDefaultAsync();
            return e;
        }

    }
}
