using AutoMapper;
using CycleManager.Domain.Dto;
using CycleManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace WebCycle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeelnemerController : ControllerBase
    {
        private readonly IGameCompetitorInEventService deelnemerService;
        private readonly IResultService resultService;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public DeelnemerController(IGameCompetitorInEventService deelnemerService, IResultService resultService, IMapper mapper, IMemoryCache cache)
        {
            this.deelnemerService = deelnemerService;
            this.resultService = resultService;
            _mapper = mapper;
            _cache = cache;
        }

        [HttpGet(Name = "deelnemers")]
        public async Task<IActionResult> GetDeelnemerListByEventId(int eventId)
        {
            string cacheKey = $"deelnemers_{eventId}";

            if (!_cache.TryGetValue(cacheKey, out List<DeelnemerDto> deelnemerResponse))
            {
                var deelnemers = await deelnemerService.GetAllCompetitorsInEvent(eventId);
                deelnemerResponse = _mapper.Map<List<DeelnemerDto>>(deelnemers);

                foreach (var deelnemer in deelnemerResponse)
                {
                    int score = 0;
                    var picks = await deelnemerService.GetAllPicks(deelnemer.Id);

                    foreach (var pick in picks)
                    {
                        var results = await resultService.GetCompetitorResultsByEventId(eventId, pick.CompetitorsInEvent.Id);
                        if (results != null)
                        {
                            score += results.TotalScore;
                        }
                    }

                    deelnemer.Punten = score;
                }
                _cache.Set(cacheKey, deelnemerResponse, TimeSpan.FromHours(24));
            }
            return Ok(deelnemerResponse);
        }

        [HttpPost("invalidate-cache")]
        public IActionResult InvalidateDeelnemerCache(int eventId)
        {
            string cacheKey = $"deelnemers_{eventId}";
            _cache.Remove(cacheKey);
            return Ok();
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
