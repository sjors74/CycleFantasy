using AutoMapper;
using CycleManager.Domain.Dto;
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
        private readonly IGameCompetitorInEventService gameCompetitorInEventService;
        private readonly IMapper _mapper;

        public DeelnemerController(IGameCompetitorInEventService deelnemerService, IResultService resultService, IGameCompetitorInEventService gameCompetitorInEventService, IMapper mapper)
        {
            this.deelnemerService = deelnemerService;
            this.resultService = resultService;
            this.gameCompetitorInEventService = gameCompetitorInEventService;
            this.gameCompetitorInEventService = gameCompetitorInEventService;
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
                var picks = await deelnemerService.GetAllPicks(deelnemer.Id);
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

        [HttpGet("Picks/{id}/event/{eventId}")]
        public async Task<IActionResult> GetListOfCompetitorsPicksForDeelnemer(int id, int eventId)
        {
            var picks = await deelnemerService.GetAllPicks(id);
            var competitorResponse = _mapper.Map<List<ResultDto>>(picks);

            foreach (var pick in competitorResponse)
            {
                var score = 0;
                var results = await resultService.GetCompetitorResultsByEventId(eventId, pick.CompetitorInEventId);
                if (results != null)
                {
                    score = score + results.TotalScore;
                }
                pick.Points = score;
                
            }

            return Ok(competitorResponse);
        }
    }
}
