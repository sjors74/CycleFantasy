using DataAccessEF.Extensions;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebCycleManager.Helpers;

namespace WebCycleManager.Controllers
{
    public class CompetitorsController : Controller
    {
        private readonly ICompetitorRepository _competitorRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly ITeamRepository _teamRepository;

        public CompetitorsController(ICompetitorRepository competitorRepository, ICountryRepository countryRepository, ITeamRepository teamRepository)
        {
            _competitorRepository = competitorRepository;
            _countryRepository = countryRepository;
            _teamRepository = teamRepository;
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
            var pageSize = ConfigurationConstants.PageSize;
            var competitors = _competitorRepository.GetAllCompetitors();
            if (!string.IsNullOrEmpty(searchString))
            {
                competitors = competitors.Where(s => s.LastName == searchString || s.FirstName.Contains(searchString));
                if (competitors == null)
                {
                    return NotFound();
                }
            }
            return View(await PaginatedList<Competitor>.CreateAsync(competitors.OrderBy(c => c.LastName).ThenBy(c => c.FirstName), pageNumber ?? 1, pageSize));
        }

        // GET: Competitors/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var competitor =  _competitorRepository.GetById((int)id);
            if (competitor == null)
            {
                return NotFound();
            }

            return View(competitor);
        }

        // GET: Competitors/Create
        public async Task<IActionResult> Create()
        {
            ViewData["TeamId"] = new SelectList(await _teamRepository.GetAll(), "TeamId", "TeamName");
            ViewData["CountryId"] = new SelectList(await CountrySelectListHelper.GetOrderedCountries(_countryRepository), "CountryId", "CountryNameLong");
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
                _competitorRepository.Add(competitor);
                await _competitorRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TeamId"] = new SelectList(await _teamRepository.GetAll(), "TeamId", "TeamName", competitor.TeamId);
            ViewData["CountryId"] = new SelectList(await CountrySelectListHelper.GetOrderedCountries(_countryRepository), "CountryId", "CountryNameLong", competitor.CountryId);
            return View(competitor);
        }

        // GET: Competitors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var competitor = _competitorRepository.GetById((int)id);
            if (competitor == null)
            {
                return NotFound();
            }
            ViewData["TeamId"] = new SelectList(await _teamRepository.GetAll(), "TeamId", "TeamName", competitor.TeamId);
            ViewData["CountryId"] = new SelectList(await CountrySelectListHelper.GetOrderedCountries(_countryRepository), "CountryId", "CountryNameLong", competitor.CountryId);
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
                    _competitorRepository.Update(competitor);
                    await _competitorRepository.SaveChangesAsync();
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
            ViewData["TeamId"] = new SelectList(await _teamRepository.GetAll(), "TeamId", "TeamName", competitor.TeamId);
            ViewData["CountryId"] = new SelectList(await CountrySelectListHelper.GetOrderedCountries(_countryRepository), "CountryId", "CountryNameLong", competitor.CountryId);
            return View(competitor);
        }

        // GET: Competitors/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var competitor = _competitorRepository.GetById((int)id);
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
            var competitor = _competitorRepository.GetById((int)id);
            if (competitor != null)
            {
                _competitorRepository.Remove(competitor);
            }
            
            await _competitorRepository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompetitorExists(int id)
        {
          return _competitorRepository.GetById(id) != null;
        }
    }
}
