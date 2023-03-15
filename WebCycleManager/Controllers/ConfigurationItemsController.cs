using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class ConfigurationItemsController : Controller
    {
        private readonly IConfigurationItemRepository _configurationItemRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public ConfigurationItemsController(IConfigurationItemRepository configurationItemRepository, IConfigurationRepository configurationRepository)
        {
            _configurationItemRepository = configurationItemRepository;
            _configurationRepository = configurationRepository;
        }

        // GET: ConfigurationItems
        public async Task<IActionResult> Index()
        {
            var configItems = await _configurationItemRepository.GetAll();
            return View(configItems);
        }

        // GET: ConfigurationItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var configurationItem = _configurationItemRepository.GetById((int)id);
            if (configurationItem == null)
            {
                return NotFound();
            }

            return View(configurationItem);
        }

        // GET: ConfigurationItems/Create
        public async Task<IActionResult> Create(int configurationId)
        {
            ViewData["ConfigurationId"] = new SelectList(await GetConfigurationList(), "Id", "ConfigurationType");
            return View();
        }

        // POST: ConfigurationItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Position,Score,ConfigurationId")] ConfigurationItemViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var configurationItem = new ConfigurationItem { ConfigurationId = vm.ConfigurationId, Position = vm.Position, Score = vm.Score };
                _configurationItemRepository.Add(configurationItem);
                await _configurationItemRepository.SaveChangesAsync();
                return RedirectToAction("Details", "Configurations", new { id = vm.ConfigurationId });
            }
            ViewData["ConfigurationId"] = new SelectList(await GetConfigurationList(), "Id", "ConfigurationType", vm.ConfigurationId);
            return View(vm);
        }

        // GET: ConfigurationItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var configurationItem = await _configurationItemRepository.GetById((int)id);
            if (configurationItem == null)
            {
                return NotFound();
            }
            var vm = new ConfigurationItemViewModel { Id = configurationItem.Id, Score = configurationItem.Score, Position = configurationItem.Position, ConfigurationId = configurationItem.ConfigurationId };
            ViewData["ConfigurationId"] = new SelectList(await GetConfigurationList(), "Id", "ConfigurationType", vm.ConfigurationId);
            return View(vm);
        }

        // POST: ConfigurationItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Position,Score,ConfigurationId")] ConfigurationItemViewModel vm)
        {
            if (id != vm.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var configurationItem = await _configurationItemRepository.GetById(vm.Id);
                    if(configurationItem != null)
                    { 
                        configurationItem.Position = vm.Position;
                        configurationItem.Score = vm.Score;
                        configurationItem.ConfigurationId = vm.ConfigurationId;
                        _configurationItemRepository.Update(configurationItem);
                        await _configurationItemRepository.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConfigurationItemExists(vm.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Configurations", new { id = vm.ConfigurationId });
            }
            ViewData["ConfigurationId"] = new SelectList(await GetConfigurationList(), "Id", "ConfigurationType", vm.ConfigurationId);
            return View(vm);
        }

        // GET: ConfigurationItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var configurationItem = await _configurationItemRepository.GetById((int)id);
            if (configurationItem == null)
            {
                return NotFound();
            }
            var vm = new ConfigurationItemViewModel {  ConfigurationId= configurationItem.ConfigurationId, Score = configurationItem.Score, Position = configurationItem.Position, Id = configurationItem.Id };
            return View(vm);
        }

        // POST: ConfigurationItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var configurationItem = await _configurationItemRepository.GetById(id);
            if (configurationItem == null)
            {
                return NotFound();
            }
            var configurationId = configurationItem.ConfigurationId;
            _configurationItemRepository.Remove(configurationItem);
            await _configurationItemRepository.SaveChangesAsync();
            return RedirectToAction("Details", "Configurations", new { id = configurationId });
        }

        private bool ConfigurationItemExists(int id)
        {
          return (_configurationItemRepository.GetById(id) != null);
        }

        private async Task<IEnumerable<Configuration>> GetConfigurationList()
        {
            return await _configurationRepository.GetAll();
        }
    }
}
