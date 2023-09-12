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
            var filteredDeelnemers = deelnemers;
            if (FilterTeam > 0)
            {
                filteredDeelnemers = deelnemers.Where(t => t.TeamId == FilterTeam).ToList();
            }

            var deelnemersViewModel = new List<CompetitorInEventViewModel>();
            foreach(var d in filteredDeelnemers)
            { 
                var cvm = new CompetitorInEventViewModel { 
                        CompetitorId = d.CompetitorId, 
                        EventNumber = GetCompetitorInEvent(eventId, d.CompetitorId).Result.EventNumber,
                        FirstName = d.FirstName, 
                        LastName = d.LastName, 
                        TeamName = GetTeam(d.TeamId).Result.TeamName,
                        CompetitorInEventId = GetCompetitorInEvent(eventId, d.CompetitorId).Result.Id,
                        EventId =eventId,
                        EventName = GetEvent(eventId).Result.EventName,
                        OutOfCompetition = GetCompetitorInEvent(eventId,d.CompetitorId).Result.OutOfCompetition,
                        TeamId = d.TeamId
                };
                deelnemersViewModel.Add(cvm);
            }
            var currentEvent = await _eventService.GetEventById(eventId);
            if (currentEvent != null)
            {
                var vm = new CompetitorsInEventViewModel(deelnemersViewModel, currentEvent.EventName, currentEvent.EventYear, currentEvent.EventId);
                var teams = await _teamService.GetAll();
                vm.Teams = teams.Select(x =>
                                  new SelectListItem()
                                  {
                                      Value = x.TeamId.ToString(),
                                      Text = x.TeamName.ToString()
                                  });
                vm.FilterTeam = FilterTeam == null ? 0 : FilterTeam;

                return View(vm);
            }
            return NotFound();
        }

        // GET: CompetitorsInEvents/Create
        public async Task<IActionResult> Create(int eventId)
        {
            var competitors = _competitorService.GetAllCompetitors();
            var competitorsList = competitors.OrderBy(c => c.FirstName).ToList();
            var teams = await _teamService.GetAll();
            var teamList = teams.OrderBy(c => c.TeamName).ToList();
            ViewBag.ListOfTeams = teamList;
            ViewBag.ListOfCompetitors = competitorsList;
            ViewBag.EventId = eventId;
            return View();
        }

        // POST: CompetitorsInEvents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int eventId, IFormCollection formCollection)
        {
            if (ModelState.IsValid)
            {
                var listOfCompetitorsInEvent = new List<CompetitorsInEvent>();
                foreach(var selectedCompetitorId in formCollection["SelectCompetitorId"])
                {
                    int.TryParse(selectedCompetitorId, out int competitorId);
                    listOfCompetitorsInEvent.Add(new CompetitorsInEvent { CompetitorId = competitorId, EventId = eventId });
                }
                await _competitorInEventService.Create(listOfCompetitorsInEvent);
                return RedirectToAction("Index", new { eventId } );
            }
            ViewData["CompetitorId"] = new SelectList(_competitorService.GetAllCompetitors().OrderBy(c => c.FirstName), "CompetitorId", "CompetitorName");
            ViewData["TeamId"] = new SelectList(await _teamService.GetAll(), "TeamId", "TeamName");
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
            var competitor = await  _competitorService.GetCompetitorById(competitorsInEvent.CompetitorId);
            var vm = GetViewModel(competitorsInEvent);
            vm.TeamId = competitor.TeamId;
            ViewData["CompetitorId"] = new SelectList(_competitorService.GetAllCompetitors().OrderBy(c => c.FirstName), "CompetitorId", "FirstName", competitorsInEvent.CompetitorId);
            ViewData["EventId"] = new SelectList(await _eventService.GetAllEvents(), "EventId", "EventName", competitorsInEvent.EventId);
            return View(vm);
        }

        // POST: CompetitorsInEvents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventNumber", "CompetitorInEventId", "OutOfCompetition","EventId", "TeamId")] CompetitorInEventViewModel vm)
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
                return RedirectToAction(nameof(Index), new { eventId = vm.EventId, FilterTeam = vm.TeamId.ToString() });
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var competitorsInEvent = await _competitorInEventService.GetCompetitorById((int)id);
            if (competitorsInEvent != null)
            {
                await _competitorInEventService.Delete(competitorsInEvent);
                return RedirectToAction(nameof(Index), new { eventId = competitorsInEvent.EventId });
            }
            
            return NotFound();
        }

        private bool CompetitorsInEventExists(int id)
        {
          return (_competitorInEventService.GetCompetitorById(id) != null);
        }

        public async Task<JsonResult> GetCompetitorForEvent(int teamId)
        {
            var competitors = await _competitorService.GetByTeamId(teamId);
            return Json(new SelectList(competitors, "CompetitorId", "CompetitorName"));
        }

        private CompetitorInEventViewModel GetViewModel(CompetitorsInEvent competitorsInEvent)
        {
            var vm = new CompetitorInEventViewModel
            {
                CompetitorId = competitorsInEvent.CompetitorId,
                CompetitorInEventId = competitorsInEvent.Id,
                EventId = competitorsInEvent.EventId,
                TeamName = competitorsInEvent.Competitor.Team != null ? competitorsInEvent.Competitor.Team.TeamName : string.Empty,
                EventNumber = competitorsInEvent.EventNumber,
                FirstName = competitorsInEvent.Competitor.FirstName,
                LastName = competitorsInEvent.Competitor.LastName,
                TeamId = competitorsInEvent.Competitor.TeamId,
                OutOfCompetition = competitorsInEvent.OutOfCompetition
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
