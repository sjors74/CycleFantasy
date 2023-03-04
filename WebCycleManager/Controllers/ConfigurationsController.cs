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
using Domain.Interfaces;

namespace WebCycleManager.Controllers
{
    public class ConfigurationsController : Controller
    {
        private readonly IConfigurationRepository _configurationRepository;

        public ConfigurationsController(IConfigurationRepository configurationRepository)
        {
           _configurationRepository = configurationRepository;
        }

        // GET: Configurations
        public async Task<IActionResult> Index()
        {
            var vm = new List<ConfigurationViewModel>();
            var configurationsDb = await _configurationRepository.GetAll();
            foreach(var configuration in configurationsDb)
            {
                vm.Add(new ConfigurationViewModel { ConfigurationName = configuration.ConfigurationType, Id = configuration.Id });
            }
              return View(vm);
        }

        // GET: Configurations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var vm = new ConfigurationViewModel();

            if (id == null )
            {
                return NotFound();
            }

            var configuration = _configurationRepository.GetById((int)id);
            if (configuration == null)
            {
                return NotFound();
            }

            vm.Id = (int)id;
            vm.ConfigurationName = configuration.ConfigurationType;
            if (configuration.ConfigurationItems != null)
            {
                foreach (var item in configuration.ConfigurationItems)
                {
                    vm.ConfigurationItems.Add(new ConfigurationItemViewModel { Id = item.Id, Position = item.Position, Score = item.Score, ConfigurationId = item.ConfigurationId });
                }
            }
            return View(vm);
        }

        // GET: Configurations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Configurations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ConfigurationName")] ConfigurationViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var configuration = new Configuration { ConfigurationType = vm.ConfigurationName };
                _configurationRepository.Add(configuration);
                await _configurationRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // GET: Configurations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var vm = new ConfigurationViewModel();
            var configuration = _configurationRepository.GetById((int)id);
            if (configuration == null)
            {
                return NotFound();
            }
            vm.Id = configuration.Id;
            vm.ConfigurationName = configuration.ConfigurationType;
            if (configuration.ConfigurationItems != null)
            {
                foreach (var item in configuration.ConfigurationItems)
                {
                    vm.ConfigurationItems.Add(new ConfigurationItemViewModel { Id = item.Id, Position = item.Position, Score = item.Score, ConfigurationId = item.ConfigurationId });
                }
            }
            return View(vm);
        }

        // POST: Configurations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ConfigurationName")] ConfigurationViewModel vm)
        {
            if (id != vm.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var configuration = _configurationRepository.GetById((int)id);
                    if (configuration != null)
                    {
                        configuration.ConfigurationType = vm.ConfigurationName;
                        _configurationRepository.Update(configuration);
                        await _configurationRepository.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConfigurationExists(vm.Id))
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
            return View(vm);
        }

        // GET: Configurations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var configuration = _configurationRepository.GetById((int)id);
            if (configuration == null)
            {
                return NotFound();
            }
            var vm = new ConfigurationViewModel { ConfigurationName = configuration.ConfigurationType, Id = configuration.Id };

            return View(vm);
        }

        // POST: Configurations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var configuration = _configurationRepository.GetById(id);
            if (configuration != null)
            {
                _configurationRepository.Remove(configuration);
            }

            await _configurationRepository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ConfigurationExists(int id)
        {
          return (_configurationRepository.GetById(id) != null);
        }
    }
}
