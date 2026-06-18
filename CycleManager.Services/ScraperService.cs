using CycleManager.Domain.Dto;
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

        public async Task RunAsync(int eventId, string eventName, int stageNumber, int year)
        {
            _logger.LogInformation($"Start scraping {eventName}: EventId={eventId}, StageId={stageNumber}");

            // =========================================
            // 1. Stage + Configuratie ophalen
            // =========================================
            var stage = await _db.Stages
                .Include(s => s.Event)
                    .ThenInclude(e => e.Configuration)
                        .ThenInclude(c => c.ConfigurationItems)
                .FirstOrDefaultAsync(s =>
                    s.StageName == stageNumber.ToString() &&
                    s.EventId == eventId);

            if (stage == null)
                throw new Exception("Stage niet gevonden");

            var configurationItems = stage.Event.Configuration.ConfigurationItems;
            var topLimit = configurationItems.Count;

            // =========================================
            // 2. Oude scraped resultaten verwijderen
            // =========================================
            var oudeResultaten = await _db.ScrapedStageResults
                .Where(r => r.StageId == stageNumber && r.EventId == eventId)
                .ToListAsync();

            _db.ScrapedStageResults.RemoveRange(oudeResultaten);
            await _db.SaveChangesAsync();

            // =========================================
            // 3. Nieuwe scrape uitvoeren
            // =========================================
            string url = $"https://www.procyclingstats.com/race/{eventName}/{year}/stage-{stageNumber}/result/result";

            var nieuweScrape = await ScrapeStageResultsAsync(url, topLimit, eventId);

            _logger.LogInformation(
                 "Scraper returned {Count} results",    
                nieuweScrape.Count);

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

            var scrapedCount = await _db.ScrapedStageResults
                .CountAsync(x =>
                    x.EventId == eventId &&
                    x.StageId == stageNumber);

            _logger.LogInformation(
                "Stored {Count} scraped results",
                scrapedCount);

            // =========================================
            // 4. Alles vooraf ophalen (GEEN N+1)
            // =========================================
            var scrapedResults = await _db.ScrapedStageResults
                .Where(r => r.EventId == eventId && r.StageId == stageNumber)
                .ToListAsync();

            var competitors = await _db.CompetitorsInEvent
                .Include(c => c.CompetitorInTeam)
                    .ThenInclude(c => c.Competitor)
                .Where(c => c.EventId == eventId)
                .ToListAsync();

            var bestaandeResults = await _db.Results
                .Where(r => r.StageId == stage.Id)
                .ToListAsync();

            // =========================================
            // 5. Dictionaries voor snelle lookup
            // =========================================

            // 0 betekent: geen rugnummer → uitsluiten
            var competitorByBib = competitors
                .Where(c => c.EventNumber > 0)
                .GroupBy(c => c.EventNumber)
                .ToDictionary(g => g.Key, g => g.First());

            var resultByCompetitorId = bestaandeResults
                .ToDictionary(r => r.CompetitorInEventId);

            var configurationByPosition = configurationItems
                .ToDictionary(ci => ci.Position);

            // =========================================
            // 6. Resultaten verwerken (geen DB calls)
            // =========================================
            _db.ChangeTracker.AutoDetectChangesEnabled = false;

            foreach (var scrapedResult in scrapedResults)
            {
                if (scrapedResult.BibNumber <= 0 ||
                    !competitorByBib.TryGetValue(scrapedResult.BibNumber, out var match))
                {
                    _logger.LogWarning($"Geen match voor bib {scrapedResult.BibNumber} - {scrapedResult.RiderName}");
                    continue;
                }

                scrapedResult.MatchedCompetitorInEventId = match.Id;

                configurationByPosition.TryGetValue(scrapedResult.Position, out var configurationItem);
                resultByCompetitorId.TryGetValue(match.Id, out var existingResult);

                if (existingResult != null)
                {
                    if (configurationItem != null)
                    {
                        existingResult.ConfigurationItemId = configurationItem.Id;
                    }
                    else
                    {
                        _db.Results.Remove(existingResult);
                        resultByCompetitorId.Remove(match.Id);
                    }
                }
                else
                {
                    if (configurationItem != null)
                    {
                        var newResult = new Result
                        {
                            StageId = stage.Id,
                            CompetitorInEventId = match.Id,
                            ConfigurationItemId = configurationItem.Id
                        };

                        _db.Results.Add(newResult);
                        resultByCompetitorId[match.Id] = newResult;
                    }
                }
            }

            _db.ChangeTracker.AutoDetectChangesEnabled = true;

            // =========================================
            // 7. Alles in 1 keer opslaan
            // =========================================
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


        /// <summary>
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="scrapedEntries"></param>
        /// <returns></returns>
        public async Task SyncStartlistAsync(int eventId, List<ScrapedStartlistEntry> scrapedEntries)
        {
            var eventEntity = await _db.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);  

            if (eventEntity == null) 
                throw new Exception($"Event {eventId} niet gevonden");

            var eventYear = eventEntity.EventYear;

            var competitorInTeams = await _db.CompetitorInTeams
                .Include(cit => cit.Competitor)
                .Include(cit => cit.Team)
                .Where(cit => cit.Year == eventYear)
                .ToListAsync();

            var existingEntries = await _db.CompetitorsInEvent
                .Where(cie => cie.EventId == eventId)
                .ToListAsync();

            var existingLookup = existingEntries
                .ToDictionary(e => e.CompetitorInTeamId);


            var byPcsAndTeam = 
                competitorInTeams
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.Competitor.PcsName) && 
                    !string.IsNullOrWhiteSpace(x.Team.PcsName))
                .GroupBy(x =>
                    $"{x.Competitor.PcsName.ToLower()}|{x.Team.PcsName.ToLower()}")
                .ToDictionary(g => g.Key, g => g.First());

            var byPcs = 
                competitorInTeams
                .Where(x =>
                !string.IsNullOrWhiteSpace(x.Competitor.PcsName))
                .GroupBy(x =>
                    x.Competitor.PcsName.ToLower())
                .ToDictionary(g => g.Key, g => g.First());

            var byNameAndTeam = 
                competitorInTeams
                .GroupBy(x =>
                    $"{NormalizeName($"{x.Competitor.LastName} {x.Competitor.FirstName}")}|{NormalizeName(x.Team.CurrentTeamName)}")
                .ToDictionary(g => g.Key, g => g.First());

            var byName = 
                competitorInTeams
                .GroupBy(x =>
                    NormalizeName($"{x.Competitor.LastName} {x.Competitor.FirstName}"))
                .ToDictionary(g => g.Key, g => g.First());

            var processedCompetitorInTeamIds =
                new HashSet<int>();

            int added = 0;
            int updated = 0;
            int unmatched = 0;

            foreach (var scraped in scrapedEntries)
            {
                if(string.IsNullOrWhiteSpace(scraped.TeamPcsName))
                {
                    _logger.LogWarning(
                        "Missing TeamPcsName for rider: {Rider} ({Pcs})",
                        scraped.RiderName,
                        scraped.PcsName);
                }

                var competitorInTeam = FindCompetitorInTeam(scraped, byPcsAndTeam, byPcs, byNameAndTeam, byName);

                if (competitorInTeam == null)
                {
                    unmatched++;

                    _logger.LogWarning(
                        "Geen match voor: {Rider} ({PcsName})",
                        scraped.RiderName,
                        scraped.PcsName);

                    continue;
                }

                processedCompetitorInTeamIds.Add(competitorInTeam.Id);

                if (string.IsNullOrWhiteSpace(
                    competitorInTeam.Competitor.PcsName)
                    && !string.IsNullOrWhiteSpace(scraped.PcsName))
                {
                    competitorInTeam.Competitor.PcsName = scraped.PcsName;
                }

                if (existingLookup.TryGetValue(
                    competitorInTeam.Id,
                    out var existing))
                {
                    existing.EventNumber = scraped.BibNumber ?? 0;
                    existing.InSelectie = true;
                    existing.RemovedFromStartList = false;

                    updated++;
                }
                else
                {
                    _db.CompetitorsInEvent.Add(new CompetitorsInEvent
                    {
                        EventId = eventId,
                        CompetitorInTeamId = competitorInTeam.Id,
                        EventNumber = scraped.BibNumber ?? 0,
                        InSelectie = true,
                        OutOfCompetition = false,
                        RemovedFromStartList = false
                    });

                    added++;
                }
            }

            var obsoleteEntries = existingEntries
                .Where(e => !processedCompetitorInTeamIds.Contains(e.CompetitorInTeamId))
                .ToList();

            if (obsoleteEntries.Any())
            {
                foreach (var entry in obsoleteEntries)
                {
                    entry.InSelectie = false;
                    entry.RemovedFromStartList = true;
                    entry.EventNumber = 0;

                    _logger.LogInformation(
                       "Renner verwijderd van startlijst: CompetitorInTeamId={CompetitorInTeamId}",
                        entry.CompetitorInTeamId);
                }
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation(
                "Startlist sync EventId {EventId}: toegevoegd={Added}, bijgewerkt={Updated}, verwijderd={Removed}, unmatched={Unmatched}",
                eventId,
                added,
                updated,
                obsoleteEntries.Count,
                unmatched);

        }

        public async Task RefreshStartlistAsync(int eventId)
        {
            var eventEntity = await _db.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventEntity == null)
                throw new Exception($"Event {eventId} niet gevonden");

            var url =
                $"https://www.procyclingstats.com/race/{eventEntity.EventCode}/{eventEntity.EventYear}/startlist";

            _logger.LogInformation(
                "Startlist synchronisatie gestart voor EventId {EventId}",
                eventId);

            var scrapedEntries = await _pcsScraper.ScrapeStartlistAsync(url);

            _logger.LogInformation(
                "{Count} renners gescrapet",
                scrapedEntries.Count);

            await SyncStartlistAsync(
                eventId,
                scrapedEntries);
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

        private static string NormalizeName(string name)
        {
            return name
                .Trim()
                .ToLowerInvariant()
                .Replace("-", "")
                .Replace(" ", "");
        }

        private CompetitorInTeam? FindCompetitorInTeam(
            ScrapedStartlistEntry scraped,
            Dictionary<string, CompetitorInTeam> byPcsAndTeam,
            Dictionary<string, CompetitorInTeam> byPcs,
            Dictionary<string, CompetitorInTeam> byNameAndTeam,
            Dictionary<string, CompetitorInTeam> byName)
        {
            // 1. PCS Rider + Team

            if (!string.IsNullOrWhiteSpace(scraped.PcsName) &&
                !string.IsNullOrWhiteSpace(scraped.TeamPcsName))
            {
                var key =
                    $"{scraped.PcsName.ToLower()}|{scraped.TeamPcsName.ToLower()}";

                if (byPcsAndTeam.TryGetValue(key, out var match))
                    return match;
            }

            // 2. PCS Rider

            if (!string.IsNullOrWhiteSpace(scraped.PcsName))
            {
                if (byPcs.TryGetValue(
                        scraped.PcsName.ToLower(),
                        out var match))
                    return match;
            }

            // 3. Naam + Team

            var nameTeamKey =
                $"{NormalizeName(scraped.RiderName)}|{NormalizeName(scraped.TeamName)}";

            if (byNameAndTeam.TryGetValue(
                    nameTeamKey,
                    out var nameTeamMatch))
                return nameTeamMatch;

            // 4. Naam

            if (byName.TryGetValue(
                    NormalizeName(scraped.RiderName),
                    out var nameMatch))
                return nameMatch;

            return null;
        }
    }
}
