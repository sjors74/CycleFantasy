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
            var databaseContext = _context.Stages;
            if (searchEventId > 0)
            {
                vm.Stages = await databaseContext.Where(e => e.EventId == searchEventId).ToListAsync();
                return View(vm);
            }
            vm.Stages = await databaseContext.ToListAsync();
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

            return View(stage);
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
        public async Task<IActionResult> Create([Bind("Id,StageName,StageOrder,StartLocation,FinishLocation,EventId")] Stage stage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(stage);
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
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", stage.EventId);
            return View(stage);
        }

        // POST: Stages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StageName,StageOrder,StartLocation,FinishLocation,EventId")] Stage stage)
        {
            if (id != stage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StageExists(stage.Id))
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

            return View(stage);
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
    }
}
