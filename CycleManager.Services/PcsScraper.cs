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
    }
}