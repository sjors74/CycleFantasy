using CycleManager.Tests.Integration.Helpers;
using Domain.Context;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace CycleManager.Tests.Integration.Manager
{
    [Collection("NonParallelTests")]
    public class RennersTests : ManagerIntegrationTestBase
    {
        [Fact]
        public async Task Index_Should_DisplayCompetitorsPerYear()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var year = 2025;

            var competitor = await CreateTestCompetitorWithTeamAsync(db, year, "Rider", "2025");

            var seasonYear = await db.SeasonYears.
                FirstAsync(sy => sy.Year == year);

            var response = await _client.GetAsync(
                $"/Competitors?seasonYearId={seasonYear.SeasonYearId}");

            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();

            html.Should().Contain(competitor.FirstName);
            html.Should().Contain(competitor.LastName);
            html.Should().Contain(competitor.Country.CountryNameShort);
        }

        [Theory]
        [InlineData("Rider One")]
        [InlineData("Two")]
        public async Task Index_SearchByName_Should_Filter(string searchTerm)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await CreateActiveSeasonYearAsync(db);

            await CreateTestCompetitorAsync(
                db,
                firstName: "Rider",
                lastName: "One");

            await CreateTestCompetitorAsync(
                db,
                firstName: "Rider",
                lastName: "Two");

            var response = await _client.GetAsync(
                $"/Competitors?searchString={Uri.EscapeDataString(searchTerm)}");

            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();

            html.Should().Contain(searchTerm);
        }

        [Fact]
        public async Task Details_Should_DisplayCompetitorData()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var competitor = await CreateTestCompetitorAsync(db);

            var savedCompetitor = await db.Competitors
                .Include(c => c.Country)
                .FirstAsync(c => c.CompetitorId == competitor.CompetitorId);

            var response = await _client.GetAsync($"/Competitors/Details/{savedCompetitor.CompetitorId}");
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();

            html.Should().Contain(savedCompetitor.FirstName);
            html.Should().Contain(savedCompetitor.LastName);
            html.Should().Contain(savedCompetitor.Country.CountryNameShort);
        }

        [Fact]
        public async Task Edit_Should_UpdateCompetitor()
        {
            var _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false // Zorg ervoor dat redirects niet automatisch worden gevolgd
            });
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var year = 2026;
            var competitor = await CreateTestCompetitorWithTeamAsync(db, year);

            var getHtml = await (await _client.GetAsync($"/Competitors/Edit/{competitor.CompetitorId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["CompetitorId"] = competitor.CompetitorId.ToString(),
                ["FirstName"] = competitor.FirstName + "_Edited",
                ["LastName"] = competitor.LastName + "_Edited",
                ["CountryId"] = competitor.CountryId.ToString(),
                ["PcsName"] = competitor.PcsName + "_Edited",
                ["PcsScraperName"] = competitor.PcsScraperName + "_Edited",
                // markeer eerste jaar als nationaal kampioen
                ["CompetitorInTeams[0].CompetitorInTeamId"] = competitor.CompetitorInTeams.ElementAt(0).Id.ToString(),
                ["CompetitorInTeams[0].TeamYearId"] = competitor.CompetitorInTeams.ElementAt(0).TeamYearId.ToString(),
                ["CompetitorInTeams[0].IsNationalChampion"] = "true"
            };

            var postResponse = await _client.PostAsync($"/Competitors/Edit/{competitor.CompetitorId}", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            // Refresh db
            var updated = db.Competitors
                            .Include(c => c.CompetitorInTeams)
                            .AsNoTracking()
                            .First(c => c.CompetitorId == competitor.CompetitorId);
            updated.FirstName.Should().EndWith("_Edited");
            updated.LastName.Should().EndWith("_Edited");
            updated.PcsName.Should().EndWith("_Edited");
            updated.PcsScraperName.Should().EndWith("_Edited");
            updated.CompetitorInTeams.ElementAt(0).IsNationalChampion.Should().BeTrue();
        }

        [Fact]
        public async Task Delete_Should_RemoveCompetitor()
        {
            var _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false // Zorg ervoor dat redirects niet automatisch worden gevolgd
            });

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var competitor = await CreateTestCompetitorAsync(db);

            var getHtml = await (await _client.GetAsync($"/Competitors/Delete/{competitor.CompetitorId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["CompetitorId"] = competitor.CompetitorId.ToString()
            };

            var postResponse = await _client.PostAsync($"/Competitors/Delete/{competitor.CompetitorId}", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            var deleted = db.Competitors.FirstOrDefault(c => c.CompetitorId == competitor.CompetitorId);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task Edit_InvalidModel_Should_ReturnViewWithErrors()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var competitor = await CreateTestCompetitorAsync(db);

            var getHtml = await (await _client.GetAsync($"/Competitors/Edit/{competitor.CompetitorId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["CompetitorId"] = competitor.CompetitorId.ToString(),
                ["FirstName"] = "", // ongeldig
                ["LastName"] = "",
                ["CountryId"] = competitor.CountryId.ToString()
            };

            var post = await _client.PostAsync($"/Competitors/Edit/{competitor.CompetitorId}", new FormUrlEncodedContent(formData));
            var html = await post.Content.ReadAsStringAsync();

            post.StatusCode.Should().Be(HttpStatusCode.OK); // blijft op zelfde pagina
            html.Should().Contain("name=\"FirstName\""); // formulier opnieuw gerenderd
        }

        [Fact]
        public async Task DeleteConfirmed_Should_NotFail_WhenCompetitorDoesNotExist()
        {
            // Arrange
            var nonExistingId = 9999;

            // Act (GET)
            var getResponse = await _client.GetAsync($"/Competitors/Delete/{nonExistingId}");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteConfirmed_Should_Redirect_WhenCompetitorDoesNotExist()
        {
            // Arrange
            var _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false // Zorg ervoor dat redirects niet automatisch worden gevolgd
            });

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var nonExistingId = 9999;

            // Eerst een geldige token ophalen van een bestaand formulier
            var competitor = await CreateTestCompetitorAsync(db);
            var getHtml = await (await _client.GetAsync($"/Competitors/Delete/{competitor.CompetitorId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            };

            // Act
            var postResponse = await _client.PostAsync($"/Competitors/Delete/{nonExistingId}", new FormUrlEncodedContent(formData));

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);
            postResponse.Headers.Location!.ToString().Should().Be("/Competitors"); // redirect naar index
        }

        [Fact]
        public async Task Details_Should_HaveNavigationButtons()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var competitor = await CreateTestCompetitorAsync(db);

            var response = await _client.GetAsync($"/Competitors/Details/{competitor.CompetitorId}");
            var html = await response.Content.ReadAsStringAsync();

            html.Should().Contain("href=\"/Competitors\"");
            html.Should().Contain($"/Competitors/Edit/{competitor.CompetitorId}");
        }

        [Fact]
        public async Task SearchCompetitors_Should_ReturnMatchingJsonResults()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var competitor = await CreateTestCompetitorAsync(db);

            // Act
            var response = await _client.GetAsync($"/Competitors/SearchCompetitors?term={competitor.LastName}");
            response.EnsureSuccessStatusCode();

            // Assert
            var json = await response.Content.ReadAsStringAsync();
            json.Should().Contain(competitor.FirstName);
            json.Should().Contain(competitor.LastName);
        }

        [Fact]
        public async Task GetCompetitorInfo_Should_ReturnTeamAndCountryData()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var year = 2026;
            var competitor = await CreateTestCompetitorWithTeamAsync(db, year);

            var seasonYearId = competitor.CompetitorInTeams
                .First()
                .TeamYear
                .SeasonYearId;

            // Act
            var response = await _client.GetAsync(
                $"/Competitors/GetCompetitorInfo?id={competitor.CompetitorId}&seasonYearId={seasonYearId}");

            response.EnsureSuccessStatusCode();

            // Assert
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(json);

            data.GetProperty("country").GetString().Should().Be(competitor.Country.CountryNameLong);
            data.GetProperty("teamName").GetString().Should().Be(competitor.CompetitorInTeams.First().TeamYear.Name);
        }
    }
}
