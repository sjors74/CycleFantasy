using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using CycleManager.Tests.Integration.Helpers;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
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

            var year = 2026;

            var country = new Country
            {
                CountryNameLong = "Nederland",
                CountryNameShort = "NL"
            };

            db.Countries.Add(country);
            await db.SaveChangesAsync();

            var team = new Team
            {
                CurrentTeamName = "Test Team",
                CountryId = country.CountryId
            };

            db.Teams.Add(team);
            await db.SaveChangesAsync();

            var seasonYear = new SeasonYear
            {
                Year = year,
                Active = true
            };

            db.SeasonYears.Add(seasonYear);
            await db.SaveChangesAsync();

            var teamYear = new TeamYear
            {
                TeamId = team.TeamId,
                Team = team,
                Year = year,
                SeasonYearId = seasonYear.SeasonYearId,
                SeasonYear = seasonYear,
                Name = team.CurrentTeamName
            };

            db.TeamYear.Add(teamYear);
            await db.SaveChangesAsync();

            var dto = new ScrapeRequestDto
            {
                TeamId = team.TeamId,
                SeasonYearId = seasonYear.SeasonYearId
            };

            // Act: POST scrape
            var response = await _client.PostAsJsonAsync(
                "/AdminScraper/ScrapeCompetitors", 
                dto);

            response.EnsureSuccessStatusCode();

            // Assert: check database
            var scraped = db.ScrapedCompetitors
                .Where(sc => sc.TeamId == team.TeamId && sc.Year == year)
                .ToList();

            scraped.Should().NotBeEmpty();
            scraped.Should().OnlyContain(sc => sc.ProcessedAt == null);

            // Check expected rider names from fake scraper
            scraped.Select(sc => sc.RiderName)
                   .Should()
                   .Contain(new[] 
                   { 
                       $"Rider One_{year}", 
                       $"Rider Two_{year}" 
                   });
        }
    }
}