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

        [Fact]
        public async Task Index_Should_DisplayAllEvents()
        {
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
            var ev = await db.Events.FirstAsync();

            var response = await _client.GetAsync($"/Events/Details/{ev.EventId}");
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();
            var decodedHtml = WebUtility.HtmlDecode(html);

            decodedHtml.Should().Contain(ev.EventName);
        }

        [Fact]
        public async Task Create_Should_AddNewEvent()
        {
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
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var @event = await db.Events.FirstOrDefaultAsync();
            if (@event == null)
            {
                @event = new Event
                {
                    EventName = "Test Event",
                    EventYear = 2025,
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(1)
                };
                db.Events.Add(@event);
                await db.SaveChangesAsync();
            }

            var getHtml = await (await _client.GetAsync($"/Events/Edit/{@event.EventId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Id"] = @event.EventId.ToString(),
                ["Name"] = @event.EventName + "_Edited",
                ["Code"] = @event.EventCode,
                ["Year"] = @event.EventYear.ToString(),
                ["StartDate"] = @event.StartDate?.ToString("yyyy-MM-dd"),
                ["EndDate"] = @event.EndDate?.ToString("yyyy-MM-dd"),
                ["IsActive"] = @event.IsActive ? "true" : "false",
                ["ShowPodium"] = @event.ShowPodium ? "true" : "false"
            };

            var postResponse = await _client.PostAsync($"/Events/Edit/{@event.EventId}", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            var updated = await db.Events.AsNoTracking().FirstAsync(e => e.EventId == @event.EventId);
            updated.EventName.Should().EndWith("_Edited");
        }

        [Fact]
        public async Task Delete_Should_RemoveEvent()
        {
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
    }
}
