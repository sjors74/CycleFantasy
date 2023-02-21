using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebCycle.Controllers
{
    [Route("api/competitors")]
    [ApiController]
    public class CompetitorController : ControllerBase
    {
        private readonly ICompetitorRepository competitorRepository;

        public CompetitorController(ICompetitorRepository competitorRepository)
        {
            this.competitorRepository = competitorRepository;
        }
        [HttpGet]
        public async Task<IEnumerable<Competitor>> GetCompetitors()
        {
            var competitors = await competitorRepository.GetAll();
            return competitors;
        }

        [HttpGet("{id}", Name = "GetCompetitorById")]
        public Competitor GetById(int id) 
        {
            return competitorRepository.GetById(id);
        }

        [HttpGet("{id}/team", Name = "GetCompetitorsByTeamId")]
        public async Task<IEnumerable<Competitor>> GetByTeamId(int id)
        {
            return await competitorRepository.GetByTeamId(id);
        }

    }
}
