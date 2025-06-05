using AutoMapper;
using CycleManager.Domain.Dto;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Domain.Dto;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebCycle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventRepository eventRepository;
        private readonly IEventService eventService;
        private readonly IResultService _resultService;
        private readonly IGameCompetitorInEventService _deelnemerService;
        private readonly IMapper _mapper;

        public EventController(IEventRepository eventRepository, IEventService eventService, IGameCompetitorInEventService deelnemerService, IResultService resultService, IMapper mapper)
        {
            this.eventRepository = eventRepository;
            this.eventService = eventService;
            _deelnemerService = deelnemerService;
            _resultService = resultService;
            this._mapper = mapper;
        }

        [HttpGet(Name = "GetActiveEvent")]
        public async Task<IActionResult> GetEvent()
        {
            var events = await eventRepository.GetActiveEvents();
            var eventResponse = _mapper.Map<List<EventDto>>(events);
            foreach(var cEvent in eventResponse)
            {
                if (cEvent.Deelnemers != null)
                {
                    foreach (var deelnemer in cEvent.Deelnemers)
                    {
                        var picks = await _deelnemerService.GetAllPicks(deelnemer.Id);
                        int totaal = 0;

                        foreach (var pick in picks)
                        {
                            var results = await _resultService.GetCompetitorResultsByEventId(cEvent.EventId, pick.CompetitorsInEventId);
                            if (results != null)
                                totaal += results.TotalScore;
                        }

                        deelnemer.Punten = totaal;
                    }
                }
            }
            return Ok(eventResponse);
        }

        [HttpGet("{id}", Name = "GetEventById")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var e = await eventService.GetEventById(id);
            var eventResponse = _mapper.Map<EventDto>(e);

            foreach(var deelnemer in eventResponse.Deelnemers)
            {
                var picks = await _deelnemerService.GetAllPicks(deelnemer.Id);
                int totaal = 0;

                foreach(var pick in picks)
                {
                    var results = await _resultService.GetCompetitorResultsByEventId(id, pick.CompetitorsInEventId);
                    if (results != null)
                        totaal += results.TotalScore;
                }

                deelnemer.Punten = totaal;
            }

                return Ok(eventResponse);
        }

        [HttpGet("{id}/stages")]
        public async Task<IActionResult> GetStagesByEventId(int id)
        {
            var stages = await eventService.GetStagesWithResultsForEvent(id);
            return Ok(stages);
        }

        [HttpGet("{userid}/user")]
        public async Task<IActionResult> GetEventByUserId(string userid)
        {
            //TODO: check this one
           // var allEvents = await eventService.GetAllEvents();
            var allEventsForUser = await eventService.GetEventsByUserId(userid);
            var nu = DateTime.UtcNow;

            var active = allEventsForUser
                .Where(e => e.StartDate <= nu && e.EndDate >= nu)
                .ToList();
            var future = allEventsForUser
                .Where(e => e.StartDate > nu)
                .ToList();
            var historic = allEventsForUser
            
                .Where(e => e.EndDate < nu)
                .ToList();

            var activeDtos = _mapper.Map<List<EventForUserDto>>(active);
            var futureDtos = _mapper.Map<List<EventForUserDto>>(future);
            var historicDtos = _mapper.Map<List<EventForUserDto>>(historic);

            activeDtos.ForEach(e => e.UserId = userid);
            futureDtos.ForEach(e => e.UserId = userid);
            historicDtos.ForEach(e => e.UserId = userid);

            var result = new EventViewDto
            {
                ActieveEvenementen = activeDtos,
                ToekomstigeEvenementen = futureDtos,
                HistorischeEvenementen = historicDtos
            };
          
            return Ok(result);
        }

        [HttpGet("{id}/teams-with-renners")]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetTeamsWithRennersForEvent(int id)
        {
            var eventExists = await eventService.GetEventById(id);
            if (eventExists == null)
            {
                return NotFound();
            }

            var teams = await eventService.GetTeamsForEvent(id);

            if(teams == null)
            {
                return NotFound();
            }
            return Ok(teams);
        }

        [HttpPost("selectie")]
        public async Task<IActionResult> SlaSelectieOp([FromBody] SelectieDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Selectie dto is null.");
            }

            try
            {
                await eventService.SaveSelectie(dto);
            }
            catch
            {
                return BadRequest("Er ging iets mis met het opslaan van je pool.");
            }
            return Ok();
        }

        [HttpPost("createpool")]
        public async Task<IActionResult> CreatePool([FromBody] DeelnemerDto dto)
        {
            if(dto == null)
            {
                return BadRequest("Deelnemer dto is null.");
            }

            var result = await eventService.CreatePoolAsync(dto);

            if (result != null && result.Id > 0)
            {
                return Ok(result);
            }

            return BadRequest("Er is iets misgegaan bij het aanmaken van de pool.");
        }

    }
}
