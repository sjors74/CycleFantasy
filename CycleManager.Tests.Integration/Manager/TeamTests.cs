using CycleManager.Domain.Dto;
using CycleManager.Services.Interfaces;
using CycleManager.Tests.Integration.Helpers;
using Domain.Context;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.RegularExpressions;

namespace CycleManager.Tests.Integration.Manager
{
    public class TeamTests
    {
        private CustomWebApplicationFactory CreateFactory() => new CustomWebApplicationFactory();
        private HttpClient CreateClient(CustomWebApplicationFactory factory)
        {
            return factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public void Factory_Should_Seed_Database()
        {
            using var factory = CreateFactory();
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var teams = db.Teams.Include(t => t.TeamYears).ToList();

            teams.Should().NotBeEmpty();
            teams.First().CurrentTeamName.Should().Be("OriginalTeam");
        }

        [Fact]
        public async Task TestIndexPage_Should_Return_OK()
        {
            using var factory = CreateFactory();
            using var client = CreateClient(factory);

            var response = await client.GetAsync("/Teams");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await response.Content.ReadAsStringAsync();
            html.Should().Contain("Teams");
        }

        [Fact]
        public async Task Index_Should_Display_SeededTeam()
        {
            using var factory = CreateFactory();
            using var client = CreateClient(factory);

            var response = await client.GetAsync("/Teams");
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();
            html.Should().Contain("OriginalTeam");
            html.Should().Contain("be");
        }

        [Fact]
        public async Task CreateTeam_Should_Return_Redirect()
        {
            using var factory = CreateFactory();
            using var client = CreateClient(factory);

            var getResponse = await client.GetAsync("/teams/create");
            var html = await getResponse.Content.ReadAsStringAsync();

            var token = Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"(.+?)\"").Groups[1].Value;

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["CurrentTeamName"] = "TestTeam",
                ["CountryId"] = "1",
                ["PcsName"] = "PCS1"
            };

            var postContent = new FormUrlEncodedContent(formData);
            var response = await client.PostAsync("/teams/create", postContent);

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location!.OriginalString.Should().Contain("/Teams");
        }

        [Fact]
        public async Task EditTeam_Should_Return_Redirect_And_UpdateTeam()
        {
            using var factory = CreateFactory();
            using var client = CreateClient(factory);

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var team = db.Teams.Include(t => t.TeamYears).First();

            var getHtml = await (await client.GetAsync($"/Teams/Edit/{team.TeamId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["TeamId"] = team.TeamId.ToString(),
                ["CurrentTeamName"] = "UpdatedTeam",
                ["CountryId"] = "2",
                ["PcsName"] = "UpdatedPCS",
                ["TeamYears[0].Year"] = "2025",
                ["TeamYears[0].Name"] = "2025",
                ["TeamYears[1].Year"] = "2026",
                ["TeamYears[1].Name"] = "2026",
                ["TeamYears[2].Year"] = "2027",
                ["TeamYears[2].Name"] = "2027",
                ["TeamYears[3].Year"] = "2028",
                ["TeamYears[3].Name"] = "2028"
            };

            var postContent = new FormUrlEncodedContent(formData);
            var postResponse = await client.PostAsync($"/Teams/Edit/{team.TeamId}", postContent);

            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);
            postResponse.Headers.Location!.OriginalString.Should().Contain("/Teams");

            // Verifieer DB
            using var verifyScope = factory.Services.CreateScope();
            var dbVerify = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedTeam = dbVerify.Teams.Include(t => t.TeamYears).First(t => t.TeamId == team.TeamId);

            updatedTeam.CurrentTeamName.Should().Be("UpdatedTeam");
            updatedTeam.PcsName.Should().Be("UpdatedPCS");
            updatedTeam.TeamYears.FirstOrDefault(y => y.Year == 2025)?.Name.Should().Be("2025");
            updatedTeam.TeamYears.FirstOrDefault(y => y.Year == 2028)?.Name.Should().Be("2028");
        }

