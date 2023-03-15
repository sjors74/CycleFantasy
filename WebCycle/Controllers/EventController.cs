using AutoMapper;
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
        private readonly IMapper _mapper;

        public EventController(IEventRepository eventRepository, IMapper mapper)
        {
            this.eventRepository = eventRepository;
            this._mapper = mapper;
        }

        [HttpGet(Name = "GetActiveEvent")]
        public async Task<IActionResult> GetEvent()
        {
            var events = await eventRepository.GetActiveEvents();
            var eventResponse = _mapper.Map<List<EventDto>>(events);
            return Ok(eventResponse);
        }

        [HttpGet("{id}", Name = "GetEventById")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var e = await eventRepository.GetEventById(id);
            var eventResponse = _mapper.Map<EventDto>(e);
            return Ok(eventResponse);
        }
    }
}
