using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using WebCycleManager.Helpers;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class ResultsController : Controller
    {
        private readonly IResultService _resultService;
        private readonly IApiClient _apiClient;
        private readonly IScoreService _scoreService;

        public ResultsController(IResultService resultService, IApiClient apiClient, IScoreService scoreService)
        {
            _resultService = resultService;
            _apiClient = apiClient;
            _scoreService = scoreService;
        }

        // GET: Results
        public async Task<IActionResult> Index(int stageId)
        {
            //first get stage-data
            var stage = await _resultService.GetStageByIdAsync(stageId);
            if (stage == null) return NotFound();

            var currentEvent = stage.Event;
            var config = currentEvent.Configuration;

            var results = await _resultService.GetResultsByStageAsync(stageId);
            var competitorsInEvent = await _resultService.GetCompetitorsInEventAsync(currentEvent.EventId);
            var configItems = await _resultService.GetConfigurationItemsByConfigAsync(config.Id);

            var resultItems = configItems.Select(ci =>
                {
                    var result = results.FirstOrDefault(r => r.ConfigurationItem.Position == ci.Position);
                    int selectedCompetitorId = result?.CompetitorInEventId ?? 0;
                    string competitorName = result?.CompetitorInEvent?.CompetitorInTeam?.Competitor != null
                     ? _resultService.GetCompetitorFullName(result.CompetitorInEvent.CompetitorInTeam.Competitor.CompetitorId)
                     : string.Empty;

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
                stage.NoScore,
                stage.NoScoreDescription,
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
            var resultsToAdd = new List<Result>();

            foreach (var item in model.Results)
            {
                var cieList = await _resultService.GetCompetitorsInEventAsync(model.EventId);
                var cie = cieList.FirstOrDefault(c => c.CompetitorInTeam.Competitor.CompetitorId == item.SelectedCompetitorId);
                if(cie != null)
                {
                    var configItems = await _resultService.GetConfigurationItemsByConfigAsync(model.ConfigurationId);
                    var configurationItem = configItems.FirstOrDefault(c => c.Position == item.Position);
                    if (configurationItem != null)
                    {
                        resultsToAdd.Add(new Result
                        {
                            CompetitorInEventId = cie.Id,
                            StageId = model.StageId,
                            ConfigurationItemId = configurationItem.Id
                        });

                    }
                }
            }

            await _resultService.AddResultsAsync(resultsToAdd);
            await _scoreService.UpdateScoresForStageAsync(model.EventId, model.StageId);
            await InvalidateCacheInApi(model.EventId);

            return RedirectToAction("Index", new { stageId = model.StageId});
        }

        //private int GetPositionFromKey(string key)
        //{
        //    key = key.Substring(key.IndexOf("[") + 1);
        //    key = key.Substring(0, key.IndexOf("]"));
        //    var positon = int.TryParse(key, out var positonNumber);
        //    return positonNumber + 1;
        //}

        // GET: Results/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null || _context.Results == null)
        //    {
        //        return NotFound();
        //    }

        //    var result = await _context.Results
        //        .Include(r => r.CompetitorInEvent)
        //        .Include(r => r.Stage)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (result == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(result);
        //}

        // GET: Results/Create
        //public IActionResult Create(int stageId)
        //{
        //    var stage = _context.Stages
        //        .Include(s => s.Event)
        //            .ThenInclude(e => e.Configuration)
        //                .ThenInclude(c => c.ConfigurationItems)
        //        .FirstOrDefault(s => s.Id == stageId);

        //    if (stage != null)
        //    {
        //        var config = stage.Event.Configuration;
        //        var configItems = config.ConfigurationItems;

        //        var resultItems = configItems.Select(ci => new ResultItemViewModel
        //        {
        //            Id = ci.Id,
        //            Position = ci.Position,
        //            CompetitorName = string.Empty
        //        }).ToList();

        //        var competitorsInEvent = _context.CompetitorsInEvent
        //              .Where(c => c.EventId.Equals(stage.EventId) && !c.OutOfCompetition)
        //              .Include(c => c.CompetitorInTeam)
        //                .ThenInclude(c => c.Competitor)
        //              .ToList();

        //        var rvm = new ResultViewModel(
        //                stage.Id, 
        //                stage.EventId, 
        //                config.Id, 
        //                $"Etappe {stage.StageName}: {stage.StartLocation}-{stage.FinishLocation}", 
        //                stage.NoScore,
        //                stage.NoScoreDescription,
        //                configItems.Count, 
        //                resultItems,
        //                competitorsInEvent
        //                );

        //        return View(rvm);
        //    }


        //    return NotFound();
        //}

        // POST: Results/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,StageId,CompetitorId")] Result result)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(result);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["CompetitorId"] = new SelectList(_context.Competitors.OrderBy(c => c.FirstName), "CompetitorId", "CompetitorName", result.CompetitorInEventId);
        //    ViewData["StageId"] = new SelectList(_context.Stages, "Id", "FinishLocation", result.StageId);
        //    return View(result);
        //}

        // GET: Results/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var result = await _resultService.GetResultByIdAsync(id.Value);
            if (result == null) return NotFound();
            
            var vm = new ResultItemViewModel
            {
                Id = result.Id,
                StageId = result.StageId,
                Position = result.ConfigurationItem.Position,
                CompetitorName = result.CompetitorInEvent.CompetitorInTeam.Competitor.CompetitorName
            };

            return View(vm);
        }

        // POST: Results/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _resultService.GetResultByIdAsync(id);
            if (result == null) return NotFound();

            await _resultService.DeleteResultAsync(result);
            return RedirectToAction(nameof(Index), new { stageId = result.StageId });
        }

        //private bool ResultExists(int id)
        //{
        //    return (_context.Results?.Any(e => e.Id == id)).GetValueOrDefault();
        //}

        //private string GetCompetitorFullName(int competitorId)
        //{
        //    var competitor = _context.Competitors.FirstOrDefault(c => c.CompetitorId == competitorId);
        //    return competitor != null ? $"{competitor.FirstName} {competitor.LastName}" : string.Empty;
        //}

        //private int GetResultId(int competitorId, int stageId)
        //{
        //    var r = _context.Results.FirstOrDefault(c => c.StageId == stageId && c.CompetitorInEvent.CompetitorInTeamId == competitorId);
        //    if (r != null)
        //    {
        //        return r.Id;
        //    }

        //    return 0;
        //}

        //public IEnumerable<SelectListItem> GetDropdownList(int eventId)
        //{
        //    var competitors = new List<SelectListItem>();

        //    var competitorsDb = _context.CompetitorsInEvent.OrderBy(c => c.EventNumber).ThenBy(c => c.CompetitorInTeam.Competitor.LastName).ThenBy(c => c.CompetitorInTeam.Competitor.FirstName).Where(c => c.EventId.Equals(eventId) && c.OutOfCompetition == false).ToList();
        //    var groupedCompetitors = competitorsDb
        //        .GroupBy(x => x.CompetitorInTeam.Competitor.CompetitorInTeams
        //            .FirstOrDefault()?.Team?.CurrentTeamName ?? "Onbekend");

        //    foreach (var group in groupedCompetitors)
        //    {
        //        var optionGroup = new SelectListGroup() { Name = group.Key };
        //        foreach (var item in group)
        //        {
        //            competitors.Add(new SelectListItem()
        //            {
        //                Value = item.Id.ToString(),
        //                Text = item.CompetitorInTeam.Competitor.CompetitorName,
        //                Group = optionGroup
        //            });
        //        }
        //    }
        //    return competitors;
        //}

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