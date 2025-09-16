using CycleManager.Domain.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace CycleManager.Services
{
    public class PcsScraper
    {
        private readonly ILogger<PcsScraper> _logger;
        public PcsScraper(ILogger<PcsScraper> logger)
        {
            _logger = logger;
        }

        public async Task<List<ScrapedStageResult>> ScrapeStageResultsAsync(string url, int topN, int eventId)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);

            var results = new List<ScrapedStageResult>();
            int validCount = 0;

            var table = doc.DocumentNode.SelectSingleNode("//table[contains(@class,'results')]");
            if (table == null)
            {
                _logger.LogWarning("Geen resultaten-tabel gevonden op URL: {Url}", url);
                return results;
            }

            var rows = table.SelectNodes(".//tr");
            if(rows == null)
            {
                _logger.LogWarning("Geen rijen gevonden in resultaten-tabel.");
                return results;
            }
           
            //foreach (var row in rows.Skip(1)) // skip header
            foreach (var row in rows)
            {
                var rowClass = row.GetAttributeValue("class", "").ToLowerInvariant();
                var cols = row.SelectNodes(".//td");

                if (cols == null)
                    continue;

                if(cols.Count == 2 && cols[1].GetAttributeValue("colspan", "") == "23")
                {
                    var note = cols[1].InnerText.Trim();
                    if (note.Contains("relegated", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("Relegated info gevonden: {Note}", note);
                    }
                    continue;
                }

                if(cols.Count < 9)
                    continue;

                if (!int.TryParse(cols[0].InnerText.Trim(), out var posNr))
                    continue;

                var bib = int.TryParse(cols[3].InnerText.Trim(), out var bibNr) ? bibNr : 0;
                var team = cols[8].InnerText.Trim();
                var riderCell = cols[7].SelectSingleNode(".//a");
                var rider = riderCell?.InnerText.Trim() ?? "(onbekend)";
                var result = new ScrapedStageResult
                {
                    EventId = eventId,
                    Position = posNr,
                    RiderName = rider,
                    TeamName = team,
                    BibNumber = bib
                };
                results.Add(result);
                validCount++;

                if(validCount >= topN)
                    break;
            }

            _logger.LogInformation("Scraping voltooid. Aantal geldige resultaten: {Count}", results.Count);
            return results;
        }
        public async Task<List<int>> ScrapeDropoutBibsAsync(string url)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);

            var dropoutBibs = new List<int>();

            // Zoek alle UL's waarvan de class begint met 'startlist'
            var ulNodes = doc.DocumentNode.SelectNodes("//ul[starts-with(@class, 'startlist')]");
            if (ulNodes == null)
            {
                _logger.LogWarning("Geen startlist UL's gevonden.");
                return dropoutBibs;
            }

            foreach (var ul in ulNodes)
            {
                var liNodes = ul.SelectNodes(".//li");
                if (liNodes == null)
                    continue;

                foreach (var li in liNodes)
                {
                    var ridersContDiv = li.SelectSingleNode(".//div[contains(@class, 'ridersCont')]");
                    if (ridersContDiv == null)
                        continue;

                    var innerUl = ridersContDiv.SelectSingleNode(".//ul");
                    if (innerUl == null)
                        continue;

                    var dropoutLis = innerUl.SelectNodes(".//li[contains(@class, 'dropout')]");
                    if (dropoutLis == null)
                        continue;

                    foreach (var dropoutLi in dropoutLis)
                    {
                        var bibSpan = dropoutLi.SelectSingleNode(".//span[contains(@class, 'bib')]");
                        if (bibSpan != null && int.TryParse(bibSpan.InnerText.Trim(), out var bibNr))
                        {
                            dropoutBibs.Add(bibNr);
                        }
                    }
                }
            }

            _logger.LogInformation("Aantal dropout bibs gevonden: {Count}", dropoutBibs.Count);
            return dropoutBibs;
        }

        public async Task<List<ScrapedCompetitor>> ScrapeCompetitorsAsync(string url, int teamId, int year)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);

            var results = new List<ScrapedCompetitor>();
            var nameTab = doc.DocumentNode
            .SelectSingleNode("//div[contains(@class,'stab') and contains(@class,'name') and contains(@class, 'riderlistcont')]");

            if (nameTab == null) return new List<ScrapedCompetitor>();

            var riderLinks = nameTab.SelectNodes(".//ul/li//div[@class='w80']//a");

            if (riderLinks == null) return new List<ScrapedCompetitor>();

            var competitors = riderLinks.Select((node, Index) => new ScrapedCompetitor
            {
                RiderName = node.InnerText.Trim(),
                TeamId = teamId,
                ImportedAt = DateTime.UtcNow
            }).ToList();

            return competitors;
        }

    }
}