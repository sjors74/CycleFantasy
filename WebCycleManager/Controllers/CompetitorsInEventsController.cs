using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class CompetitorsInEventsController : Controller
    {
        private ICompetitorsInEventRepository _competitorsInEventRepository;
        private ICompetitorRepository _competitorRepository;
        private IEventRepository _eventRepository;
        private ITeamRepository _teamRepository;

        public CompetitorsInEventsController(ICompetitorsInEventRepository competitorsInEventRepository, ICompetitorRepository competitorRepository, IEventRepository eventRepository,
            ITeamRepository teamRepository)
        {
            _competitorsInEventRepository = competitorsInEventRepository;
            _competitorRepository = competitorRepository;   
            _eventRepository = eventRepository;
            _teamRepository = teamRepository;
        }

        // GET: CompetitorsInEvents
        public async Task<IActionResult> Index(int eventId, string FilterTeam = "")
        {
            var deelnemers = await _competitorsInEventRepository.GetCompetitors(eventId);
            var filteredDeelnemers = deelnemers;
            if (!string.IsNullOrEmpty(FilterTeam))
            {
                filteredDeelnemers = deelnemers.Where(t => t.TeamId.ToString() == FilterTeam).ToList();
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
                        CompetitorInEventId = GetCompetitorInEvent(eventId, d.CompetitorId).Result.CompetitorInEventId,
                        EventId =eventId,
                        EventName = GetEvent(eventId).Result.EventName,
                        TeamId = d.TeamId
                };
                deelnemersViewModel.Add(cvm);
            }
            var currentEvent = await _eventRepository.GetById(eventId);
            if (currentEvent != null)
            {
                var vm = new CompetitorsInEventViewModel(deelnemersViewModel, currentEvent.EventName, currentEvent.EventYear, currentEvent.EventId);
                var teams = await _teamRepository.GetAll();
                vm.Teams = teams.Select(x =>
                                  new SelectListItem()
                                  {
                                      Value = x.TeamId.ToString(),
                                      Text = x.TeamName.ToString()
                                  });
                vm.FilterTeam = FilterTeam == null ? string.Empty : FilterTeam;

                return View(vm);
            }
            return NotFound();
        }

        // GET: CompetitorsInEvents/Create
        public async Task<IActionResult> Create(int eventId)
        {
            var competitors = await _competitorRepository.GetAll();
            var competitorsList = competitors.OrderBy(c => c.FirstName).ToList();
            var teams = await _teamRepository.GetAll();
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
                _competitorsInEventRepository.AddRange(listOfCompetitorsInEvent);
                await _competitorsInEventRepository.SaveChangesAsync();
                return RedirectToAction("Index", new { eventId } );
            }
            ViewData["CompetitorId"] = new SelectList(_competitorRepository.GetAllCompetitors().OrderBy(c => c.FirstName), "CompetitorId", "CompetitorName");
            ViewData["TeamId"] = new SelectList(await _teamRepository.GetAll(), "TeamId", "TeamName");
            return View();
        }

        // GET: CompetitorsInEvents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var competitorsInEvent = await _competitorsInEventRepository.GetById((int)id);
            if (competitorsInEvent == null)
            {
                return NotFound();
            }
            var competitor = await  _competitorRepository.GetById(competitorsInEvent.CompetitorId);
            var vm = GetViewModel(competitorsInEvent);
            vm.TeamId = competitor.TeamId;
            ViewData["CompetitorId"] = new SelectList(_competitorRepository.GetAllCompetitors().OrderBy(c => c.FirstName), "CompetitorId", "FirstName", competitorsInEvent.CompetitorId);
            ViewData["EventId"] = new SelectList(await _eventRepository.GetAll(), "EventId", "EventName", competitorsInEvent.EventId);
            return View(vm);
        }

        // POST: CompetitorsInEvents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventNumber", "CompetitorInEventId", "EventId", "TeamId")] CompetitorInEventViewModel vm)
        {
            if (id != vm.CompetitorInEventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var competitorInEvent = await _competitorsInEventRepository.GetById(id);
                    if (competitorInEvent == null) 
                    { 
                        return NotFound(); 
                    }
                    competitorInEvent.EventNumber = vm.EventNumber;
                    _competitorsInEventRepository.Update(competitorInEvent);
                    await _competitorsInEventRepository.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
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

            var competitorsInEvent = await _competitorsInEventRepository.GetById((int)id);
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
            var competitorsInEvent = await _competitorsInEventRepository.GetById((int)id);
            if (competitorsInEvent != null)
            {
                _competitorsInEventRepository.Remove(competitorsInEvent);
                await _competitorsInEventRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { eventId = competitorsInEvent.EventId });
            }
            
            return NotFound();
        }

        private bool CompetitorsInEventExists(int id)
        {
          return (_competitorsInEventRepository.GetById(id) != null);
        }

        public async Task<JsonResult> GetCompetitorForEvent(int teamId)
        {
            var competitors = await _competitorRepository.GetByTeamId(teamId);
            return Json(new SelectList(competitors, "CompetitorId", "CompetitorName"));
        }

        private CompetitorInEventViewModel GetViewModel(CompetitorsInEvent competitorsInEvent)
        {
            var vm = new CompetitorInEventViewModel
            {
                CompetitorId = competitorsInEvent.CompetitorId,
                CompetitorInEventId = competitorsInEvent.CompetitorInEventId,
                EventId = competitorsInEvent.EventId,
                TeamName = competitorsInEvent.Competitor.Team != null ? competitorsInEvent.Competitor.Team.TeamName : string.Empty,
                EventNumber = competitorsInEvent.EventNumber,
                FirstName = competitorsInEvent.Competitor.FirstName,
                LastName = competitorsInEvent.Competitor.LastName,
                TeamId = competitorsInEvent.Competitor.TeamId
            };
            return vm;
        }

        private async Task<Competitor> GetCompetitor(int id)
        {
            return await _competitorRepository.GetById(id);
        }

        public async Task<CompetitorsInEvent> GetCompetitorInEvent(int eventId, int competitorId)
        {
            var c = await _competitorsInEventRepository.GetCompetitorsInEventByIds(eventId, competitorId);
            return c;
        }

        private async Task<Team> GetTeam(int id)
        {
            var t = await _teamRepository.GetById(id);
            return t;
        }

        public async Task<Event> GetEvent(int id)
        {
            var e = await _eventRepository.GetById(id);
            return e;
        }
    }
}
