using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
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

        public async Task<List<int>> ScrapeDropoutBibsAsync(string url)
        {
            var dropoutBibs = new List<int>();

            var context = await _browser.NewContextAsync(new()
            {
                UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                    "AppleWebKit/537.36 (KHTML, like Gecko) " +
                    "Chrome/124.0.0.0 Safari/537.36",

                ViewportSize = new ViewportSize
                {
                    Width = 1920,
                    Height = 1080
                },

                Locale = "nl-NL"
            });

            var page = await context.NewPageAsync();

            try
            {
                await page.GotoAsync(url, new()
                {
                    WaitUntil = WaitUntilState.NetworkIdle,
                    Timeout = 60000
                });

                // Wacht op echte startlist
                await page.WaitForSelectorAsync("ul.startlist_v4", new()
                {
                    Timeout = 30000
                });

                // Pak dropout bibs
                var bibElements = await page.QuerySelectorAllAsync(
                    "li.dropout span.bib"
                );

                foreach (var element in bibElements)
                {
                    var text = (await element.InnerTextAsync())?.Trim();

                    if (int.TryParse(text, out var bibNr))
                    {
                        dropoutBibs.Add(bibNr);
                    }
                }

                _logger.LogInformation(
                    "Aantal dropout bibs gevonden: {Count}",
                    dropoutBibs.Count
                );
            }
            catch (TimeoutException)
            {
                _logger.LogWarning(
                    "Geen startlist/dropouts gevonden voor {Url}",
                    url
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Fout bij scrapen van dropout bibs.");
            }
            finally
            {
                await page.CloseAsync();
                await context.CloseAsync();
            }

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
                    WaitUntil = WaitUntilState.DOMContentLoaded,
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

        public async Task<List<ScrapedStageResult>> ScrapeStageResultsAsync(string url, int topN, int eventId)
        {
            var results = new List<ScrapedStageResult>();

            var context = await _browser.NewContextAsync();
            var page = await context.NewPageAsync();

            try
            {
                await page.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 30000
                });

                var tableSelector = "table tbody tr";

                try
                {
                    await page.WaitForSelectorAsync(tableSelector, new()
                    {
                        State = WaitForSelectorState.Attached,
                        Timeout = 15000
                    });
                }
                catch(TimeoutException)
                {
                    _logger.LogInformation("Nog geen resultaten beschikbaar op {Url}", url);

                    return results;
                }

                var rows = await page.QuerySelectorAllAsync(tableSelector);

                int validCount = 0;

                foreach (var row in rows)
                {
                    var cols = await row.QuerySelectorAllAsync("td");
                    if (cols == null || cols.Count < 5)
                        continue;

                    // Position
                    var posText = (await cols[0].InnerTextAsync()).Trim();
                    if (!int.TryParse(posText, out var position))
                        continue;

                    // Bib
                    var bibText = (await cols[3].InnerTextAsync()).Trim();
                    int.TryParse(bibText, out var bib);

                    // Rider
                    var riderElement = await cols[7].QuerySelectorAsync("a");
                    var rider = riderElement == null
                        ? ""
                        : (await riderElement.InnerTextAsync()).Trim();

                    if (string.IsNullOrWhiteSpace(rider))
                        continue;

                    // Team
                    var teamElement = await cols[8].QuerySelectorAsync("a");
                    var team = teamElement == null
                        ? (await cols[8].InnerTextAsync()).Trim()
                        : (await teamElement.InnerTextAsync()).Trim();

                    results.Add(new ScrapedStageResult
                    {
                        EventId = eventId,
                        Position = position,
                        RiderName = rider,
                        TeamName = team,
                        BibNumber = bib
                    });

                    validCount++;
                    if (validCount >= topN)
                        break;
                }

                return results;
            }
            finally
            {
                await page.CloseAsync();
                await context.CloseAsync();
            }
        }

    }
}