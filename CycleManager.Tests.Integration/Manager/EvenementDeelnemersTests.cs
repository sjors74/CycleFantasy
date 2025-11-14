using CycleManager.Domain.Models;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.RegularExpressions;

namespace CycleManager.Tests.Integration.Manager
{
    public class EvenementDeelnemersTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public EvenementDeelnemersTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private ApplicationDbContext GetDbContext() =>
            _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        private string ExtractRequestVerificationToken(string html)
        {
            var match = Regex.Match(html, @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""");
            return match.Success ? match.Groups[1].Value : throw new InvalidOperationException("Antiforgery token not found");
        }

        [Fact]
        public async Task Index_ShouldShowListOfDeelnemers_WithCorrectScores()
        {
            var db = GetDbContext();
            db.Database.EnsureCreated();

            // Arrange: Users, Event, Configuratie, Teams, Competitors, Results, Picks
            var user1 = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "Tester", Email = "alice@test.com" };
            var user2 = new ApplicationUser { Id = "u2", FirstName = "Bob", LastName = "Builder", Email = "bob@test.com" };
            db.Users.AddRange(user1, user2);

            var configuratie = new Configuration { Id = 1, ConfigurationType = "Default" };
            db.Configurations.Add(configuratie);
            await db.SaveChangesAsync();

            var ev = new Event { EventName = "Tour de Integratie", EventYear = 2025, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(3), IsActive = true, Configuration = configuratie };
            db.Events.Add(ev);
            await db.SaveChangesAsync();

            var conf1 = new ConfigurationItem { Configuration = configuratie, Position = 1, Score = 50 };
            var conf2 = new ConfigurationItem { Configuration = configuratie, Position = 2, Score = 30 };
            db.ConfigurationItems.AddRange(conf1, conf2);
            await db.SaveChangesAsync();

            var gc1 = new GameCompetitorEvent { EventId = ev.EventId, UserId = user1.Id, TeamName = "Team Alice" };
            var gc2 = new GameCompetitorEvent { EventId = ev.EventId, UserId = user2.Id, TeamName = "Team Bob" };
            db.GameCompetitorsEvent.AddRange(gc1, gc2);
            await db.SaveChangesAsync();

            var country = new Country { CountryNameShort = "NL", CountryNameLong = "Nederland" };
            db.Countries.Add(country);
            await db.SaveChangesAsync();

            var team = new Team { CurrentTeamName = "TestTeam", CountryId = country.CountryId };
            db.Teams.Add(team);
            await db.SaveChangesAsync();

            var c1 = new Competitor { FirstName = "Rene", LastName = "Zon", CountryId = country.CountryId };
            var c2 = new Competitor { FirstName = "Bert", LastName = "Kaas", CountryId = country.CountryId };
            db.Competitors.AddRange(c1, c2);
            await db.SaveChangesAsync();

            var cit1 = new CompetitorInTeam { CompetitorId = c1.CompetitorId, TeamId = team.TeamId, Year = 2025 };
            var cit2 = new CompetitorInTeam { CompetitorId = c2.CompetitorId, TeamId = team.TeamId, Year = 2025 };
            db.CompetitorInTeams.AddRange(cit1, cit2);
            await db.SaveChangesAsync();

            var cie1 = new CompetitorsInEvent { EventId = ev.EventId, CompetitorInTeamId = cit1.Id };
            var cie2 = new CompetitorsInEvent { EventId = ev.EventId, CompetitorInTeamId = cit2.Id };
            db.CompetitorsInEvent.AddRange(cie1, cie2);
            await db.SaveChangesAsync();

            var stage = new Stage { EventId = ev.EventId, StageName = "Etappe 1", StageOrder = 1 };
            db.Stages.Add(stage);
            await db.SaveChangesAsync();

            db.Results.AddRange(
                new Result { StageId = stage.Id, CompetitorInEventId = cie1.Id, ConfigurationItemId = conf1.Id },
                new Result { StageId = stage.Id, CompetitorInEventId = cie2.Id, ConfigurationItemId = conf2.Id }
            );
            db.GameCompetitorEventPicks.AddRange(
                new GameCompetitorEventPick { GameCompetitorEventId = gc1.Id, CompetitorsInEventId = cie1.Id },
                new GameCompetitorEventPick { GameCompetitorEventId = gc2.Id, CompetitorsInEventId = cie2.Id }
            );
            await db.SaveChangesAsync();

            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/GameCompetitorEvents?eventId={ev.EventId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await response.Content.ReadAsStringAsync();
            html.Should().Contain("Team Alice");
            html.Should().Contain("Team Bob");
            html.Should().Contain("50");
            html.Should().Contain("30");
        }

        [Fact]
        public async Task Create_ShouldAddNewGameCompetitor()
        {
            var db = GetDbContext();
            db.Database.EnsureCreated();

            var user = new ApplicationUser { Id = "u3", FirstName = "Chris", LastName = "Demo", Email = "chris@demo.com" };
            db.Users.Add(user);

            var ev = new Event { EventName = "Vuelta Demo", EventYear = 2025, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(3), IsActive = true };
            db.Events.Add(ev);
            await db.SaveChangesAsync();

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            var getResponse = await client.GetAsync($"/GameCompetitorEvents/Create?eventId={ev.EventId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var html = await getResponse.Content.ReadAsStringAsync();
            var token = ExtractRequestVerificationToken(html);

            var formData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", token },
                { "EventId", ev.EventId.ToString() },
                { "UserId", user.Id },
                { "TeamName", "Team Chris" }
            };
            var content = new FormUrlEncodedContent(formData);

            var postResponse = await client.PostAsync("/GameCompetitorEvents/Create", content);
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            using var verifyDbScope = _factory.Services.CreateScope();
            var dbVerify = verifyDbScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbVerify.GameCompetitorsEvent.Any(gc => gc.TeamName == "Team Chris").Should().BeTrue();
        }

        [Fact]
        public async Task Edit_ShouldUpdateTeamName()
        {
            var db = GetDbContext();
            db.Database.EnsureCreated();

            var user = new ApplicationUser { Id = "user4", FirstName = "Edit", LastName = "Test", Email = "edit@test.com" };
            db.Users.Add(user);

            var ev = new Event { EventName = "EditEvent", EventYear = 2025, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1), IsActive = true };
            db.Events.Add(ev);

            var gc = new GameCompetitorEvent { EventId = ev.EventId, UserId = user.Id, TeamName = "OldName" };
            db.GameCompetitorsEvent.Add(gc);
            await db.SaveChangesAsync();

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            var getResponse = await client.GetAsync($"/GameCompetitorEvents/Edit/{gc.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var html = await getResponse.Content.ReadAsStringAsync();
            var token = ExtractRequestVerificationToken(html);

            var formData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", token },
                { "Id", gc.Id.ToString() },
                { "EventId", ev.EventId.ToString() },
                { "UserId", user.Id },
                { "TeamName", "NewName" }
            };
            var content = new FormUrlEncodedContent(formData);

            var postResponse = await client.PostAsync($"/GameCompetitorEvents/Edit/{gc.Id}", content);
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            using var verifyDbScope = _factory.Services.CreateScope();
            var dbVerify = verifyDbScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbVerify.GameCompetitorsEvent.First(g => g.Id == gc.Id).TeamName.Should().Be("NewName");
        }

        [Fact]
        public async Task Delete_ShouldRemoveGameCompetitor()
        {
            var db = GetDbContext();
            db.Database.EnsureCreated();

            var user = new ApplicationUser { Id = "user5", FirstName = "Del", LastName = "Test", Email = "del@test.com" };
            db.Users.Add(user);

            var ev = new Event { EventName = "DeleteEvent", EventYear = 2025, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1), IsActive = true };
            db.Events.Add(ev);

            var gc = new GameCompetitorEvent { EventId = ev.EventId, UserId = user.Id, TeamName = "DelTeam" };
            db.GameCompetitorsEvent.Add(gc);
            await db.SaveChangesAsync();

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            var getResponse = await client.GetAsync($"/GameCompetitorEvents/Delete/{gc.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var html = await getResponse.Content.ReadAsStringAsync();
            var token = ExtractRequestVerificationToken(html);

            var formData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", token },
                { "id", gc.Id.ToString() }
            };
            var content = new FormUrlEncodedContent(formData);

            var postResponse = await client.PostAsync($"/GameCompetitorEvents/Delete/{gc.Id}", content);
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            using var verifyDbScope = _factory.Services.CreateScope();
            var dbVerify = verifyDbScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbVerify.GameCompetitorsEvent.Any(g => g.Id == gc.Id).Should().BeFalse();
        }

        [Fact]
        public async Task Details_ShouldShowPicksAndScores_FromConfiguratie()
        {
            var db = GetDbContext();
            db.Database.EnsureCreated();

            var user = new ApplicationUser { Id = "u6", FirstName = "Daan", LastName = "Test", Email = "daan@test.com" };
            db.Users.Add(user);

            var configuratie = new Configuration { ConfigurationType = "Details Config" };
            db.Add(configuratie);
            await db.SaveChangesAsync();

            var ev = new Event { EventName = "DetailsEvent", EventYear = 2025, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(2), IsActive = true, Configuration = configuratie };
            db.Events.Add(ev);
            await db.SaveChangesAsync();

            var confItem = new ConfigurationItem { Position = 1, Score = 45 };
            db.ConfigurationItems.Add(confItem);
            await db.SaveChangesAsync();

            var gc = new GameCompetitorEvent { EventId = ev.EventId, UserId = user.Id, TeamName = "Team Daan" };
            db.GameCompetitorsEvent.Add(gc);

            var country = new Country { CountryNameShort = "NL", CountryNameLong = "Nederland" };
            db.Countries.Add(country);
            await db.SaveChangesAsync();

            var team = new Team { CurrentTeamName = "Toppers", CountryId = country.CountryId };
            db.Teams.Add(team);
            await db.SaveChangesAsync();

            var comp = new Competitor { FirstName = "Jan", LastName = "Deelnemer", CountryId = country.CountryId };
            db.Competitors.Add(comp);
            await db.SaveChangesAsync();

            var cit = new CompetitorInTeam { CompetitorId = comp.CompetitorId, TeamId = team.TeamId, Year = 2025 };
            db.CompetitorInTeams.Add(cit);
            await db.SaveChangesAsync();

            var cie = new CompetitorsInEvent { EventId = ev.EventId, CompetitorInTeamId = cit.Id };
            db.CompetitorsInEvent.Add(cie);
            await db.SaveChangesAsync();

            var stage = new Stage { EventId = ev.EventId, StageName = "Etappe 1", StageOrder = 1 };
            db.Stages.Add(stage);
            await db.SaveChangesAsync();

            db.Results.Add(new Result { StageId = stage.Id, CompetitorInEventId = cie.Id, ConfigurationItemId = confItem.Id });
            db.GameCompetitorEventPicks.Add(new GameCompetitorEventPick { GameCompetitorEventId = gc.Id, CompetitorsInEventId = cie.Id });
            await db.SaveChangesAsync();

            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/GameCompetitorEvents/Details?id={gc.Id}&eventId={ev.EventId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await response.Content.ReadAsStringAsync();
            html.Should().Contain("Team Daan");
            html.Should().Contain("Jan");
            html.Should().Contain("Deelnemer");
            html.Should().Contain("45");
        }
    }
}
