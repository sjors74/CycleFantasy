using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using CycleManager.Tests.Integration.Helpers;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.RegularExpressions;

namespace CycleManager.Tests.Integration.Manager
{
    public class TeamTests : ManagerIntegrationTestBase
    {
        [Fact]
        public async Task TestIndexPage_Should_Return_OK()
        {
            var client = _client;

            var response = await client.GetAsync("/Teams");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await response.Content.ReadAsStringAsync();
            html.Should().Contain("Teams");
        }

        [Fact]
        public async Task CreateTeam_Should_Return_Redirect()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

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
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var team = await CreateTestTeamWithThreeYearsAsync(db);

            var getHtml = await (await client.GetAsync($"/Teams/Edit/{team.TeamId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["TeamId"] = team.TeamId.ToString(),
                ["CurrentTeamName"] = "UpdatedTeam",
                ["CountryId"] = "2",
                ["PcsName"] = "UpdatedPCS",
                ["TeamYears[0].SeasonYearId"] = "1",
                ["TeamYears[0].Year"] = "2025",
                ["TeamYears[0].Name"] = "2025",
                ["TeamYears[1].SeasonYearId"] = "2",
                ["TeamYears[1].Year"] = "2026",
                ["TeamYears[1].Name"] = "2026",
                ["TeamYears[2].SeasonYearId"] = "3",
                ["TeamYears[2].Year"] = "2027",
                ["TeamYears[2].Name"] = "2027",
            };

            var postContent = new FormUrlEncodedContent(formData);
            var postResponse = await client.PostAsync($"/Teams/Edit/{team.TeamId}", postContent);

            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);
            postResponse.Headers.Location!.OriginalString.Should().Contain("/Teams");

            // Verifieer DB
            using var verifyScope = _factory.Services.CreateScope();
            var dbVerify = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedTeam = dbVerify.Teams.Include(t => t.TeamYears).First(t => t.TeamId == team.TeamId);

            updatedTeam.CurrentTeamName.Should().Be("UpdatedTeam");
            updatedTeam.PcsName.Should().Be("UpdatedPCS");
            updatedTeam.TeamYears.Should().HaveCount(3);
            updatedTeam.TeamYears.Should().ContainSingle(y => y.Year == 2025 && y.Name == "2025");
            updatedTeam.TeamYears.Should().ContainSingle(y => y.Year == 2026 && y.Name == "2026");
            updatedTeam.TeamYears.Should().ContainSingle(y => y.Year == 2027 && y.Name == "2027");
        }

        [Fact]
        public async Task Delete_Should_Remove_Team_And_Redirect()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var country = new Country
            {
                CountryNameLong = "Nederland",
                CountryNameShort = "NL"
            };

            db.Countries.Add(country);
            await db.SaveChangesAsync();

            var team = new Team
            {
                CurrentTeamName = "Delete Test Team",
                CountryId = country.CountryId
            };

            db.Teams.Add(team);
            await db.SaveChangesAsync();

            var teamId = team.TeamId;

            var getResponse = await client.GetAsync($"/Teams/Delete/{teamId}");
            getResponse.EnsureSuccessStatusCode();

            var getHtml = await getResponse.Content.ReadAsStringAsync();

            var token = Regex.Match(
                getHtml, 
                "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"(.+?)\""
            ).Groups[1].Value;

