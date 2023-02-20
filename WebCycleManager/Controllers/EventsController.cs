using Domain.Context;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class EventsController : Controller
    {
        private readonly DatabaseContext _context;

        public EventsController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            var vm = new EventViewModel();
            var events = await _context.Events
                          .OrderByDescending(e => e.EventYear)
                          .ThenBy(e => e.StartDate)
                          .ToListAsync();

            foreach (var e in events)
            {
                vm.Events.Add(CreateViewModel(e));
            }
            return View(vm);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            var stagesList = new List<StageViewModel>();
            var stages = _context.Stages.Where(e => e.EventId.Equals(@event.EventId)).OrderBy(c => c.StageOrder).ToList();
            foreach (var stage in stages)
            {
                var stagesViewModel =
                    new StageViewModel
                    {
                        StageId = stage.Id,
                        StageName = stage.StageName,
                        StageOrder = stage.StageOrder,
                        StartLocation = stage.StartLocation,
                        FinishLocation = stage.FinishLocation,
                        EventId = stage.EventId,
                        EventName = stage.Event.EventName
                    };
                stagesViewModel.AantalPosities = _context.Results.Where(r => r.StageId.Equals(stage.Id)).Count();
                stagesList.Add(stagesViewModel);

            }
            var vm = CreateViewModel(@event);
            vm.Stages = stagesList;
            vm.StagesInEvent = stagesList.Count;
            return View(vm);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            ViewData["ConfigurationId"] = new SelectList(_context.Configurations, "Id", "ConfigurationType");
            return View();
        }

        // POST: Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Year,StartDate,EndDate,ConfigurationId")] EventItemViewModel @event)
        {
            if (ModelState.IsValid)
            {
                var e = CreateFromViewModel(@event);
                _context.Add(e); 
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            var vm = CreateViewModel(@event);
            ViewData["ConfigurationId"] = new SelectList(_context.Configurations, "Id", "ConfigurationType");
            return View(vm);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Year,StartDate,EndDate,ConfigurationId")] EventItemViewModel @event)
        {
            if (id != @event.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var e = CreateFromViewModel(@event);
                    _context.Update(e);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.Id))
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

            return View(@event);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            var vm = CreateViewModel(@event);
            return View(vm);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Events == null)
            {
                return Problem("Entity set 'DatabaseContext.Events'  is null.");
            }
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                _context.Events.Remove(@event);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
          return (_context.Events?.Any(e => e.EventId == id)).GetValueOrDefault();
        }

        public EventItemViewModel CreateViewModel(Event @event)
        {
            var vm = new EventItemViewModel
            {
                Id = @event.EventId,
                Name = @event.EventName,
                Year = @event.EventYear,
                StartDate = (DateTime)@event.StartDate,
                EndDate = (DateTime)@event.EndDate,
                ConfigurationId = @event.ConfigurationId
            };
            return vm;
        }

        public Event CreateFromViewModel(EventItemViewModel vm)
        {
            var @event = _context.Events.Find(vm.Id);
            try
            {
                if (@event == null)
                {
                    @event = new Event();
                }
                @event.EventName = vm.Name;
                @event.EventYear = vm.Year;
                @event.StartDate = vm.StartDate;
                @event.EndDate = vm.EndDate;
                @event.ConfigurationId = vm.ConfigurationId;

                return @event;
                        
            }
            catch
            {
                throw;
            }
        }
    }
}