        [Fact]
        public async Task Delete_Should_Remove_Team_And_Redirect()
        {
            using var factory = CreateFactory();
            using var client = CreateClient(factory);

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var teamId = db.Teams.First().TeamId;

            var getHtml = await (await client.GetAsync($"/teams/delete/{teamId}")).Content.ReadAsStringAsync();
            var token = Regex.Match(getHtml, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"(.+?)\"").Groups[1].Value;

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["TeamId"] = teamId.ToString()
            });

            var response = await client.PostAsync($"/teams/delete/{teamId}", content);
            response.StatusCode.Should().Be(HttpStatusCode.Found);

            // Controleer dat team weg is
            using var verifyScope = factory.Services.CreateScope();
            var dbVerify = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbVerify.Teams.Any(t => t.TeamId == teamId).Should().BeFalse();
        }

        [Fact]
        public async Task Details_Should_DisplayRidersPerYear()
        {
            using var factory = CreateFactory();
            using var client = CreateClient(factory);

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var team = db.Teams.Include(t => t.TeamYears)
                               .Include(t => t.CompetitorInTeams)
                                   .ThenInclude(cit => cit.Competitor)
                               .First();

            var year = 2025;

            var response = await client.GetAsync($"/Teams/Details/{team.TeamId}?year={year}");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            html.Should().Contain(team.CurrentTeamName);

            var competitorsForYear = team.CompetitorInTeams
                .Where(cit => cit.Year == year)
                .Select(cit => cit.Competitor.CompetitorName);

            foreach (var riderName in competitorsForYear)
                html.Should().Contain(riderName);
        }

        [Fact]
        public async Task ScrapeCompetitors_Should_Add_NewCompetitor_ForGivenYear()
        {
            using var factory = CreateFactory();
            using var client = CreateClient(factory);

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var scraperService = scope.ServiceProvider.GetRequiredService<IScraperService>();

            db.Competitors.RemoveRange(db.Competitors);
            await db.SaveChangesAsync();

            var dto = new ScrapeRequestDto { TeamId = 1, Year = 2025 };

            await scraperService.RunCompetitorsAsync(dto.TeamId, dto.Year);
            await scraperService.ImportScrapedCompetitorsAsync();

            var competitor = db.Competitors.FirstOrDefault(c => c.ScraperName.Contains("_2025"));
            competitor.Should().NotBeNull();

            var cit = db.CompetitorInTeams.FirstOrDefault(c => c.TeamId == 1 && c.CompetitorId == competitor.CompetitorId && c.Year == 2025);
            cit.Should().NotBeNull();

            var htmlResponse = await client.GetAsync($"/Teams/Details/1?year=2025");
            htmlResponse.EnsureSuccessStatusCode();
            var html = await htmlResponse.Content.ReadAsStringAsync();
            html.Should().Contain(competitor.ScraperName);
        }

        [Fact]
        public async Task Edit_Should_Add_NewTeamYear()
        {
            using var factory = CreateFactory();
            using var client = CreateClient(factory);

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var team = db.Teams.Include(t => t.TeamYears).First();

            var getHtml = await (await client.GetAsync($"/Teams/Edit/{team.TeamId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["TeamId"] = team.TeamId.ToString(),
                ["CurrentTeamName"] = team.CurrentTeamName,
                ["CountryId"] = team.CountryId.ToString(),
                ["PcsName"] = team.PcsName,
                ["TeamYears[0].Year"] = "2025",
                ["TeamYears[0].Name"] = "Team2025Renamed",
                ["TeamYears[3].Year"] = "2028",
                ["TeamYears[3].Name"] = "Team2028New"
            };

            var postContent = new FormUrlEncodedContent(formData);
            var postResponse = await client.PostAsync($"/Teams/Edit/{team.TeamId}", postContent);

            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            using var verifyScope = factory.Services.CreateScope();
            var dbVerify = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedTeam = dbVerify.Teams.Include(t => t.TeamYears).First(t => t.TeamId == team.TeamId);
            updatedTeam.TeamYears.FirstOrDefault(y => y.Year == 2028)?.Name.Should().Be("Team2028New");
        }

        [Fact]
        public async Task Edit_Should_Remove_TeamYear()
        {
            using var factory = new CustomWebApplicationFactory();
            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false});
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var team = db.Teams.Include(t => t.TeamYears).First();

            var getHtml = await (await client.GetAsync($"/Teams/Edit/{team.TeamId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            // Verwijder 2026 door hem weg te laten
            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["TeamId"] = team.TeamId.ToString(),
                ["CurrentTeamName"] = team.CurrentTeamName,
                ["CountryId"] = team.CountryId.ToString(),
                ["PcsName"] = team.PcsName,
                ["TeamYears[0].Year"] = "2025",
                ["TeamYears[0].Name"] = "Team2025Renamed",
                ["TeamYears[2].Year"] = "2027",
                ["TeamYears[2].Name"] = "Team2027Renamed"
            };

            var postResponse = await client.PostAsync($"/Teams/Edit/{team.TeamId}", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            using var verifyScope = factory.Services.CreateScope();
            var dbVerify = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedTeam = dbVerify.Teams.Include(t => t.TeamYears).First(t => t.TeamId == team.TeamId);

            updatedTeam.TeamYears.FirstOrDefault(y => y.Year == 2026).Should().BeNull();
        }

        [Fact]
        public async Task ScrapeCompetitors_NoData_Should_NotFail()
        {
            var teamId = 1;
            var year = 2030; // veronderstel: geen data voor dit jaar
            using var factory = new CustomWebApplicationFactory();
            factory.ResetDatabase();

            using var scope = factory.Services.CreateScope();
            var scraperService = scope.ServiceProvider.GetRequiredService<IScraperService>();

            await scraperService.RunCompetitorsAsync(teamId, year); // zou geen exception moeten geven
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.ScrapedCompetitors.Where(sc => sc.TeamId == teamId && sc.Year == year).Should().BeEmpty();
        }

    }
}
