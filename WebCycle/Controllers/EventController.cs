using AutoMapper;
using CycleManager.Domain.Dto;
using CycleManager.Services;
using Domain.Dto;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebCycle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventRepository eventRepository;
        private readonly IEventService eventService;
        private readonly IMapper _mapper;

        public EventController(IEventRepository eventRepository, IEventService eventService, IMapper mapper)
        {
            this.eventRepository = eventRepository;
            this.eventService = eventService;
            this._mapper = mapper;
        }

        [HttpGet(Name = "GetActiveEvent")]
        public async Task<IActionResult> GetEvent()
        {
            //TODO: add GetActiveEvents to service and remove repository from this class!
            var events = await eventRepository.GetActiveEvents();
            var eventResponse = _mapper.Map<List<EventDto>>(events);
            return Ok(eventResponse);
        }

        [HttpGet("{id}", Name = "GetEventById")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var e = await eventService.GetEventById(id);
            var eventResponse = _mapper.Map<EventDto>(e);
            return Ok(eventResponse);
        }

        [HttpGet("{id}/stages")]
        public async Task<IActionResult> GetStagesByEventId(int id)
        {
            var stages = await eventService.GetStagesWithResultsForEvent(id);
            return Ok(stages);
        }

        [HttpGet("{userid}/user")]
        public async Task<IActionResult> GetEventByUserId(string userid)
        {
            var allEvents = await eventService.GetEventsByUserId(userid);
            var nu = DateTime.UtcNow;

            var active = allEvents
                .Where(e => e.StartDate <= nu && e.EndDate >= nu)
                .ToList();
            var future = allEvents
                .Where(e => e.StartDate > nu)
                .ToList();

            var activeDtos = _mapper.Map<List<EventForUserDto>>(active);
            var futureDtos = _mapper.Map<List<EventForUserDto>>(future);

            activeDtos.ForEach(e => e.UserId = userid);
            futureDtos.ForEach(e => e.UserId = userid);

            var result = new EventViewDto
            {
                ActieveEvenementen = activeDtos,
                ToekomstigeEvenementen = futureDtos
            };
          
            return Ok(result);
        }
    }
}
