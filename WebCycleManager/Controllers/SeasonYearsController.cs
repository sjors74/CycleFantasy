using CycleManager.Domain.Models;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebCycleManager.Models;

namespace WebCycleManager.Controllers
{
    public class SeasonYearsController : Controller
    {
        private readonly ISeasonYearService _service;

        public SeasonYearsController(ISeasonYearService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var years = await _service.GetAllAsync();

            var vm = years
                .OrderByDescending(x => x.Year)
                .Select(x => new SeasonYearViewModel
                {
                    SeasonYearId = x.SeasonYearId,
                    Year = x.Year,
                    Active = x.Active
                })
                .ToList();

            return View(vm);
        }

        public IActionResult Create()
        {
            return View(new SeasonYearViewModel
            {
                Active = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SeasonYearViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var year = new SeasonYear
                {
                    Year = model.Year,
                    Active = model.Active
                };

                await _service.CreateAsync(year);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var year = await _service.GetByIdAsync(id);

            if (year == null)
                return NotFound();

            var model = new SeasonYearViewModel
            {
                SeasonYearId = year.SeasonYearId,
                Year = year.Year,
                Active = year.Active
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SeasonYear model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var year = new SeasonYear
            {
                SeasonYearId = model.SeasonYearId,
                Year = model.Year,
                Active = model.Active
            };

            await _service.UpdateAsync(year);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var year = await _service.GetByIdAsync(id);

            if (year == null)
                return NotFound();

            var model = new SeasonYearViewModel
            {
                SeasonYearId = year.SeasonYearId,
                Year = year.Year,
                Active = year.Active
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetSeasonYears()
        {
            var years = await _service.GetAllAsync();

            return Ok(years);
        }
    }
}
