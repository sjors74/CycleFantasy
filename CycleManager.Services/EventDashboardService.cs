using AutoMapper;
using CycleManager.Domain.Dto;
using CycleManager.Services.Interfaces;
using Domain.Context;
using Domain.Dto;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Services
{
    public class EventDashboardService : IEventDashboardService
    {
        private readonly ApplicationDbContext _db;
        private readonly IEventService _eventService;
        private readonly IMapper _mapper;
        public EventDashboardService(ApplicationDbContext db, IMapper mapper, IEventService eventService)
        {
            _db = db;
            _eventService = eventService;
            _mapper = mapper;

        }
        public async Task<EventDashboardDto> GetDashboardAsync(string userId)
        {
            var now = DateTime.UtcNow;

            // 1. ACTUEEL
            var actueelEvents = await _db.Events
                .Where(e =>
                    e.IsActive).ToListAsync();

            // 2. TOEKOMST
            var toekomstEvents= await _db.Events
                .Where(e =>
                    e.IsActive &&
                    e.StartDate > now).ToListAsync();

            // 3. HISTORISCH (user-based)
            var historischEvents = await _db.Events
                .Where(e =>
                    e.EndDate < now &&
                    e.GameCompetitorEvents.Any(g => g.UserId == userId)).ToListAsync();

            // Execute in parallel (belangrijk voor performance)
            var actueelDtos = _mapper.Map<List<EventForUserDto>>(actueelEvents);
            var toekomstDtos = _mapper.Map<List<EventForUserDto>>(toekomstEvents);
            var historischDtos = _mapper.Map<List<EventForUserDto>>(historischEvents);

            var userEvents = await _eventService.GetEventsByUserId(userId);

            var userLookup = userEvents.ToDictionary(x => x.EventId);

            MergeUserData(actueelDtos, userLookup);
            MergeUserData(toekomstDtos, userLookup);
            MergeUserData(historischDtos, userLookup);


            return new EventDashboardDto
            {
                Titel = "Evenementen",
                Actueel = actueelDtos,
                Toekomst = toekomstDtos,
                Historisch = historischDtos
            };
        }


        private static void MergeUserData(IEnumerable<EventForUserDto> events, Dictionary<int, EventForUserDto> userLookup)
        {
            foreach (var evt in events)
            {
                if (userLookup.TryGetValue(evt.EventId, out var userEvent))
                {
                    evt.Deelnemers = userEvent.Deelnemers;
                    evt.IsIngeschreven = userEvent.IsIngeschreven;
                    evt.CanSubscribe = userEvent.CanSubscribe;
                }
            }
        }
    }
}