            token.Should().NotBeNullOrEmpty();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["TeamId"] = teamId.ToString()
            });

            var response = await client.PostAsync(
                $"/teams/delete/{teamId}", 
                content);

            response.StatusCode.Should().Be(HttpStatusCode.Found);

            // Controleer dat team weg is
            using var verifyScope = _factory.Services.CreateScope();
            var dbVerify = verifyScope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            dbVerify.Teams.Any(t => t.TeamId == teamId)
                .Should().BeFalse();
        }

        [Fact]
        public async Task Details_Should_DisplayRidersPerYear()
        {
            var client = _client;

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var year = 2025;

            var teamYear = await CreateTestTeamWithYearAsync(db, year);

            var response = await client.GetAsync(
                $"/Teams/Details/{teamYear.TeamId}?year={year}");

            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();

            html.Should().Contain(teamYear.Name);

        }

        [Fact]
        public async Task ScrapeCompetitors_Should_Add_NewCompetitor_ForGivenYear()
        {
            var client = _client;

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var scraperService = scope.ServiceProvider.GetRequiredService<IScraperService>();

            db.Competitors.RemoveRange(db.Competitors);
            await db.SaveChangesAsync();

            var team = await CreateTestTeamWithYearAsync(db, 2025);

            var teamYear = await db.TeamYear
                .Include(ty => ty.SeasonYear)
                .FirstAsync(ty =>
                    ty.TeamId == team.TeamId &&
                    ty.SeasonYear.Year == 2025);

            var belgium = new Country
            {
                CountryNameLong = "België",
                CountryNameShort = "be"
            };

            db.Countries.Add(belgium);
            await db.SaveChangesAsync();

            await scraperService.RunCompetitorsAsync(teamYear.TeamYearId);
            await scraperService.ImportScrapedCompetitorsAsync();

            var competitor = db.Competitors
                .FirstOrDefault(c => c.PcsScraperName != null &&
                                     c.PcsScraperName.Contains("_2025"));
            competitor.Should().NotBeNull();

            var cit = db.CompetitorInTeams.FirstOrDefault(c =>
                c.TeamYear.TeamId == team.TeamId &&
                c.CompetitorId == competitor.CompetitorId &&
                c.TeamYear.Year == 2025);

            cit.Should().NotBeNull();

            var htmlResponse = await client.GetAsync($"/Teams/Details/1?year=2025");
            htmlResponse.EnsureSuccessStatusCode();
            var html = await htmlResponse.Content.ReadAsStringAsync();
            html.Should().Contain(competitor.PcsScraperName);
        }

        [Fact]
        public async Task Edit_Should_Update_ExistingTeamYear()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Arrange
            var teamYear = await CreateTestTeamWithYearAsync(db, 2025);
            var team = teamYear.Team;

            var getResponse = await client.GetAsync(
                $"/Teams/Edit/{team.TeamId}");

            getResponse.EnsureSuccessStatusCode();

            var getHtml = await getResponse.Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["TeamId"] = team.TeamId.ToString(),
                ["CurrentTeamName"] = team.CurrentTeamName,
                ["CountryId"] = team.CountryId!.Value.ToString(),
                ["PcsName"] = team.PcsName ?? "",

                ["TeamYears[0].SeasonYearId"] =
                    teamYear.SeasonYearId.ToString(),

                ["TeamYears[0].Year"] = "2025",
                ["TeamYears[0].Name"] = "Team2025Renamed"
            };

            var postContent = new FormUrlEncodedContent(formData);

            // Act
            var postResponse = await client.PostAsync(
                $"/Teams/Edit/{team.TeamId}",
                postContent);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            using var verifyScope = _factory.Services.CreateScope();
            var dbVerify = verifyScope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var updatedTeam = await dbVerify.Teams
                .Include(t => t.TeamYears)
                .FirstAsync(t => t.TeamId == team.TeamId);

            var updatedTeamYear = updatedTeam.TeamYears
                .FirstOrDefault(ty =>
                    ty.SeasonYearId == teamYear.SeasonYearId);

            updatedTeamYear.Should().NotBeNull();
            updatedTeamYear!.Name.Should().Be("Team2025Renamed");
        }

        [Fact]
        public async Task Edit_Should_Remove_TeamYear()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var team = await CreateTestTeamWithThreeYearsAsync(db);

            var getHtml = await (await client.GetAsync($"/Teams/Edit/{team.TeamId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            // Verwijder 2026 door hem weg te laten
            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["TeamId"] = team.TeamId.ToString(),
                ["CurrentTeamName"] = team.CurrentTeamName,
                ["CountryId"] = team.CountryId!.Value.ToString(),
                ["PcsName"] = team.PcsName,
                ["TeamYears[0].TeamYearId"] = "1",
                ["TeamYears[0].SeasonYearId"] = "1",
                ["TeamYears[0].Year"] = "2025",
                ["TeamYears[0].Name"] = "Team2025Renamed",
                ["TeamYears[1].TeamYearId"] = "3",
                ["TeamYears[1].SeasonYearId"] = "3",
                ["TeamYears[1].Year"] = "2027",
                ["TeamYears[1].Name"] = "Team2027Renamed"
            };

            var postResponse = await client.PostAsync($"/Teams/Edit/{team.TeamId}", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            using var verifyScope = _factory.Services.CreateScope();
            var dbVerify = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedTeam = dbVerify.Teams.Include(t => t.TeamYears).First(t => t.TeamId == team.TeamId);

            updatedTeam.TeamYears.FirstOrDefault(y => y.Year == 2026).Should().BeNull();
        }

        [Fact]
        public async Task ScrapeCompetitors_NoData_Should_NotFail()
        {
            // Arrange
            using var factory = new CustomWebApplicationFactory();
            factory.ResetDatabase();

            using var scope = factory.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var scraperService = scope.ServiceProvider.GetRequiredService<IScraperService>();

            var seasonYear = new SeasonYear
            {
                Year = 2025
            };

            var team = new Team
            {
                CurrentTeamName = "Test Team",
                CountryId = 1,
                PcsName = "Test Team"
            };

            var teamYear = new TeamYear
            {
                TeamId = team.TeamId,
                Team = team,
                SeasonYearId = seasonYear.SeasonYearId,
                SeasonYear = seasonYear,
                Year = 2025,
                Name = "Test Team 2025"
            };

            db.SeasonYears.Add(seasonYear);
            db.Teams.Add(team);
            db.TeamYear.Add(teamYear);

            await db.SaveChangesAsync();

            // Controleer expliciet dat de testdata geen competitors bevat
            db.CompetitorInTeams
                .Where(cit => cit.TeamYearId == teamYear.TeamYearId)
                .Should()
                .BeEmpty();

            // Act
            var act = async () =>
                await scraperService.RunCompetitorsAsync(teamYear.TeamYearId);

            // Assert
            await act.Should().NotThrowAsync();
        }
    }
}
