using CycleManager.Services;
using Domain.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebCycleManager.Controllers
{
    public class AdminScraperController : Controller
    {
        private readonly ScraperService _scraperService;
        private readonly ApplicationDbContext _db;

        public AdminScraperController(ScraperService scraperService, ApplicationDbContext db)
        {
            _scraperService = scraperService;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> ScrapeAndPair(int stageId, int eventId, string eventName, int year)
        {
            var stage = await _db.Stages
                .Include(s => s.Event)
                .FirstOrDefaultAsync(s => s.Id == stageId);

            if (stage == null)
            {
                TempData["Error"] = "Stage niet gevonden.";
                return RedirectToAction("Index", "Stages", new { eventId = 0 });
            }

            await _scraperService.RunAsync(
                eventId: eventId,
                eventName: eventName,
                stageNumber: stageId,
                year: year
               
            );

            TempData["Success"] = "Scrape voltooid.";
            return RedirectToAction("Index", "Stages", new { eventId = stage.EventId });
        }
    }
}
