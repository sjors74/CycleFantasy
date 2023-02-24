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

        public async Task<IEnumerable<Event>> GetActiveEvents(int id)
        {
            var eventList = await context.Events.Where(e => e.EventId.Equals(id)).ToListAsync();
            return eventList;   

        }
    }
}
