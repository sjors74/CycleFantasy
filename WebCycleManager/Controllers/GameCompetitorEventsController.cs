using DataAccessEF.Migrations;
using Domain.Context;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class GameCompetitorEventsController : Controller
    {
        private readonly DatabaseContext _context;
        private List<ResultLine> _resultLines;
        public GameCompetitorEventsController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: GameCompetitorEvents
        public async Task<IActionResult> Index(int eventId)
        {
            var databaseContext = _context.GameCompetitorsEvent.Where(e => e.EventId.Equals(eventId));
            return View(await databaseContext.ToListAsync());
        }


        //public async Task<IActionResult> Index()
        //{

        //    var databaseContext = _context.GameCompetitorEventPicks
        //        .Include(c => c.CompetitorsInEvent)
        //        .Include(g => g.GameCompetitorEvent)
        //        .Where(c => c.CompetitorsInEvent.EventId.Equals(2) && c.GameCompetitorEvent.GameCompetitorId.Equals(1));


        //    return View(await databaseContext.ToListAsync());
        //}

        public async Task<IActionResult> Picks(int Id, int eventId)
        {
//            var gameCompetitorForPicksId = _context.GameCompetitorEventPicks.FirstOrDefault(c => c.GameCompetitorEventId == Id).Id;
//            if(gameCompetitorForPicksId > 0)
//            {
                //var competitors = _context.GameCompetitorEventPicks.Include(c => c.CompetitorsInEvent).Where(c => c.Id == gameCompetitorForPicksId).SelectMany(c => c.CompetitorsInEvent).ToList();
                //var resultDict = new Dictionary<int, int>();
                //resultDict = competitors.ToDictionary(r => r.CompetitorId, r => r.CompetitorInEventId);

                //var currentEvent = _context.Events.FirstOrDefault(e => e.EventId == eventId);
                //var resultItems = new List<GameCompetitorInEventItemViewModel>();
                //var config = currentEvent.Configuration;
                //var numberOfconfigItems = _context.ConfigurationItems.Where(l => l.ConfigurationId.Equals(config.Id)).Count();

                //for (int i = 0; i < numberOfconfigItems; i++)
                //{
                //    var compEventId= 0;
                //    var compId = 0;
                //    var rivm = new GameCompetitorInEventItemViewModel();
                //    rivm.DropdownList = GetDropdownList(currentEvent.EventId);
                //    rivm.EventId = eventId;
                //    rivm.GameCompetitorEventPickId = gameCompetitorForPicksId;

                //    try
                //    {
                //        compEventId = competitors[i].CompetitorInEventId;
                //        compId = competitors[i].CompetitorId;
                //        rivm.CompetitorName = GetCompetitorFullName(compId);
                //        rivm.SelectedCompetitorId = compEventId;
                //        resultItems.Add(rivm);
                //    }
                //    catch (ArgumentOutOfRangeException ex)
                //    {
                //        resultItems.Add(rivm);
                //        continue;

                //    }
                //    catch(Exception ex)
                //    {
                //        throw;
                //    }
                    
                //}
                //var rvm = new GameCompetitorInEventViewModel(eventId , currentEvent, gameCompetitorForPicksId, resultItems, numberOfconfigItems);

                //return View(rvm);
            //}

            return NotFound();
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
                        //var gameCompetitorEvent = _context.GameCompetitorsEvent.FirstOrDefault(c => c.Id == gameCompetitorInEventPickId);
                        var gameCompetitor = _context.GameCompetitorEventPicks.FirstOrDefault();// c => c.GameCompetitorEventId == gameCompetitorInEventPickId);
                        //var competitorInEvent = _context.CompetitorsInEvent.Where(c => c.CompetitorInEventId == competitorId).ToList();
                        //var position = GetPositionFromKey(key);
                        //var configurationId = int.TryParse(formCollection["configurationId"], out var configId);
                        //var configurationItem = await _context.ConfigurationItems.FirstOrDefaultAsync(c => c.ConfigurationId.Equals(configId) && c.Position.Equals(position));
                        //if (competitorInEvent != null)
                        //{
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

            var competitorsInEventPicks = _context.GameCompetitorEventPicks
                .Include(c => c.CompetitorsInEvent).ThenInclude(a => a.Competitor)
                .Include(g => g.GameCompetitorEvent).ThenInclude(b => b.GameCompetitor)
                .Where(c => c.CompetitorsInEvent.EventId.Equals(eventId) && c.GameCompetitorEvent.Id.Equals(id));
            
            var competitorsInEventResults = _context.Results
                .Include(c => c.ConfigurationItem).ThenInclude(i => i.Configuration)
                .Include(r => r.Stage)
                .Where(a => a.Stage.EventId.Equals(eventId));

            _resultLines = competitorsInEventResults.GroupBy(g => g.CompetitorInEventId)
                .Select(cl => new ResultLine
                {
                    CompetitorInEventId = cl.First().CompetitorInEventId,
                    Score = cl.Sum(c => c.ConfigurationItem.Score),
                }).ToList();

            var resultList = new List<ResultLine>();
            foreach(var competitorInEventPick in competitorsInEventPicks)
            {
                resultList.Add(
                new ResultLine
                {
                    CompetitorInEventId = competitorInEventPick.CompetitorsInEvent.Id,
                    FirstName = competitorInEventPick.CompetitorsInEvent.Competitor.FirstName,
                    LastName = competitorInEventPick.CompetitorsInEvent.Competitor.LastName,
                    Score = GetScoreFromResultList(competitorInEventPick.CompetitorsInEvent.Id)
                });
            }

            var orderList = resultList.OrderByDescending(a => a.Score).ThenBy(a => a.LastName).ToList();
            return View(orderList);
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
    }
}

public class ResultLine
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Score { get; set; }
    public int CompetitorInEventId { get; set; }
}