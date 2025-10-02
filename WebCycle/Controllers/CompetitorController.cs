using CycleManager.Domain.Dto;
using CycleManager.Services.Interfaces;
using Domain.Dto;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebCycle.Controllers
{
    [Route("api/competitors")]
    [ApiController]
    public class CompetitorController : ControllerBase
    {
        private readonly ICompetitorService _competitorService;

        public CompetitorController(ICompetitorRepository competitorRepository, ICompetitorService competitorService)
        {
            _competitorService = competitorService;
        }
        [HttpGet]
        public async Task<IEnumerable<CompetitorDto>> GetCompetitors()
        {
            var competitors = await _competitorService.GetAllCompetitors(DateTime.Now.Year);
            return competitors;
        }

        [HttpGet("{id}", Name = "GetCompetitorById")]
        public async Task<Competitor> GetById(int id) 
        {
            return await _competitorService.GetCompetitorById(id);
        }

        [HttpGet("{id}/team", Name = "GetCompetitorsByTeamId")]
        public async Task<IEnumerable<CompetitorInTeamDto>> GetByTeamId(int id, int year)
        {
            return await _competitorService.GetByTeamId(id, year);
        }

    }
}
