using CycleManager.Tests.Integration.Helpers;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace CycleManager.Tests.Integration.Manager
{
    [Collection("NonParallelTests")]
    public class LandenTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public LandenTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task Index_Should_DisplayAllCountries()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var countries = await db.Countries.ToListAsync();

            var response = await _client.GetAsync("/Countries");
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();

            // Decode HTML zodat speciale tekens correct zijn
            var decodedHtml = WebUtility.HtmlDecode(html);

            foreach (var country in countries)
            {
                decodedHtml.Should().Contain(country.CountryNameLong);
                decodedHtml.Should().Contain(country.CountryNameShort);

                // Optioneel: check aantal renners
                var competitorsCount = await db.Competitors.CountAsync(c => c.CountryId == country.CountryId);
                decodedHtml.Should().Contain(competitorsCount.ToString());
            }
        }

        [Fact]
        public async Task Details_Should_DisplayCountry()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var country = await db.Countries.FirstAsync();

            var response = await _client.GetAsync($"/Countries/Details/{country.CountryId}");
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();

            // HTML entities decoderen zodat ë, é, enz. correct zijn
            var decodedHtml = WebUtility.HtmlDecode(html);

            decodedHtml.Should().Contain(country.CountryNameLong);
            decodedHtml.Should().Contain(country.CountryNameShort);
        }
        [Fact]
        public async Task Create_Should_AddNewCountry()
        {
            var getHtml = await (await _client.GetAsync("/Countries/Create")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["CountryNameLong"] = "Testland",
                ["CountryNameShort"] = "TL"
            };

            var postResponse = await _client.PostAsync("/Countries/Create", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var created = await db.Countries.FirstOrDefaultAsync(c => c.CountryNameLong == "Testland");
            created.Should().NotBeNull();
            created.CountryNameShort.Should().Be("TL");
        }

        [Fact]
        public async Task Edit_Should_UpdateCountry()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var country = await db.Countries.FirstAsync();

            var getHtml = await (await _client.GetAsync($"/Countries/Edit/{country.CountryId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["CountryId"] = country.CountryId.ToString(),
                ["CountryNameLong"] = country.CountryNameLong + "_Edited",
                ["CountryNameShort"] = country.CountryNameShort + "_E"
            };

            var postResponse = await _client.PostAsync($"/Countries/Edit/{country.CountryId}", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            var updated = await db.Countries.AsNoTracking().FirstAsync(c => c.CountryId == country.CountryId);
            updated.CountryNameLong.Should().EndWith("_Edited");
            updated.CountryNameShort.Should().EndWith("_E");
        }

        [Fact]
        public async Task Delete_Should_RemoveCountry()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var country = new Country { CountryNameLong = "DeleteLand", CountryNameShort = "DL" };
            db.Countries.Add(country);
            await db.SaveChangesAsync();

            var getHtml = await (await _client.GetAsync($"/Countries/Delete/{country.CountryId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["CountryId"] = country.CountryId.ToString()
            };

            var postResponse = await _client.PostAsync($"/Countries/Delete/{country.CountryId}", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            var deleted = await db.Countries.FirstOrDefaultAsync(c => c.CountryId == country.CountryId);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task DeleteConfirmed_Should_NotFail_WhenCountryDoesNotExist()
        {
            // Arrange
            var nonExistingId = 9999;

            // Act (GET)
            var getResponse = await _client.GetAsync($"/Countries/Delete/{nonExistingId}");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteConfirmed_Should_Redirect_WhenCountryDoesNotExist()
        {
            // Arrange
            var nonExistingId = 9999;

            // Eerst een geldige token ophalen van een bestaand formulier
            var getHtml = await (await _client.GetAsync("/Countries/Delete/1")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            };

            // Act
            var postResponse = await _client.PostAsync($"/Countries/Delete/{nonExistingId}", new FormUrlEncodedContent(formData));

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);
            postResponse.Headers.Location!.ToString().Should().Be("/Countries"); // redirect naar index
        }

        [Fact]
        public async Task Create_Should_ReturnView_WhenModelInvalid()
        {
            // GET token
            var getHtml = await (await _client.GetAsync("/Countries/Create")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            // POST met lege verplichte velden
            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["CountryNameLong"] = "", // verplicht veld leeg
                ["CountryNameShort"] = "" // verplicht veld leeg
            };

            var postResponse = await _client.PostAsync("/Countries/Create", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK); // blijft op dezelfde pagina

            var html = await postResponse.Content.ReadAsStringAsync();

            // Controleer dat de foutmelding zichtbaar is
            html.Should().Contain("The Land field is required")
                .And.Contain("The Afkorting field is required");
        }

        [Fact]
        public async Task Edit_Should_ReturnView_WhenModelInvalid()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var country = await db.Countries.FirstAsync();

            var getHtml = await (await _client.GetAsync($"/Countries/Edit/{country.CountryId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            // POST met lege verplichte velden
            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["CountryId"] = country.CountryId.ToString(),
                ["CountryNameLong"] = "",
                ["CountryNameShort"] = ""
            };

            var postResponse = await _client.PostAsync($"/Countries/Edit/{country.CountryId}", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK); // blijft op dezelfde pagina

            var html = await postResponse.Content.ReadAsStringAsync();

            // Controleer dat de foutmelding zichtbaar is
            html.Should().Contain("The Land field is required")
                .And.Contain("The Afkorting field is required");
        }

    }
}
