using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Domain.Context;
using Domain.Models;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class StagesController : Controller
    {
        private readonly DatabaseContext _context;

        public StagesController(DatabaseContext context)
        {
            _context = context;
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
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stage == null)
            {
                return NotFound();
            }

            var vm = CreateViewModel(stage);

            return View(vm);
        }

        // GET: Stages/Create
        public IActionResult Create()
        {
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName");
            return View();
        }

        // POST: Stages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StageId,StageName,StageOrder,StartLocation,FinishLocation,EventId")] StageViewModel stage)
        {
            if (ModelState.IsValid)
            {
                var s = CreateFromViewModel(stage);

                _context.Add(s);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", stage.EventId);
            return View(stage);
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
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", stage.EventId);
            return View(vm);
        }

        // POST: Stages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StageId,StageName,StageOrder,StartLocation,FinishLocation,EventId")] StageViewModel stage)
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
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", stage.EventId);
            return View(stage);
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
                return Problem("Entity set 'DatabaseContext.Stages'  is null.");
            }
            var stage = await _context.Stages.FindAsync(id);
            if (stage != null)
            {
                _context.Stages.Remove(stage);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StageExists(int id)
        {
          return (_context.Stages?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public StageViewModel CreateViewModel(Stage stage)
        {
            var vm = new StageViewModel
            {
                StageId = stage.Id,
                StageName = stage.StageName,
                StageOrder = stage.StageOrder,
                StartLocation = stage.StartLocation,
                FinishLocation = stage.FinishLocation,
                EventId = stage.EventId,
                EventName = stage.Event == null ? string.Empty : stage.Event.EventName,                
                EventYear = stage.Event == null ? int.MinValue : stage.Event.EventYear,
            };
            return vm;
        }

        public Stage CreateFromViewModel(StageViewModel vm)
        {
            var stage = _context.Stages.Find(vm.StageId);
            try
            {
                if (stage == null)
                {
                    stage = new Stage();
                }
                stage.StageName = vm.StageName;
                stage.StageOrder = vm.StageOrder;
                stage.StartLocation = vm.StartLocation;
                stage.FinishLocation = vm.FinishLocation;
                stage.EventId = vm.EventId;

                return stage;
            }
            catch
            {
                throw;
            }
        }
    }
}
