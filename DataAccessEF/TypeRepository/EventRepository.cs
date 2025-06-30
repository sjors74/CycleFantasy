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

        public async Task<int> GetAantalDeelnemers(int eventId)
        {
            var eventGameCompetitors = await context.GameCompetitorsEvent
                .Where(e => e.EventId == eventId)
                .ToListAsync();
            return eventGameCompetitors == null ? 0 : eventGameCompetitors.Count();
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
                    .FirstOrDefaultAsync(e => e.EventId == id);
            return e;
        }

        public async Task<EventDetailsViewModel?> GetEventDetailsViewModelById(int eventId)
        {
            return await context.Events
                .Where(e => e.EventId == eventId)
                .Select(e => new EventDetailsViewModel
                {
                    EventId = e.EventId,
                    EventCode = e.EventCode,
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
                return Enumerable.Empty<TeamDto>();

            var competitorsInEvent = await context.CompetitorsInEvent
                .Where(cie => cie.EventId == id)
                .Include(cie => cie.Competitor)
                    .ThenInclude(c => c.Country)
                .ToListAsync();

            var eventTeams = await context.EventTeam
                .Where(et => et.EventId == id)
                .Include(et => et.Team)
                .OrderBy(et => et.Team.TeamName)
                .ToListAsync();

            var teams = eventTeams.Select(et => new TeamDto
            { 
                   Id = et.Team.TeamId,
                   Naam = et.Team.TeamName,
                   Renners = competitorsInEvent
                    .Where(cie => cie.Competitor.TeamId == et.Team.TeamId)
                    .OrderByDescending(cie => cie.InSelectie)
                    .Select(cie => new CompetitorDto
                    {
                            CompetitorId = cie.Competitor.CompetitorId,
                            CompetitorName = cie.CompetitorName,
                            CountryShort = cie.Competitor.Country.CountryNameShort,
                            TeamName = et.Team.TeamName,
                            InSelectie = cie.InSelectie
                    }).ToList()
            }).ToList();
            return teams;
        }
    }
}