using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CycleManager.Tests.Integration.Manager
{
    [Collection("NonParallelTests")]
    public class EvenementDetailsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public EvenementDetailsTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        private async Task<Event> EnsureTestEventAsync(bool hasCode = true, bool withStages = true)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Database leegmaken (optioneel, voor echte isolatie per test)
            await db.Database.EnsureCreatedAsync();

            // Elke test krijgt een unieke identifier
            var unique = Guid.NewGuid().ToString("N").Substring(0, 6);

            // Zorg dat er altijd een configuratie is
            var config = await db.Configurations.FirstOrDefaultAsync();
            if (config == null)
            {
                config = new Configuration
                {
                    ConfigurationType = "Default Config " + unique
                };
                db.Configurations.Add(config);
                await db.SaveChangesAsync();
            }

            // Nieuw event, altijd vers aanmaken
            var ev = new Event
            {
                EventName = $"Test Event {unique}",
                EventCode = hasCode ? $"TE{unique}" : null,
                EventYear = 2025,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(5),
                IsActive = true,
                ShowPodium = false,
                ConfigurationId = config.Id
            };

            db.Events.Add(ev);
            await db.SaveChangesAsync();

            if (withStages)
            {
                db.Stages.AddRange(
                    new Stage
                    {
                        EventId = ev.EventId,
                        StageName = "Stage 1",
                        StageDate = DateTime.Today,
                        StageOrder = 1,
                        StartLocation = "A",
                        FinishLocation = "B"
                    },
                    new Stage
                    {
                        EventId = ev.EventId,
                        StageName = "Stage 2",
                        StageDate = DateTime.Today.AddDays(1),
                        StageOrder = 2,
                        StartLocation = "C",
                        FinishLocation = "D",
                        NoScore = true
                    }
                );
                await db.SaveChangesAsync();
            }

            return ev;
        }


        [Fact]
        public async Task Details_Should_ReturnNotFound_WhenEventDoesNotExist()
        {
            var response = await _client.GetAsync("/Events/Details/999999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Details_Should_DisplayFallback_WhenNoStages()
        {
            var ev = await EnsureTestEventAsync(withStages: false);
            var response = await _client.GetAsync($"/Events/Details/{ev.EventId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var html = await response.Content.ReadAsStringAsync();

            html.Should().Contain("Er zijn nog geen etappes toegevoegd aan dit evenement");
        }

        [Fact]
        public async Task Details_Should_DisplayStages_WhenStagesExist()
        {
            var ev = await EnsureTestEventAsync();
            var response = await _client.GetAsync($"/Events/Details/{ev.EventId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var html = await response.Content.ReadAsStringAsync();

            // Stage-namen en locaties aanwezig
            html.Should().Contain("Stage 1").And.Contain("A").And.Contain("B");
            html.Should().Contain("Stage 2").And.Contain("C").And.Contain("D");

            // Uitslag links aanwezig
            html.Should().Contain("/Results?stageId=");

            // Aantal posities
            html.Should().Contain("5").And.Contain("3");
        }

        [Fact]
        public async Task Details_Should_DisplayScraperButtons_WhenEventHasCode()
        {
            // Arrange
            var ev = await EnsureTestEventAsync(hasCode: true, withStages: true);

            // Act
            var response = await _client.GetAsync($"/Events/Details/{ev.EventId}");
            var html = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Scrape dropouts button
            html.Should().Contain($"/AdminScraper/ScrapeDropouts?eventId={ev.EventId}")
                .And.Contain($"eventName={ev.EventCode}");

            // Scrape stage results button
            html.Should().Contain($"/AdminScraper/ScrapeAndPair?stageId=")
                .And.Contain($"eventId={ev.EventId}");
        }

        [Fact]
        public async Task Details_Should_Not_DisplayScraperButtons_WhenEventHasNoCode()
        {
            // Arrange
            var ev = await EnsureTestEventAsync(hasCode: false, withStages: true);

            // Act
            var response = await _client.GetAsync($"/Events/Details/{ev.EventId}");
            var html = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Geen scraper-knoppen zichtbaar
            html.Should().NotContain("/AdminScraper/ScrapeDropouts")
                .And.NotContain("/AdminScraper/ScrapeAndPair");
        }


        [Fact]
        public async Task Details_Should_DisplayNavigationLinks()
        {
            var ev = await EnsureTestEventAsync();
            var response = await _client.GetAsync($"/Events/Details/{ev.EventId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var html = await response.Content.ReadAsStringAsync();

            // Terug naar lijst
            html.Should().Contain("href=\"/Events\"");
            // Edit button
            html.Should().Contain($"href=\"/Events/Edit/{ev.EventId}\"");
        }
    }
}
