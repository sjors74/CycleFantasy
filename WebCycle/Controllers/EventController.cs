using AutoMapper;
using CycleManager.Domain.Dto;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Domain.Dto;
using Microsoft.AspNetCore.Mvc;

namespace WebCycle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IResultService _resultService;
        private readonly IGameCompetitorInEventService _deelnemerService;
        private readonly ITeamService _teamService;
        private readonly IMapper _mapper;

        public EventController(IEventService eventService, IGameCompetitorInEventService deelnemerService, ITeamService teamService, IResultService resultService, IMapper mapper)
        {
            _eventService = eventService;
            _deelnemerService = deelnemerService;
            _resultService = resultService;
            _teamService = teamService;
            this._mapper = mapper;
        }

        [HttpGet(Name = "GetActiveEvent")]
        public async Task<IActionResult> GetEvent()
        {
            var events = await _eventService.GetActiveEvents();
            var eventResponse = _mapper.Map<List<EventDto>>(events);

            foreach(var cEvent in eventResponse)
            {
                if(cEvent.Deelnemers != null)
                {
                    var deelnemerScores = await _resultService.GetScoresByEventIdAsync(cEvent.EventId);

                    foreach(var deelnemer in cEvent.Deelnemers)
                    {
                        var scoresForDeelnemer = deelnemerScores
                            .Where(s => s.GameCompetitorEventId == deelnemer.Id)
                            .ToList();

                        var last = scoresForDeelnemer
                            .OrderByDescending(s => s.StageId)
                            .FirstOrDefault();

                        deelnemer.Punten = last?.TotalScore ?? 0;
                        deelnemer.LaatsteScore = last?.LaatsteScore ?? 0;
                    }
                }
            }

            return Ok(eventResponse);
        }

        [HttpGet("{id}", Name = "GetEventById")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var e = await _eventService.GetEventById(id);
            if(e == null)
            {
                return NotFound();
            }
            var eventResponse = _mapper.Map<EventDto>(e);

            foreach (var deelnemer in eventResponse.Deelnemers)
            {
                var picks = await _deelnemerService.GetAllPicks(deelnemer.Id);
                int totaal = 0;

                foreach (var pick in picks)
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
            var stages = await _eventService.GetStagesWithResultsForEvent(id);
            return Ok(stages);
        }

        [HttpGet("{userid}/user")]
        public async Task<IActionResult> GetEventByUserId(string userid)
        {
            var allEvents = await _eventService.GetAllEvents();
            var allEventsForUser = await _eventService.GetEventsByUserId(userid);
            var nu = DateTime.UtcNow;

            var active = allEventsForUser
                .Where(e => e.StartDate <= nu && e.EndDate >= nu)
                .ToList();
            var future = allEvents
                .Where(e => e.StartDate > nu)
                .ToList();
            var futureWithUser = allEventsForUser
                .Where(e => e.StartDate > nu)
                .ToList();
            var historic = allEventsForUser
                .Where(e => e.EndDate < nu)
                .ToList();

            var futureAllDtos = _mapper.Map<List<EventForUserDto>>(future);
            var userIds = new HashSet<int>(futureWithUser.Select(e => e.EventId));

            futureWithUser.ForEach(e =>
            {
                e.UserId = userid;
                e.IsIngeschreven = true;
            });

            var notIngeschrevenDtos = futureAllDtos
                .Where(e => !userIds.Contains(e.EventId))
                .ToList();

            notIngeschrevenDtos.ForEach(e =>
            {
                e.UserId = userid;
                e.IsIngeschreven = false;
            });

            var futureDtos = futureWithUser
                .Concat(notIngeschrevenDtos)
                .ToList();

            active.ForEach(e => e.UserId = userid);
            historic.ForEach(e => e.UserId = userid);

            var result = new EventViewDto
            {
                ActieveEvenementen = active,
                ToekomstigeEvenementen = futureDtos,
                HistorischeEvenementen = historic
            };

            return Ok(result);
        }

        [HttpGet("{id}/teams-with-renners")]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetTeamsWithRennersForEvent(int id)
        {
            var eventExists = await _eventService.GetEventById(id);
            if (eventExists == null)
            {
                return NotFound();
            }

            var teams = await _eventService.GetTeamsForEvent(id);

            if (teams == null)
            {
                return NotFound();
            }
            return Ok(teams);
        }


        [HttpGet("team/{teamId}/teams-with-more-renners")]
        public async Task<ActionResult<IEnumerable<CompetitorDto>>> GetTeamsWithRennersFromTeam(int teamId)
        {
            var year = DateTime.Now.Year;
            var team = await _teamService.GetTeamForCurrentYear(teamId, year);

            if (team == null)
            {
                return NotFound();
            }

            // Haal competitors via CompetitorInTeams
            var competitors = team.CompetitorInTeams
                .Where(cit => cit.Year == year)
                .Select(cit => new CompetitorInSelectieDto
                {
                    CompetitorInTeamId = cit.Id,
                    FirstName = cit.Competitor.FirstName,
                    LastName = cit.Competitor.LastName,
                    PcsName = cit.Competitor.PcsName
                }).ToList();

            return Ok(competitors);
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
                await _eventService.SaveSelectie(dto);
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
            if (dto == null)
            {
                return BadRequest("Deelnemer dto is null.");
            }

            var result = await _eventService.CreatePoolAsync(dto);

            if (result != null && result.Id > 0)
            {
                return Ok(result);
            }

            return BadRequest("Er is iets misgegaan bij het aanmaken van de pool.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeelnemer(int id)
        {
            try
            {
                await _eventService.DeletePoolAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Fout bij verwijderen deelnemer.");
            }
        }

        [HttpGet("{id}/deelnemers")]
        public async Task<int> GetDeelnemersAantal(int id)
        {
            return await _eventService.GetAantalDeelnemers(id);
        }
    }
}
