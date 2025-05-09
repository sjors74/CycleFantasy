using AutoMapper;
using CycleManager.Services.Interfaces;
using Domain.Dto;
using Microsoft.AspNetCore.Mvc;

namespace WebCycle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeelnemerController : ControllerBase
    {
        private readonly IGameCompetitorInEventService deelnemerService;
        private readonly IResultService resultService;
        private readonly IMapper _mapper;

        public DeelnemerController(IGameCompetitorInEventService deelnemerService, IResultService resultService, IMapper mapper)
        {
            this.deelnemerService = deelnemerService;
            this.resultService = resultService;
            this._mapper = mapper;
        }

        [HttpGet(Name = "deelnemers")]
        public async Task<IActionResult> GetDeelnemerListByEventId(int eventId)
        {
            
            var deelnemers = await deelnemerService.GetAllCompetitorsInEvent(eventId);
            var deelnemerResponse = _mapper.Map<List<DeelnemerDto>>(deelnemers);

            foreach (var deelnemer in deelnemerResponse)
            {
                var score = 0;
                var picks = await deelnemerService.GetPicks(eventId, deelnemer.Id);
                foreach (var pick in picks)
                {
                    var results = await resultService.GetCompetitorResultsByEventId(eventId, pick.CompetitorsInEvent.Id);
                    if(results != null)
                    {
                        score = score + results.TotalScore;
                    }
                }
                deelnemer.Punten = score;
            }
            
            return Ok(deelnemerResponse);
        }

        [HttpGet("results")]
        public async Task<IActionResult> GetResultsByCompetitorEventId([FromQuery] int eventId, [FromQuery] int competitorInEventId)
        {
            var results = await resultService.GetCompetitorResultsByEventId(eventId, competitorInEventId);
            return Ok(results);
        }
    }
}
