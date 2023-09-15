using CycleManager.Services;
using CycleManager.Services.Interfaces;
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
        private readonly IGameCompetitorInEventService _gameCompetitorEventService;
        private readonly IResultService _resultService;
        private readonly ICompetitorInEventService _competitorInEventService;
        private readonly IEventService _eventService;
        private readonly IGameCompetitorService _gameCompetitorService;
        private List<ResultLineViewModel> _resultLines = new List<ResultLineViewModel>();
        private readonly DatabaseContext _context;

        public GameCompetitorEventsController(IGameCompetitorInEventService gameCompetitorInEventService, 
            IResultService resultService, IEventService eventService, IGameCompetitorService gameCompetitorService, 
            ICompetitorInEventService competitorInEventService, DatabaseContext context)
        {
            _gameCompetitorEventService = gameCompetitorInEventService;
            _resultService = resultService;
            _eventService = eventService;
            _gameCompetitorService = gameCompetitorService;
            _competitorInEventService = competitorInEventService;
            _context = context;
        }

        // GET: GameCompetitorEvents
        public async Task<IActionResult> Index(int eventId)
        {
            var model = new List<GameCompetitorInEventViewModel>();
            var teamsForEvent = await _gameCompetitorEventService.GetAllCompetitorsInEvent(eventId);
            var competitorsInEventPicks = _gameCompetitorEventService.GetPicks(eventId);
            var competitorsInEventResults = await _resultService.GetResultsByEventId(eventId);
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

            foreach(var team in teamsForEvent)
            {
                var teamId = team.Id;
                var teamName = team.TeamName;
                var gamecompetitor = groupedGameCompetitors.Where(c => c.GameCompetitorInEventId.Equals(teamId)).FirstOrDefault();
                if (gamecompetitor != null)
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
                            OutOfCompetition = competitorInEventPick.CompetitorsInEvent.OutOfCompetition,
                            EventId = (int)eventId,

                        });
                    }
                    gamecompetitor.CompetitorsInEvent = resultList;
                    gamecompetitor.Id = gamecompetitor.GameCompetitorInEventId;
                    gamecompetitor.EventId = eventId;
                    gamecompetitor.Score = resultList.Sum(c => c.Score);
                    model.Add(gamecompetitor);
                }
                else
                {
                    gamecompetitor = new GameCompetitorInEventViewModel();
                    gamecompetitor.Id = teamId;
                    gamecompetitor.EventId = eventId;
                    gamecompetitor.TeamName = teamName;
                    gamecompetitor.Score = 0;
                    model.Add(gamecompetitor);
                }
            }

             return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details(int eventId, int gameCompetitorInEventId, IFormCollection formCollection)
        {
            var resultList = new List<GameCompetitorEventPick>();
            foreach (var key in formCollection.Keys)
            {
                if (key.Contains("SelectedCompetitorId"))
                {
                    var value = formCollection[key];
                    int.TryParse(value, out var competitorId);
                    if (competitorId > 0)
                    {
                        var gameCompetitor = _context.GameCompetitorsEvent.FirstOrDefault(g => g.Id.Equals(gameCompetitorInEventId));
                        var competitor = _context.CompetitorsInEvent.FirstOrDefault(c => c.Id == competitorId);
                        if (gameCompetitor != null && competitor != null)
                        {
                            resultList.Add(new GameCompetitorEventPick { CompetitorsInEvent = competitor, GameCompetitorEvent = gameCompetitor});
                        }
                    }
                }
            }

            _context.GameCompetitorEventPicks.AddRange(resultList);
            _context.SaveChanges();
            return RedirectToAction("Details", "GameCompetitorEvents", new { eventId });
        }


        // GET: GameCompetitorEvents/Details/5
        public async Task<IActionResult> Details(int? id, int? eventId)
        {

            _resultLines = null;
            var model = new GameCompetitorInEventViewModel();
            model.EventId = (int)eventId;
            model.GameCompetitorInEventId = (int)id;
            model.NumberOfPicks = _gameCompetitorEventService.GetNumberOfPicks((int)eventId, (int)id);
            model.DropdownList = GetDropdownList((int)eventId);
            //todo get data from servie (teamname)
            if (TempData["suggestedCompetitors"] != null)
            {
                var competitorIds = TempData["suggestedCompetitors"] as IEnumerable<int>;
                model.SuggestedCompetitors = new List<CompetitorsInEvent>();
                foreach(var competitorId in competitorIds)
                {
                    var competitorInEvent = await _competitorInEventService.GetCompetitorsInEventByIds(model.EventId, competitorId);
                    if (competitorInEvent != null)
                    {
                        model.SuggestedCompetitors.Add(competitorInEvent);
                    }
                }
            }

            var competitorsInEventPicks = _gameCompetitorEventService.GetPicks((int)eventId, (int)id).ToList();
            if (competitorsInEventPicks != null && competitorsInEventPicks.Count > 0)
            {
                model.TeamName = competitorsInEventPicks.First().GameCompetitorEvent.TeamName;
            }
            var competitorsInEventResults = await _resultService.GetResultsByEventId((int)eventId);

            if (competitorsInEventResults != null)
            {
                _resultLines = competitorsInEventResults.GroupBy(g => g.CompetitorInEventId)
                    .Select(cl => new ResultLineViewModel
                    {
                        CompetitorInEventId = cl.First().CompetitorInEventId,
                        Score = cl.Sum(c => c.ConfigurationItem.Score),
                    }).ToList();
            }

            var resultList = new List<ResultLineViewModel>();
            if (competitorsInEventPicks != null)
            {


                foreach (var competitorInEventPick in competitorsInEventPicks)
                {
                    resultList.Add(
                    new ResultLineViewModel
                    {
                        CompetitorInEventId = competitorInEventPick.CompetitorsInEvent.Id,
                        FirstName = competitorInEventPick.CompetitorsInEvent.Competitor.FirstName,
                        LastName = competitorInEventPick.CompetitorsInEvent.Competitor.LastName,
                        Score = GetScoreFromResultList(competitorInEventPick.CompetitorsInEvent.Id),
                        OutOfCompetition = competitorInEventPick.CompetitorsInEvent.OutOfCompetition,
                        DropdownList = GetDropdownList((int)eventId),
                        SelectedCompetitorId = competitorInEventPick.CompetitorsInEvent.Id,
                        EventId = (int)eventId,
                    });;
                }
                model.Score = resultList.Sum(c => c.Score);
                var orderedList = resultList.OrderByDescending(c => c.Score).ThenBy(c => c.LastName).ToList();
                model.CompetitorsInEvent = orderedList;
            }
                
            return View(model);

        }
        // GET: GameCompetitorEvents/Create
        public async Task<IActionResult> Create()
        {
            ViewData["EventId"] = new SelectList(await _eventService.GetAllEvents(), "EventId", "EventName");
            ViewData["GameCompetitorId"] = new SelectList(await _gameCompetitorService.GetAllGameCompetitors(), "Id", "FirstName");
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
                await _gameCompetitorEventService.Create(gameCompetitorEvent);
                return RedirectToAction(nameof(Index), new { eventId = gameCompetitorEvent.EventId });
            }
            ViewData["EventId"] = new SelectList(await _eventService.GetAllEvents(), "EventId", "EventName", gameCompetitorEvent.EventId);
            ViewData["GameCompetitorId"] = new SelectList(await _gameCompetitorService.GetAllGameCompetitors(), "Id", "FirstName", gameCompetitorEvent.GameCompetitorId);
            return View(gameCompetitorEvent);
        }

        // GET: GameCompetitorEvents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gameCompetitorEvent = await _gameCompetitorEventService.GetCompetitorEventById((int)id);
            if (gameCompetitorEvent == null)
            {
                return NotFound();
            }
            ViewData["EventId"] = new SelectList(await _eventService.GetAllEvents(), "EventId", "EventName", gameCompetitorEvent.EventId);
            ViewData["GameCompetitorId"] = new SelectList(await _gameCompetitorService.GetAllGameCompetitors(), "Id", "FirstName", gameCompetitorEvent.GameCompetitorId);
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
                    await _gameCompetitorEventService.Update(gameCompetitorEvent);
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
                return RedirectToAction(nameof(Details), new { eventId = gameCompetitorEvent.EventId});
            }
            ViewData["EventId"] = new SelectList(await _eventService.GetAllEvents(), "EventId", "EventName", gameCompetitorEvent.EventId);
            ViewData["GameCompetitorId"] = new SelectList(await _gameCompetitorService.GetAllGameCompetitors(), "Id", "FirstName", gameCompetitorEvent.GameCompetitorId);
            return RedirectToAction(nameof(Details), new { eventId = gameCompetitorEvent.EventId });
        }

        // GET: GameCompetitorEvents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            return View();

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
            int eventId = 0;
            if (gameCompetitorEvent != null)
            {
                eventId = gameCompetitorEvent.EventId;
                var picks = _context.GameCompetitorEventPicks.Where(g => g.GameCompetitorEvent.Equals(gameCompetitorEvent));
                _context.GameCompetitorEventPicks.RemoveRange(picks);
                _context.GameCompetitorsEvent.Remove(gameCompetitorEvent);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { eventId });
        }

        private bool GameCompetitorEventExists(int id)
        {
            return true;

          //return (_context.GameCompetitorsEvent?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public IEnumerable<SelectListItem> GetDropdownList(int eventId)
        {
            var competitors = new List<SelectListItem>();

            var competitorsDb = _context.CompetitorsInEvent.OrderBy(c => c.Competitor.FirstName).Where(c => c.EventId.Equals(eventId) && c.OutOfCompetition.Equals(false)).ToList();
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
            //var competitor = _context.Competitors.FirstOrDefault(c => c.CompetitorId.Equals(competitorId));
            //if (competitor != null)
            //{
            //    return $"{competitor.FirstName} {competitor.LastName}";
            //}
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

        public async Task<IActionResult> FillList(int id, int picks, int eventId)
        {
            //id = the gamecompetitor
            var a = picks; // get pciks for id
            var b = 15; //should be configuration item
            // a check current gamecompetitorevent for number of picks
            // b determine how many picks you should add
            // c pick random competitorsinevent: c = (b - a)
            var c = b - a;
            var suggestedCompetitiorsList =  await _gameCompetitorEventService.GetCompetitors(eventId, c);
            var suggestedCompetitorsId = new List<int>();
            suggestedCompetitorsId = suggestedCompetitiorsList.Select(c => c.CompetitorId).ToList();
            TempData["suggestedCompetitors"] = suggestedCompetitorsId;
            // and make sure there are no duplicates in a + c
            // return to detailview for the gamecompetitorevent with picks (a) and suggested picks (c)
            return RedirectToAction("Details", new { id, eventId });
        }
    }   
}