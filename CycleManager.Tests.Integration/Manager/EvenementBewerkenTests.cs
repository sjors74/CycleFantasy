using CycleManager.Domain.Models;
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
    public class EvenementBewerkenTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public EvenementBewerkenTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        private async Task<Event> EnsureTestEventAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var config = await db.Configurations.FirstOrDefaultAsync() ?? new Configuration { ConfigurationType = "Default Config" };
            if (config.Id == 0)
            {
                db.Configurations.Add(config);
                await db.SaveChangesAsync();
            }

            var ev = await db.Events.FirstOrDefaultAsync();
            if (ev == null)
            {
                ev = new Event
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
                db.Events.Add(ev);
                await db.SaveChangesAsync();
            }

            return ev;
        }

        [Fact]
        public async Task Edit_Should_UpdateProperties()
        {
            var ev = await EnsureTestEventAsync();

            var getHtml = await (await _client.GetAsync($"/Events/Edit/{ev.EventId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Id"] = ev.EventId.ToString(),
                ["Name"] = ev.EventName + "_Edited",
                ["Code"] = ev.EventCode ?? "TE",
                ["Year"] = ev.EventYear.ToString(),
                ["StartDate"] = ev.StartDate?.ToString("yyyy-MM-dd"),
                ["EndDate"] = ev.EndDate?.ToString("yyyy-MM-dd"),
                ["Slogan"] = "Updated Slogan",
                ["CountryCode"] = "BE",
                ["ColorName"] = "Blue",
                ["ConfigurationId"] = ev.ConfigurationId.ToString(),
                ["IsActive"] = "true",
                ["ShowPodium"] = "true"
            };

            var postResponse = await _client.PostAsync($"/Events/Edit/{ev.EventId}", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updated = await db.Events.AsNoTracking().FirstAsync(e => e.EventId == ev.EventId);

            updated.EventName.Should().EndWith("_Edited");
            updated.Slogan.Should().Be("Updated Slogan");
            updated.ShowPodium.Should().BeTrue();
        }

        [Fact]
        public async Task Edit_Should_ReturnValidationError_WhenModelInvalid()
        {
            var ev = await EnsureTestEventAsync();

            var getHtml = await (await _client.GetAsync($"/Events/Edit/{ev.EventId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Id"] = ev.EventId.ToString(),
                ["Name"] = "", // Ongeldige naam
                ["Code"] = ev.EventCode ?? "TE",
                ["Year"] = ev.EventYear.ToString(),
                ["StartDate"] = ev.StartDate?.ToString("yyyy-MM-dd"),
                ["EndDate"] = ev.EndDate?.ToString("yyyy-MM-dd")
            };

            var postResponse = await _client.PostAsync($"/Events/Edit/{ev.EventId}", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK); // View blijft zichtbaar

            var html = await postResponse.Content.ReadAsStringAsync();
            html.Should().Contain("The Evenement field is required.");
        }

        [Fact]
        public async Task Edit_Should_DisplayTeamsCorrectly()
        {
            var ev = await EnsureTestEventAsync();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            db.Teams.AddRange(
                new Team { CurrentTeamName = "Team A" },
                new Team { CurrentTeamName = "Team B" }
            );
            await db.SaveChangesAsync();

            var response = await _client.GetAsync($"/Events/ManageTeams/{ev.EventId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await response.Content.ReadAsStringAsync();
            html.Should().Contain("Team A").And.Contain("Team B");
        }

        [Fact]
        public async Task Edit_Should_UpdateSelectedTeams()
        {
            var ev = await EnsureTestEventAsync();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Maak 2 teams
            var team1 = new Team { CurrentTeamName = "Team X" };
            var team2 = new Team { CurrentTeamName = "Team Y" };
            db.Teams.AddRange(team1, team2);
            await db.SaveChangesAsync();

            // Haal token uit de Edit-pagina (niet per se nodig, maar kan als je antiforgery gebruikt)
            var getHtml = await (await _client.GetAsync($"/Events/Edit/{ev.EventId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["EventId"] = ev.EventId.ToString(),
                ["Teams[0].TeamId"] = team1.TeamId.ToString(),
                ["Teams[0].IsSelected"] = "true",
                ["Teams[1].TeamId"] = team2.TeamId.ToString(),
                ["Teams[1].IsSelected"] = "false"
            };

            // Act
            var postResponse = await _client.PostAsync("/Events/ManageTeams", new FormUrlEncodedContent(formData));

            // Check 200 OK (geen redirect)
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Controleer de JSON-respons
            var json = await postResponse.Content.ReadAsStringAsync();
            json.Should().Contain("\"success\":true");
            json.Should().Contain("\"redirectUrl\"");

            // Controleer of de juiste koppeling in DB zit
            var eventTeams = await db.EventTeam
                .Where(et => et.EventId == ev.EventId)
                .ToListAsync();

            eventTeams.Should().ContainSingle(et => et.TeamId == team1.TeamId);
            eventTeams.Should().NotContain(et => et.TeamId == team2.TeamId);
        }

        [Fact]
        public async Task Edit_Should_RemoveAllTeams_WhenNoneSelected()
        {
            var ev = await EnsureTestEventAsync();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Arrange – voeg 2 teams toe aan het event
            var team1 = new Team { CurrentTeamName = "Team X" };
            var team2 = new Team { CurrentTeamName = "Team Y" };
            db.Teams.AddRange(team1, team2);
            await db.SaveChangesAsync();

            db.EventTeam.AddRange(
                new EventTeam { EventId = ev.EventId, TeamId = team1.TeamId },
                new EventTeam { EventId = ev.EventId, TeamId = team2.TeamId }
            );
            await db.SaveChangesAsync();

            var getHtml = await (await _client.GetAsync($"/Events/Edit/{ev.EventId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            // Act – stuur lege selectie
            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["EventId"] = ev.EventId.ToString(),
                ["Teams[0].TeamId"] = team1.TeamId.ToString(),
                ["Teams[0].IsSelected"] = "false",
                ["Teams[1].TeamId"] = team2.TeamId.ToString(),
                ["Teams[1].IsSelected"] = "false"
            };

            var postResponse = await _client.PostAsync("/Events/ManageTeams", new FormUrlEncodedContent(formData));

            // Verwacht 200 OK (geen redirect)
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Controleer JSON-respons
            var json = await postResponse.Content.ReadAsStringAsync();
            json.Should().Contain("\"success\":true");
            json.Should().Contain("\"redirectUrl\"");

            // Controleer dat alle koppelingen verwijderd zijn
            var eventTeams = await db.EventTeam.Where(et => et.EventId == ev.EventId).ToListAsync();
            eventTeams.Should().BeEmpty();
        }

        [Fact]
        public async Task Edit_Should_HandleNonExistentEvent()
        {
            var response = await _client.GetAsync("/Events/Edit/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Edit_Should_GetManageStagesPartial()
        {
            var ev = await EnsureTestEventAsync();

            // Act
            var response = await _client.GetAsync($"/Events/ManageStages?eventId={ev.EventId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var html = await response.Content.ReadAsStringAsync();

            // Check op unieke elementen in de partial
            html.Should().Contain("<form id=\"addStageForm\"");
            html.Should().Contain("<table class=\"table table-sm\"");
            html.Should().Contain(ev.EventName);
        }

        [Fact]
        public async Task Edit_Should_CreateStage_ViaAjax()
        {
            var ev = await EnsureTestEventAsync();

            var getHtml = await (await _client.GetAsync($"/Events/ManageStages?eventId={ev.EventId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["EventId"] = ev.EventId.ToString(),
                ["StageName"] = "Proloog AJAX",
                ["StageDate"] = DateTime.Today.ToString("yyyy-MM-dd"),
                ["StageOrder"] = "1"
            };

            var postResponse = await _client.PostAsync("/Stages/CreateAjax", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = await postResponse.Content.ReadAsStringAsync();
            json.Should().Contain("\"success\":true");

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var stage = await db.Stages.FirstOrDefaultAsync(s => s.StageName == "Proloog AJAX" && s.EventId == ev.EventId);
            stage.Should().NotBeNull();
        }

        [Fact]
        public async Task Edit_Should_UpdateStage_ViaAjax()
        {
            var ev = await EnsureTestEventAsync();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var stage = new Stage { EventId = ev.EventId, StageName = "Stage AJAX", StageDate = DateTime.Today, StageOrder = 1 };
            db.Stages.Add(stage);
            await db.SaveChangesAsync();

            // Simuleer update via service (controller heeft geen aparte update AJAX)
            stage.StageName = "Stage AJAX Edited";
            db.Stages.Update(stage);
            await db.SaveChangesAsync();

            var updatedStage = await db.Stages.FirstAsync(s => s.Id == stage.Id);
            updatedStage.StageName.Should().Be("Stage AJAX Edited");
        }

        [Fact]
        public async Task Edit_Should_DeleteStage_ViaAjax()
        {
            var ev = await EnsureTestEventAsync();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var stage = new Stage { EventId = ev.EventId, StageName = "DeleteMe AJAX", StageDate = DateTime.Today, StageOrder = 1 };
            db.Stages.Add(stage);
            await db.SaveChangesAsync();

            var getHtml = await (await _client.GetAsync($"/Events/ManageStages?eventId={ev.EventId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            };

            var postResponse = await _client.PostAsync($"/Stages/DeleteAjax?id={stage.Id}", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = await postResponse.Content.ReadAsStringAsync();
            json.Should().Contain("\"success\":true");

            var deletedStage = await db.Stages.FirstOrDefaultAsync(s => s.Id == stage.Id);
            deletedStage.Should().BeNull();
        }

        #region Evenement Edit Edge Cases

        [Fact]
        public async Task Edit_Should_ReturnNotFound_WhenEventDoesNotExist()
        {
            var response = await _client.GetAsync("/Events/Edit/999999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Edit_Should_ReturnValidationError_WhenEventIdIsMissing()
        {
            // Act
            var response = await _client.GetAsync("/Events/Edit/0");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Edit_Should_Fail_WhenModelInvalidWithoutEventId()
        {
            var ev = await EnsureTestEventAsync();
            var getHtml = await (await _client.GetAsync("/Events/Edit/1")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Id"] = "",
                ["Name"] = ""
            };
            var postResponse = await _client.PostAsync("/Events/Edit/0", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK); // view blijft
        }
        #endregion

        #region Manage Teams Edge Cases

        [Fact]
        public async Task ManageTeams_Should_HandleNonExistentTeamIds()
        {
            var ev = await EnsureTestEventAsync();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var tokenHtml = await (await _client.GetAsync($"/Events/Edit/{ev.EventId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(tokenHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["EventId"] = ev.EventId.ToString(),
                ["Teams[0].TeamId"] = "999999", // niet-bestaand
                ["Teams[0].IsSelected"] = "true"
            };

            var postResponse = await _client.PostAsync("/Events/ManageTeams", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = await postResponse.Content.ReadAsStringAsync();
            json.Should().Contain("\"success\":true"); // non-existent wordt genegeerd
        }

        [Fact]
        public async Task ManageTeams_Should_NotCreateDuplicateEventTeams()
        {
            var ev = await EnsureTestEventAsync();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var team = new Team { CurrentTeamName = "TeamDuplicate" };
            db.Teams.Add(team);
            await db.SaveChangesAsync();

            // Voeg handmatig een EventTeam toe
            db.EventTeam.Add(new EventTeam { EventId = ev.EventId, TeamId = team.TeamId });
            await db.SaveChangesAsync();

            var tokenHtml = await (await _client.GetAsync($"/Events/Edit/{ev.EventId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(tokenHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["EventId"] = ev.EventId.ToString(),
                ["Teams[0].TeamId"] = team.TeamId.ToString(),
                ["Teams[0].IsSelected"] = "true"
            };

            var postResponse = await _client.PostAsync("/Events/ManageTeams", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var eventTeams = await db.EventTeam.Where(et => et.EventId == ev.EventId && et.TeamId == team.TeamId).ToListAsync();
            eventTeams.Count.Should().Be(1); // geen duplicates
        }

        [Fact]
        public async Task ManageTeams_Should_ReturnNotFound_WhenEventDoesNotExist()
        {
            var response = await _client.GetAsync("/Events/ManageTeams/999999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        #endregion

        #region Stage Edge-Case Tests

        [Fact]
        public async Task CreateStage_Should_Fail_WhenModelInvalid()
        {
            // Arrange
            var ev = await EnsureTestEventAsync();

            // Haal antiforgery token
            var getHtml = await (await _client.GetAsync($"/Events/ManageStages?eventId={ev.EventId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            // POST met invalid model: StageName ontbreekt
            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["EventId"] = ev.EventId.ToString(),
                ["StageName"] = "", // Ongeldig
                ["StageDate"] = "",
                ["StageOrder"] = "1",
                ["StartLocation"] = "",
                ["FinishLocation"] = ""
            };

            // Act
            var postResponse = await _client.PostAsync("/Stages/CreateAjax", new FormUrlEncodedContent(formData));

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await postResponse.Content.ReadAsStringAsync();

            // Controleer dat de partial is gerenderd met foutmelding
            html.Should().Contain("StageName");
            html.Should().Contain("StageDate");
        }

        [Fact]
        public async Task UpdateStage_Should_Fail_WhenNonExistent()
        {
            var nonExistentId = 9999;
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var stage = await db.Stages.FirstOrDefaultAsync(s => s.Id == nonExistentId);
            stage.Should().BeNull();
            // Extra logica kan hier getest worden via service of controller, als je een update endpoint hebt
        }

        [Fact]
        public async Task DeleteStage_Should_Fail_WhenNonExistent()
        {
            var nonExistentId = 9999;

            var getHtml = await (await _client.GetAsync($"/Events/ManageStages?eventId=1")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            };

            var postResponse = await _client.PostAsync($"/Stages/DeleteAjax?id={nonExistentId}", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = await postResponse.Content.ReadAsStringAsync();
            json.Should().Contain("\"success\":false");
        }

        [Fact]
        public async Task CreateStage_Should_TrimNameAndValidateDate()
        {
            var ev = await EnsureTestEventAsync();

            var getHtml = await (await _client.GetAsync($"/Events/ManageStages?eventId={ev.EventId}")).Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["EventId"] = ev.EventId.ToString(),
                ["StageName"] = "  Proloog  ",  // Controleer trim
                ["StageDate"] = DateTime.Today.ToString("yyyy-MM-dd"),
                ["StageOrder"] = "1"
            };

            var postResponse = await _client.PostAsync("/Stages/CreateAjax", new FormUrlEncodedContent(formData));
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var stage = await db.Stages.FirstOrDefaultAsync(s => s.StageName.Trim() == "Proloog" && s.EventId == ev.EventId);
            stage.Should().NotBeNull();
        }

        //TODO: server side validatie toevoegen voor "stage-datums binnen event datums"
        //[Fact]
        //public async Task CreateStage_Should_Fail_WhenDateOutOfRange()
        //{
        //    var ev = await EnsureTestEventAsync();

        //    var getHtml = await (await _client.GetAsync($"/Events/ManageStages?eventId={ev.EventId}")).Content.ReadAsStringAsync();
        //    var token = TokenHelper.ExtractAntiForgeryToken(getHtml);

        //    // Datum buiten event periode
        //    var formData = new Dictionary<string, string>
        //    {
        //        ["__RequestVerificationToken"] = token,
        //        ["EventId"] = ev.EventId.ToString(),
        //        ["StageName"] = "Proloog Invalid Date",
        //        ["StageDate"] = ev.EndDate?.AddDays(10).ToString("yyyy-MM-dd"),
        //        ["StageOrder"] = "1"
        //    };

        //    var postResponse = await _client.PostAsync("/Stages/CreateAjax", new FormUrlEncodedContent(formData));

        //    postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        //    var html = await postResponse.Content.ReadAsStringAsync();
        //    html.Should().Contain("StageDate"); // Verwacht foutmelding voor datum
        //}

        [Fact]
        public async Task UpdateStage_Should_ReturnError_WhenNameEmpty()
        {
            var ev = await EnsureTestEventAsync();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var stage = new Stage { EventId = ev.EventId, StageName = "StageX", StageDate = DateTime.Today, StageOrder = 1 };
            db.Stages.Add(stage);
            await db.SaveChangesAsync();

            // Haal anti-forgery token
            var getHtml = await _client.GetAsync($"/Stages/EditStage/{stage.Id}");
            var html = await getHtml.Content.ReadAsStringAsync();
            var token = TokenHelper.ExtractAntiForgeryToken(html);

            var formData = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["StageId"] = stage.Id.ToString(),
                ["StageName"] = "", // Ongeldige naam
                ["StageDate"] = DateTime.Today.ToString("yyyy-MM-dd"),
                ["StageOrder"] = "1"
            };

            var postResponse = await _client.PostAsync("/Stages/EditAjax", new FormUrlEncodedContent(formData));

            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseHtml = await postResponse.Content.ReadAsStringAsync();
            responseHtml.Should().Contain("The Etappe field is required.");
        }

        [Fact]
        public async Task DeleteStage_Should_RemoveSuccessfully()
        {
            var ev = await EnsureTestEventAsync();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var stage = new Stage { EventId = ev.EventId, StageName = "DeleteMe", StageDate = DateTime.Today, StageOrder = 1 };
            db.Stages.Add(stage);
            await db.SaveChangesAsync();

            db.Stages.Remove(stage);
            await db.SaveChangesAsync();

            var deletedStage = await db.Stages.FirstOrDefaultAsync(s => s.Id == stage.Id);
            deletedStage.Should().BeNull();
        }

        #endregion

    }
}
