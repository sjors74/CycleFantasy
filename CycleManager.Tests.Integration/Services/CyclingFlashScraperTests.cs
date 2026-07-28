using CycleManager.Services;
using Microsoft.Extensions.Logging;

namespace CycleManager.Tests.Integration.Services
{
    public class CyclingFlashScraperTests
    {
        [Fact]
        public async Task CyclingFlashScraper_Should_Find_Rows()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Information)
                    .AddConsole();
            });

            var logger = loggerFactory.CreateLogger<CyclingFlashScraper>();

            var scraper = new CyclingFlashScraper(logger);

            var result = await scraper.ScrapePageResultAsync(
                "gc",
                1,
                DateTime.Today);

            Assert.NotNull(result);
        }
    }
}
