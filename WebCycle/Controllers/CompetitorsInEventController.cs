using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;


namespace WebCycle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitorsInEventController : ControllerBase
    {
        private readonly ICompetitorsInEventRepository competitorsInEventRepository;
        public CompetitorsInEventController(ICompetitorsInEventRepository competitorsInEventRepository)
        {
            this.competitorsInEventRepository = competitorsInEventRepository;
        }

        [HttpGet("{id}", Name = "GetCompetitorsByEventId")]
        public async Task<IEnumerable<Competitor>> GetById(int id)
        { 
            var c = await competitorsInEventRepository.GetCompetitors(id);
            return c;
        }

        [HttpGet("{id}/{number}", Name = "GetRandomCompetitorsByEventId")]
        public async Task<IEnumerable<Competitor>> GetRandomById(int id, int number)
        {
            var c = await competitorsInEventRepository.GetRandomNumberofCompetitors(id, number);
            return c;
        }
    }
}
