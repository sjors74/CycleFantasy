using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace CycleManager.Services
{
    public class PcsScraper : IPcsScraper
    {
        private readonly IBrowser _browser;
        private readonly ILogger<PcsScraper> _logger;
        public PcsScraper(ILogger<PcsScraper> logger, IBrowser browser)
        {
            _logger = logger;
            _browser = browser;
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
            var competitors = new List<ScrapedCompetitor>();

            var context = await _browser.NewContextAsync();
            var page = await context.NewPageAsync();

            try
            {
                // Forceer de 'name' tab door querystring (veilige manier)
                var separator = url.Contains('?') ? "&" : "?";
                var nameUrl = url + separator + "x=1&snav=name";

                await page.GotoAsync(nameUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle,
                    Timeout = 30000
                });

                // Prefereren: wacht tot er daadwerkelijk li-items zijn
                var liSelector = "div.stab.name.riderlistcont ul.teamlist li";

                try
                {
                    await page.WaitForSelectorAsync(liSelector, new()
                    {
                        State = WaitForSelectorState.Attached,
                        Timeout = 10000
                    });
                }
                catch (PlaywrightException)
                {
                    // Fallback: probeer eerst de tab aan te klikken (als querystring niets deed)
                    var tabSelector = "div.borderbox.w30.right .tabnav1 .snav a[href*='snav=name']";
                    var tab = await page.QuerySelectorAsync(tabSelector);
                    if (tab != null)
                    {
                        await tab.ClickAsync();
                        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                        await page.WaitForSelectorAsync(liSelector, new() { Timeout = 10000 });
                    }
                    else
                    {
                        // als geen tab gevonden: dump HTML / screenshot voor debugging
                        var html = await page.ContentAsync();
                        System.IO.File.WriteAllText("pcs_page_debug.html", html);
                        await page.ScreenshotAsync(new PageScreenshotOptions { Path = "pcs_debug.png" });
                        throw new Exception("Kon teamlist niet vinden (geen tab of li). Debugfiles: pcs_page_debug.html, pcs_debug.png");
                    }
                }

                // Nu items uitlezen (veilig: controleer null en empty)
                var items = await page.QuerySelectorAllAsync(liSelector);
                if (items == null || items.Count == 0)
                {
                    // nog steeds niets — debug dump
                    var html = await page.ContentAsync();
                    System.IO.File.WriteAllText("pcs_page_empty.html", html);
                    await page.ScreenshotAsync(new PageScreenshotOptions { Path = "pcs_empty.png" });
                    return competitors; // of throw new Exception(...)
                }

                foreach (var item in items)
                {
                    var nameElement = await item.QuerySelectorAsync("a");
                    var name = nameElement == null ? "" : (await nameElement.InnerTextAsync()).Trim();

                    var flagElement = await item.QuerySelectorAsync("span.flag");
                    var countryCode = "";
                    if (flagElement != null)
                    {
                        var classAttr = await flagElement.GetAttributeAsync("class") ?? "";
                        var parts = classAttr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 1) countryCode = parts[1].Trim();
                    }
                    if (!string.IsNullOrEmpty(name))
                    {
                        competitors.Add(new ScrapedCompetitor
                        {
                            RiderName = name,
                            TeamId = teamId,
                            Year = year,
                            CountryShortName = countryCode,
                            ImportedAt = DateTime.UtcNow
                        });
                    }
                }

                return competitors;
            }
            finally
            {
                await page.CloseAsync();
                await context.CloseAsync();
            }
        }
    }
}