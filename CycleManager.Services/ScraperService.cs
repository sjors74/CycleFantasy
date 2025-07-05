using CycleManager.Domain.Models;
using CycleManager.Services.Settings;
using Domain.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CycleManager.Services
{
    public class ScraperService
    {
        private readonly PcsScraper _pcsScraper;
        private readonly ScraperSettings _settings;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ScraperService> _logger;

        public ScraperService(ApplicationDbContext db, IOptions<ScraperSettings> options, PcsScraper pcsScraper, ILogger<ScraperService> logger)
        {
            _db = db;
            _logger = logger;
            _pcsScraper = pcsScraper;
            _settings = options.Value;
        }

        public async Task RunAsync(int eventId, string eventName, int year, int stageNumber)
        {
            var stage = await _db.Stages
            .Include(s => s.Event)
                        .FirstOrDefaultAsync(s => s.StageName == stageNumber.ToString() && s.EventId == eventId);


            string url = $"https://www.procyclingstats.com/race/{eventName}/{year}/stage-{stageNumber}";
            _logger.LogInformation($"Start scraping {eventName}: EventId={eventId}, StageId={stageNumber}");

            var nieuweScrape = await ScrapeStageResultsAsync(url, _settings.TopLimit, eventId);

            foreach (var scraped in nieuweScrape)
            {
                var existing = await _db.ScrapedStageResults.FirstOrDefaultAsync(r =>
                    r.StageId == stageNumber &&
                    r.EventId == eventId &&
                    r.Position == scraped.Position);

                if(existing != null)
                {
                    existing.RiderName = scraped.RiderName;
                    existing.TeamName = scraped.TeamName;
                    existing.BibNumber = scraped.BibNumber;
                    existing.ImportedAt = DateTime.UtcNow;
                }
                else
                {
                    _db.ScrapedStageResults.Add(new ScrapedStageResult
                    {
                        EventId = eventId,
                        StageId = stageNumber,
                        BibNumber = scraped.BibNumber,
                        RiderName = scraped.RiderName,
                        TeamName = scraped.TeamName,
                        Position = scraped.Position,
                    });
                }
            }

            await _db.SaveChangesAsync();

            var scrapedResults = await _db.ScrapedStageResults
                .Where(r => r.EventId == eventId && r.StageId == stageNumber)
                .ToListAsync();

            var competitors = await _db.CompetitorsInEvent
                .Include(c => c.Competitor)
                .Where(c => c.EventId == eventId)
                .ToListAsync();

            foreach (var scrapedResult in scrapedResults)
            {
                var match = competitors.FirstOrDefault(c => c.EventNumber == scrapedResult.BibNumber);

                if (match != null)
                {
                    scrapedResult.MatchedCompetitorInEventId = match.Id;
                    var existingResult = await _db.Results.FirstOrDefaultAsync(r =>
                        r.StageId == stage.Id &&
                        r.CompetitorInEventId == match.Id);


                    var configurationItemId = await GetIdForPositon(2, scrapedResult.Position); //TODO: 2 = configuratie wielerevenement groot
                    if (existingResult != null)
                    {
                        if (configurationItemId > 0)
                        {
                            existingResult.ConfigurationItemId = configurationItemId;
                        }
                        else
                        {
                            _db.Results.Remove(existingResult);
                        }
                    }
                    else
                    {
                        if (configurationItemId > 0)
                        {
                            _db.Results.Add(new Result
                            {
                                StageId = stage.Id,
                                CompetitorInEventId = match.Id,
                                ConfigurationItemId = await GetIdForPositon(2, scrapedResult.Position) //TODO: 2 = configuratie wielerevenement groot
                            });
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"Geen match voor bib {scrapedResult.BibNumber} - {scrapedResult.RiderName}");
                }
            }

            await _db.SaveChangesAsync();
        }

        private Task<List<ScrapedStageResult>> ScrapeStageResultsAsync(string url, int topN, int eventId)
        {
            return _pcsScraper.ScrapeStageResultsAsync(url, topN, eventId);
        }

        private async Task<int> GetIdForPositon(int configuratieId, int position)
        {
            var configurationItem = await _db.ConfigurationItems.FirstOrDefaultAsync(ci =>
                ci.Position == position &&
                ci.ConfigurationId == configuratieId);

            return configurationItem?.Id ?? 0;
        }
    }
}
