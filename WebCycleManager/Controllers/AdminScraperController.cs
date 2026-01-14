using CycleManager.Domain.Dto;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebCycleManager.Controllers
{
    public class AdminScraperController : Controller
    {
        private readonly IScraperService _scraperService;
        private readonly IScoreService _scoreService;
        private readonly IAdminScraperService _adminScraperService;

        public AdminScraperController(
            IScraperService scraperService, 
            IScoreService scoreService, 
            IAdminScraperService adminScraperService)
        {
            _scraperService = scraperService;
            _scoreService = scoreService;
            _adminScraperService = adminScraperService;
        }
        
        [HttpGet]
        public async Task<IActionResult> ScrapeAndPair(int stageId, int eventId, string eventName, int year)
        {
            var stage = await _adminScraperService.GetStageByIdAsync(stageId); 
            if (stage == null)
            { 
                TempData["Error"] = "Stage niet gevonden.";
                return RedirectToAction("Details", "Events", new { eventId });
            }

            int.TryParse(stage.StageName, out var stageNumber);

            await _scraperService.RunAsync(eventId, eventName, stageNumber, year);
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
            var team = await _adminScraperService.GetTeamByIdAsync(dto.TeamId);
            if (team == null)
                throw new Exception("Team not found while trying to scrape competitors.");

            await _scraperService.RunCompetitorsAsync(dto.TeamId, dto.Year);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ImportScrapedCompetitors()
        {
            await _scraperService.ImportScrapedCompetitorsAsync();
            return RedirectToAction("Index", "Teams");
        }
    }
}
