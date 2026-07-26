using CycleManager.Domain.Dto;
using CycleManager.Services.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace WebCycleManager.Controllers
{
    public class AdminScraperController : Controller
    {
        private readonly IScraperService _scraperService;
        private readonly IScoreService _scoreService;
        private readonly IAdminScraperService _adminScraperService;
        private readonly ITeamService _teamService;

        public AdminScraperController(
            IScraperService scraperService, 
            IScoreService scoreService, 
            IAdminScraperService adminScraperService,
            ITeamService teamService)
        {
            _scraperService = scraperService;
            _scoreService = scoreService;
            _adminScraperService = adminScraperService;
            _teamService = teamService;
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

            BackgroundJob.Enqueue<IScrapeOrchestratorService>(x =>
                x.RunStageScrapeAsync(
                    eventId,
                    eventName,
                    stageId,
                    stageNumber,
                    year));

            TempData["Success"] = "Scrape aangemaakt.";
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
            var teamYear = await _teamService.GetByTeamAndSeasonAsync(dto.TeamId, dto.SeasonYearId);
            if (teamYear == null)
                throw new Exception("Team not found while trying to scrape competitors.");


            await _scraperService.RunCompetitorsAsync(teamYear.TeamYearId);

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
