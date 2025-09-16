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

            // Oude resultaten verwijderen
            var oudeResultaten = _db.ScrapedStageResults
                .Where(r => r.StageId == stageNumber && r.EventId == eventId);
            _db.ScrapedStageResults.RemoveRange(oudeResultaten);
            await _db.SaveChangesAsync();

            string url = $"https://www.procyclingstats.com/race/{eventName}/{year}/stage-{stageNumber}";
            _logger.LogInformation($"Start scraping {eventName}: EventId={eventId}, StageId={stageNumber}");

            var nieuweScrape = await ScrapeStageResultsAsync(url, _settings.TopLimit, eventId);

            foreach (var scraped in nieuweScrape)
            {
                _db.ScrapedStageResults.Add(new ScrapedStageResult
                {
                    EventId = eventId,
                    StageId = stageNumber,
                    BibNumber = scraped.BibNumber,
                    RiderName = scraped.RiderName,
                    TeamName = scraped.TeamName,
                    Position = scraped.Position,
                    ImportedAt = DateTime.UtcNow
                });
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

        public async Task RunDropoutsAsync(int eventId, string eventName, int year)
        {
            string url = $"https://www.procyclingstats.com/race/{eventName}/{year}/startlist";
            _logger.LogInformation($"Start scraping dropouts {eventName}: EventId={eventId}");

            var dropoutBibs = await ScrapeDropoutsAsync(url);
            int updateCount = 0;

            var competitors = await _db.CompetitorsInEvent
                .Include(c => c.Competitor)
                .Where(c => c.EventId == eventId)
                .ToListAsync();

            foreach (var competitor in competitors)
            {
                if (dropoutBibs.Contains(competitor.EventNumber))
                {
                    if(!competitor.OutOfCompetition)
                    {
                        competitor.OutOfCompetition = true;
                        updateCount++;
                    }
                }
            }

            if (updateCount > 0)
            {
                await _db.SaveChangesAsync();
                _logger.LogInformation($"{updateCount} deelnemers gemarkeerd als uitgevallen voor EventId {eventId}.");
            }
        }

        public async Task RunCompetitorsAsync(int teamId, int year)
        {
            var team = await _db.Teams
                .Where(t => t.TeamId == teamId)
                .FirstOrDefaultAsync();

            if (team == null) return;

            string url = $"https://www.procyclingstats.com/team/{team.PcsName}-{year}/overview/start";
            _logger.LogInformation($"Start scraping competitors for team {team.TeamName}, year {year}");

            var competitors = await _pcsScraper.ScrapeCompetitorsAsync(url, teamId, year);

            foreach (var c in competitors)
            {
                //TODO: save competitors to table.
                Console.WriteLine($"{c.RiderName} - {team.TeamName} (id {c.TeamId}) imported at {c.ImportedAt})");
            }
        }

        private Task<List<ScrapedStageResult>> ScrapeStageResultsAsync(string url, int topN, int eventId)
        {
            return _pcsScraper.ScrapeStageResultsAsync(url, topN, eventId);
        }


        private Task<List<int>> ScrapeDropoutsAsync(string url)
        {
            return _pcsScraper.ScrapeDropoutBibsAsync(url);
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
