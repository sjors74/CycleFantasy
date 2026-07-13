using CycleManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
            return View(years);
        }
    }
}
