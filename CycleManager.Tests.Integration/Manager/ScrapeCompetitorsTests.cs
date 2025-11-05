using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using CycleManager.Tests.Integration.Helpers;
using Domain.Context;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace CycleManager.Tests.Integration.Manager
{
    public class ScrapeCompetitorsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public ScrapeCompetitorsTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;

            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Vervang echte ScraperService door een fake
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IScraperService));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddScoped<IScraperService, FakeScraperService>();
                });
            }).CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task ScrapeCompetitors_Should_Insert_ScrapedCompetitors_For_Team_And_Year()
        {
            // Arrange: pick a seeded team and year
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var team = db.Teams.Include(t => t.TeamYears).First();
            var year = team.TeamYears.First().Year;

            var dto = new
            {
                TeamId = team.TeamId,
                Year = year
            };

            // Act: POST scrape
            var response = await _client.PostAsJsonAsync("/AdminScraper/ScrapeCompetitors", dto);
            response.EnsureSuccessStatusCode();

            // Assert: check database
            var scraped = db.ScrapedCompetitors
                .Where(sc => sc.TeamId == team.TeamId && sc.Year == year)
                .ToList();

            scraped.Should().NotBeEmpty();
            scraped.Should().OnlyContain(sc => sc.ProcessedAt == null);

            // Check expected rider names from fake scraper
            scraped.Select(sc => sc.RiderName)
                   .Should().Contain(new[] { $"Rider One_{year}", $"Rider Two_{year}" });
        }
    }
}