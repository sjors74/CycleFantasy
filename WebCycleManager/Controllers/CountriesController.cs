using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class CountriesController : Controller
    {
        private readonly ICountryService _countryService;
        private readonly ICompetitorService _competitorService;

        public CountriesController(ICompetitorService competitorService, ICountryService countryService)
        {
            _countryService = countryService;
            _competitorService = competitorService;
        }

        // GET: Countries
        public async Task<IActionResult> Index()
        {
            var countryViewModel = new List<CountryViewModel>();
            var countries = await _countryService.GetAll();
            foreach(var country in  countries.OrderBy(c => c.CountryNameLong))
            {
                var competitorsCount = await _competitorService.GetCompetitorsByCountry(country.CountryId);
                countryViewModel.Add(new CountryViewModel
                {
                    Id = country.CountryId,
                    Name = country.CountryNameLong,
                    ShortName = country.CountryNameShort,
                    CompetitorsCount = competitorsCount
                });
            }
            return View(countryViewModel);
        }

        // GET: Countries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var country = await _countryService.GetById((int)id);
            if (country == null)
            {
                return NotFound();
            }

            return View(country);
        }

        // GET: Countries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Countries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CountryId,CountryNameLong,CountryNameShort")] Country country)
        {
            if (ModelState.IsValid)
            {
                await _countryService.Create(country);
                return RedirectToAction(nameof(Index));
            }
            return View(country);
        }

        // GET: Countries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var country = await _countryService.GetById((int)id);
            if (country == null)
            {
                return NotFound();
            }
            return View(country);
        }

        // POST: Countries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CountryId,CountryNameLong,CountryNameShort")] Country country)
        {
            if (id != country.CountryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _countryService.Update(country);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CountryExists(country.CountryId))
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
            return View(country);
        }

        // GET: Countries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var country = await _countryService.GetById((int)id);
            if (country == null)
            {
                return NotFound();
            }

            return View(country);
        }

        // POST: Countries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var country =  await  _countryService.GetById((int)id);
            if (country != null)
            {
                await _countryService.Delete(country);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool CountryExists(int id)
        {
          return (_countryService.GetById(id) != null);
        }
    }
}