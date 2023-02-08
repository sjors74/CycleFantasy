using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;


namespace WebCycle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitorsInEventController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        public CompetitorsInEventController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet("{id}", Name = "GetCompetitorsByEventId")]
        public IEnumerable<CompetitorsInEvent> GetAll() //to do: make a custom method for eventId in repository
        {
            return unitOfWork.CompetitorsInEvent.GetAll();
        }
    }
}
