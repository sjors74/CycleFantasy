using CycleManager.Domain.Models;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class EventsController : Controller
    {
        private IEventService _eventService;
        private ITeamService _teamService;
        private IStageService _stageService;
        private IResultService _resultService;
        private IConfigurationService _configurationService;

        public EventsController(IEventService eventService, ITeamService teamService,
            IStageService stageService, IResultService resultService, 
            IConfigurationService configurationService)
        {
            _eventService = eventService;
            _teamService = teamService;
            _stageService = stageService;
            _resultService = resultService;
            _configurationService = configurationService;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            var vm = new EventViewModel();
            var events = await _eventService.GetAllEvents();

            foreach (var e in events)
            {
                vm.Events.Add(CreateViewModel(e));
            }
            return View(vm);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id is not int eventId)
                return NotFound();

            var vm = await _eventService.GetEventDetailsViewModelById(eventId);

            if (vm == null)
                return NotFound();

            return View(vm);
        }

        // GET: Events/Create
        public async Task<ActionResult> Create()
        {
            var configurations = await _configurationService.GetAllConfigurations();
            ViewData["ConfigurationId"] = new SelectList(configurations, "Id", "ConfigurationType");
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Code,Year,StartDate,EndDate,Slogan,CountryCode,ColorName,ConfigurationId,IsActive,ShowPodium")] EventItemViewModel @event)
        {
            if (ModelState.IsValid)
            {
                var e = await CreateFromViewModel(@event);
                await  _eventService.Create(e);
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

            var @event = await _eventService.GetEventById((int)id);
            if (@event == null)
            {
                return NotFound();
            }
            var vm = CreateViewModel(@event);
            var configurations = await _configurationService.GetAllConfigurations();
            ViewData["ConfigurationId"] = new SelectList(configurations, "Id", "ConfigurationType");
            return View(vm);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Code,Year,StartDate,EndDate,Slogan,CountryCode,ColorName,ConfigurationId,IsActive,ShowPodium")] EventItemViewModel @event)
        {
            if (id != @event.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var e = await CreateFromViewModel(@event);

                    await _eventService.Update(e);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (_eventService.GetEventById(@event.Id) == null)
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
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _eventService.GetEventById((int)id);
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
            var @event = await _eventService.GetEventById(id);
            if (@event != null)
            {
                await _eventService.Delete(@event);
            }
            
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ManageTeams(int id)
        {
            var eventEntity = await _eventService.GetEventById(id);
            if(eventEntity == null) return NotFound();

            var allTeams = await _teamService.GetAll();

            var model = new EventTeamsViewModel
            {
                EventId = eventEntity.EventId,
                EventName = eventEntity.EventName,
                Teams = allTeams.Select(t => new TeamSelection
                {
                    TeamId = t.TeamId,
                    TeamName = t.CurrentTeamName,
                    IsSelected = eventEntity.EventTeams.Any(et => et.TeamId == t.TeamId)
                })
                .OrderByDescending(t => t.IsSelected)
                .ThenBy(t => t.TeamName)
                .ToList()
            };

            return PartialView("_ManageTeamsPartial",model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageTeams(EventTeamsViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var eventEntity = await _eventService.GetEventById(vm.EventId);

                if (eventEntity == null) return NotFound();

                // Huidige koppelingen verwijderen
                eventEntity.EventTeams?.Clear();

                // Nieuwe koppelingen toevoegen
                foreach (var team in vm.Teams.Where(t => t.IsSelected))
                {
                    eventEntity.EventTeams?.Add(new EventTeam
                    {
                        EventId = vm.EventId,
                        TeamId = team.TeamId
                    });
                }

                await _eventService.Update(eventEntity);

                return RedirectToAction("Edit", new { id = vm.EventId });
            }
            return PartialView("_ManageTeamsPartial", vm);
        }

        public EventItemViewModel CreateViewModel(Event @event)
        {
            var vm = new EventItemViewModel
            {
                Id = @event.EventId,
                Name = @event.EventName,
                Code = @event.EventCode,
                Year = @event.EventYear,
                StartDate = @event.StartDate.HasValue ? (DateTime)@event.StartDate : DateTime.MinValue,
                EndDate = @event.EndDate.HasValue ? (DateTime)@event.EndDate : DateTime.MaxValue,
                Slogan = @event.Slogan,
                ColorName = @event.ColorName,
                CountryCode = @event.CountryCode,
                IsActive = @event.IsActive,
                ShowPodium = @event.ShowPodium,
                ConfigurationId = @event.ConfigurationId,
                SelectedTeamsCount = @event.EventTeams?.Count ?? 0
            };
            return vm;
        }

        public async Task<Event> CreateFromViewModel(EventItemViewModel vm)
        {
            var @event = await _eventService.GetEventById(vm.Id);
            try
            {
                if (@event == null)
                {
                    @event = new Event();
                }
                @event.EventName = vm.Name;
                @event.EventCode = vm.Code;
                @event.EventYear = vm.Year;
                @event.StartDate = vm.StartDate;
                @event.EndDate = vm.EndDate;
                @event.Slogan = vm.Slogan;
                @event.CountryCode = vm.CountryCode;
                @event.ColorName = vm.ColorName;
                @event.IsActive = vm.IsActive;
                @event.ShowPodium = vm.ShowPodium;
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
