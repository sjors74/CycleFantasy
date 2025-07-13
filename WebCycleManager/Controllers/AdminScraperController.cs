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
                return RedirectToAction("Details", "Events", new { eventId });
            }

            await _scraperService.RunAsync(
                eventId: eventId,
                eventName: eventName,
                stageNumber: stageId,
                year: year
               
            );

            TempData["Success"] = "Scrape voltooid.";
            return RedirectToAction("Details", "Events", new { id = eventId });
        }

        [HttpGet]
        public async Task<IActionResult> ScrapeDropouts(int eventId, string eventName, int year)
        {
            await _scraperService.RunDropoutsAsync(eventId, eventName, year);
            return RedirectToAction("Details", "Events", new { id = eventId });
        }
    }
}
