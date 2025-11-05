using CycleManager.Domain.Dto;
using CycleManager.Services.Interfaces;
using CycleManager.Tests.Integration.Helpers;
using Domain.Context;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace CycleManager.Tests.Integration.Manager
{
    public class TeamTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;
       
        public TeamTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task TestIndexPage_Should_Return_OK()
        {
            // Act
            var response = await _client.GetAsync("/Teams");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await response.Content.ReadAsStringAsync();
            html.Should().Contain("Teams");
        }

        [Fact]
        public void Factory_Should_Seed_Database()
        {
            using var factory = new CustomWebApplicationFactory();
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var teams = db.Teams.Include(t => t.TeamYears).ToList();

            teams.Should().NotBeEmpty();
            teams.First().CurrentTeamName.Should().Be("OriginalTeam");
        }

        [Fact]
        public async Task Index_Should_Display_SeededTeam()
        {
            // Arrange
            using var factory = new CustomWebApplicationFactory();
            using var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/Teams");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            // Assert: seeded data
            html.Should().Contain("OriginalTeam");
            html.Should().Contain("be");
        }

        [Fact]
        public async Task CreateTeam_Should_Return_Redirect()
        {
            // Arrange
            var getResponse = await _client.GetAsync("/teams/create");
            var html = await getResponse.Content.ReadAsStringAsync();

            var tokenMatch = Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"(.+?)\"");
            var token = tokenMatch.Groups[1].Value;

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["CurrentTeamName"] = "TestTeam",
                ["CountryId"] = "1", // moet overeenkomen met seeded CountryId
                ["PcsName"] = "PCS1"
            };

            var vm = new FormUrlEncodedContent(formData);

            // Act
            var response = await _client.PostAsync(
                "teams/create", 
                vm
            );
            var html2 = await response.Content.ReadAsStringAsync();
            await File.WriteAllTextAsync("response.html", html2);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location!.OriginalString.Should().Contain("/Teams");
            var redirectUrl = response.Headers.Location!.OriginalString;
            var followResponse = await _client.GetAsync(redirectUrl);
            followResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task EditTeam_Should_Return_Redirect_And_UpdateTeam()
        {
            // Arrange
            var getResponse = await _client.GetAsync("/teams/edit/1");
            var html = await getResponse.Content.ReadAsStringAsync();

            var tokenMatch = Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"(.+?)\"");
            var token = tokenMatch.Groups[1].Value;
            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["TeamId"] = "1",
                ["CurrentTeamName"] = "UpdatedTeam",
                ["CountryId"] = "2",
                ["PcsName"] = "UpdatedPCS",
                ["TeamYears[0].Year"] = "2025",
                ["TeamYears[0].Name"] = "2025", // leeg mag, maar het veld moet bestaan
                ["TeamYears[1].Year"] = "2026",
                ["TeamYears[1].Name"] = "2026",
                ["TeamYears[2].Year"] = "2027",
                ["TeamYears[2].Name"] = "2027",
                ["TeamYears[3].Year"] = "2028",
                ["TeamYears[3].Name"] = "2028"
            };

            var vm = new FormUrlEncodedContent(formData);

            // Act
            var response = await _client.PostAsync("teams/edit/1", vm);

            // Assert redirect
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location!.OriginalString.Should().Contain("/Teams");

            // Follow-up GET om te controleren dat team is aangepast

            var followResponse = await _client.GetAsync(response.Headers.Location!);
            followResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var html2 = await followResponse.Content.ReadAsStringAsync();
            html2.Should().Contain("UpdatedTeam");
            html2.Should().Contain("be");
        }

        [Fact]
        public async Task Details_Should_DisplayRidersPerYear()
        {
            // Arrange
            using var factory = new CustomWebApplicationFactory();

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var team = db.Teams
                .Include(t => t.TeamYears)
                .Include(t => t.CompetitorInTeams)
                    .ThenInclude(cit => cit.Competitor)
                .First();

            var year = 2025;

            // Act
            using var client = factory.CreateClient();
            var response = await client.GetAsync($"/Teams/Details/{team.TeamId}?year={year}");
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains(team.CurrentTeamName, html);
            Assert.Contains($"href=\"/Teams/Details/{team.TeamId}?year={year}\"", html);

            var competitorsForYear = team.CompetitorInTeams
                .Where(cit => cit.Year == year)
                .Select(cit => cit.Competitor.CompetitorName);

            foreach (var riderName in competitorsForYear)
            {
                Assert.Contains(riderName, html);
            }
        }

        [Fact]
        public async Task Edit_Should_Update_TeamNamesPerYear()
        {
            using var factory = new CustomWebApplicationFactory();
            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var team = db.Teams.Include(t => t.TeamYears).First();

            // Get Edit page
            var getResponse = await client.GetAsync($"/Teams/Edit/{team.TeamId}");
            getResponse.EnsureSuccessStatusCode();
            var getHtml = await getResponse.Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            // Formdata
            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["TeamId"] = team.TeamId.ToString(),
                ["CurrentTeamName"] = "UpdatedTeam",
                ["CountryId"] = team.CountryId.ToString(),
                ["PcsName"] = "PCS_Updated",
                ["TeamYears[0].Year"] = "2025",
                ["TeamYears[0].Name"] = "Team2025Renamed",
                ["TeamYears[2].Year"] = "2027",
                ["TeamYears[2].Name"] = "Team2027Renamed"
            };

            var postContent = new FormUrlEncodedContent(formData);

            // Act
            var postResponse = await client.PostAsync($"/Teams/Edit/{team.TeamId}", postContent);

            // Assert redirect
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);
            postResponse.Headers.Location?.OriginalString.Should().Be("/Teams");

            // Refreshed context voor DB check
            using var verifyScope = factory.Services.CreateScope();
            var dbVerify = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedTeam = dbVerify.Teams.Include(t => t.TeamYears).First(t => t.TeamId == team.TeamId);

            updatedTeam.CurrentTeamName.Should().Be("UpdatedTeam");
            updatedTeam.PcsName.Should().Be("PCS_Updated");

            // TeamYears check
            updatedTeam.TeamYears.FirstOrDefault(y => y.Year == 2025)?.Name.Should().Be("Team2025Renamed");
            updatedTeam.TeamYears.FirstOrDefault(y => y.Year == 2026).Should().BeNull();
            updatedTeam.TeamYears.FirstOrDefault(y => y.Year == 2027)?.Name.Should().Be("Team2027Renamed");
        }

        [Fact]
        public async Task Delete_Should_Remove_Team_And_Redirect()
        {
            // Arrange
            var getResponse = await _client.GetAsync("/teams/delete/1");
            var html = await getResponse.Content.ReadAsStringAsync();
            var token = Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"(.+?)\"").Groups[1].Value;

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["TeamId"] = "1"
            });

            // Act
            var response = await _client.PostAsync("/teams/delete/1", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            var followUp = await _client.GetAsync("/Teams");
            var htmlAfter = await followUp.Content.ReadAsStringAsync();

            htmlAfter.Should().NotContain("SeedTeam");
        }

        [Fact]
        public async Task ScrapeCompetitors_Should_AddRiders_ForGivenYear()
        {
            // Arrange
            var teamId = 1;
            var year = 2025;
            var dto = new ScrapeRequestDto { TeamId = teamId, Year = year };

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var scraperService = scope.ServiceProvider.GetRequiredService<IScraperService>();

            // Act: Stap 1 - Scrape (voegt fake ScrapedCompetitors toe)
            await scraperService.RunCompetitorsAsync(dto.TeamId, dto.Year);

            // Assert: controleer dat de scraped competitors bestaan in ScrapedCompetitors tabel
            var scrapedRiders = db.ScrapedCompetitors
                .Where(sc => sc.TeamId == teamId && sc.Year == year)
                .ToList();

            scrapedRiders.Should().NotBeEmpty();
            scrapedRiders.All(sc => sc.ProcessedAt == null).Should().BeTrue();

            var expectedNames = new[] { "Rider One_2025", "Rider Two_2025" };
            scrapedRiders.Select(sc => sc.RiderName).Should().Contain(expectedNames);

            // Act: Stap 2 - Import (zet ze om naar echte CompetitorInTeam records)
            await scraperService.ImportScrapedCompetitorsAsync();

            // Assert: check dat renners zichtbaar zijn op de details pagina
            var htmlResponse = await _client.GetAsync($"/Teams/Details/{teamId}?year={year}");
            htmlResponse.EnsureSuccessStatusCode();
            var html = await htmlResponse.Content.ReadAsStringAsync();

            foreach (var rider in expectedNames)
            {
                html.Should().Contain(rider);
            }
        }
    }
}

