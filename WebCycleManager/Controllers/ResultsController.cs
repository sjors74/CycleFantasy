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
            // Stage
            var stage = await _resultService.GetStageByIdAsync(stageId);
            if (stage == null)
                return NotFound();

            var currentEvent = stage.Event;
            var config = currentEvent.Configuration;

            // Data
            var results = await _resultService.GetResultsByStageAsync(stageId);
            var specialResults = await _resultService.GetSpecialResultsByStageAsync(stageId);
            var specials = await _resultService.GetSpecialResultsByStageAsync(stageId);

            var competitorsInEvent = await _resultService.GetCompetitorsInEventAsync(currentEvent.EventId);
            var configItems = await _resultService.GetConfigurationItemsByConfigAsync(config.Id);
            var configurationSpecialItems = await _resultService.GetConfigurationItemSpecialsAsync(config.Id);
            var rows = new List<StageResultRowViewModel>();

            // Normale uitslag
            rows.AddRange(configItems.Select(ci =>
            {
                var result = results.FirstOrDefault(r => r.ConfigurationItem.Position == ci.Position);

                return new StageResultRowViewModel
                {
                    IsSpecial = false,

                    Position = ci.Position,

                    Id = result?.Id ?? 0,
                    SelectedCompetitorId = result?.CompetitorInEventId ?? 0,

                    CompetitorName = result?.CompetitorInEvent?
                        .CompetitorInTeam?
                        .Competitor?
                        .CompetitorName ?? string.Empty
                };
            }));

            // Specials
            rows.AddRange(configurationSpecialItems.Select(s =>
            {
                var result = specialResults.FirstOrDefault(sr => sr.SpecialId == s.Id);

                return new StageResultRowViewModel
                {
                    IsSpecial = true,

                    SpecialId = s.Id,
                    SpecialName = s.Question.ToString(),

                    Id = result?.Id ?? 0,
                    SelectedCompetitorId = result?.CompetitorInEventId ?? 0,

                    CompetitorName = result?.CompetitorInEvent?
                        .CompetitorInTeam?
                        .Competitor?
                        .CompetitorName ?? string.Empty
                };
            }));

            var vm = new ResultViewModel(
                stage.Id,
                stage.EventId,
                config.Id,
                $"Etappe {stage.StageName}: {stage.StartLocation}-{stage.FinishLocation}",
                stage.NoScore,
                stage.NoScoreDescription,
                configItems.Count,
                rows,
                competitorsInEvent
            );

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ResultViewModel model)
        {
            var cieList = await _resultService.GetCompetitorsInEventAsync(model.EventId);
            var configItems = await _resultService.GetConfigurationItemsByConfigAsync(model.ConfigurationId);
            var configurationSpecialItems = await _resultService.GetConfigurationItemSpecialsAsync(model.ConfigurationId);

            var resultsToAdd = new List<Result>();
            var specialResultsToAdd = new List<SpecialResult>();

            foreach (var row in model.Rows)
            {
                if (row.SelectedCompetitorId == 0)
                    continue;

                var cie = cieList.FirstOrDefault(c => c.CompetitorInTeam.Competitor.CompetitorId == row.SelectedCompetitorId);
                if (cie == null)
                    continue;

                if (row.IsSpecial)
                {
                    var specialItem = configurationSpecialItems.FirstOrDefault(s => s.Id == row.SpecialId);
                    if(specialItem != null)
                    {
                        specialResultsToAdd.Add(new SpecialResult
                        {
                            CompetitorInEventId = cie.Id,
                            StageId = model.StageId,
                            SpecialId = specialItem.Id
                        });
                    }
                }
                else
                {
                    var configurationItem = configItems.FirstOrDefault(c => c.Position == row.Position);
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
            await _resultService.AddSpecialResultsAsync(specialResultsToAdd);
            await _scoreService.UpdateScoresForStageAsync(model.EventId, model.StageId);
            await InvalidateCacheInApi(model.EventId);

            return RedirectToAction("Index", new { stageId = model.StageId});
        }

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

        // GET: Results/DeleteSpecial/5
        public async Task<IActionResult> DeleteSpecial(int? id)
        {
            if (id == null) return NotFound();

            var special = await _resultService.GetSpecialResultByIdAsync(id.Value);
            if (special == null) return NotFound();

            var vm = new SpecialResultItemViewModel
            {
                Id = special.Id,
                SpecialId = special.SpecialId ?? 0,
                StageId = special.StageId,
                SpecialName = special.Special.Question.ToString(),
                CompetitorName = special.CompetitorInEvent?.CompetitorInTeam?.Competitor?.CompetitorName ?? string.Empty
            };

            return View(vm);
        }

        // POST: Results/DeleteSpecial/5
        [HttpPost, ActionName("DeleteSpecial")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSpecialConfirmed(int id)
        {
            var special = await _resultService.GetSpecialResultByIdAsync(id);
            if (special == null) return NotFound();

            await _resultService.DeleteSpecialResultAsync(special.Id);

            return RedirectToAction(nameof(Index), new { stageId = special.StageId });
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