using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System.Globalization;

namespace CycleManager.Services
{
    public class CyclingFlashScraper : ICyclingFlashScraper
    {
        private readonly ILogger<CyclingFlashScraper> _logger;
        private const string BaseUrl = "https://cyclingflash.com";
        private const string RankingPath = "cyclingflash-365-ranking";

        public CyclingFlashScraper(ILogger<CyclingFlashScraper> logger)
        {
            _logger = logger;
        }

        public async Task<List<ScrapeCompetitorRating>> ScrapePageResultAsync(
            string category,
            int pageNumber,
            DateTime ratingDate)
        {
            var result = new List<ScrapeCompetitorRating>();

            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    Headless = true,
                    Channel = "chrome"
                });

            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                Locale = "nl-NL",
                ViewportSize = new ViewportSize
                {
                    Width = 1920,
                    Height = 1080
                }
            });

            var page = await context.NewPageAsync();

            var url = BuildUrl(category, pageNumber);

            _logger.LogInformation("Scraping category {Category}, page {Page}", category, pageNumber);

            await page.GotoAsync(url);

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.WaitForTimeoutAsync(3000);

            _logger.LogInformation(
                "Current url = {Url}",
                page.Url);

            var rows = await GetRowsAsync(page);

            if (!rows.Any())
            {
                _logger.LogInformation(
                    "No rows found for {Category} page {Page}",
                    category,
                    pageNumber);

                return result;
            }

            foreach (var row in rows)
            {
                try
                {
                    var competitor = await ParseRowAsync(
                        row,
                        category,
                        ratingDate);

                    if (competitor != null)
                    {
                        result.Add(competitor);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Could not parse row on {Category} {Page}", category, pageNumber);
                }
            }


            _logger.LogInformation(
                "{Category} page {Page}: {Count} competitors scraped.",
                category,
                pageNumber,
                result.Count);

            return result;
        }

        public async Task<List<ScrapeCompetitorRating>> ScrapeCompetitorRatingsAsync(string profileUrl, DateTime ratingDate)
        {
            const int maxAttempts = 3;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    _logger.LogInformation("Scraping competitor profile {Url}, attempt {Attempt}/{MaxAttempts}",
                        profileUrl,
                        attempt,
                        maxAttempts);

                    using var playwright = await Playwright.CreateAsync();

                    await using var browser = await playwright.Chromium.LaunchAsync(
                        new BrowserTypeLaunchOptions
                        {
                            Headless = false,
                            Channel = "chrome"
                        });

                    var context = await browser.NewContextAsync(
                        new BrowserNewContextOptions
                        {
                            Locale = "nl-NL",
                            ViewportSize = new ViewportSize
                            {
                                Width = 1920,
                                Height = 1080
                            }
                        });

                    var page = await context.NewPageAsync();

                    var url = BuildProfileUrl(profileUrl);

                    await page.GotoAsync(url, new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.DOMContentLoaded,
                        Timeout = 30000
                    });

                    _logger.LogInformation("Current url = {Url}", page.Url);
                    _logger.LogInformation("Page title = {Title}", await page.TitleAsync());

                    var bodyText = await page.Locator("body").InnerTextAsync();

                    _logger.LogInformation(
                        "Body length: {Length}",
                        bodyText.Length);

                    var competitorName = (await page.TitleAsync())
                        .Replace(" - Profile & Career Stats", "")
                        .Replace(" | CyclingFlash", "")
                        .Trim();

                    var rows = await GetProfileRowsAsync(page);

                    var ratings = await ParseProfileRowsAsync(
                        rows,
                        profileUrl,
                        competitorName,
                        ratingDate);

                    if (ratings.Count > 0)
                    {
                        _logger.LogInformation(
                            "Profile {Profile} : {Count} ratings scraped on attempt {Attempt}.",
                            profileUrl,
                            ratings.Count,
                            attempt);

                        return ratings;
                    }

                    _logger.LogWarning(
                        "Profile {Profile}: 0 ratings scraped on attempt {Attempt}/{MaxAttempts}",
                        profileUrl,
                        attempt,
                        maxAttempts);

                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Error scraping profile {Profile} on attempt {Attempt}/{MaxAttempts}",
                        profileUrl,
                        attempt,
                        maxAttempts);

                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                }
            }

            _logger.LogWarning(
                "Profile {Profile}: no ratings scraped after {MaxAttempts} attempts",
                profileUrl,
                maxAttempts);

            return new List<ScrapeCompetitorRating>();
        }

        private async Task<IReadOnlyList<ILocator>> GetRowsAsync(IPage page)
        {
            return await page
                .Locator("table tbody tr")
                .AllAsync();
        }

        private async Task<ScrapeCompetitorRating> ParseRowAsync(
            ILocator row,
            string category,
            DateTime ratingDate)
        {
            var cells = await row.Locator("td").AllAsync();

            if (cells.Count < 4)
            {
                _logger.LogWarning("Unexpected number of columns.");
                return null;
            }

            var riderLink = cells[2].Locator("a");

            var rider = (await riderLink.InnerTextAsync()).Trim();

            var profile = await riderLink.GetAttributeAsync("href");

            if (!int.TryParse(
               (await cells[3].InnerTextAsync()).Trim(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var rating))
            {
                _logger.LogWarning(
                    "Could not parse rating for {Rider}",
                    rider);

                return null;
            }

            return new ScrapeCompetitorRating
            {
                CompetitorName = rider,
                Rating = rating,
                ProfileUrl = profile,
                RatingDate = ratingDate,
                RatingCategoryCode = category,
                Source = "CyclingFlash"
            };
        }

        private async Task<IReadOnlyList<ILocator>> GetProfileRowsAsync(IPage page)
        {
            return await page
                .Locator("a[href^='/cyclingflash-365-ranking/']")
                .AllAsync();
        }

        private async Task<List<ScrapeCompetitorRating>> ParseProfileRowsAsync(
            IReadOnlyList<ILocator> rows,
            string profileUrl,
            string competitorName,
            DateTime ratingDate)
        {
            var result = new List<ScrapeCompetitorRating>();

            foreach (var row in rows)
            {
                var text = await row.InnerTextAsync();

                var lines = text
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToList();

                if (lines.Count < 2)
                {
                    continue;
                }

                var category = lines[0];
                var ratingText = lines[^1];

                if (!int.TryParse(
                    ratingText,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var rating))
                {
                    _logger.LogWarning(
                        "Could not parse rating for {Competitor}: {Text}",
                        competitorName,
                        text);

                    continue;
                }

                result.Add(new ScrapeCompetitorRating
                {
                    CompetitorName = competitorName,
                    RatingCategoryCode = category,
                    Rating = rating,
                    ProfileUrl = profileUrl,
                    RatingDate = ratingDate,
                    Source = "CyclingFlashProfile"
                });
            }

            return result;
        }

        private static string BuildUrl(string category, int page)
        {
            return $"{BaseUrl}/{RankingPath}/{category}/men-elite?page={page}";
        }

        private static string BuildProfileUrl(string profileUrl)
        {
            return $"{BaseUrl}/{profileUrl.TrimStart('/')}";
        }
    }
}
