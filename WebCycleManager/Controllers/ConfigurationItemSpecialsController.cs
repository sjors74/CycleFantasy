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
    public class ConfigurationItemSpecialsController : Controller
    {
        private readonly IConfigurationService _configurationService;

        public ConfigurationItemSpecialsController(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        // GET: ConfigurationItems
        public async Task<IActionResult> Index()
        {
            var configItems = await _configurationService.GetAllConfigurationItemSpecials();
            return View(configItems);
        }

        // GET: ConfigurationItemSpecials/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var configurationItem = await _configurationService.GetConfigurationItemSpecialById((int)id);
            if (configurationItem == null)
            {
                return NotFound();
            }

            return View(configurationItem);
        }

        // GET: ConfigurationItemSpecials/Create
        public async Task<IActionResult> Create(int configurationId)
        {
            ViewData["ConfigurationId"] = new SelectList(await GetConfigurationList(), "Id", "ConfigurationType");
            return View();
        }

        // POST: ConfigurationItemSpecials/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConfigurationItemsSpecialViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var configurationItem = new ConfigurationItemSpecial 
                {
                    ConfigurationId = vm.ConfigurationId, 
                    Question = vm.Question, 
                    Score = vm.Score 
                };

                var success = await _configurationService.CreateItemSpecial(configurationItem);

                if (!success)
                {
                    ModelState.AddModelError("Question", "Deze vraag bestaat al binnen deze configuratie.");
                }
                else
                {
                    return RedirectToAction("Details", "Configurations", new { id = vm.ConfigurationId });
                }    
            }
            ViewData["ConfigurationId"] = new SelectList(await GetConfigurationList(), "Id", "ConfigurationType", vm.ConfigurationId);
            return View(vm);
        }

        // GET: ConfigurationItemSpecials/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var configurationItem = await _configurationService.GetConfigurationItemSpecialById((int)id);
            if (configurationItem == null)
            {
                return NotFound();
            }
            var vm = new ConfigurationItemsSpecialViewModel { Id = configurationItem.Id, Score = configurationItem.Score, Question = configurationItem.Question, ConfigurationId = configurationItem.ConfigurationId };
            ViewData["ConfigurationId"] = new SelectList(await GetConfigurationList(), "Id", "ConfigurationType", vm.ConfigurationId);
            return View(vm);
        }

        // POST: ConfigurationItemSpecials/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ConfigurationItemsSpecialViewModel vm)
        {
            if (id != vm.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var configurationItem = await _configurationService.GetConfigurationItemSpecialById(vm.Id);
                if (configurationItem == null)
                {
                    return NotFound();
                }
                else
                {
                    configurationItem.Question = vm.Question;
                    configurationItem.Score = vm.Score;
                    configurationItem.ConfigurationId = vm.ConfigurationId;

                    var success = await _configurationService.UpdateItemSpecial(configurationItem);
                    if (!success)
                    {
                        ModelState.AddModelError("Question", "Deze vraag bestaat al binnen deze configuratie.");
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

        // GET: ConfigurationItemSpecials/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var configurationItem = await _configurationService.GetConfigurationItemSpecialById((int)id);
            if (configurationItem == null)
            {
                return NotFound();
            }
            var vm = new ConfigurationItemsSpecialViewModel {  ConfigurationId= configurationItem.ConfigurationId, Score = configurationItem.Score, Question = configurationItem.Question, Id = configurationItem.Id };
            return View(vm);
        }

        // POST: ConfigurationItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var configurationItem = await _configurationService.GetConfigurationItemSpecialById(id);
            if (configurationItem == null)
            {
                return NotFound();
            }
            var configurationId = configurationItem.ConfigurationId;
            await _configurationService.DeleteItemSpecial(configurationItem);
            return RedirectToAction("Details", "Configurations", new { id = configurationId });
        }

        private async Task<bool> ConfigurationItemSpecialExists(int id)
        {
            var item = await _configurationService.GetConfigurationItemSpecialById(id);
            return  item != null;
        }

        private async Task<IEnumerable<Configuration>> GetConfigurationList()
        {
            return await _configurationService.GetAllConfigurations();
        }
    }
}
