using CycleManager.Services.Interfaces;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class ConfigurationItemsController : Controller
    {
        private readonly IConfigurationService _configurationService;

        public ConfigurationItemsController(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        // GET: ConfigurationItems
        public async Task<IActionResult> Index()
        {
            var configItems = await _configurationService.GetAllConfigurationItems();
            return View(configItems);
        }

        // GET: ConfigurationItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var configurationItem = await _configurationService.GetConfigurationItemById((int)id);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConfigurationItemViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var configurationItem = new ConfigurationItem 
                {
                    ConfigurationId = vm.ConfigurationId, 
                    Position = vm.Position, 
                    Score = vm.Score 
                };

                var success = await _configurationService.CreateItem(configurationItem);

                if (!success)
                {
                    ModelState.AddModelError("Position", "Deze positie bestaat al binnen deze configuratie.");
                }
                else
                {
                    return RedirectToAction("Details", "Configurations", new { id = vm.ConfigurationId });
                }    
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

            var configurationItem = await _configurationService.GetConfigurationItemById((int)id);
            if (configurationItem == null)
            {
                return NotFound();
            }
            var vm = new ConfigurationItemViewModel { Id = configurationItem.Id, Score = configurationItem.Score, Position = configurationItem.Position, ConfigurationId = configurationItem.ConfigurationId };
            ViewData["ConfigurationId"] = new SelectList(await GetConfigurationList(), "Id", "ConfigurationType", vm.ConfigurationId);
            return View(vm);
        }

        // POST: ConfigurationItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ConfigurationItemViewModel vm)
        {
            if (id != vm.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var configurationItem = await _configurationService.GetConfigurationItemById(vm.Id);
                if (configurationItem == null)
                {
                    return NotFound();
                }
                else
                {
                    configurationItem.Position = vm.Position;
                    configurationItem.Score = vm.Score;
                    configurationItem.ConfigurationId = vm.ConfigurationId;

                    var success = await _configurationService.UpdateItem(configurationItem);

                    if (!success)
                    {
                        ModelState.AddModelError("Position", "Deze positie bestaat al binnen deze configuratie.");
                    }
                    else
                    {
                        return RedirectToAction("Details", "Configurations", new { id = vm.ConfigurationId });
                    }
                }
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

            var configurationItem = await _configurationService.GetConfigurationItemById((int)id);
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
            var configurationItem = await _configurationService.GetConfigurationItemById(id);
            if (configurationItem == null)
            {
                return NotFound();
            }
            var configurationId = configurationItem.ConfigurationId;
            await _configurationService.DeleteItem(configurationItem);
            return RedirectToAction("Details", "Configurations", new { id = configurationId });
        }

        private async Task<bool> ConfigurationItemExists(int id)
        {
            var item = await _configurationService.GetConfigurationItemById(id);
            return  item != null;
        }

        private async Task<IEnumerable<Configuration>> GetConfigurationList()
        {
            return await _configurationService.GetAllConfigurations();
        }
    }
}
