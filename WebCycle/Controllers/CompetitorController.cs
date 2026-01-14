using CycleManager.Domain.Dto;
using CycleManager.Services.Interfaces;
using Domain.Dto;
using Domain.Interfaces;
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
        public async Task<IActionResult> GetCompetitors()
        {
            try
            {
                var competitors = await _competitorService.GetAllCompetitors(DateTime.Now.Year) ?? new List<CompetitorDto>();
                return Ok(competitors);
            }
            catch (Exception)
            {
                return StatusCode(500, "Fout bij het ophalen van renners.");
            }
        }

        [HttpGet("{id}", Name = "GetCompetitorById")]
        public async Task<IActionResult> GetById(int id) 
        {
            try
            {
                var competitor = await _competitorService.GetCompetitorById(id);
                if (competitor == null)
                    return NotFound();

                return Ok(competitor);
            }
            catch (Exception)
            {
                return StatusCode(500, "Fout bij ophalen van renner.");
            }
            
        }

        [HttpGet("{id}/team", Name = "GetCompetitorsByTeamId")]
        public async Task<IActionResult> GetByTeamId(int id, int year)
        {
            try
            {
                var competitors = await _competitorService.GetByTeamId(id, year) ?? new List<CompetitorInTeamDto>();
                return Ok(competitors);
            }
            catch (Exception)
            {
                return StatusCode(500, "Fout bij ophalen van renners/team.");
            }
        }

    }
}
