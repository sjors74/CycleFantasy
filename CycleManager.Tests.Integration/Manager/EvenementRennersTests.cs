using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using Domain.Context;
using Domain.Dto;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Text.RegularExpressions;
using WebCycleManager.Controllers;

namespace CycleManager.Tests.Integration.Manager
{
    [Collection("NonParallelTests")]
    public class EvenementRennersTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public EvenementRennersTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }

        private async Task<(Event ev, Team team1, Team team2)> EnsureTestEventWithTeamsAsync(bool withRiders = true)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.EnsureCreatedAsync();

            var unique = Guid.NewGuid().ToString("N").Substring(0, 6);

            // Landen aanmaken
            var countryNl = new Country { CountryNameShort = "NL", CountryNameLong = "Nederland" };
            var countryBe = new Country { CountryNameShort = "BE", CountryNameLong = "België" };
            db.Countries.AddRange(countryNl, countryBe);

            // Configuratie
            var config = await db.Configurations.FirstOrDefaultAsync();
            if (config == null)
            {
                config = new Configuration { ConfigurationType = $"Default Config {unique}" };
                db.Configurations.Add(config);
            }

            // Teams
            var team1 = new Team { CurrentTeamName = $"Team A {unique}", CountryId = countryNl.CountryId };
            var team2 = new Team { CurrentTeamName = $"Team B {unique}", CountryId = countryBe.CountryId };
            db.Teams.AddRange(team1, team2);

            // Event
            var ev = new Event
            {
                EventName = $"RidersTest {unique}",
                EventCode = $"RT{unique}",
                EventYear = 2025,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(3),
                IsActive = true,
                ConfigurationId = config.Id
            };
            db.Events.Add(ev);

            // Renners + koppeling
            if (withRiders)
            {
                // Competitors
                var comp1 = new Competitor { FirstName = "Jan", LastName = "Jansen", CountryId = countryNl.CountryId, PcsName = "jan_jansen", ScraperName = "jan-jansen" };
                var comp2 = new Competitor { FirstName = "Piet", LastName = "Pietersen", CountryId = countryBe.CountryId, PcsName = "piet_pietersen", ScraperName = "piet-pietersen" };
                db.Competitors.AddRange(comp1, comp2);

                // CompetitorInTeam
                var cit1 = new CompetitorInTeam { CompetitorId = comp1.CompetitorId, TeamId = team1.TeamId, IsNationalChampion = false };
                var cit2 = new CompetitorInTeam { CompetitorId = comp2.CompetitorId, TeamId = team2.TeamId, IsNationalChampion = false };
                db.CompetitorInTeams.AddRange(cit1, cit2);

                // CompetitorInEvent
                var cie1 = new CompetitorsInEvent
                {
                    EventId = ev.EventId,
                    CompetitorInTeamId = cit1.Id,
                    EventNumber = 11,
                    InSelectie = true,
                    OutOfCompetition = false
                };
                db.CompetitorsInEvent.Add(cie1);
            }
            await db.SaveChangesAsync();

            return (ev, team1, team2);
        }

        [Fact]
        public async Task Riders_Should_DisplaySelectedRiders()
        {
            var (ev, _, _) = await EnsureTestEventWithTeamsAsync();

            var response = await _client.GetAsync($"/CompetitorsInEvents?eventId={ev.EventId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await response.Content.ReadAsStringAsync();
            html.Should().Contain("Renners")
                .And.Contain("Jan Jansen")
                .And.Contain("In selectie")
                .And.Contain("Uitgevallen");
        }

        [Fact]
        public async Task Riders_Should_FilterByTeam()
        {
            var (ev, team1, team2) = await EnsureTestEventWithTeamsAsync();

            var response = await _client.GetAsync($"/CompetitorsInEvents?eventId={ev.EventId}&teamId={team1.TeamId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await response.Content.ReadAsStringAsync();
            html.Should().Contain(team1.CurrentTeamName)
                .And.NotContain(team2.CurrentTeamName);
        }

        [Fact]
        public async Task EditRider_Should_UpdateSelectionAndOutOfCompetitionStatus()
        {
            // Arrange – testevent + db-context
            var (ev, _, _) = await EnsureTestEventWithTeamsAsync();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var cie = await db.CompetitorsInEvent.Include(x => x.CompetitorInTeam).FirstAsync();

            // GET om de antiforgery token te halen
            var getResponse = await _client.GetAsync($"/CompetitorsInEvents/Edit/{cie.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var getHtml = await getResponse.Content.ReadAsStringAsync();
            var token = ExtractAntiForgeryToken(getHtml);

            // POST – stuur gewijzigde data
            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["id"] = cie.Id.ToString(),
                ["EventId"] = ev.EventId.ToString(),
                ["CompetitorInEventId"] = cie.Id.ToString(),
                ["EventNumber"] = "99",
                ["InSelection"] = "false",
                ["OutOfCompetition"] = "true"
            };

            var post = await _client.PostAsync($"/CompetitorsInEvents/Edit/{cie.Id}", new FormUrlEncodedContent(formData));

            // Assert redirect (update gelukt)
            post.StatusCode.Should().Be(HttpStatusCode.Redirect);

            // Belangrijk: gebruik een nieuwe scope om de data vers uit de database te halen
            using var verifyScope = _factory.Services.CreateScope();
            var dbVerify = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var updated = await dbVerify.CompetitorsInEvent.FirstAsync(x => x.Id == cie.Id);

            // Assert – waarden moeten aangepast zijn
            updated.EventNumber.Should().Be(99);
            updated.InSelectie.Should().BeFalse();
            updated.OutOfCompetition.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteRider_Should_RemoveFromEvent()
        {
            var (ev, _, _) = await EnsureTestEventWithTeamsAsync();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var cie = await db.CompetitorsInEvent.FirstAsync();

            // Eerst de antiforgery token ophalen (zoals je al doet bij Create)
            var getResponse = await _client.GetAsync($"/CompetitorsInEvents/Delete/{cie.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var getHtml = await getResponse.Content.ReadAsStringAsync();

            var token = ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["id"] = cie.Id.ToString()
            };
            var delete = await _client.PostAsync($"/CompetitorsInEvents/Delete/{cie.Id}", new FormUrlEncodedContent(formData));
            delete.StatusCode.Should().Be(HttpStatusCode.Redirect);

            (await db.CompetitorsInEvent.AnyAsync(x => x.Id == cie.Id)).Should().BeFalse();
        }

        [Fact]
        public async Task AddRiders_Should_AddSelectedRiders_WithDefaults()
        {
            // Arrange: setup test data
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var config = await db.Configurations.FirstOrDefaultAsync() ?? new Configuration { ConfigurationType = "Default Config" };
            if (config.Id == 0)
            {
                db.Configurations.Add(config);
                await db.SaveChangesAsync();
            }

            var ev = new Event
            {
                EventName = "Test Event",
                EventYear = 2025,
                ConfigurationId = config.Id,
                IsActive = true
            };
            db.Events.Add(ev);
            await db.SaveChangesAsync();

            var team = new Team { CurrentTeamName = "Test Team" };
            db.Teams.Add(team);
            await db.SaveChangesAsync();

            var competitor = new Competitor { FirstName = "Jan", LastName = "Tester", CountryId = 1 };
            db.Competitors.Add(competitor);
            await db.SaveChangesAsync();

            var competitorInTeam = new CompetitorInTeam
            {
                CompetitorId = competitor.CompetitorId,
                TeamId = team.TeamId,
                Year = 2025
            };
            db.CompetitorInTeams.Add(competitorInTeam);
            await db.SaveChangesAsync();

            // GET de Create-pagina om anti-forgery token te krijgen
            var getResponse = await _client.GetAsync($"/CompetitorsInEvents/Create?eventId={ev.EventId}&filterTeam={team.TeamId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var getHtml = await getResponse.Content.ReadAsStringAsync();

            // Haal token uit formulier
            var token = ExtractAntiForgeryToken(getHtml);

            // POST de geselecteerde renners
            var postData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", token },
                { "eventId", ev.EventId.ToString() },
                { "SelectCompetitorId", competitorInTeam.Id.ToString() } // selected competitor
            };
            var postContent = new FormUrlEncodedContent(postData);

            var postResponse = await _client.PostAsync("/CompetitorsInEvents/Create", postContent);
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found); // redirect na succesvolle POST

            // Assert: kijk dat de CompetitorInEvent is toegevoegd
            var added = await db.CompetitorsInEvent.FirstOrDefaultAsync(c => c.EventId == ev.EventId && c.CompetitorInTeamId == competitorInTeam.Id);
            added.Should().NotBeNull();
            added.EventNumber.Should().Be(0); // default
            added.InSelectie.Should().BeFalse(); // default
            added.OutOfCompetition.Should().BeFalse(); // default
        }

        [Fact]
        public async Task EditRider_NonExistent_ShouldReturnNotFound()
        {
            var nonExistentId = 999999;

            var getResponse = await _client.GetAsync($"/CompetitorsInEvents/Edit/{nonExistentId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteRider_NonExistent_ShouldReturnNotFound()
        {
            var nonExistentId = 999999;

            var getResponse = await _client.GetAsync($"/CompetitorsInEvents/Delete/{nonExistentId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task AddRiders_NoSelection_ShouldNotAdd()
        {
            // Arrange: event en team
            var (ev, team, _) = await EnsureTestEventWithTeamsAsync(withRiders: false);

            // GET de Create pagina om antiforgery token te krijgen
            var getResponse = await _client.GetAsync($"/CompetitorsInEvents/Create?eventId={ev.EventId}&filterTeam={team.TeamId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var getHtml = await getResponse.Content.ReadAsStringAsync();
            var token = ExtractAntiForgeryToken(getHtml);

            // POST: stuur het formulier zonder selectie
            var postData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["eventId"] = ev.EventId.ToString(),
                ["filterTeam"] = (team?.TeamId ?? 0).ToString()
                // geen SelectCompetitorId
            };
            var postResponse = await _client.PostAsync("/CompetitorsInEvents/Create", new FormUrlEncodedContent(postData));

            // Assert: redirect (ook zonder selectie moet het 302 teruggeven)
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            // Controleer dat er geen nieuwe CompetitorsInEvent is toegevoegd
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await db.CompetitorsInEvent.AnyAsync(e => e.EventId == ev.EventId)).Should().BeFalse();
        }

        [Fact]
        public async Task EditRider_PostWithoutToken_ShouldFail()
        {
            var (ev, _, _) = await EnsureTestEventWithTeamsAsync();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var cie = await db.CompetitorsInEvent.FirstAsync();

            var formData = new Dictionary<string, string>
            {
                ["id"] = cie.Id.ToString(),
                ["EventId"] = ev.EventId.ToString(),
                ["CompetitorInEventId"] = cie.Id.ToString(),
                ["EventNumber"] = "99",
                ["InSelection"] = "false",
                ["OutOfCompetition"] = "true"
            };

            var postResponse = await _client.PostAsync($"/CompetitorsInEvents/Edit/{cie.Id}", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task AddRiders_Multiple_ShouldAddAll()
        {
            // Arrange – setup test event met teams en renners
            var (ev, team1, team2) = await EnsureTestEventWithTeamsAsync(withRiders: false);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Voeg 2 renners toe aan team1
            var comp1 = new Competitor { FirstName = "Renner1", LastName = "Test", CountryId = 1 };
            var comp2 = new Competitor { FirstName = "Renner2", LastName = "Test", CountryId = 1 };
            db.Competitors.AddRange(comp1, comp2);
            await db.SaveChangesAsync();

            var cit1 = new CompetitorInTeam { CompetitorId = comp1.CompetitorId, TeamId = team1.TeamId, Year = 2025 };
            var cit2 = new CompetitorInTeam { CompetitorId = comp2.CompetitorId, TeamId = team1.TeamId, Year = 2025 };
            db.CompetitorInTeams.AddRange(cit1, cit2);
            await db.SaveChangesAsync();

            // GET de Create-pagina om anti-forgery token te verkrijgen
            var getResponse = await _client.GetAsync($"/CompetitorsInEvents/Create?eventId={ev.EventId}&filterTeam={team1.TeamId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var getHtml = await getResponse.Content.ReadAsStringAsync();
            var token = ExtractAntiForgeryToken(getHtml);

            // POST – meerdere renners
            var postData = new List<KeyValuePair<string, string>>
            {
                new("__RequestVerificationToken", token),
                new("eventId", ev.EventId.ToString()),
                new("SelectCompetitorId", cit1.Id.ToString()),
                new("SelectCompetitorId", cit2.Id.ToString())
            };
            var postContent = new FormUrlEncodedContent(postData);

            // Act
            var postResponse = await _client.PostAsync("/CompetitorsInEvents/Create", postContent);

            // Assert redirect → succesvol
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            // Nieuwe scope om DB te controleren
            using var verifyScope = _factory.Services.CreateScope();
            var dbVerify = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var added = await dbVerify.CompetitorsInEvent
                .Where(c => c.EventId == ev.EventId)
                .ToListAsync();

            added.Should().HaveCount(2);
            added.Select(a => a.CompetitorInTeamId).Should().Contain(cit1.Id).And.Contain(cit2.Id);

            // Controleer defaults
            foreach (var a in added)
            {
                a.EventNumber.Should().Be(0);
                a.InSelectie.Should().BeFalse();
                a.OutOfCompetition.Should().BeFalse();
            }
        }

        [Fact]
        public async Task DeleteRider_OneOfMultiple_OthersRemain_HttpClient()
        {
            // Maak een unieke database voor deze test
            var factory = new CustomWebApplicationFactory();
            var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // -------------------------
            // Arrange: Event, team, renners
            // -------------------------
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var country = new Country { CountryNameShort = "NL", CountryNameLong = "Nederland" };
                db.Countries.Add(country);
                await db.SaveChangesAsync();

                var team = new Team { CurrentTeamName = "Team A", CountryId = country.CountryId };
                db.Teams.Add(team);
                await db.SaveChangesAsync();

                var ev = new Event { EventName = "Test Event", EventYear = 2025, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(2), IsActive = true };
                db.Events.Add(ev);
                await db.SaveChangesAsync();

                var competitor1 = new Competitor { FirstName = "Jan", LastName = "Jansen", CountryId = country.CountryId };
                var competitor2 = new Competitor { FirstName = "Piet", LastName = "Pietersen", CountryId = country.CountryId };
                db.Competitors.AddRange(competitor1, competitor2);
                await db.SaveChangesAsync();

                var cit1 = new CompetitorInTeam { CompetitorId = competitor1.CompetitorId, TeamId = team.TeamId, Year = 2025 };
                var cit2 = new CompetitorInTeam { CompetitorId = competitor2.CompetitorId, TeamId = team.TeamId, Year = 2025 };
                db.CompetitorInTeams.AddRange(cit1, cit2);
                await db.SaveChangesAsync();

                var cie1 = new CompetitorsInEvent { EventId = ev.EventId, CompetitorInTeamId = cit1.Id, EventNumber = 10 };
                var cie2 = new CompetitorsInEvent { EventId = ev.EventId, CompetitorInTeamId = cit2.Id, EventNumber = 11 };
                db.CompetitorsInEvent.AddRange(cie1, cie2);
                await db.SaveChangesAsync();
            }

            // -------------------------
            // Act: GET Delete-page om antiforgery token te halen
            // -------------------------
            using var scope2 = factory.Services.CreateScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var cieToDelete = await db2.CompetitorsInEvent.FirstAsync();

            var getResponse = await client.GetAsync($"/CompetitorsInEvents/Delete/{cieToDelete.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var getHtml = await getResponse.Content.ReadAsStringAsync();

            var token = ExtractAntiForgeryToken(getHtml);

            // -------------------------
            // POST Delete
            // -------------------------
            var postData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["id"] = cieToDelete.Id.ToString()
            };

            var postResponse = await client.PostAsync($"/CompetitorsInEvents/Delete/{cieToDelete.Id}", new FormUrlEncodedContent(postData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.Redirect); // redirect bij succes

            // -------------------------
            // Assert: De andere renner blijft
            // -------------------------
            using var scope3 = factory.Services.CreateScope();
            var db3 = scope3.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var remaining = await db3.CompetitorsInEvent.ToListAsync();
            remaining.Count.Should().Be(1);
            remaining.First().CompetitorInTeamId.Should().NotBe(cieToDelete.CompetitorInTeamId);
        }

        [Fact]
        public async Task AddRiders_InvalidCompetitorId_ShouldNotAdd()
        {
            var (ev, team, _) = await EnsureTestEventWithTeamsAsync(withRiders: false);

            // GET de Create-pagina om antiforgery token te halen
            var getResponse = await _client.GetAsync($"/CompetitorsInEvents/Create?eventId={ev.EventId}&filterTeam={team.TeamId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var getHtml = await getResponse.Content.ReadAsStringAsync();
            var token = ExtractAntiForgeryToken(getHtml);


            // POST met ongeldig CompetitorInTeamId
            var postData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["eventId"] = ev.EventId.ToString(),
                ["SelectCompetitorId"] = "999999" // bestaat niet
            };
            var postResponse = await _client.PostAsync("/CompetitorsInEvents/Create", new FormUrlEncodedContent(postData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found); // redirect

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await db.CompetitorsInEvent.AnyAsync(e => e.EventId == ev.EventId)).Should().BeFalse();
        }

        [Fact]
        public async Task AddRiders_NonExistentEvent_ShouldReturnNotFound()
        {
            var invalidEventId = 999999;

            var getResponse = await _client.GetAsync($"/CompetitorsInEvents/Create?eventId={invalidEventId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var postData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = "fake-token",
                ["eventId"] = invalidEventId.ToString(),
                ["SelectCompetitorId"] = "1"
            };
            var postResponse = await _client.PostAsync("/CompetitorsInEvents/Create", new FormUrlEncodedContent(postData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest); // POST zonder geldig event
        }

        [Fact]
        public void FilterCompetitors_ShouldIncludeOnlyEventTeams()
        {
            // Arrange
            var competitors = new List<CompetitorDto>
            {
                new CompetitorDto { CompetitorId = 1, FirstName= "Ab", LastName = "Pogacar", Teams = new List<CompetitorInTeamDto> { new CompetitorInTeamDto { TeamId = 10 } } },
                new CompetitorDto { CompetitorId = 2, FirstName = "B", LastName = "Evenepoel", Teams = new List<CompetitorInTeamDto> { new CompetitorInTeamDto { TeamId = 99 } } },
            };
            var teamIds = new List<int> { 10 }; // Alleen team 10 hoort erbij

            // Act
            var result = CompetitorsInEventsController.FilterCompetitors(competitors, teamIds, null);

            // Assert
            result.Should().HaveCount(1);
            result.First().CompetitorName.Should().Be("Ab Pogacar");
        }

        [Fact]
        public void FilterCompetitors_WithFilterTeam_ShouldReturnOnlyThatTeam()
        {
            // Arrange
            var competitors = new List<CompetitorDto>
            {
                new CompetitorDto { CompetitorId = 1, FirstName = "A", LastName = "Pogacar", Teams = new List<CompetitorInTeamDto> { new CompetitorInTeamDto { TeamId = 10 } } },
                new CompetitorDto { CompetitorId = 2,FirstName = "B", LastName = "Evenepoel",  Teams = new List<CompetitorInTeamDto> { new CompetitorInTeamDto { TeamId = 20 } } },
            };
            var teamIds = new List<int> { 10, 20 };

            // Act
            var result = CompetitorsInEventsController.FilterCompetitors(competitors, teamIds, 20);

            // Assert
            result.Should().HaveCount(1);
            result.First().CompetitorName.Should().Be("B Evenepoel");
        }

        [Fact]
        public void FilterCompetitors_ShouldHandleNullOrEmptySafely()
        {
            // Arrange
            List<CompetitorDto>? competitors = null;
            var teamIds = new List<int> { 1, 2 };

            // Act
            var result1 = CompetitorsInEventsController.FilterCompetitors(competitors, teamIds, null);
            var result2 = CompetitorsInEventsController.FilterCompetitors(new List<CompetitorDto>(), teamIds, null);

            // Assert
            result1.Should().BeEmpty();
            result2.Should().BeEmpty();
        }

        [Fact]
        public void ParseSelectedCompetitorIds_ParsesValidInts_AndSkipsInvalids()
        {
            // Arrange
            var form = new FormCollection(new Dictionary<string, StringValues>
            {
                { "SelectCompetitorId", new StringValues(new[] { "1", "x", "3", "" }) }
            });

            // Act
            var result = InvokeParseMethod(form);

            // Assert
            result.Should().BeEquivalentTo(new List<int> { 1, 3 });
        }

        // 🔹 Helper to call the private ParseSelectedCompetitorIds via reflection
        private static List<int> InvokeParseMethod(IFormCollection form)
        {
            var method = typeof(CompetitorsInEventsController)
                .GetMethod("ParseSelectedCompetitorIds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (List<int>)method.Invoke(null, new object[] { form });
        }

        private static string ExtractAntiForgeryToken(string html)
        {
            var match = Regex.Match(html, @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""");
            return match.Success ? match.Groups[1].Value : throw new InvalidOperationException("Token not found");
        }

    }

}
