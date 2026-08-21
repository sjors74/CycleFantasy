using AutoMapper;
using CycleManager.Domain.Dto;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace WebCycle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeelnemerController : ControllerBase
    {
        private readonly IGameCompetitorInEventService deelnemerService;
        private readonly IEventService _eventService;
        private readonly IResultService resultService;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public DeelnemerController(IGameCompetitorInEventService deelnemerService, IEventService eventService, IResultService resultService, IMapper mapper, IMemoryCache cache)
        {
            this.deelnemerService = deelnemerService;
            this.resultService = resultService;
            _eventService = eventService;
            _mapper = mapper;
            _cache = cache;
        }

        [HttpGet(Name = "deelnemers")]
        public async Task<IActionResult> GetDeelnemerListByEventId(int eventId)
        {
            string cacheKey = $"deelnemers_{eventId}";

            if (!_cache.TryGetValue(cacheKey, out List<DeelnemerDto> deelnemerResponse))
            {
                var deelnemers = await deelnemerService.GetAllCompetitorsInEvent(eventId) ?? new List<GameCompetitorEvent>();
                deelnemerResponse = _mapper.Map<List<DeelnemerDto>>(deelnemers) ?? new List<DeelnemerDto>();

                foreach (var deelnemer in deelnemerResponse)
                {
                    int normaal = 0;
                    int specials = 0;

                    var picks = await deelnemerService.GetAllPicks(deelnemer.Id) ?? new List<GameCompetitorEventPick>();

                    foreach (var pick in picks)
                    {
                        var results = await resultService.GetCompetitorResultsByEventId(eventId, pick.CompetitorsInEvent.Id);
                        if (results != null)
                        {

                            normaal += results.NormalScore;
                            specials += results.SpecialScore;
                        }
                    }

                    deelnemer.NormalePunten = normaal;
                    deelnemer.SpecialePunten = specials;
                    deelnemer.Punten = normaal + specials;
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
            if (picks == null)
                return Ok(new List<CompetitorRankingDto>());

            var competitorResponse = _mapper.Map<List<CompetitorRankingDto>>(picks);

            var pickDetails = await resultService.GetPickDetailsAsync(eventId, id);

            var scoreLookup = pickDetails.ToDictionary(p => p.CompetitorInEventId);

            foreach (var pick in competitorResponse)
            {
                if(scoreLookup.TryGetValue(pick.CompetitorInEventId, out var score))
                {
                    pick.NormalPoints = score.NormalScore;
                    pick.SpecialPoints = score.SpecialScore;
                    pick.Points = score.TotalScore;
                    pick.LatestPoints = score.LastScore;
                    pick.Specials = score.Specials;
                }
                else
                {
                    pick.NormalPoints = 0;
                    pick.SpecialPoints = 0;
                    pick.Points = 0;
                    pick.LatestPoints = 0;
                    pick.Specials = [];
                }
            }

            return Ok(competitorResponse);
        }

        [HttpGet("DeelnemerMetPicks")]
        public async Task<IActionResult> GetDeelnemersMetPicks(int eventId)
        {
            var currentEvent = await _eventService.GetEventById(eventId);
            var result = new List<DeelnemerMetPicksDto>();

            if(currentEvent?.GameCompetitorEvents == null)
            {
                return Ok(result);
            }

            foreach (var deelnemer in currentEvent.GameCompetitorEvents)
            {
                var picks = await deelnemerService.GetAllPicks(deelnemer.Id);
                var picksDto = _mapper.Map<List<CompetitorRankingDto>>(picks ?? new List<GameCompetitorEventPick>());

                foreach (var pick in picksDto)
                {
                    var results = await resultService.GetCompetitorResultsByEventId(
                        eventId,
                        pick.CompetitorInEventId);

                    if (results != null)
                    {
                        pick.NormalPoints = results.NormalScore;
                        pick.SpecialPoints = results.SpecialScore;

                        // oude property behouden indien ergens nog gebruikt
                        pick.Points = results.TotalScore;
                    }
                    else
                    {
                        pick.NormalPoints = 0;
                        pick.SpecialPoints = 0;
                        pick.Points = 0;
                    }
                }

                result.Add(new DeelnemerMetPicksDto
                {
                    Id = deelnemer.Id,
                    DeelnemerNaam = $"{deelnemer?.User?.FirstName} {deelnemer?.User?.LastName}",
                    PoolNaam = deelnemer.TeamName,
                    UserId = deelnemer.UserId,
                    Picks = picksDto
                });
            }

            return Ok(result);
        }

        [HttpGet("Deelnemer/MetPunten/{eventId}")]
        public async Task<IActionResult> GetDeelnemersMetPuntenVoorEvent(int eventId)
        {
            var currentEvent = await _eventService.GetEventById(eventId);
            var result = new List<DeelnemerDto>();

            if (currentEvent?.GameCompetitorEvents == null)
                return Ok(result);

            foreach (var deelnemer in currentEvent.GameCompetitorEvents)
            {
                var picks = await deelnemerService.GetAllPicks(deelnemer.Id)
                    ?? new List<GameCompetitorEventPick>();

                int normalPoints = 0;
                int specialPoints = 0;

                foreach (var pick in picks)
                {
                    var score = await resultService
                        .GetCompetitorResultsByEventId(eventId, pick.CompetitorsInEventId);

                    if (score != null)
                    {
                        normalPoints += score.NormalScore;
                        specialPoints += score.SpecialScore;
                    }
                }

                var dto = _mapper.Map<DeelnemerDto>(deelnemer);

                dto.NormalePunten= normalPoints;
                dto.SpecialePunten = specialPoints;
                dto.Punten = normalPoints + specialPoints;

                result.Add(dto);
            }

            return Ok(result);
        }

        [HttpGet("Picks/{deelnemerId}")]
        public async Task<IActionResult> GetPicksForDeelnemer(int deelnemerId)
        {
            var picks = await deelnemerService.GetAllPicksAsCompetitorIds(deelnemerId) ?? new List<int>();
            var picksDto = _mapper.Map<List<int>>(picks) ?? new List<int>();
            return Ok(picksDto);
        }

        [HttpPut("renamepool")]
        public async Task<IActionResult> RenamePool([FromBody] RenamePoolDto dto)
        {
            var success = await deelnemerService.RenamePoolAsync(dto);
            if (!success)
                BadRequest();

            return Ok();        }
    }
}
