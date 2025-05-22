using CycleManager.Domain.ViewModel;
using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class EventRepository : GenericRepository<Event>, IEventRepository
    {
        public EventRepository(ApplicationDbContext context) : base(context) 
        { 
        }

        public async Task<IEnumerable<Event>> GetActiveEvents()
        {
            var eventList = await context.Events
                .Include(s => s.Stages)
                .Include(e => e.Configuration)
                .Where(e => e.IsActive.Equals(true))
                .AsNoTracking()
                .ToListAsync();
            return eventList;   
        }

        public async Task<IEnumerable<Event>> GetAllEvents()
        {
            var eventList = 
                await context.Events
                .Include(e => e.Configuration)
                .Include(s => s.Stages)
                .OrderByDescending(e => e.EventYear)
                .ThenBy(e => e.StartDate)
                .AsNoTracking()
                .ToListAsync();
            return eventList;
        }

        public async Task<Event> GetEventById(int id)
        {
            var e = await context.Events
                .Include(s => s.Stages)
                .Include(e => e.Configuration)
                .Where(e => e.EventId.Equals(id))
                .AsNoTracking()
                .FirstOrDefaultAsync();
            return e;
        }

        public async Task<EventDetailsViewModel?> GetEventDetailsViewModelById(int eventId)
        {
            return await context.Events
                .Where(e => e.EventId == eventId)
                .Select(e => new EventDetailsViewModel
                {
                    EventId = e.EventId,
                    EventName = e.EventName,
                    Slogan = e.Slogan,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Stages = e.Stages
                        .OrderBy(s => s.StageOrder)
                        .Select(s => new StageViewModel
                        {
                            StageId = s.Id,
                            StageName = s.StageName,
                            StageOrder = s.StageOrder,
                            StartLocation = s.StartLocation,
                            FinishLocation = s.FinishLocation,
                            AantalPosities = s.Results.Count
                        }).ToList()
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }
}
