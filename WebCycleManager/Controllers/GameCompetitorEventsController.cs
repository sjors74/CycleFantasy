using Domain.Context;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class GameCompetitorEventsController : Controller
    {
        private readonly DatabaseContext _context;
        private List<ResultLineViewModel> _resultLines;
        public GameCompetitorEventsController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: GameCompetitorEvents
        public async Task<IActionResult> Index(int eventId)
        {
            var model = new List<GameCompetitorInEventViewModel>();
            var competitorsInEventPicks = _context.GameCompetitorEventPicks
                .Include(c => c.CompetitorsInEvent).ThenInclude(a => a.Competitor)
                .Include(g => g.GameCompetitorEvent).ThenInclude(b => b.GameCompetitor)
                .Where(c => c.CompetitorsInEvent.EventId.Equals(eventId));

            var competitorsInEventResults = _context.Results
                            .Include(c => c.ConfigurationItem).ThenInclude(i => i.Configuration)
                            .Include(r => r.Stage)
                            .Where(a => a.Stage.EventId.Equals(eventId));

            var groupedGameCompetitors = competitorsInEventPicks.GroupBy(c => c.GameCompetitorEvent.Id).Select(g => new GameCompetitorInEventViewModel
            {
                GameCompetitorInEventId = g.Key,
                TeamName = g.First().GameCompetitorEvent.TeamName
            });


            _resultLines = competitorsInEventResults.GroupBy(g => g.CompetitorInEventId)
                .Select(cl => new ResultLineViewModel
                {
                    CompetitorInEventId = cl.First().CompetitorInEventId,
                    Score = cl.Sum(c => c.ConfigurationItem.Score),
                }).ToList();

            foreach (var gamecompetitor in groupedGameCompetitors.ToList())
            {
                var resultList = new List<ResultLineViewModel>();
                var filteredPicks = competitorsInEventPicks.Where(c => c.GameCompetitorEvent.Id.Equals(gamecompetitor.GameCompetitorInEventId));
                foreach (var competitorInEventPick in filteredPicks)
                {
                    resultList.Add(
                    new ResultLineViewModel
                    {
                        CompetitorInEventId = competitorInEventPick.CompetitorsInEvent.Id,
                        FirstName = competitorInEventPick.CompetitorsInEvent.Competitor.FirstName,
                        LastName = competitorInEventPick.CompetitorsInEvent.Competitor.LastName,
                        Score = GetScoreFromResultList(competitorInEventPick.CompetitorsInEvent.Id),
                        EventId = (int)eventId,

                    });
                }
                gamecompetitor.CompetitorsInEvent = resultList;
                gamecompetitor.Id = gamecompetitor.GameCompetitorInEventId;
                gamecompetitor.EventId = eventId;
                gamecompetitor.GameCompetitorName = GetGameCompetitorName(gamecompetitor.GameCompetitorInEventId);
                gamecompetitor.Score = resultList.Sum(c => c.Score);
                model.Add(gamecompetitor);
            }

             return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int eventId, int gameCompetitorInEventPickId, IFormCollection formCollection)
        {
            var resultList = new List<CompetitorsInEvent>();
            foreach (var key in formCollection.Keys)
            {
                if (key.Contains("SelectedCompetitorId"))
                {
                    var value = formCollection[key];

                    int.TryParse(value, out var competitorId);
                    if (competitorId > 0)
                    {

                        var gameCompetitor = _context.GameCompetitorEventPicks.FirstOrDefault();// c => c.GameCompetitorEventId == gameCompetitorInEventPickId);
                        resultList.Add(new CompetitorsInEvent{ Id = competitorId });

                            foreach (var competitor in resultList)
                            {
                                //gameCompetitor.CompetitorsInEvent.Add(competitor);
                            }

                            _context.GameCompetitorEventPicks.Update(gameCompetitor);
                        //}
                        

                    }
                }
            }

            //_context.GameCompetitorEventPicks.AddRange(resultList);
            _context.SaveChanges();
            return RedirectToAction("Index", "GameCompetitorEvents", new { Id = eventId });
        }


        // GET: GameCompetitorEvents/Details/5
        public async Task<IActionResult> Details(int? id, int? eventId)
        {
            if (id == null || _context.GameCompetitorsEvent == null)
            {
                return NotFound();
            }
            _resultLines = null;
            var model = new GameCompetitorInEventViewModel();
            model.EventId = (int)eventId;
            model.GameCompetitorInEventId = (int)id;

            var competitorsInEventPicks = _context.GameCompetitorEventPicks
                .Include(c => c.CompetitorsInEvent).ThenInclude(a => a.Competitor)
                .Include(g => g.GameCompetitorEvent).ThenInclude(b => b.GameCompetitor)
                .Where(c => c.CompetitorsInEvent.EventId.Equals(eventId) && c.GameCompetitorEvent.Id.Equals(id));

            model.TeamName = competitorsInEventPicks.First().GameCompetitorEvent.TeamName;
            var competitorsInEventResults = _context.Results
                .Include(c => c.ConfigurationItem).ThenInclude(i => i.Configuration)
                .Include(r => r.Stage)
                .Where(a => a.Stage.EventId.Equals(eventId));

            _resultLines = competitorsInEventResults.GroupBy(g => g.CompetitorInEventId)
                .Select(cl => new ResultLineViewModel
                {
                    CompetitorInEventId = cl.First().CompetitorInEventId,
                    Score = cl.Sum(c => c.ConfigurationItem.Score),
                }).ToList();

            var resultList = new List<ResultLineViewModel>();
            foreach (var competitorInEventPick in competitorsInEventPicks)
            {
                resultList.Add(
                new ResultLineViewModel
                {
                    CompetitorInEventId = competitorInEventPick.CompetitorsInEvent.Id,
                    FirstName = competitorInEventPick.CompetitorsInEvent.Competitor.FirstName,
                    LastName = competitorInEventPick.CompetitorsInEvent.Competitor.LastName,
                    Score = GetScoreFromResultList(competitorInEventPick.CompetitorsInEvent.Id),
                    EventId = (int)eventId,

                });
            }
            model.Score = resultList.Sum(c => c.Score);
            var orderedList = resultList.OrderByDescending(c => c.Score).ThenBy(c => c.LastName).ToList();
            model.CompetitorsInEvent = orderedList;
            return View(model);

        }
        // GET: GameCompetitorEvents/Create
        public IActionResult Create()
        {
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName");
            ViewData["GameCompetitorId"] = new SelectList(_context.GameCompetitors, "Id", "FirstName");
            return View();
        }

        // POST: GameCompetitorEvents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TeamName,GameCompetitorId,EventId")] GameCompetitorEvent gameCompetitorEvent)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gameCompetitorEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", gameCompetitorEvent.EventId);
            ViewData["GameCompetitorId"] = new SelectList(_context.GameCompetitors, "Id", "FirstName", gameCompetitorEvent.GameCompetitorId);
            return View(gameCompetitorEvent);
        }

        // GET: GameCompetitorEvents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.GameCompetitorsEvent == null)
            {
                return NotFound();
            }

            var gameCompetitorEvent = await _context.GameCompetitorsEvent.FindAsync(id);
            if (gameCompetitorEvent == null)
            {
                return NotFound();
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", gameCompetitorEvent.EventId);
            ViewData["GameCompetitorId"] = new SelectList(_context.GameCompetitors, "Id", "FirstName", gameCompetitorEvent.GameCompetitorId);
            ViewBag.GameEventId = gameCompetitorEvent.EventId;
            return View(gameCompetitorEvent);
        }

        // POST: GameCompetitorEvents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TeamName,GameCompetitorId,EventId")] GameCompetitorEvent gameCompetitorEvent)
        {
            if (id != gameCompetitorEvent.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gameCompetitorEvent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameCompetitorEventExists(gameCompetitorEvent.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { eventId = gameCompetitorEvent.EventId});
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", gameCompetitorEvent.EventId);
            ViewData["GameCompetitorId"] = new SelectList(_context.GameCompetitors, "Id", "FirstName", gameCompetitorEvent.GameCompetitorId);
            return View(gameCompetitorEvent);
        }

        // GET: GameCompetitorEvents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.GameCompetitorsEvent == null)
            {
                return NotFound();
            }
            var gamePicks = _context.GameCompetitorEventPicks.FirstOrDefault();
                //Include(c => c.CompetitorsInEvent).Where(c => c.CompetitorsInEvent.Select(c => c.CompetitorInEventId == id).FirstOrDefault());
            //var gameCompetitorEvent = await _context.GameCompetitorsEvent
            //    .Include(g => g.Event)
            //    .Include(g => g.GameCompetitor)
            //    .FirstOrDefaultAsync(m => m.Id == id);
            if (gamePicks == null)
            {
                return NotFound();
            }

            //var vm = new GameCompetitorInEventViewModel
            //{
            //    Id = result.Id,
            //    StageId = result.StageId,
            //    //Position = result.ConfigurationItem.Position,
            //    CompetitorName = gamePicks..CompetitorInEvent.Competitor.CompetitorName
            //};

            //return View(vm);

            return View(gamePicks);
        }

        // POST: GameCompetitorEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.GameCompetitorsEvent == null)
            {
                return Problem("Entity set 'DatabaseContext.GameCompetitorsEvent'  is null.");
            }
            var gameCompetitorEvent = await _context.GameCompetitorsEvent.FindAsync(id);
            if (gameCompetitorEvent != null)
            {
                _context.GameCompetitorsEvent.Remove(gameCompetitorEvent);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GameCompetitorEventExists(int id)
        {
          return (_context.GameCompetitorsEvent?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public IEnumerable<SelectListItem> GetDropdownList(int eventId)
        {
            var competitors = new List<SelectListItem>();

            var competitorsDb = _context.CompetitorsInEvent.OrderBy(c => c.Competitor.FirstName).Where(c => c.EventId.Equals(eventId)).ToList();
            var groupedCompetitors = competitorsDb.GroupBy(x => x.Competitor.Team.TeamName);
            foreach (var group in groupedCompetitors)
            {
                var optionGroup = new SelectListGroup() { Name = group.Key };
                foreach (var item in group)
                {
                    competitors.Add(new SelectListItem()
                    {
                        Value = item.Id.ToString(),
                        Text = item.Competitor.CompetitorName,
                        Group = optionGroup
                    });
                }
            }
            return competitors;
        }

        private int GetCompetitorIdFromList(int i, IEnumerable<CompetitorsInEvent> competitors)
        {
            var indexingCompetitors = competitors.ToList();
            var competitor = indexingCompetitors[i];
            return competitor.Id;
        }

        private string GetCompetitorFullName(int competitorId)
        {
            var competitor = _context.Competitors.FirstOrDefault(c => c.CompetitorId.Equals(competitorId));
            if (competitor != null)
            {
                return $"{competitor.FirstName} {competitor.LastName}";
            }
            return string.Empty;
        }

        private int GetScoreFromResultList(int competitorInEventId)
        {
            foreach(var resultLine in _resultLines)
            {
                if(resultLine.CompetitorInEventId == competitorInEventId)
                {
                    return resultLine.Score;
                }
            }
            return 0;
        }

        private string GetGameCompetitorName(int competitorId)
        {
            var gameCompetitor = _context.GameCompetitorsEvent.Include(c => c.GameCompetitor)
                .FirstOrDefault(c => c.Id == competitorId);
            return $"{gameCompetitor.GameCompetitor.FirstName} {gameCompetitor.GameCompetitor.LastName}";
        }
    }
}