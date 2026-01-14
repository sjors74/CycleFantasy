using CycleManager.Domain.Models;
using CycleManager.Services.Helpers;
using CycleManager.Services.Interfaces;
using CycleManager.Services.Settings;
using Domain.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CycleManager.Services
{
    public class ScraperService : IScraperService
    {
        private readonly IPcsScraper _pcsScraper;
        private readonly ScraperSettings _settings;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ScraperService> _logger;

        public ScraperService(ApplicationDbContext db, IOptions<ScraperSettings> options, IPcsScraper pcsScraper, ILogger<ScraperService> logger)
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
                .Include(c => c.CompetitorInTeam)
                .ThenInclude(c => c.Competitor)
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
                .Include(c => c.CompetitorInTeam)
                    .ThenInclude(c => c.Competitor)
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

            string url = $"https://www.procyclingstats.com/team/{team.PcsName}-{year}/overview/start-v3";
            _logger.LogInformation($"Start scraping competitors for team {team.CurrentTeamName}, year {year}");

            var competitors = await _pcsScraper.ScrapeCompetitorsAsync(url, teamId, year);

            _db.ScrapedCompetitors.AddRange(competitors);
            await _db.SaveChangesAsync();            
        }

        public async Task ImportScrapedCompetitorsAsync()
        {
            var scraped = await _db.ScrapedCompetitors
                .Where(sc => sc.ProcessedAt == null)
                .ToListAsync();

            if (scraped.Count == 0)
                _logger.LogInformation("Geen nieuwe scraped competitors om te importeren.");

            var competitors = await _db.Competitors
                .Include(c => c.Country)
                .ToListAsync();

            var competitorLookup = competitors
                .GroupBy(c => (c.FirstName?.ToLowerInvariant() ?? "", c.LastName?.ToLowerInvariant() ?? ""))
                .ToDictionary(g => g.Key, g => g.First());

            var competitorByScraperName = competitors
                .Where(c => !string.IsNullOrEmpty(c.ScraperName))
                .GroupBy(c => c.ScraperName.ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var c in competitors)
            {
                if (string.IsNullOrWhiteSpace(c.ScraperName))
                    continue;

                var key = c.ScraperName.Trim().ToLower();

                // Als hij al bestaat -> negeren of loggen
                if (!competitorByScraperName.ContainsKey(key))
                    competitorByScraperName[key] = c;
            }

            var countries = await _db.Countries.ToListAsync();
            var countryLookup = countries.ToDictionary(c => c.CountryNameShort, StringComparer.OrdinalIgnoreCase);

            // Cache bestaande CompetitorInTeams (hashset)
            var existingCompetitorInTeams = await _db.CompetitorInTeams
                .Select(c => new { c.CompetitorId, c.TeamId, c.Year })
                .ToListAsync();

            var competitorInTeamSet = existingCompetitorInTeams
                .Select(x => ((object)x.CompetitorId, x.TeamId, x.Year))
                .ToHashSet();

            var newCompetitors = new List<Competitor>();
            var newCompetitorInTeams = new List<CompetitorInTeam>();
            var newCountries = new List<Country>();

            foreach (var sc in scraped)
            {
                Country country = null;

                if (!string.IsNullOrEmpty(sc.CountryShortName))
                {
                    if (!countryLookup.TryGetValue(sc.CountryShortName, out country))
                    {
                        country = new Country
                        {
                            CountryNameShort = sc.CountryShortName,
                            CountryNameLong = CountryHelper.GetName(sc.CountryShortName),
                        };

                        newCountries.Add(country);
                        countryLookup[sc.CountryShortName] = country;
                    }
                }

                Competitor competitor = null;
                var scraperKey = sc.RiderName.ToLower();

                if (competitorByScraperName.TryGetValue(scraperKey, out competitor))
                {
                    if (country != null)
                        competitor.Country = country;
                }
                else
                {

                    var (firstName, lastName) = SplitNamesHelper.SplitName(sc.RiderName);
                    firstName = SplitNamesHelper.FormatName(firstName);
                    lastName = SplitNamesHelper.FormatName(lastName);
                    var key = (firstName.ToLower(), lastName.ToLower());

                    // Zoek bestaande competitor (case-insensitive)
                    if (!competitorLookup.TryGetValue(key, out competitor))
                    {
                        competitor = new Competitor
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            Country = country,
                            ScraperName = sc.RiderName
                        };

                        newCompetitors.Add(competitor);

                        competitorLookup[key] = competitor;
                        competitorByScraperName[scraperKey] = competitor;
                    }
                    else
                    {
                        competitor.ScraperName = sc.RiderName;
                        if (country != null)
                            competitor.Country = country;

                        competitorByScraperName[scraperKey] = competitor;
                    }
                }

                object competitorKey = competitor.CompetitorId == 0 ? competitor : competitor.CompetitorId;

                var citKey = (competitorKey, sc.TeamId, sc.Year);

                if (!competitorInTeamSet.Contains(citKey))
                {
                    newCompetitorInTeams.Add(new CompetitorInTeam
                    {
                        Competitor = competitor,
                        TeamId = sc.TeamId,
                        Year = sc.Year
                    });

                    competitorInTeamSet.Add(citKey);
                }

                sc.ProcessedAt = DateTime.UtcNow;
            }

            if (newCountries.Count > 0)
                await _db.Countries.AddRangeAsync(newCountries);

            if (newCompetitors.Count > 0)
                await _db.Competitors.AddRangeAsync(newCompetitors);

            if (newCompetitorInTeams.Count > 0)
                await _db.CompetitorInTeams.AddRangeAsync(newCompetitorInTeams);

            await _db.SaveChangesAsync();
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
