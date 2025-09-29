using CycleManager.Services;
using CycleManager.Services.Interfaces;
using DataAccessEF.Migrations;
using Domain.Context;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class StagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStageService _stageService;
        private readonly IEventService _eventService;

        public StagesController(ApplicationDbContext context, IStageService stageService, IEventService eventService)
        {
            _context = context;
            _stageService = stageService;
            _eventService = eventService;
        }

        // GET: Stages
        public async Task<IActionResult> Index(int searchEventId)
        {
            var vm = new StagesViewModel();
            vm.CurrentSearchEventId = searchEventId;
            var events = await _context.Events.OrderBy(e => e.EventName).ToListAsync();
            var eventSelectList = new List<SelectListItem>();
            foreach(var e in events)
            {
                eventSelectList.Add(new SelectListItem { Text = string.Concat(e.EventName, " ", e.EventYear), Value = e.EventId.ToString() });
            }
            vm.Events = eventSelectList;
            var stages = _context.Stages;

            //if search filter has value, return filtered stage list
            if (searchEventId > 0)
            {
                var stagesDb = await stages.Where(e => e.EventId == searchEventId).ToListAsync();
                foreach(var s in stagesDb)
                {
                    vm.Stages.Add(CreateViewModel(s));
                }    
                return View(vm);
            }


            foreach (var s in stages)
            {
                vm.Stages.Add(CreateViewModel(s));
            }

            return View(vm);
        }

        // GET: Stages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Stages == null)
            {
                return NotFound();
            }

            var stage = await _context.Stages
                .Include(e => e.Event)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stage == null)
            {
                return NotFound();
            }

            var vm = CreateViewModel(stage);

            return View(vm);
        }

        // GET: Stages/Create
        //public IActionResult Create()
        //{
        //    ViewData["EventId"] = new SelectList(
        //        _context.Events
        //            .Select(e => new {
        //                e.EventId,
        //                Text = e.EventName + " (" + e.EventYear + ")"
        //            })
        //            .ToList(),
        //        "EventId",
        //        "Text"
        //    );

        //    return View();
        //}

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
        public async Task<IActionResult> CreateAjax(StageCreateViewModel stage)
        {
            if (!ModelState.IsValid)
            {
                // Return partial met errors
                stage.Events = await GetEventSelectListAsync();
                return PartialView("_ManageStagesPartial", stage);
            }

            var existingEvent = await _eventService.GetEventById(stage.EventId);

            if (existingEvent == null)
            {
                return Json(new { succes = false, message = "Evenement niet gevonden." });
            }

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

            _context.Add(entity);
            await _context.SaveChangesAsync();

            // Succes: return JSON
            return Json(new 
            { 
                success = true, 
                redirectUrl = Url.Action("Edit", "Events", new { id = stage.EventId })
            });
        }
            

        // GET: Stages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Stages == null)
            {
                return NotFound();
            }

            var stage = await _context.Stages.FindAsync(id);
            if (stage == null)
            {
                return NotFound();
            }
            var vm = CreateViewModel(stage);
            ViewData["EventId"] = new SelectList(
                _context.Events
                    .Select(e => new {
                        e.EventId,
                        Text = e.EventName + " (" + e.EventYear + ")"
                    })
                    .ToList(),
                "EventId",
                "Text",
                stage.EventId
            );
            return View(vm);
        }

        // POST: Stages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StageId,StageDate,StageName,StageOrder,StartLocation,FinishLocation,NoScore,NoScoreDescription,EventId")] StageViewModel stage)
        {
            if (id != stage.StageId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var s = CreateFromViewModel(stage);

                    _context.Update(s);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StageExists(stage.StageId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventId"] = new SelectList(
                _context.Events
                    .Select(e => new {
                        e.EventId,
                        Text = e.EventName + " (" + e.EventYear + ")"
                    })
                    .ToList(),
                "EventId",
                "Text",
                stage.EventId
            );
            return View(stage);
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
                StageDate = DateOnly.FromDateTime(stage.StageDate),
                StartLocation = stage.StartLocation,
                FinishLocation = stage.FinishLocation
            };
            return PartialView("_EditStageModal", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAjax(StageViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_EditStageModal", model);

            var stage = await _stageService.GetStageById(model.StageId);
            if (stage == null) return NotFound();

            stage.StageName = model.StageName;
            stage.StageOrder = model.StageOrder;
            stage.StageDate = model.StageDate.ToDateTime(TimeOnly.MinValue);
            stage.StartLocation = model.StartLocation;
            stage.FinishLocation = model.FinishLocation;

            await _stageService.UpdateStage(stage);

            return Json(new { 
                success = true,
                stage = new
                {
                    id = stage.Id,
                    date = stage.StageDate.ToString("dd-MM-yyyy"),
                    name = stage.StageName,
                    start = stage.StartLocation,
                    finish = stage.FinishLocation
                }
            });
        }

        // GET: Stages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Stages == null)
            {
                return NotFound();
            }

            var stage = await _context.Stages
                .Include(s => s.Event)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stage == null)
            {
                return NotFound();
            }
            var vm = CreateViewModel(stage);
            return View(vm);
        }

        // POST: Stages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Stages == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Stages'  is null.");
            }
            var stage = await _context.Stages.FindAsync(id);
            if (stage != null)
            {
                _context.Stages.Remove(stage);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            bool deleted = await _stageService.DeleteStage(id);
            return Json(new { success = deleted });
        }

        private bool StageExists(int id)
        {
          return (_context.Stages?.Any(e => e.Id == id)).GetValueOrDefault();
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
            };
            return vm;
        }

        public Stage CreateFromViewModel(StageViewModel vm)
        {
            var stage = _context.Stages
                .Include(s => s.Event)
                .FirstOrDefault(s => s.Id == vm.StageId);

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

                return stage;
            }
            catch
            {
                throw;
            }
        }
        private async Task<IEnumerable<SelectListItem>> GetEventSelectListAsync()
        {
            return await _context.Events
                .OrderByDescending(e => e.EventYear)
                .ThenBy(e => e.StartDate)
                .Select(e => new SelectListItem
                {
                    Value = e.EventId.ToString(),
                    Text = e.EventName + " (" + e.EventYear + ")"
                })
                .ToListAsync();
        }
    }
}
