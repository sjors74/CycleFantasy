using CycleManager.Domain.Dto;
using CycleManager.Domain.ViewModel;
using Domain.Context;
using Domain.Dto;
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
                .Include(e => e.GameCompetitorEvents)
                    .ThenInclude(e => e.User)
                .Include(s => s.Stages)
                    .ThenInclude(r => r.Results)
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
                        .ThenInclude(r => r.Results)
                    .Include(c => c.GameCompetitorEvents)
                        .ThenInclude(gce => gce.Renners)
                    .Include(c => c.GameCompetitorEvents)
                        .ThenInclude(gce => gce.User)
                    //.Include(c => c.CompetitorsInEvent)
                        //.ThenInclude(ci => ci.Competitor)
                        //    .ThenInclude(comp => comp.Team)
                    //.Include(c => c.CompetitorsInEvent)
                    //    .ThenInclude(ci => ci.Competitor)
                    //        .ThenInclude(comp => comp.Country)
                    //.Include(c => c.CompetitorsInEvent)
                    //    .ThenInclude(ci => ci.GameCompetitorEventPicks) // BELANGRIJK
                    //.Include(e => e.Configuration)
                    .FirstOrDefaultAsync(e => e.EventId == id); return e;
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

        public async Task<IEnumerable<TeamDto>> GetTeamsForEvent(int id)
        {
            var eventExists = await context.Events.AnyAsync(e => e.EventId == id);
            if (!eventExists)
                return null;
            var teams = await context.EventTeam
                .Where(et => et.EventId == id)
                .Include(et => et.Team)
                    .ThenInclude(t => t.Competitors) // laad ook renners
                    .OrderBy(et => et.Team.TeamName)
                    .Select(et => new TeamDto
                    {
                        Id = et.Team.TeamId,
                        Naam = et.Team.TeamName,
                        Renners = et.Team.Competitors.Select(r => new CompetitorDto
                        {
                            CompetitorId = r.CompetitorId,
                            CompetitorName = r.CompetitorName,
                            CountryShort = r.Country.CountryNameShort,
                            TeamName = et.Team.TeamName
                        }).ToList()
                    })
                    .ToListAsync();
            return teams;
        }
    }
}
