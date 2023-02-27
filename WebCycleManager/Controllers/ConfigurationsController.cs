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
    public class ConfigurationsController : Controller
    {
        private readonly DatabaseContext _context;

        public ConfigurationsController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Configurations
        public async Task<IActionResult> Index()
        {
            var vm = new List<ConfigurationViewModel>();
            var configurationsDb = await _context.Configurations.ToListAsync();
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

            var configuration = await _context.Configurations
                .FirstOrDefaultAsync(m => m.Id == id);
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
                _context.Add(configuration);
                await _context.SaveChangesAsync();
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
            var configuration = await _context.Configurations.FindAsync(id);
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
                    var configuration = await _context.Configurations.FindAsync(id);
                    if (configuration != null)
                    {
                        configuration.ConfigurationType = vm.ConfigurationName;
                        _context.Update(configuration);
                        await _context.SaveChangesAsync();
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

            var configuration = await _context.Configurations
                .FirstOrDefaultAsync(m => m.Id == id);
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
            if (_context.Configurations == null)
            {
                return Problem("Entity set 'DatabaseContext.Configurations'  is null.");
            }
            var configuration = await _context.Configurations.FindAsync(id);
            if (configuration != null)
            {
                _context.Configurations.Remove(configuration);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ConfigurationExists(int id)
        {
          return (_context.Configurations?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
