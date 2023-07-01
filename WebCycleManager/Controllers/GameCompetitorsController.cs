using System;
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
    public class GameCompetitorsController : Controller
    {
        private readonly DatabaseContext _context;

        public GameCompetitorsController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: GameCompetitors
        public async Task<IActionResult> Index()
        {
              return _context.GameCompetitors != null ? 
                          View(await _context.GameCompetitors.ToListAsync()) :
                          Problem("Entity set 'DatabaseContext.GameCompetitors'  is null.");
        }

        // GET: GameCompetitors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.GameCompetitors == null)
            {
                return NotFound();
            }

            var gameCompetitor = await _context.GameCompetitors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gameCompetitor == null)
            {
                return NotFound();
            }

            return View(gameCompetitor);
        }

        // GET: GameCompetitors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GameCompetitors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName")] GameCompetitor gameCompetitor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gameCompetitor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(gameCompetitor);
        }

        // GET: GameCompetitors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.GameCompetitors == null)
            {
                return NotFound();
            }

            var gameCompetitor = await _context.GameCompetitors.FindAsync(id);
            if (gameCompetitor == null)
            {
                return NotFound();
            }
            return View(gameCompetitor);
        }

        // POST: GameCompetitors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName")] GameCompetitor gameCompetitor)
        {
            if (id != gameCompetitor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gameCompetitor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameCompetitorExists(gameCompetitor.Id))
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
            return View(gameCompetitor);
        }

        // GET: GameCompetitors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.GameCompetitors == null)
            {
                return NotFound();
            }

            var gameCompetitor = await _context.GameCompetitors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gameCompetitor == null)
            {
                return NotFound();
            }

            return View(gameCompetitor);
        }

        // POST: GameCompetitors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.GameCompetitors == null)
            {
                return Problem("Entity set 'DatabaseContext.GameCompetitors'  is null.");
            }
            var gameCompetitor = await _context.GameCompetitors.FindAsync(id);
            if (gameCompetitor != null)
            {
                _context.GameCompetitors.Remove(gameCompetitor);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GameCompetitorExists(int id)
        {
          return (_context.GameCompetitors?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
