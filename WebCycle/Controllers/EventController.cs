using AutoMapper;
using CycleManager.Services;
using DataAccessEF.TypeRepository;
using DataAccessEF.UnitOfWork;
using Domain.Dto;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Http;
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

    }
}
