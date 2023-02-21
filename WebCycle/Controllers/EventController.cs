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
        public EventController(IEventRepository eventRepository)
        {
            this.eventRepository = eventRepository;
        }

        [HttpGet("{isActive}", Name = "GetActiveEvent")]
        public async Task<IEnumerable<Event>> GetEvent(bool isActive)
        {
            return await eventRepository.GetActiveEvents(isActive);
        }

    }
}
