﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Domain.Context;
using Domain.Models;

namespace WebCycleManager.Controllers
{
    public class ConfigurationItemsController : Controller
    {
        private readonly DatabaseContext _context;

        public ConfigurationItemsController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: ConfigurationItems
        public async Task<IActionResult> Index()
        {
            var databaseContext = _context.ConfigurationItems.Include(c => c.Configuration);
            return View(await databaseContext.ToListAsync());
        }

        // GET: ConfigurationItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ConfigurationItems == null)
            {
                return NotFound();
            }

            var configurationItem = await _context.ConfigurationItems
                .Include(c => c.Configuration)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (configurationItem == null)
            {
                return NotFound();
            }

            return View(configurationItem);
        }

        // GET: ConfigurationItems/Create
        public IActionResult Create()
        {
            ViewData["ConfigurationId"] = new SelectList(_context.Configurations, "Id", "ConfigurationType");
            return View();
        }

        // POST: ConfigurationItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Position,Score,ConfigurationId")] ConfigurationItem configurationItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(configurationItem);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Configurations", new { id = configurationItem.ConfigurationId });
            }
            ViewData["ConfigurationId"] = new SelectList(_context.Configurations, "Id", "ConfigurationType", configurationItem.ConfigurationId);
            return View(configurationItem);
        }

        // GET: ConfigurationItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ConfigurationItems == null)
            {
                return NotFound();
            }

            var configurationItem = await _context.ConfigurationItems.FindAsync(id);
            if (configurationItem == null)
            {
                return NotFound();
            }
            ViewData["ConfigurationId"] = new SelectList(_context.Configurations, "Id", "ConfigurationType", configurationItem.ConfigurationId);
            return View(configurationItem);
        }

        // POST: ConfigurationItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Position,Score,ConfigurationId")] ConfigurationItem configurationItem)
        {
            if (id != configurationItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(configurationItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConfigurationItemExists(configurationItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Configurations", new { id = configurationItem.ConfigurationId });
            }
            ViewData["ConfigurationId"] = new SelectList(_context.Configurations, "Id", "ConfigurationType", configurationItem.ConfigurationId);
            return View(configurationItem);
        }

        // GET: ConfigurationItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ConfigurationItems == null)
            {
                return NotFound();
            }

            var configurationItem = await _context.ConfigurationItems
                .Include(c => c.Configuration)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (configurationItem == null)
            {
                return NotFound();
            }

            return View(configurationItem);
        }

        // POST: ConfigurationItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ConfigurationItems == null)
            {
                return Problem("Entity set 'DatabaseContext.ConfigurationItems'  is null.");
            }
            var configurationItem = await _context.ConfigurationItems.FindAsync(id);
            if (configurationItem != null)
            {
                _context.ConfigurationItems.Remove(configurationItem);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Configurations", new { id = configurationItem.ConfigurationId });
        }

        private bool ConfigurationItemExists(int id)
        {
          return (_context.ConfigurationItems?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
