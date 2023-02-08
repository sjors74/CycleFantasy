using Domain.Context;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class TeamsController : Controller
    {
        private readonly DatabaseContext _context;
        public TeamsController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Teams
        public async Task<IActionResult> Index()
        {
            var _teamViewModels = new List<TeamViewModel>();
            foreach (var team in await _context.Teams.OrderBy(t => t.TeamName).ToListAsync())
            {
                _teamViewModels.Add(new TeamViewModel
                {
                    Id = team.TeamId,
                    TeamName = team.TeamName,
                    CountryNameShort = team.Country.CountryNameShort,
                    CompetitorsInTeam = team.Competitors.Count,
                });
            }
            return View(_teamViewModels);

         //return  != null ? 
         //                 View(await _context.Teams.OrderBy(t => t.TeamName).ToListAsync()) :
         //                 Problem("Entity set 'DatabaseContext.Teams'  is null.");
        }

        // GET: Teams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Teams == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .FirstOrDefaultAsync(m => m.TeamId == id);
            if (team == null)
            {
                return NotFound();
            }
            var competitorsList = new List<CompetitorViewModel>();
            foreach (var comp in team.Competitors)
            {
                var compViewModel = new CompetitorViewModel { CompetitorId = comp.CompetitorId, FirstName = comp.FirstName, LastName = comp.LastName, Land = comp.Country.CountryNameShort };
                competitorsList.Add(compViewModel);
            }
            var vm = new TeamViewModel { Id = team.TeamId, TeamName = team.TeamName, CompetitorsInTeam = team.Competitors.Count, CountryNameShort = team.Country.CountryNameShort, Competitors = competitorsList };
            return View(vm);
        }

        // GET: Teams/Create
        public IActionResult Create()
        {
            ViewData["CountryId"] = new SelectList(_context.Country.OrderBy(c => c.CountryNameLong), "CountryId", "CountryNameLong");
            return View();
        }

        // POST: Teams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeamId,TeamName,CountryId")] Team team)
        {
            if (ModelState.IsValid)
            {
                _context.Add(team);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        // GET: Teams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Teams == null)
            {
                return NotFound();
            }

            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }
            ViewData["CountryId"] = new SelectList(_context.Country.OrderBy(c => c.CountryNameLong), "CountryId", "CountryNameLong");
            return View(team);
        }

        // POST: Teams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TeamId,TeamName")] Team team)
        {
            if (id != team.TeamId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(team);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamExists(team.TeamId))
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
            return View(team);
        }

        // GET: Teams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Teams == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .FirstOrDefaultAsync(m => m.TeamId == id);
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Teams == null)
            {
                return Problem("Entity set 'DatabaseContext.Teams'  is null.");
            }
            var team = await _context.Teams.FindAsync(id);
            if (team != null)
            {
                _context.Teams.Remove(team);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeamExists(int id)
        {
          return (_context.Teams?.Any(e => e.TeamId == id)).GetValueOrDefault();
        }
    }
}
