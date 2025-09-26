using CycleManager.Domain.Dto;
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
        private readonly IUserService _userService;
        private List<ResultLineViewModel> _resultLines = new List<ResultLineViewModel>();
        private readonly ApplicationDbContext _context;

        public GameCompetitorEventsController(IGameCompetitorInEventService gameCompetitorInEventService, 
            IResultService resultService, IEventService eventService, IUserService userService,
            ICompetitorInEventService competitorInEventService, ApplicationDbContext context)
        {
            _gameCompetitorEventService = gameCompetitorInEventService;
            _resultService = resultService;
            _eventService = eventService;
            _userService = userService;
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
                    Score = cl.Sum(c => c.ConfigurationItems.First().Score), //todo : gaat dit goed???
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
                            FirstName = competitorInEventPick.CompetitorsInEvent.CompetitorInTeam.Competitor.FirstName,
                            LastName = competitorInEventPick.CompetitorsInEvent.CompetitorInTeam.Competitor.LastName,
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
            ViewData["EventId"] = eventId;
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
            model.NumberOfPicks = await _gameCompetitorEventService.GetNumberOfPicks((int)eventId, (int)id);
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

            var competitorsInEventPicks = await _gameCompetitorEventService.GetAllPicks((int)id);
            if (competitorsInEventPicks != null && competitorsInEventPicks.Count() > 0)
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
                        Score = cl.Sum(c => c.ConfigurationItems.First().Score), //TODO : gaat dit goed?
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
                        FirstName = competitorInEventPick.CompetitorsInEvent.CompetitorInTeam.Competitor.FirstName,
                        LastName = competitorInEventPick.CompetitorsInEvent.CompetitorInTeam.Competitor.LastName,
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
        public async Task<IActionResult> Create(int eventId)
        {
            var users = await _userService.GetAllUsers();
            var userList = users.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = $"{u.FirstName} {u.LastName} ({u.Email})"
            }).ToList();

            userList.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "Selecteer..."
            });

            ViewData["Users"] = userList;

            var dto = new DeelnemerCreateDto
            {
                EventId = eventId
            };
            return View(dto);
        }

        // POST: GameCompetitorEvents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DeelnemerCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var users = await _userService.GetAllUsers();
                ViewData["Users"] = users.Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.FirstName} {u.LastName} ({u.Email})"
                }).ToList();

                return View(dto);
            }
            await _gameCompetitorEventService.CreateGameCompetitorEventAsync(dto);
            return RedirectToAction(nameof(Index), new { eventId = dto.EventId});
        }

        // GET: GameCompetitorEvents/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _gameCompetitorEventService.GetGameCompetitorEventById(id);
            if (entity == null) return NotFound();

            var dto = new DeelnemerEditDto
            {
                Id = entity.Id,
                TeamName = entity.TeamName,
                UserId = entity.UserId,
                EventId = entity.EventId
            };

            await PopulateDropDowns();

            return View(dto);
        }

        // POST: GameCompetitorEvents/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DeelnemerEditDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropDowns();
                return View(dto);
            }

            await _gameCompetitorEventService.UpdateAsync(dto);
            return RedirectToAction("Index", new { eventId = dto.EventId});
        }

        // GET: GameCompetitorEvents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _gameCompetitorEventService.GetGameCompetitorEventById(id.Value);
            if (entity == null) return NotFound();

            var dto = new DeelnemerDeleteDto
            {
                Id = entity.Id,
                TeamName = entity.TeamName,
                UserName = $"{entity?.User?.FirstName} {entity?.User?.LastName}",
                EventName = entity.Event.EventName,
                EventId = entity.EventId
            };
            return View(dto);
        }

        // POST: GameCompetitorEvents/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(DeelnemerDeleteDto dto)
        {
            var entity = await _gameCompetitorEventService.GetGameCompetitorEventById(dto.Id);
            if (entity != null)
            {
                var picks = _context.GameCompetitorEventPicks
                    .Where(g => g.GameCompetitorEventId == entity.Id);
                _context.GameCompetitorEventPicks.RemoveRange(picks);
                _context.GameCompetitorsEvent.Remove(entity);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", new { eventId = dto.EventId });
        }

        private bool GameCompetitorEventExists(int id)
        {
            return true;

          //return (_context.GameCompetitorsEvent?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public IEnumerable<SelectListItem> GetDropdownList(int eventId)
        {
            var competitors = new List<SelectListItem>();

            var competitorsDb = _context.CompetitorsInEvent
                .Include(c => c.CompetitorInTeam)
                    .ThenInclude(c => c.Competitor)
                    .ThenInclude(c => c.CompetitorInTeams)
                        .ThenInclude(cit => cit.Team)
                .Where(c => c.EventId == eventId && !c.OutOfCompetition)
                .OrderBy(c => c.CompetitorInTeam.Competitor.FirstName)
                .ToList();

            var groupedCompetitors = competitorsDb
                .GroupBy(x => x.CompetitorInTeam?.Team?.CurrentTeamName ?? "Onbekend");
            
            foreach (var group in groupedCompetitors)
            {
                var optionGroup = new SelectListGroup() { Name = group.Key };
                foreach (var item in group)
                {
                    competitors.Add(new SelectListItem()
                    {
                        Value = item.Id.ToString(),
                        Text = item.CompetitorInTeam.Competitor.CompetitorName,
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
            suggestedCompetitorsId = suggestedCompetitiorsList.Select(c => c.CompetitorInTeamId).ToList();
            TempData["suggestedCompetitors"] = suggestedCompetitorsId;
            // and make sure there are no duplicates in a + c
            // return to detailview for the gamecompetitorevent with picks (a) and suggested picks (c)
            return RedirectToAction("Details", new { id, eventId });
        }

        /// <summary>
        /// vul dropdowns voor view
        /// </summary>
        /// <returns></returns>
        private async Task PopulateDropDowns()
        {
            // Users dropdown
            var users = await _userService.GetAllUsers();
            ViewBag.UserId = users
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.FirstName} {u.LastName} ({u.Email})"
                })
                .ToList();

            // Event dropdown (als je die nog nodig hebt)
            var events = await _eventService.GetAllEvents();
            ViewBag.EventId = events
                .Select(e => new SelectListItem
                {
                    Value = e.EventId.ToString(),
                    Text = e.EventName
                })
                .ToList();
        }
    }   
}