using CycleManager.Domain.Dto;
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
                await page.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle,
                    Timeout = 60000
                });

                await page.WaitForTimeoutAsync(8000);

                var rows = page.Locator("table.results tbody tr");
                int count = await rows.CountAsync();
                int validCount = 0;

                for (int i = 0; i < count; i++)
                {
                    var row = rows.Nth(i);
                    var cols = row.Locator("td");
                    int colCount = await cols.CountAsync();
                    if (colCount < 5)
                        continue;


                    // Position
                    var posText = (await cols.Nth(0).InnerTextAsync()).Trim();
                    if (!int.TryParse(posText, out var position))
                        continue;

                    // Bib
                    var bibText = (await cols.Nth(3).InnerTextAsync()).Trim();
                    int.TryParse(bibText, out var bib);

                    var rider = (await cols
                        .Nth(7)
                        .Locator("a")
                        .InnerTextAsync())
                        .Trim();

                    if (string.IsNullOrWhiteSpace(rider))
                        continue;

                    // Team
                    var teamCell = cols.Nth(8);

                    var teamLink = teamCell.Locator("a");

                    string team;

                    if (await teamLink.CountAsync() > 0)
                    {
                        team = (await teamLink.InnerTextAsync()).Trim();
                    }
                    else
                    {
                        team = (await teamCell.InnerTextAsync()).Trim();
                    }
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

        public async Task<List<ScrapedStartlistEntry>> ScrapeStartlistAsync(string url)
        {
            var results = new List<ScrapedStartlistEntry>();

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
                await page.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle,
                    Timeout = 60000
                });

                // PCS laadt soms nog wat dynamisch in
                await page.WaitForTimeoutAsync(3000);

                // Wacht op de startlijst-container
                await page.WaitForSelectorAsync("ul.startlist_v4", new()
                {
                    Timeout = 15000
                });

                // Teams zitten direct onder ul.startlist_v4
                var teams = page.Locator("ul.startlist_v4 > li");

                var teamCount = await teams.CountAsync();

                for (int t = 0; t < teamCount; t++)
                {
                    var team = teams.Nth(t);

                    // Teamnaam
                    var teamName = "";
                    string teamPcsName = "";

                    var teamLink = team.Locator("a.team");

                    if (await teamLink.CountAsync() > 0)
                    {
                        teamName =
                            (await teamLink.InnerTextAsync()).Trim();

                        var teamHref =
                            await teamLink.GetAttributeAsync("href");

                        if (!string.IsNullOrWhiteSpace(teamHref))
                        {
                            teamPcsName = teamHref
                                .Replace("team/", "")
                                .Trim('/');
                        }
                    }

                    
                    

                    // Renners binnen dit team
                    var riders = team.Locator(".ridersCont > ul > li");

                    var riderCount = await riders.CountAsync();

                    for (int r = 0; r < riderCount; r++)
                    {
                        var rider = riders.Nth(r);

                        // BIB
                        int? bib = null;

                        var bibLocator = rider.Locator(".bib");

                        if (await bibLocator.CountAsync() > 0)
                        {
                            var bibText =
                                (await bibLocator.InnerTextAsync()).Trim();

                            if (int.TryParse(bibText, out var parsedBib))
                            {
                                bib = parsedBib;
                            }
                        }

                        // Rider link
                        var riderLink = rider.Locator("a[href^='rider/']");

                        if (!await riderLink.IsVisibleAsync())
                            continue;

                        if (await riderLink.CountAsync() == 0)
                            continue;

                        var riderName =
                            (await riderLink.InnerTextAsync()).Trim();

                        if (string.IsNullOrWhiteSpace(riderName))
                            continue;

                        var href =
                            await riderLink.GetAttributeAsync("href");

                        string riderSlug = "";

                        if (!string.IsNullOrWhiteSpace(href))
                        {
                            riderSlug = href
                                .Replace("rider/", "")
                                .Trim('/');
                        }

                        results.Add(new ScrapedStartlistEntry
                        {
                            RiderName = riderName,
                            PcsName = riderSlug,
                            TeamName = teamName,
                            TeamPcsName = teamPcsName,
                            BibNumber = bib
                        });
                    }
                }

                _logger.LogInformation(
                    "PCS startlist scraping voltooid. {RiderCount} renners gevonden.",
                    results.Count);

                // Controle op dubbele renners binnen hetzelfde team
                var duplicateRiders = results
                    .GroupBy(x => new { x.TeamName, x.PcsName })
                    .Where(g => g.Count() > 1)
                    .ToList();

                foreach (var duplicate in duplicateRiders)
                {
                    _logger.LogWarning(
                        "Dubbele renner gevonden. Team={Team} Rider={Rider} Aantal={Count}",
                        duplicate.Key.TeamName,
                        duplicate.Key.PcsName,
                        duplicate.Count());
                }

                // Controle op dubbele startnummers binnen hetzelfde team
                var duplicateBibs = results
                    .Where(x => x.BibNumber.HasValue)
                    .GroupBy(x => new { x.TeamName, x.BibNumber })
                    .Where(g => g.Count() > 1)
                    .ToList();

                foreach (var duplicate in duplicateBibs)
                {
                    var riders = string.Join(
                        ", ",
                        duplicate.Select(x => x.RiderName));

                    _logger.LogWarning(
                        "Dubbel startnummer gevonden. Team={Team} Bib={Bib} Renners={Riders}",
                        duplicate.Key.TeamName,
                        duplicate.Key.BibNumber,
                        riders);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Fout tijdens scrapen van PCS startlist: {Url}",
                    url);

                // Extra debug-info
                _logger.LogInformation("Huidige URL: {Url}", page.Url);

                await page.ScreenshotAsync(new()
                {
                    Path = $"pcs-startlist-error-{DateTime.UtcNow:yyyyMMddHHmmss}.png",
                    FullPage = true
                });

                throw;
            }

            finally
            {
                await page.CloseAsync();
                await context.CloseAsync();
            }
        }

        public Task<List<ScrapedStageSpecialResult>> ScrapeStageSpecialResultsAsync(string url, int eventId, int stageId)
        {
            throw new NotImplementedException();
        }
    }
}