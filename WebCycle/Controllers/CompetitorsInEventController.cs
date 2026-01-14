using AutoMapper;
using CycleManager.Services.Interfaces;
using Domain.Dto;
using Microsoft.AspNetCore.Mvc;


namespace WebCycle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitorsInEventController : ControllerBase
    {
        private readonly ICompetitorInEventService _competitorInEventService;
        private readonly IMapper _mapper;

        public CompetitorsInEventController(ICompetitorInEventService competitorInEventService, IMapper mapper)
        {
            _competitorInEventService = competitorInEventService;
            this._mapper = mapper;
        }

        [HttpGet("{id}", Name = "GetCompetitorsByEventId")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var c = await _competitorInEventService.GetCompetitorById(id);
                if (c == null)
                    return NotFound();

                var competitorsResponse = _mapper.Map<CompetitorDto>(c);
                return Ok(competitorsResponse);
            }
            catch (Exception)
            {
                return StatusCode(500, "Er ging iets mis bij het ophalen van de renner.");
            }
        }

        /// <summary>
        /// Gat a random number of competitors in a list 
        /// </summary>
        /// <param name="id">eventId</param>
        /// <param name="number">#</param>
        /// <returns></returns>
        [HttpGet("{id}/{number}", Name = "GetRandomCompetitorsByEventId")]
        public async Task<IActionResult> GetRandomById(int id, int number)
        {
            try
            {
                var competitors = await _competitorInEventService.GetRandomNumberofCompetitors(id, number);
                if (competitors == null || !competitors.Any())
                    return NotFound($"Geen renners gevonden voor eventId {id}.");

                var competitorsResponse = _mapper.Map<List<CompetitorDto>>(competitors);
                return Ok(competitorsResponse);
            }
            catch (Exception)
            {
                return StatusCode(500, "Er is een fout opgetreden bij het ophalen van de renners.");
            }
        }
    }
}
