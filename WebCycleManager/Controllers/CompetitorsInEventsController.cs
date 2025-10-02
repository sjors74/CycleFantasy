using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class CompetitorsInEventsController : Controller
    {
        private ICompetitorInEventService _competitorInEventService;
        private IEventService _eventService;
        private ITeamService _teamService;
        private ICompetitorService _competitorService;

        public CompetitorsInEventsController(ICompetitorInEventService competitorInEventService, IEventService eventService, ITeamService teamService, ICompetitorService competitorService)
        {
            _competitorInEventService = competitorInEventService;
            _eventService = eventService;
            _teamService = teamService;
            _competitorService = competitorService;
        }

        // GET: CompetitorsInEvents
        public async Task<IActionResult> Index(int eventId, int? FilterTeam = 0)
        {
            var deelnemers = await _competitorInEventService.GetCompetitors(eventId);

            if (FilterTeam > 0)
            {
                deelnemers = deelnemers
                    .Where(t => t.CompetitorInTeam.Team.TeamId == FilterTeam)
                    .ToList();
            }
            var currentEvent = await _eventService.GetEventById(eventId);
            if (currentEvent == null) return NotFound();

            var deelnemersViewModel = deelnemers.Select(d =>
            {
                var competitor = d.CompetitorInTeam.Competitor;
                var team = d.CompetitorInTeam.Team;

                return new CompetitorInEventViewModel
                {
                    CompetitorId = d.CompetitorInTeamId,
                    EventNumber = d.EventNumber,
                    FirstName = competitor?.FirstName ?? "",
                    LastName = competitor?.LastName ?? "",
                    TeamName = team?.CurrentTeamName ?? "onbekend",
                    CompetitorInEventId = d.Id,
                    EventId = d.EventId,
                    EventName = currentEvent.EventName,
                    OutOfCompetition = d.OutOfCompetition,
                    InSelection = d.InSelectie,
                    TeamId = team?.TeamId ?? 0
                };
            })
            .OrderBy(x => x.EventNumber)
            .ThenBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToList();

            var teams = await _teamService.GetTeamsForEvent(eventId);
            var teamList = teams.OrderBy(c => c.CurrentTeamName).ToList();

            var vm = new CompetitorsInEventViewModel(
                deelnemersViewModel,
                currentEvent.EventName,
                currentEvent.EventYear,
                currentEvent.EventId)
            {
                Teams = teamList.Select(t => new SelectListItem()
                {
                    Value = t.TeamId.ToString(),
                    Text = t.CurrentTeamName
                }),
                FilterTeam = FilterTeam ?? 0
            };
            return View(vm);
        }

        // GET: CompetitorsInEvents/Create
        public async Task<IActionResult> Create(int eventId, int? filterTeam)
        {
            var competitors = await _competitorService.GetAllCompetitors(DateTime.Now.Year);
            if(filterTeam.HasValue && filterTeam.Value > 0)
            {
                competitors = competitors.Where(c => c.Teams.Any(t => t.TeamId == filterTeam.Value)).ToList();
            }

            var competitorsList = competitors
                .SelectMany(c => c.Teams.Select(t => new
                {
                    CompetitorId = t.CompetitorInTeamId,
                    CompetitorName = c.CompetitorName
                }))
                .OrderBy(x => x.CompetitorName)
                .ToList();

            ViewBag.ListOfCompetitors = new SelectList(competitorsList, "CompetitorId", "CompetitorName");
            var teams = await _teamService.GetTeamsForEvent(eventId);
            var teamList = teams.OrderBy(c => c.CurrentTeamName).ToList();
            ViewBag.ListOfTeams = teamList;
            ViewBag.EventId = eventId;
            return View();
        }

        // POST: CompetitorsInEvents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int eventId, int? filterTeam, IFormCollection formCollection)
        {
            if (ModelState.IsValid)
            {
                var listOfCompetitorsInEvent = new List<CompetitorsInEvent>();
                foreach(var selectedCompetitorId in formCollection["SelectCompetitorId"])
                {
                    int.TryParse(selectedCompetitorId, out int competitorId);
                    listOfCompetitorsInEvent.Add(new CompetitorsInEvent { CompetitorInTeamId = competitorId, EventId = eventId });
                }
                await _competitorInEventService.Create(listOfCompetitorsInEvent);
                return RedirectToAction("Index", new { eventId, filterTeam } );
            }
            var competitors = (await _competitorService.GetAllCompetitors(DateTime.Now.Year))
                .OrderBy(c => c.CompetitorName)
                .ToList();

            if (filterTeam.HasValue && filterTeam.Value > 0)
            {
                competitors = competitors.Where(c => c.Teams.Any(t => t.TeamId == filterTeam.Value)).ToList();
            }

            ViewData["CompetitorId"] = new SelectList(competitors, "CompetitorId", "CompetitorName");
            ViewData["TeamId"] = new SelectList(await _teamService.GetTeamsForEvent(eventId), "TeamId", "TeamName");
            return View();
        }

        // GET: CompetitorsInEvents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var competitorsInEvent = await _competitorInEventService.GetCompetitorById((int)id);
            if (competitorsInEvent == null)
            {
                return NotFound();
            }
            var competitor = await  _competitorService.GetCompetitorById(competitorsInEvent.CompetitorInTeamId);
            var vm = GetViewModel(competitorsInEvent);
            var team = competitor?.CompetitorInTeams.FirstOrDefault()?.Team;
            vm.TeamId = team?.TeamId ?? 0;
            ViewData["EventId"] = new SelectList(await _eventService.GetAllEvents(), "EventId", "EventName", competitorsInEvent.EventId);
            return View(vm);
        }

        // POST: CompetitorsInEvents/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int?filterTeam, CompetitorInEventViewModel vm)
        {
            if (id != vm.CompetitorInEventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var competitorInEvent = await _competitorInEventService.GetCompetitorById(id);
                    if (competitorInEvent == null) 
                    { 
                        return NotFound(); 
                    }
                    competitorInEvent.EventNumber = vm.EventNumber;
                    competitorInEvent.OutOfCompetition = vm.OutOfCompetition;
                    competitorInEvent.InSelectie = vm.InSelection;
                    await _competitorInEventService.Update(competitorInEvent);
                }
                catch
                {
                    if (!CompetitorsInEventExists(vm.CompetitorInEventId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { eventId = vm.EventId, FilterTeam = filterTeam });
            }
            return View(vm);
        }

        // GET: CompetitorsInEvents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var competitorsInEvent = await _competitorInEventService.GetCompetitorById((int)id);
            if (competitorsInEvent == null)
            {
                return NotFound();
            }

            var vm = GetViewModel(competitorsInEvent);
            return View(vm);
        }

        // POST: CompetitorsInEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int? filterTeam)
        {
            var competitorsInEvent = await _competitorInEventService.GetCompetitorById((int)id);
            if (competitorsInEvent != null)
            {
                await _competitorInEventService.Delete(competitorsInEvent);
                return RedirectToAction(nameof(Index), new { eventId = competitorsInEvent.EventId, filterTeam });
            }
            
            return NotFound();
        }

        private bool CompetitorsInEventExists(int id)
        {
          return (_competitorInEventService.GetCompetitorById(id) != null);
        }

        [HttpGet]
        public async Task<IActionResult> GetCompetitorForEvent(int teamId, int year)
        {
            var competitors = await _competitorService.GetByTeamId(teamId, year);
            var result = competitors
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .Select(c => new
            {
                value = c.CompetitorInTeamId,
                text = $"{c.LastName}, {c.FirstName}"
            });
            return Json(result);
        }

        private CompetitorInEventViewModel GetViewModel(CompetitorsInEvent competitorsInEvent)
        {
            var competitor = competitorsInEvent.CompetitorInTeam.Competitor;

            // Kies het eerste team (of filter op een specifiek jaar)
            var team = competitor?.CompetitorInTeams.FirstOrDefault()?.Team;

            var vm = new CompetitorInEventViewModel
            {
                CompetitorId = competitorsInEvent.CompetitorInTeamId,
                CompetitorInEventId = competitorsInEvent.Id,
                EventId = competitorsInEvent.EventId,
                TeamName = team?.CurrentTeamName ?? string.Empty,
                EventNumber = competitorsInEvent.EventNumber,
                FirstName = competitor?.FirstName ?? string.Empty,
                LastName = competitor?.LastName ?? string.Empty,
                TeamId = team?.TeamId ?? 0,
                OutOfCompetition = competitorsInEvent.OutOfCompetition,
                InSelection = competitorsInEvent.InSelectie
            };

            return vm;
        }

        private async Task<Competitor> GetCompetitor(int id)
        {
            return await _competitorService.GetCompetitorById(id);
        }

        public async Task<CompetitorsInEvent> GetCompetitorInEvent(int eventId, int competitorId)
        {
            var c = await _competitorInEventService.GetCompetitorsInEventByIds(eventId, competitorId);
            return c;
        }

        private async Task<Team> GetTeam(int id)
        {
            var t = await _teamService.GetTeamById(id);
            return t;
        }

        public async Task<Event> GetEvent(int id)
        {
            var e = await _eventService.GetEventById(id);
            return e;
        }
    }
}
