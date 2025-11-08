using CycleManager.Tests.Integration.Helpers;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Net;

namespace CycleManager.Tests.Integration.Manager
{
    [Collection("NonParallelTests")]
    public class EvenementenTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public EvenementenTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        private async Task ResetDatabaseAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Events.RemoveRange(db.Events);
            await db.SaveChangesAsync();
        }

        [Fact]
        public async Task Index_Should_DisplayAllEvents()
        {
            await ResetDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var events = await db.Events.ToListAsync();

            var response = await _client.GetAsync("/Events");
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();
            var decodedHtml = WebUtility.HtmlDecode(html);

            foreach (var ev in events)
            {
                decodedHtml.Should().Contain(ev.EventName);
                decodedHtml.Should().Contain(ev.EventYear.ToString());
                decodedHtml.Should().Contain(ev.StartDate?.ToString("yyyy-MM-dd"));
                decodedHtml.Should().Contain(ev.EndDate?.ToString("yyyy-MM-dd"));
            }
        }

        [Fact]
        public async Task Details_Should_DisplayEvent()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Zorg dat er minstens één event bestaat
            var ev = await db.Events.FirstOrDefaultAsync();
            if (ev == null)
            {
                ev = new Event
                {
                    EventName = "Details Event",
                    EventCode = "DE",
                    EventYear = 2025,
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(1),
                    IsActive = true,
                    ShowPodium = false
                };
                db.Events.Add(ev);
                await db.SaveChangesAsync();
            }

            // Haal de details-pagina op
            var response = await _client.GetAsync($"/Events/Details/{ev.EventId}");
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();
            var decodedHtml = WebUtility.HtmlDecode(html);

            // Controleer of de eventnaam correct wordt weergegeven
            decodedHtml.Should().Contain(ev.EventName);
        }

        [Fact]
        public async Task Create_Should_AddNewEvent()
        {
            await ResetDatabaseAsync();

            var getHtml = await (await _client.GetAsync("/Events/Create")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Name"] = "Test Event",
                ["Code"] = "TE",
                ["Year"] = DateTime.Now.Year.ToString(),
                ["StartDate"] = DateTime.Today.ToString("yyyy-MM-dd"),
                ["EndDate"] = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"),
                ["IsActive"] = "true",
                ["ShowPodium"] = "false"
            };

            var postResponse = await _client.PostAsync("/Events/Create", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var created = await db.Events.FirstOrDefaultAsync(e => e.EventName == "Test Event");
            created.Should().NotBeNull();
        }

        [Fact]
        public async Task Edit_Should_UpdateEvent()
        {
            await ResetDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Zorg dat er een geldige configuratie bestaat (sommige events vereisen dat)
            var config = db.Configurations.FirstOrDefault();
            if (config == null)
            {
                config = new Configuration
                { 
                    ConfigurationType = "Default Config",
                };
                db.Configurations.Add(config);
                await db.SaveChangesAsync();
            }

            // Voeg een nieuw event toe dat we kunnen bewerken
            var @event = new Event
            {
                EventName = "Test Event",
                EventCode = "TE",
                EventYear = 2025,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(5),
                IsActive = true,
                ShowPodium = false,
                ConfigurationId = config.Id
            };
            db.Events.Add(@event);
            await db.SaveChangesAsync();

            // Haal de editpagina op en extract de antiforgery-token
            var getHtml = await (await _client.GetAsync($"/Events/Edit/{@event.EventId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            // Vul het formulier in met geldige data
            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Id"] = @event.EventId.ToString(),
                ["Name"] = @event.EventName + "_Edited",
                ["Code"] = @event.EventCode ?? "TE",
                ["Year"] = @event.EventYear.ToString(),
                ["StartDate"] = @event.StartDate?.ToString("yyyy-MM-dd"),
                ["EndDate"] = @event.EndDate?.ToString("yyyy-MM-dd"),
                ["Slogan"] = "Updated Slogan",
                ["CountryCode"] = "BE",
                ["ColorName"] = "Blue",
                ["ConfigurationId"] = config.Id.ToString(),
                ["IsActive"] = "true",
                ["ShowPodium"] = "true"
            };

            // Stuur de POST-request en controleer de redirect
            var postResponse = await _client.PostAsync($"/Events/Edit/{@event.EventId}", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            // Controleer in de database dat de update is doorgevoerd
            var updated = await db.Events.AsNoTracking().FirstAsync(e => e.EventId == @event.EventId);
            updated.EventName.Should().EndWith("_Edited");
            updated.Slogan.Should().Be("Updated Slogan");
            updated.ShowPodium.Should().BeTrue();
        }

        [Fact]
        public async Task Delete_Should_RemoveEvent()
        {
            await ResetDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var ev = new Event { EventName = "Delete Event", EventYear = 2025 };
            db.Events.Add(ev);
            await db.SaveChangesAsync();

            var getHtml = await (await _client.GetAsync($"/Events/Delete/{ev.EventId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            };

            var postResponse = await _client.PostAsync($"/Events/Delete/{ev.EventId}", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            var deleted = await db.Events.FirstOrDefaultAsync(e => e.EventId == ev.EventId);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task DeleteConfirmed_Should_NotFail_WhenEventDoesNotExist()
        {
            var response = await _client.GetAsync("/Events/Delete/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteConfirmed_Should_Redirect_WhenEventDoesNotExist()
        {
            await ResetDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var existingEvent = await db.Events.FirstOrDefaultAsync();
            if (existingEvent == null)
            {
                existingEvent = new Event { EventName = "Temp Event", EventYear = 2025 };
                db.Events.Add(existingEvent);
                await db.SaveChangesAsync();
            }

            var getHtml = await (await _client.GetAsync($"/Events/Delete/{existingEvent.EventId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            };

            // Post naar een non-existent ID
            var postResponse = await _client.PostAsync("/Events/Delete/9999", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);
            postResponse.Headers.Location!.ToString().Should().Be("/Events");
        }

        [Fact]
        public async Task Create_Should_ReturnValidationError_WhenModelInvalid()
        {
            await ResetDatabaseAsync();

            // Haal de Create-pagina op om de anti-forgery token te krijgen
            var getHtml = await (await _client.GetAsync("/Events/Create")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            // Formulier met ongeldig model (lege naam en foutieve datums)
            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Name"] = "",                     // Ongeldige naam
                ["Code"] = "TE",
                ["Year"] = DateTime.Now.Year.ToString(),
                ["StartDate"] = DateTime.Today.AddDays(5).ToString("yyyy-MM-dd"),
                ["EndDate"] = DateTime.Today.ToString("yyyy-MM-dd"), // Start > End
                ["IsActive"] = "true",
                ["ShowPodium"] = "false"
            };

            var postResponse = await _client.PostAsync("/Events/Create", new FormUrlEncodedContent(formData));

            // Geen redirect bij invalid model, statuscode blijft OK
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await postResponse.Content.ReadAsStringAsync();
            var decodedHtml = WebUtility.HtmlDecode(html);

            // Controleer dat foutmeldingen in de pagina aanwezig zijn
            decodedHtml.Should().Contain("The Evenement field is required");
        }

        [Fact]
        public async Task Edit_Should_ReturnValidationError_WhenModelInvalid()
        {
            await ResetDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Zorg dat er een configuratie bestaat
            var config = db.Configurations.FirstOrDefault();
            if (config == null)
            {
                config = new Configuration { ConfigurationType = "Default Config" };
                db.Configurations.Add(config);
                await db.SaveChangesAsync();
            }

            // Voeg een event toe dat we gaan bewerken
            var @event = new Event
            {
                EventName = "Invalid Edit Event",
                EventCode = "IE",
                EventYear = 2025,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(5),
                IsActive = true,
                ShowPodium = false,
                ConfigurationId = config.Id
            };
            db.Events.Add(@event);
            await db.SaveChangesAsync();

            // Haal de editpagina op en extract antiforgery token
            var getHtml = await (await _client.GetAsync($"/Events/Edit/{@event.EventId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            // Verstuur ongeldig formulier (bijv. lege naam)
            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Id"] = @event.EventId.ToString(),
                ["Name"] = "", // Ongeldig
                ["Code"] = @event.EventCode,
                ["Year"] = @event.EventYear.ToString(),
                ["StartDate"] = @event.StartDate?.ToString("yyyy-MM-dd"),
                ["EndDate"] = @event.EndDate?.ToString("yyyy-MM-dd"),
                ["IsActive"] = "true",
                ["ShowPodium"] = "false",
                ["ConfigurationId"] = config.Id.ToString()
            };

            var postResponse = await _client.PostAsync($"/Events/Edit/{@event.EventId}", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await postResponse.Content.ReadAsStringAsync();
            var decodedHtml = WebUtility.HtmlDecode(html);

            // Controleer of de validatie-fout wordt weergegeven
            decodedHtml.Should().Contain("The Evenement field is required");

            var updated = await db.Events.AsNoTracking().FirstAsync(e => e.EventId == @event.EventId);
            updated.EventName.Should().Be("Invalid Edit Event");
        }
    }
}
