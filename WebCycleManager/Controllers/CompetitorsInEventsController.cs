using Domain.Context;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class CompetitorsInEventsController : Controller
    {
        private readonly DatabaseContext _context;

        public CompetitorsInEventsController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: CompetitorsInEvents
        public async Task<IActionResult> Index(int eventId, string FilterTeam = null)
        {
            var deelnemers = _context.CompetitorsInEvent
                .Include(c => c.Competitor)
                .Include(c => c.Event)
                .Where(c => c.EventId.Equals(eventId))
                .OrderBy(c => c.EventNumber)
                .ToList();

            var filteredDeelnemers = deelnemers;
            if (FilterTeam != null)
            {
                filteredDeelnemers = deelnemers.Where(t => t.Competitor.Team.TeamId.ToString() == FilterTeam).ToList();
            }

            var deelnemersViewModel = new List<CompetitorInEventViewModel>();
            foreach(var d in filteredDeelnemers)
            { 
                var cvm = new CompetitorInEventViewModel { 
                        CompetitorId = d.CompetitorId, 
                        EventNumber = d.EventNumber, 
                        FirstName = d.Competitor.FirstName, 
                        LastName = d.Competitor.LastName, 
                        TeamName = d.Competitor.Team.TeamName,
                        CompetitorInEventId = d.CompetitorInEventId,
                        EventId = d.EventId,
                        EventName = d.Event.EventName,
                        TeamId = d.Competitor.TeamId
                };
                deelnemersViewModel.Add(cvm);
            }
            var currentEvent = _context.Events.FirstOrDefault(e => e.EventId.Equals(eventId));
            if (currentEvent != null)
            {
                var vm = new CompetitorsInEventViewModel(deelnemersViewModel, currentEvent.EventName, currentEvent.EventYear, currentEvent.EventId);
                vm.Teams =_context.Teams.Select(x =>
                                  new SelectListItem()
                                  {
                                      Value = x.TeamId.ToString(),
                                      Text = x.TeamName.ToString()
                                  });
                vm.FilterTeam = FilterTeam;

                return View(vm);
            }
            return NotFound();
        }

        // GET: CompetitorsInEvents/Create
        public IActionResult Create(int eventId)
        {
            var competitors = _context.Competitors.OrderBy(c => c.FirstName);
            var teams = _context.Teams.OrderBy(c => c.TeamName);
            ViewBag.ListOfTeams = teams;
            ViewBag.ListOfCompetitors = competitors;
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

                _context.AddRange(listOfCompetitorsInEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { eventId } );
            }
            ViewData["CompetitorId"] = new SelectList(_context.Competitors.OrderBy(c => c.FirstName), "CompetitorId", "CompetitorName");
            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "TeamName");
            return View();
        }

        // GET: CompetitorsInEvents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.CompetitorsInEvent == null)
            {
                return NotFound();
            }

            var competitorsInEvent = await _context.CompetitorsInEvent.FindAsync(id);
            if (competitorsInEvent == null)
            {
                return NotFound();
            }
            var vm = new CompetitorInEventViewModel
            {
                CompetitorId = competitorsInEvent.CompetitorId,
                CompetitorInEventId = competitorsInEvent.CompetitorInEventId,
                EventId = competitorsInEvent.EventId,
                TeamName = competitorsInEvent.Competitor.Team.TeamName,
                EventNumber = competitorsInEvent.EventNumber,
                FirstName = competitorsInEvent.Competitor.FirstName,
                LastName = competitorsInEvent.Competitor.LastName,
                TeamId = competitorsInEvent.Competitor.TeamId
            };
            ViewData["CompetitorId"] = new SelectList(_context.Competitors, "CompetitorId", "FirstName", competitorsInEvent.CompetitorId);
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", competitorsInEvent.EventId);
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
                    var competitorInEvent = await _context.CompetitorsInEvent.FindAsync(id);
                    if (competitorInEvent == null) 
                    { 
                        return NotFound(); 
                    }
                    competitorInEvent.EventNumber = vm.EventNumber;
                    _context.Update(competitorInEvent);
                    await _context.SaveChangesAsync();
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
            //ViewData["CompetitorId"] = new SelectList(_context.Competitors, "CompetitorId", "FirstName", competitorsInEvent.CompetitorId);
            //ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", competitorInEvent.EventId);
            return View(vm);
        }

        // GET: CompetitorsInEvents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.CompetitorsInEvent == null)
            {
                return NotFound();
            }

            var competitorsInEvent = await _context.CompetitorsInEvent
                .Include(c => c.Competitor)
                .Include(c => c.Event)
                .FirstOrDefaultAsync(m => m.CompetitorInEventId == id);
            if (competitorsInEvent == null)
            {
                return NotFound();
            }

            var vm = new CompetitorInEventViewModel { 
                CompetitorId = competitorsInEvent.CompetitorId, 
                CompetitorInEventId = competitorsInEvent.CompetitorInEventId, 
                EventId = competitorsInEvent.EventId,
                TeamName = competitorsInEvent.Competitor.Team.TeamName,
                EventNumber = competitorsInEvent.EventNumber,
                FirstName = competitorsInEvent.Competitor.FirstName,
                LastName = competitorsInEvent.Competitor.LastName,
                TeamId = competitorsInEvent.Competitor.TeamId
            };
            return View(vm);
        }

        // POST: CompetitorsInEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.CompetitorsInEvent == null)
            {
                return Problem("Entity set 'DatabaseContext.CompetitorsInEvent'  is null.");
            }
            var competitorsInEvent = await _context.CompetitorsInEvent.FindAsync(id);
            if (competitorsInEvent != null)
            {
                _context.CompetitorsInEvent.Remove(competitorsInEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { eventId = competitorsInEvent.EventId });
            }
            
            return NotFound();
        }

        private bool CompetitorsInEventExists(int id)
        {
          return (_context.CompetitorsInEvent?.Any(e => e.CompetitorInEventId == id)).GetValueOrDefault();
        }

        public JsonResult GetCompetitorForEvent(int teamId)
        {
            List<Competitor> competitorsInEvent = new List<Competitor>();
            competitorsInEvent = (from competitor in _context.Competitors
                                  where competitor.TeamId == teamId
                                  select competitor).ToList();

            return Json(new SelectList(competitorsInEvent, "CompetitorId", "CompetitorName"));
        }
    }
}
