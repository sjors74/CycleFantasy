using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class EventsController : Controller
    {
        private IEventRepository _eventRepository;
        private IStageRepository _stageRepository;
        private IConfigurationRepository _configurationRepository;
        private IResultsRepository _resultsRepository;

        public EventsController(IEventRepository eventRepository, IStageRepository stageRepository, IConfigurationRepository configurationRepository, IResultsRepository resultsRepository)
        {
            _eventRepository = eventRepository;
            _stageRepository = stageRepository;
            _configurationRepository = configurationRepository;
            _resultsRepository = resultsRepository;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            var vm = new EventViewModel();
            var events = await _eventRepository.GetAllEvents();

            foreach (var e in events)
            {
                vm.Events.Add(CreateViewModel(e));
            }
            return View(vm);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = _eventRepository.GetById((int)id);
            if (@event == null)
            {
                return NotFound();
            }

            var stagesList = new List<StageViewModel>();
            var stages = await _stageRepository.GetByEventId(@event.EventId);
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
                        EventName = stage.Event == null ? string.Empty : stage.Event.EventName
                    };
                var results = await _resultsRepository.GetResultsByStageId(stage.Id);
                stagesViewModel.AantalPosities = results;
                stagesList.Add(stagesViewModel);

            }
            var vm = CreateViewModel(@event);
            vm.Stages = stagesList;
            vm.StagesInEvent = stagesList.Count;
            return View(vm);
        }

        // GET: Events/Create
        public async Task<ActionResult> Create()
        {
            var configurations = await _configurationRepository.GetAll();
            ViewData["ConfigurationId"] = new SelectList(configurations, "Id", "ConfigurationType");
            return View();
        }

        // POST: Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Year,StartDate,EndDate,ConfigurationId,IsActive")] EventItemViewModel @event)
        {
            if (ModelState.IsValid)
            {
                var e = CreateFromViewModel(@event);
                _eventRepository.Add(e);
                await _eventRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = _eventRepository.GetById((int)id);
            if (@event == null)
            {
                return NotFound();
            }
            var vm = CreateViewModel(@event);
            var configurations = await _configurationRepository.GetAll();
            ViewData["ConfigurationId"] = new SelectList(configurations, "Id", "ConfigurationType");
            return View(vm);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Year,StartDate,EndDate,ConfigurationId,IsActive")] EventItemViewModel @event)
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

                    _eventRepository.Update(e);
                    await _eventRepository.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (_eventRepository.GetById(@event.Id) == null)
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
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = _eventRepository.GetById((int)id);
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
            var @event = _eventRepository.GetById(id);
            if (@event != null)
            {
                _eventRepository.Remove(@event);
            }
            
            await _eventRepository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public EventItemViewModel CreateViewModel(Event @event)
        {
            var vm = new EventItemViewModel
            {
                Id = @event.EventId,
                Name = @event.EventName,
                Year = @event.EventYear,
                StartDate = @event.StartDate.HasValue ? (DateTime)@event.StartDate : DateTime.MinValue,
                EndDate = @event.EndDate.HasValue ? (DateTime)@event.EndDate : DateTime.MaxValue,
                IsActive = @event.IsActive,
                ConfigurationId = @event.ConfigurationId
            };
            return vm;
        }

        public Event CreateFromViewModel(EventItemViewModel vm)
        {
            var @event = _eventRepository.GetById(vm.Id);
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
                @event.IsActive = vm.IsActive;
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
