using Domain.Context;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebCycleManager.Helpers;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class ResultsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IApiClient _apiClient;

        public ResultsController(ApplicationDbContext context, IApiClient apiClient)
        {
            _context = context;
            _apiClient = apiClient;
        }

        // GET: Results
        public IActionResult Index(int stageId)
        {
            //first get stage-data
            var stage = _context.Stages
                .AsNoTracking()
                .Include(s => s.Event)
                .ThenInclude(e => e.Configuration)
                .FirstOrDefault(s => s.Id == stageId);

            if (stage == null)
                return NotFound();

            var currentEvent = stage.Event;
            var config = currentEvent.Configuration;

            var results = _context.Results
                .AsNoTracking()
                .Where(r => r.StageId == stageId)
                .Include(r => r.CompetitorInEvent)
                    .ThenInclude(cie => cie.Competitor)
                .Include(r => r.ConfigurationItem)
                .ToList();

            var competitorsInEvent = _context.CompetitorsInEvent
                .AsNoTracking()
                .Where(c => c.EventId.Equals(currentEvent.EventId) && !c.OutOfCompetition)
                .Include(c => c.Competitor)
                .ToList();

            var configItems = _context.ConfigurationItems
                .AsNoTracking()
                .Where(ci => ci.ConfigurationId == config.Id)
                .OrderBy(ci => ci.Position)
                .ToList();

            var resultItems = configItems.Select(ci =>
                {
                    var result = results.FirstOrDefault(r => r.ConfigurationItem.Position == ci.Position);
                    int selectedCompetitorId = result?.CompetitorInEventId ?? 0;
                    string competitorName = string.Empty;
                    if(result != null && result.CompetitorInEvent?.Competitor != null)
                    {
                        competitorName = GetCompetitorFullName(result.CompetitorInEvent.Competitor.CompetitorId);
                    }
                    else if(selectedCompetitorId > 0)
                    {
                        competitorName = GetCompetitorFullName(selectedCompetitorId);
                    }

                    return new ResultItemViewModel
                    {
                        Position = ci.Position,
                        CompetitorName = competitorName,
                        SelectedCompetitorId = selectedCompetitorId,
                        Id = result?.Id ?? 0,
                        StageId = stageId,
                    };
                }).ToList();

            var rvm = new ResultViewModel(
                stage.Id,
                stage.EventId,
                config.Id,
                $"Etappe {stage.StageName}: {stage.StartLocation}-{stage.FinishLocation}",
                configItems.Count,
                resultItems,
                competitorsInEvent
                );

            return View(rvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ResultViewModel model)
        {
            var resultList = new List<Result>();

            foreach (var item in model.Results)
            {
                var cie = await _context.CompetitorsInEvent
                    .FirstOrDefaultAsync(c => c.CompetitorId == item.SelectedCompetitorId && c.EventId == model.EventId);
    
                if(cie != null)
                {
                    var configurationItem = await _context.ConfigurationItems
                        .FirstOrDefaultAsync(c => c.ConfigurationId == model.ConfigurationId && c.Position == item.Position);

                    if (configurationItem != null)
                    {
                        resultList.Add(new Result
                        {
                            CompetitorInEventId = cie.Id,
                            StageId = model.StageId,
                            ConfigurationItemId = configurationItem.Id
                        });

                    }
                }
            }

            _context.Results.AddRange(resultList);
            await _context.SaveChangesAsync();

            await InvalidateCacheInApi(model.EventId);

            //var competitors = _context.Competitors.OrderBy(c => c.LastName).ToList();
            //model.DropdownList = new SelectList(competitors, "CompetitorId", "CompetitorName");
            return RedirectToAction("Index", new { stageId = model.StageId});
        }

        private int GetPositionFromKey(string key)
        {
            key = key.Substring(key.IndexOf("[") + 1);
            key = key.Substring(0, key.IndexOf("]"));
            var positon = int.TryParse(key, out var positonNumber);
            return positonNumber + 1;
        }

        // GET: Results/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Results == null)
            {
                return NotFound();
            }

            var result = await _context.Results
                .Include(r => r.CompetitorInEvent)
                .Include(r => r.Stage)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (result == null)
            {
                return NotFound();
            }

            return View(result);
        }

        // GET: Results/Create
        public IActionResult Create(int stageId)
        {
            var stage = _context.Stages
                .Include(s => s.Event)
                    .ThenInclude(e => e.Configuration)
                        .ThenInclude(c => c.ConfigurationItems)
                .FirstOrDefault(s => s.Id == stageId);

            if (stage != null)
            {
                var config = stage.Event.Configuration;
                var configItems = config.ConfigurationItems;

                var resultItems = configItems.Select(ci => new ResultItemViewModel
                {
                    Id = ci.Id,
                    Position = ci.Position,
                    CompetitorName = string.Empty
                }).ToList();

                var competitorsInEvent = _context.CompetitorsInEvent
                      .Where(c => c.EventId.Equals(stage.EventId) && !c.OutOfCompetition)
                      .Include(c => c.Competitor)
                      .ToList();

                var rvm = new ResultViewModel(
                        stage.Id, 
                        stage.EventId, 
                        config.Id, 
                        $"Etappe {stage.StageName}: {stage.StartLocation}-{stage.FinishLocation}", 
                        configItems.Count, 
                        resultItems,
                        competitorsInEvent
                        );

                return View(rvm);
            }


            return NotFound();
        }

        // POST: Results/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StageId,CompetitorId")] Result result)
        {
            if (ModelState.IsValid)
            {
                _context.Add(result);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompetitorId"] = new SelectList(_context.Competitors.OrderBy(c => c.FirstName), "CompetitorId", "CompetitorName", result.CompetitorInEventId);
            ViewData["StageId"] = new SelectList(_context.Stages, "Id", "FinishLocation", result.StageId);
            return View(result);
        }

        // GET: Results/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Results == null)
            {
                return NotFound();
            }

            var result = await _context.Results
                .Include(r => r.CompetitorInEvent)
                    .ThenInclude(r => r.Competitor)
                .Include(r => r.Stage)
                .Include(r => r.ConfigurationItem)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (result == null)
            {
                return NotFound();
            }
            var vm = new ResultItemViewModel
            {
                Id = result.Id,
                StageId = result.StageId,
                Position = result.ConfigurationItem.Position,
                CompetitorName = result.CompetitorInEvent.Competitor.CompetitorName
            };

            return View(vm);
        }

        // POST: Results/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Results == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Results'  is null.");
            }
            var result = await _context.Results.FindAsync(id);
            if (result != null)
            {
                _context.Results.Remove(result);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { stageId = result.StageId });
        }

        private bool ResultExists(int id)
        {
            return (_context.Results?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private string GetCompetitorFullName(int competitorId)
        {
            var competitor = _context.Competitors.FirstOrDefault(c => c.CompetitorId == competitorId);
            return competitor != null ? $"{competitor.FirstName} {competitor.LastName}" : string.Empty;
        }

        private int GetResultId(int competitorId, int stageId)
        {
            var r = _context.Results.FirstOrDefault(c => c.StageId == stageId && c.CompetitorInEvent.CompetitorId == competitorId);
            if (r != null)
            {
                return r.Id;
            }

            return 0;
        }

        public IEnumerable<SelectListItem> GetDropdownList(int eventId)
        {
            var competitors = new List<SelectListItem>();

            var competitorsDb = _context.CompetitorsInEvent.OrderBy(c => c.EventNumber).ThenBy(c => c.Competitor.LastName).ThenBy(c => c.Competitor.FirstName).Where(c => c.EventId.Equals(eventId) && c.OutOfCompetition == false).ToList();
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

        private async Task<bool> InvalidateCacheInApi(int eventId)
        {
            try
            {
                var response = await _apiClient.PostToApiAsync($"Deelnemer/invalidate-cache?eventId={eventId}");
                return response.IsSuccessStatusCode;
            }
            catch(HttpRequestException ex)
            {
                Console.WriteLine($"[ERROR] HTTP fout bij cache invalidatie: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Onverwachte fouten
                Console.WriteLine($"[ERROR] Onverwachte fout: {ex}");
            }
            return false;
        }
    }
}