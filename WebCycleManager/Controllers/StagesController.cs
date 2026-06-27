using CycleManager.Domain.Enums;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class StagesController : Controller
    {
        private readonly IStageService _stageService;
        private readonly IEventService _eventService;

        public StagesController(IStageService stageService, IEventService eventService)
        {
            _stageService = stageService;
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new StageCreateViewModel
            {
                Events = await GetEventSelectListAsync()
            };
                
            return View(model);
        }

        // POST: Stages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StageCreateViewModel stage)
        {
            if (ModelState.IsValid)
            {
                var entity = new Stage
                {
                    StageDate = stage.StageDate,
                    StageName = stage.StageName,
                    StageOrder = stage.StageOrder,
                    StartLocation = stage.StartLocation,
                    FinishLocation = stage.FinishLocation,
                    NoScore = stage.NoScore,
                    NoScoreDescription = stage.NoScoreDescription,
                    EventId = stage.EventId
                };

                await _stageService.AddStage(entity);
                return RedirectToAction("Index", new { searchEventId = stage.EventId });
            }
            stage.Events = await GetEventSelectListAsync();
            return View(stage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAjax(ManageStageViewModel model)
        {
            // Work with the nested StageCreateViewModel that the form posts as NewStage.*
            var stage = model?.NewStage ?? new StageCreateViewModel { EventId = model?.EventStages?.EventId ?? 0 };

            // Empty submission?
            if (IsEmptyStage(stage))
            {
                var vm = await BuildManageStagesViewModel(stage.EventId, stage, "Geen stage ingevoerd.");
                return PartialView("~/Views/Events/_ManageStagesPartial.cshtml", vm);
            }

            // Validation
            if (!ModelState.IsValid)
            {
                var vm = await BuildManageStagesViewModel(stage.EventId, stage);
                return PartialView("~/Views/Events/_ManageStagesPartial.cshtml", vm);
            }

            // Persist
            await _stageService.AddStage(new Stage
            {
                StageName = stage.StageName,
                StageDate = stage.StageDate,
                StageOrder = stage.StageOrder,
                StartLocation = stage.StartLocation,
                FinishLocation = stage.FinishLocation,
                EventId = stage.EventId,
                NoScore = stage.NoScore,
                NoScoreDescription = stage.NoScoreDescription
            });

            var successModel = await BuildManageStagesViewModel(stage.EventId);
            return PartialView("~/Views/Events/_ManageStagesPartial.cshtml", successModel);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stage = await _stageService.GetStageById(id.Value);
            if (stage == null) return NotFound();

            var vm = new StageDeleteViewModel
            {
                StageId = stage.Id,
                StageName = stage.StageName,
                StageDescription = $"{stage.StartLocation}->{stage.FinishLocation}",
                EventId = stage.EventId
            };

            return View(vm);
        }

        // POST: Stages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int eventId)
        {                
            await _stageService.DeleteStage(id);

            return RedirectToAction("Details", "Events", new { id = eventId });
        }

        [HttpGet]
        public async Task<IActionResult> EditStage(int id)
        {
            var stage = await _stageService.GetStageById(id);

            if (stage == null)
                return NotFound();

            var vm = new StageViewModel
            {
                StageId = stage.Id,
                EventId = stage.EventId,
                StageName = stage.StageName,
                StageOrder = stage.StageOrder,
                NoScore = stage.NoScore,
                NoScoreDescription = stage.NoScoreDescription,
                StageDate = DateOnly.FromDateTime(stage.StageDate),
                StartLocation = stage.StartLocation,
                FinishLocation = stage.FinishLocation,
                EventStartDate = stage.Event?.StartDate.HasValue == true
                    ? DateOnly.FromDateTime(stage.Event.StartDate.Value)
                    : DateOnly.MinValue,
                EventEndDate = stage.Event?.EndDate.HasValue == true
                    ? DateOnly.FromDateTime(stage.Event.EndDate.Value)
                    : DateOnly.MaxValue,
                ScrapeStatus = stage.ScrapeStatus,
                AvailableStatuses = Enum.GetValues<ScrapeStatus>()
                .Select(x => new SelectListItem
                {
                    Value = x.ToString(),
                    Text = x.ToString()
                })
            };
            return PartialView("_EditStagePartial", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAjax(StageViewModel model)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                || Request.Headers["Accept"].Any(h => h.Contains("application/json"));
            if (!ModelState.IsValid)
            {
                if (isAjax)
                    return PartialView("_EditStagePartial", model);

                return PartialView("EditStagePartial", model);
            }

            var stage = await _stageService.GetStageById(model.StageId);
            if (stage == null) return NotFound();

            stage.StageName = model.StageName;
            stage.StageOrder = model.StageOrder;
            stage.StageDate = model.StageDate.ToDateTime(TimeOnly.MinValue);
            stage.StartLocation = model.StartLocation;
            stage.FinishLocation = model.FinishLocation;
            stage.NoScore = model.NoScore;
            stage.NoScoreDescription = model.NoScoreDescription;
            stage.ScrapeStatus = model.ScrapeStatus;

            await _stageService.UpdateStage(stage);

            if (isAjax)
            {
                return Json(new
                {
                    success = true,
                    stage = new
                    {
                        id = stage.Id,
                        date = stage.StageDate.ToString("dd-MM-yyyy"),
                        name = stage.StageName,
                        start = stage.StartLocation,
                        finish = stage.FinishLocation,
                        noscore = stage.NoScore,
                        noscoredescription = stage.NoScoreDescription
                    }
                });
            }
            else
            {
                var modelPartial = await BuildManageStagesViewModel(stage.EventId);
                return PartialView("~/Views/Events/_ManageStagesPartial.cshtml", modelPartial);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            var stage = await _stageService.GetStageById(id);
            if (stage == null)
                return BadRequest();

            await _stageService.DeleteStage(id);

            var model = await BuildManageStagesViewModel(stage.EventId);
            return PartialView("~/Views/Events/_ManageStagesPartial.cshtml", model);
        }

        public StageViewModel CreateViewModel(Stage stage)
        {
            var vm = new StageViewModel
            {
                StageDate = DateOnly.FromDateTime(stage.StageDate),
                StageId = stage.Id,
                StageName = stage.StageName,
                StageOrder = stage.StageOrder,
                StartLocation = stage.StartLocation,
                FinishLocation = stage.FinishLocation,
                NoScore = stage.NoScore,
                NoScoreDescription = stage.NoScoreDescription,
                EventId = stage.EventId,
                EventName = stage.Event == null ? string.Empty : stage.Event.EventName,                
                EventYear = stage.Event == null ? int.MinValue : stage.Event.EventYear,
                ScrapeStatus = stage.ScrapeStatus,
                AvailableStatuses = Enum.GetValues<ScrapeStatus>()
                    .Select(x => new SelectListItem
                    {
                        Value = x.ToString(),
                        Text = x.ToString()
                    })
            };
            return vm;
        }

        public async Task<Stage> CreateFromViewModel(StageViewModel vm)
        {
            var stage = await _stageService.GetStageById(vm.StageId);

            try
            {
                if (stage == null)
                { 
                    stage = new Stage();
                }
                stage.StageDate = vm.StageDate.ToDateTime(TimeOnly.MinValue);
                stage.StageName = vm.StageName;
                stage.StageOrder = vm.StageOrder;
                stage.StartLocation = vm.StartLocation;
                stage.FinishLocation = vm.FinishLocation;
                stage.NoScore = vm.NoScore;
                stage.NoScoreDescription = vm.NoScoreDescription;
                stage.EventId = vm.EventId;
                stage.ScrapeStatus = vm.ScrapeStatus;  

                return stage;
            }
            catch
            {
                throw;
            }
        }
        private async Task<IEnumerable<SelectListItem>> GetEventSelectListAsync()
        {
            return _eventService.GetAllEvents().Result
                .OrderByDescending(e => e.EventYear)
                .ThenBy(e => e.StartDate)
                .Select(e => new SelectListItem
                {
                    Value = e.EventId.ToString(),
                    Text = e.EventName + " (" + e.EventYear + ")"
                })
                .ToList();
        }

        private bool IsEmptyStage(StageCreateViewModel stage)
        {
            return string.IsNullOrWhiteSpace(stage.StageName)
                && stage.StageDate == default
                && string.IsNullOrWhiteSpace(stage.StartLocation)
                && string.IsNullOrWhiteSpace(stage.FinishLocation);
        }

        private async Task<ManageStageViewModel> BuildManageStagesViewModel(
            int eventId,
            StageCreateViewModel? newStage = null,
            string? uiErrorMessage = null)
        {
            var eventEntity = await _eventService.GetEventById(eventId);
            var stages = await _stageService.GetStagesByEventId(eventId) ?? new List<Stage>();

            return new ManageStageViewModel
            {
                EventStages = new EventStagesViewModel
                {
                    EventId = eventId,
                    EventName = eventEntity.EventName ?? "Onbekend evenement",
                    EventStartDate = eventEntity.StartDate ?? DateTime.Today,
                    EventEndDate = eventEntity.EndDate ?? DateTime.Today.AddDays(1),
                    Stages = stages
                },
                NewStage = newStage ?? new StageCreateViewModel {  EventId = eventId},
                UiErrorMessage = uiErrorMessage
            };
        }
    }
}
