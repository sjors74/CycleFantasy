using Domain.Context;
using DataAccessEF.Extensions;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace WebCycleManager.Controllers
{
    public class CompetitorsController : Controller
    {
        private readonly DatabaseContext _context;

        public CompetitorsController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Competitors
        public async Task<IActionResult> Index(string currentFilter, string searchString, int? pageNumber)
        {
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            
            ViewData["CurrentFilter"] = searchString;
            var pageSize = 20;

            var competitors = from s in _context.Competitors
                              .Include(c => c.Team)
                              .Include(c => c.Country)
                              .OrderBy(cp => cp.LastName)
                              select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                competitors = competitors.Where(s => s.FirstName.Contains(searchString)
                                       || s.LastName.Contains(searchString));
            }

            return View(await PaginatedList<Competitor>.CreateAsync(competitors.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Competitors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Competitors == null)
            {
                return NotFound();
            }

            var competitor = await _context.Competitors
                .Include(c => c.Team)
                .Include(c => c.Country)
                .FirstOrDefaultAsync(m => m.CompetitorId == id);
            if (competitor == null)
            {
                return NotFound();
            }

            return View(competitor);
        }

        // GET: Competitors/Create
        public IActionResult Create()
        {
            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "TeamName");
            ViewData["CountryId"] = new SelectList(_context.Country.OrderBy(c => c.CountryNameLong), "CountryId", "CountryNameLong");
            return View();
        }

        // POST: Competitors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CompetitorId,FirstName,LastName,TeamId, CountryId")] Competitor competitor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(competitor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "TeamName", competitor.TeamId);
            ViewData["CountryId"] = new SelectList(_context.Country.OrderBy(c => c.CountryNameLong), "CountryId", "CountryNameLong", competitor.CountryId);
            return View(competitor);
        }

        // GET: Competitors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Competitors == null)
            {
                return NotFound();
            }

            var competitor = await _context.Competitors.FindAsync(id);
            if (competitor == null)
            {
                return NotFound();
            }
            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "TeamName", competitor.TeamId);
            ViewData["CountryId"] = new SelectList(_context.Country.OrderBy(c => c.CountryNameLong), "CountryId", "CountryNameLong", competitor.CountryId);
            return View(competitor);
        }

        // POST: Competitors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CompetitorId,FirstName,LastName,TeamId,CountryId")] Competitor competitor)
        {
            if (id != competitor.CompetitorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(competitor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompetitorExists(competitor.CompetitorId))
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
            ViewData["TeamId"] = new SelectList(_context.Teams, "TeamId", "TeamName", competitor.TeamId);
            ViewData["CountryId"] = new SelectList(_context.Country.OrderBy(c => c.CountryNameLong), "CountryId", "CountryNameLong", competitor.CountryId);
            return View(competitor);
        }

        // GET: Competitors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Competitors == null)
            {
                return NotFound();
            }

            var competitor = await _context.Competitors
                .Include(c => c.Team)
                .FirstOrDefaultAsync(m => m.CompetitorId == id);
            if (competitor == null)
            {
                return NotFound();
            }

            return View(competitor);
        }

        // POST: Competitors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Competitors == null)
            {
                return Problem("Entity set 'DatabaseContext.Competitors'  is null.");
            }
            var competitor = await _context.Competitors.FindAsync(id);
            if (competitor != null)
            {
                _context.Competitors.Remove(competitor);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompetitorExists(int id)
        {
          return (_context.Competitors?.Any(e => e.CompetitorId == id)).GetValueOrDefault();
        }
    }
}
