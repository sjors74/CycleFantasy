using CycleManager.Domain.Dto;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
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

        public GameCompetitorEventsController(IGameCompetitorInEventService gameCompetitorInEventService, 
            IResultService resultService, IEventService eventService, IUserService userService,
            ICompetitorInEventService competitorInEventService)
        {
            _gameCompetitorEventService = gameCompetitorInEventService;
            _resultService = resultService;
            _eventService = eventService;
            _userService = userService;
            _competitorInEventService = competitorInEventService;
        }

        // GET: GameCompetitorEvents
        public async Task<IActionResult> Index(int eventId)
        {
            // 1. Alle resultaten ophalen
            var resultDtos = (await _resultService.GetResultsByEventId(eventId)).ToList();
            var pointsByCompetitor = resultDtos
                .ToDictionary(r => r.CompetitorInEventId, r => r.Points);

            // 2. Alle picks ophalen
            var picks = _gameCompetitorEventService.GetPicks(eventId).ToList();

            // 3. Alle GameCompetitorEvents ophalen
            var allGameCompetitors = await _gameCompetitorEventService.GetAllCompetitorsInEvent(eventId);

            // 4. Model samenstellen
            var model = allGameCompetitors
                .Select(gameCompetitor =>
                {
                    // alle picks van dit team
                    var teamPicks = picks
                        .Where(p => p.GameCompetitorEvent.Id == gameCompetitor.Id)
                        .Select(p => p.CompetitorsInEventId)
                        .Distinct();

                    // sommeer scores van deze renners (0 als geen resultaat)
                    var totalScore = teamPicks.Sum(cid => pointsByCompetitor.TryGetValue(cid, out var score) ? score : 0);

                    return new GameCompetitorInEventViewModel
                    {
                        GameCompetitorInEventId = gameCompetitor.Id,
                        TeamName = gameCompetitor.TeamName,
                        GameCompetitorName = $"{gameCompetitor?.User?.FirstName} {gameCompetitor?.User?.LastName}",
                        Score = totalScore,
                        EventId = gameCompetitor.EventId,
                        Id = gameCompetitor.Id
                    };
                })
                .OrderByDescending(m => m.Score)
                .ToList();

            ViewData["EventId"] = eventId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(GameCompetitorInEventViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var pick in model.CompetitorsInEvent)
                {
                    var competitors = await _competitorInEventService.GetCompetitors(model.EventId);

                    pick.Competitors = competitors
                        .OrderBy(c => c.CompetitorInTeam.Competitor.LastName)
                        .Select(c => new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = $"{c.CompetitorInTeam.Competitor.FirstName} {c.CompetitorInTeam.Competitor.LastName}"
                        })
                        .ToList();
                }
                return View(model);
            }


            var newPicks = model.CompetitorsInEvent
                    .Where(p => p.PickId == 0 && p.SelectedCompetitorId.HasValue)
                    .Select(p => new GameCompetitorEventPick
                    {
                        GameCompetitorEventId = model.Id,
                        CompetitorsInEventId = p.SelectedCompetitorId.Value
                    })
                    .ToList();

            if (newPicks.Any())
            {
                await _gameCompetitorEventService.AddPicks(newPicks);
            }

            // Redirect of terug naar view
            return RedirectToAction("Details", new { eventId = model.EventId});
        }

        // GET: GameCompetitorEvents/Details/5
        public async Task<IActionResult> Details(int? id, int? eventId)
        {
            if (id == null || eventId == null)
            {
                return NotFound();
            }

            var model = new GameCompetitorInEventViewModel
            {
                Id = id.Value,
                EventId = eventId.Value
            };

            // Resultaten ophalen
            var resultDtos = (await _resultService.GetResultsByEventId(eventId.Value)).ToList();
            var pointsByCompetitor = resultDtos.ToDictionary(r => r.CompetitorInEventId, r => r.Points);

            // Team picks ophalen
            var teamPicks = _gameCompetitorEventService
                .GetPicks(eventId.Value)
                .Where(p => p.GameCompetitorEventId == id)
                .ToList();

            // Alle mogelijke competitors voor de dropdown
            var competitors = await _competitorInEventService.GetCompetitors(eventId.Value);

            var dropdownList = competitors
                .OrderBy(c => c.CompetitorInTeam.Competitor.LastName)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(), // dit is competitorInEvent.Id
                    Text = $"{c.CompetitorInTeam.Competitor.FirstName} {c.CompetitorInTeam.Competitor.LastName}"
                })
                .ToList();

            model.DropdownList = dropdownList;
            model.TeamName = teamPicks?.FirstOrDefault()?.GameCompetitorEvent?.TeamName ?? "onbekend";

            // Bestaande picks omzetten
            var picks = teamPicks
                .Select(p =>
                {
                    var competitorId = p.CompetitorsInEventId;
                    var score = pointsByCompetitor.TryGetValue(competitorId, out var s) ? s : 0;

                    return new PickDetailViewModel
                    {
                        CompetitorInEventId = competitorId,
                        FirstName = p.CompetitorsInEvent.CompetitorInTeam.Competitor.FirstName ?? "onbekend",
                        LastName = p.CompetitorsInEvent.CompetitorInTeam.Competitor.LastName ?? "onbekend",
                        CompetitorName = p.CompetitorsInEvent.CompetitorInTeam.Competitor.CompetitorName ?? "onbekend",
                        IsOutOfCompetition = p.CompetitorsInEvent.OutOfCompetition,
                        Score = score,
                        PickId = p.Id,
                        SelectedCompetitorId = competitorId,   // dit zorgt dat de juiste waarde in de dropdown geselecteerd is
                        Competitors = dropdownList             // dropdown altijd vullen
                    };
                })
                .OrderByDescending(m => m.Score)
                .ToList();

            // Als er nog lege plekken zijn → vullen met lege picks
            var existingCount = picks.Count;
            for (int i = existingCount; i < 15; i++)
            {
                picks.Add(new PickDetailViewModel
                {
                    Competitors = dropdownList
                });
            }

            model.CompetitorsInEvent = picks;
            model.NumberOfPicks = picks.Count(p => p.PickId > 0);
            model.Score = picks.Sum(x => x.Score);

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
                EventName = entity?.Event?.EventName,
                EventId = entity.EventId
            };
            return View(dto);
        }

        // POST: GameCompetitorEvents/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _gameCompetitorEventService.GetGameCompetitorEventById(id);
            if (entity != null)
            {
                await _gameCompetitorEventService.DeleteGameCompetitorEventAsync(id);
                return RedirectToAction("Index", new { eventId = entity.EventId });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePick(int pickId)
        {
            // Verwijder de pick uit de database
            await _gameCompetitorEventService.RemovePickFromEvent(pickId);
            return Ok();
        }

        public async Task<IEnumerable<SelectListItem>> GetDropdownList(int eventId)
        {
            return await _gameCompetitorEventService.GetDropdownListAsync(eventId);
        }

        private int GetCompetitorIdFromList(int i, IEnumerable<CompetitorsInEvent> competitors)
        {
            var indexingCompetitors = competitors.ToList();
            var competitor = indexingCompetitors[i];
            return competitor.Id;
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

        //TODO: implement services instead of using context directly
        //[HttpGet]
        //public async Task<IActionResult> FillList(int id, int picks, int eventId)
        //{
        //    // id = GameCompetitorEvent.Id
        //    // 'picks' parameter is ignored for current DB state; we compute currentCount from DB.

        //    const int maxPicks = 15; // TODO: haal uit configuratie: event.Configuration etc.

        // TODO haal de gamecompetitor op via service/repo (deelnemer)
        //    var gameCompetitor = await _context.GameCompetitorsEvent
        //        .AsTracking()
        //        .FirstOrDefaultAsync(g => g.Id == id);

        //    if (gameCompetitor == null)
        //        return NotFound();

        // TODO :Haal het aantal reeds gekozen Picks op voor deze GameCompetitorEvent via service/repo
        //    huidig aantal picks uit DB
        //    var currentCount = await _context.GameCompetitorEventPicks
        //        .CountAsync(p => p.GameCompetitorEventId == id);

        //    var slotsToFill = Math.Max(0, maxPicks - currentCount);
        //    if (slotsToFill == 0)
        //    {
        //        // niets te doen
        //        return RedirectToAction("Details", new { id, eventId });
        //    }

        //    // Vraag mogelijke kandidaten op — let op: service moet kandidaten teruggeven
        //    // die nog niet gepickt zijn (of we filteren hier zelf).
        //    // Verwacht: GetCompetitors(eventId, count) -> lijst van CompetitorsInEvent of DTO met CompetitorInTeamId
        //    var candidates = await _gameCompetitorEventService.GetCompetitors(eventId, slotsToFill);

        //    if (candidates == null || !candidates.Any())
        //    {
        //        return RedirectToAction("Details", new { id, eventId });
        //    }

        //    // haal al gepickte competitors in event op, om duplicaten te voorkomen
        //    var alreadyPickedIds = await _context.GameCompetitorEventPicks
        //        .Where(p => p.GameCompetitorEventId == id)
        //        .Select(p => p.CompetitorsInEventId)
        //        .ToListAsync();

        //    // Transformeer candidates naar ids (pas aan op jouw DTO)
        //    var candidateIds = candidates.Select(c => c.CompetitorInTeamId).Distinct().ToList();

        //    // Filter bestaande picks
        //    var toAddIds = candidateIds
        //        .Where(cid => !alreadyPickedIds.Contains(cid))
        //        .Take(slotsToFill)
        //        .ToList();

        //    // In het onwaarschijnlijke geval dat service teruggeeft > beschikbare unique ids,
        //    // we beperken met Take(slotsToFill) hierboven.

        //    foreach (var cid in toAddIds)
        //    {
        //        var pick = new GameCompetitorEventPick
        //        {
        //            GameCompetitorEventId = id,
        //            CompetitorsInEventId = cid
        //        };
        //        _context.GameCompetitorEventPicks.Add(pick);
        //    }

        //    // Persist
        //    await _context.SaveChangesAsync();

        //    // Optioneel: zet de suggesties in TempData als je die in Details wilt tonen
        //    TempData["suggestedCompetitors"] = toAddIds;

        //    return RedirectToAction("Details", new { id, eventId });
        //}

        public async Task<IActionResult> FillList(int id, int picks, int eventId)
        {
            //id = the gamecompetitor
            var a = picks; // get pciks for id
            var b = 15; //should be configuration item
            // a check current gamecompetitorevent for number of picks
            // b determine how many picks you should add
            // c pick random competitorsinevent: c = (b - a)
            var c = b - a;
            var suggestedCompetitiorsList = await _gameCompetitorEventService.GetCompetitors(eventId, c);
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