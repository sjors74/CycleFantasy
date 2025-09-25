using CycleManager.Domain.Dto;
using CycleManager.Services;
using Domain.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebCycleManager.Controllers
{
    public class AdminScraperController : Controller
    {
        private readonly ScraperService _scraperService;
        private readonly ScoreService _scoreService;
        private readonly ApplicationDbContext _db;

        public AdminScraperController(ScraperService scraperService, ScoreService scoreService, ApplicationDbContext db)
        {
            _scraperService = scraperService;
            _scoreService = scoreService;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> ScrapeAndPair(int stageId, int eventId, string eventName, int year)
        {
            var stage = await _db.Stages
                .Include(s => s.Event)
                .FirstOrDefaultAsync(s => s.Id == stageId);
            if (stage == null)
                throw new Exception("Stage not found while trying to scrape results.");

            int.TryParse(stage.StageName, out var stageNumber);

            if (stage == null)
            {
                TempData["Error"] = "Stage niet gevonden.";
                return RedirectToAction("Details", "Events", new { eventId });
            }

            await _scraperService.RunAsync(
                eventId: eventId,
                eventName: eventName,
                stageNumber: stageNumber,
                year: year
            );

            await _scoreService.UpdateScoresForStageAsync(eventId, stageId);

            TempData["Success"] = "Scrape voltooid.";
            return RedirectToAction("Details", "Events", new { id = eventId });
        }

        [HttpGet]
        public async Task<IActionResult> ScrapeDropouts(int eventId, string eventName, int year)
        {
            await _scraperService.RunDropoutsAsync(eventId, eventName, year);
            return RedirectToAction("Details", "Events", new { id = eventId });
        }               

        [HttpPost]
        public async Task<IActionResult> ScrapeCompetitors([FromBody] ScrapeRequestDto dto)
        {
            var team = await _db.Teams
                .FirstOrDefaultAsync(t => t.TeamId == dto.TeamId);
            if (team == null)
                throw new Exception("Team not found while trying to scrape competitors.");

            await _scraperService.RunCompetitorsAsync(dto.TeamId, dto.Year);

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> ImportScrapedCompetitors()
        {
            await _scraperService.ImportScrapedCompetitorsAsync();
            return Ok();
        }
    }
}
