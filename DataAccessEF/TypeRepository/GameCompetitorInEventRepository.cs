using AutoMapper;
using CycleManager.Domain.Dto;
using CycleManager.Domain.Interfaces;
using Domain.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class GameCompetitorInEventRepository : GenericRepository<GameCompetitorEvent>, IGameCompetitorInEventRepository
    {
        private readonly IMapper _mapper;
        public GameCompetitorInEventRepository(ApplicationDbContext context, IMapper mapper) : base(context) 
        {
            _mapper = mapper;
        }

        public async Task<GameCompetitorEvent?> CreateGameCompetitorEventAsync(DeelnemerCreateDto dto)
        {
            var entity = _mapper.Map<GameCompetitorEvent>(dto);
            await context.GameCompetitorsEvent.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<GameCompetitorEvent>> GetAllGameCompetitorsInEventByEventId(int eventId)
        {
            var gameCompetitorsInEvent = context.GameCompetitorsEvent
                .Include(gce => gce.User)
                .Where(c => c.EventId.Equals(eventId));
            return await gameCompetitorsInEvent.ToListAsync();
        }

        public async Task<GameCompetitorEvent?> GetGameCompetitorInEventById(int id)
        {

            return await context.GameCompetitorsEvent
                .AsNoTracking()
                .Include(e => e.Event)
                .Include(u => u.User)
                .FirstOrDefaultAsync(gce => gce.Id == id);
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
                            .ThenInclude(c => c.CompetitorInTeam)
                                    .ThenInclude(cit => cit.Team)
                .Include(e => e.GameCompetitorEvents.Where(gce => gce.UserId == userId))
                    .ThenInclude(gce => gce.Renners)
                        .ThenInclude(p => p.CompetitorsInEvent)
                            .ThenInclude(p => p.CompetitorInTeam)
                            .ThenInclude(ci => ci.Competitor)
                                .ThenInclude(c => c.Country)
                .Include(e => e.GameCompetitorEvents.Where(gce => gce.UserId == userId))
                    .ThenInclude(gce => gce.Renners)
                        .ThenInclude(p => p.CompetitorsInEvent)
                            .ThenInclude(ci => ci.Event)
                .ToListAsync();


            return events;
        }

        public async Task<GameCompetitorEvent> GetyCompetitorWithPicksById(int id)
        {
            return await context.GameCompetitorsEvent
                .Include(p => p.Renners) // of Picks
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
