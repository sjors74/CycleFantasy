using CycleManager.Domain.Models;
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
    public class TeamTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly string _dbName;
       
        public TeamTests(CustomWebApplicationFactory factory)
        {
            _dbName = Guid.NewGuid().ToString();

            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Verwijder bestaande DbContext registratie
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Voeg InMemory DbContext toe met specifieke naam
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseInMemoryDatabase(_dbName));

                    // Seed de database
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                    SeedData(db);
                });
            }).CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false

            });
        }

        private void SeedData(ApplicationDbContext db)
        {
            if (!db.Countries.Any())
            {
                db.Countries.Add(new Country { CountryId = 1, CountryNameLong = "Nederland", CountryNameShort = "nl" });
                db.Countries.Add(new Country { CountryId = 2, CountryNameLong = "België", CountryNameShort = "be" });
                db.SaveChanges();
            }

            if (!db.Teams.Any())
            {

                var team = new Team
                {
                    TeamId = 1,
                    CurrentTeamName = "SeedTeam",
                    CountryId = 2,
                    TeamYears = new List<TeamYear>()
                };

                var years = new List<TeamYear>
                {
                    new TeamYear { Year = 2025, Name = "2025", Team = team },
                    new TeamYear { Year = 2026, Name = "2026", Team = team },
                    new TeamYear { Year = 2027, Name = "2027", Team = team },
                    new TeamYear { Year = 2028, Name = "2028", Team = team }
                };

                team.TeamYears = years;

                db.Teams.Add(team);
                db.SaveChanges();
            }
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
        public async Task TestIndexPage_Should_Display_Teams_With_CountryNames()
        {
            // Act
            var response = await _client.GetAsync("/Teams");
            response.EnsureSuccessStatusCode(); // 200

            var html = await response.Content.ReadAsStringAsync();

            // Assert: check dat de teamnaam in de index staat
            html.Should().Contain("SeedTeam");

            // Assert: check dat de landnaam in de index staat
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

    }
}

