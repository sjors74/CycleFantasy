using CycleManager.Domain.Enums;
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
        private const string BaseUrl = "https://cyclingflash.com/cyclingflash-365-ranking";

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
                    Headless = false,
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

        private static string BuildUrl(string category, int page)
        {
            return $"{BaseUrl}/{category}/men-elite?page={page}";
        }
    }
}
