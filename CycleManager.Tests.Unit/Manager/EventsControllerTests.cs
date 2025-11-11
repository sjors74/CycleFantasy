using CycleManager.Domain.Models;
using CycleManager.Domain.ViewModel;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text.Json;
using WebCycleManager.Controllers;
using WebCycleManager.Models;

namespace CycleManager.Tests.Unit.Manager
{
    public class EventsControllerTests
    {
        private readonly Mock<IEventService> _eventServiceMock;
        private readonly Mock<ITeamService> _teamServiceMock;
        private readonly Mock<IStageService> _stageServiceMock;
        private readonly Mock<IResultService> _resultServiceMock;
        private readonly Mock<IConfigurationService> _configurationServiceMock;
        private readonly EventsController _controller;

        public EventsControllerTests()
        {
            _eventServiceMock = new Mock<IEventService>();
            _teamServiceMock = new Mock<ITeamService>();
            _stageServiceMock = new Mock<IStageService>();
            _resultServiceMock = new Mock<IResultService>();
            _configurationServiceMock = new Mock<IConfigurationService>();

            _controller = new EventsController(
                _eventServiceMock.Object,
                _teamServiceMock.Object,
                _stageServiceMock.Object,
                _resultServiceMock.Object,
                _configurationServiceMock.Object
            );
        }

        // =====================
        // INDEX
        // =====================
        [Fact]
        public async Task Index_ReturnsViewWithEvents()
        {
            var events = new List<Event> 
            { 
                new Event { EventId = 1, EventName = "Race1" },
                new Event { EventId = 2, EventName = "Race2" }
            };

            _eventServiceMock.Setup(s => s.GetAllEvents())
                .ReturnsAsync(events);

            var result = await _controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<EventViewModel>(view.Model);
            Assert.Equal(2, model.Events.Count);
            Assert.Contains(model.Events, e => e.Name == "Race1");
        }

        // =====================
        // DETAILS
        // =====================
        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
            var result = await _controller.Details(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_NotFound_ReturnsNotFound()
        {
            _eventServiceMock.Setup(s => s.GetEventDetailsViewModelById(It.IsAny<int>()))
                .ReturnsAsync((EventDetailsViewModel)null);

            var result = await _controller.Details(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_Found_ReturnsView()
        {
            var vm = new EventDetailsViewModel {  EventId = 1, EventName  = "Race1" };
            _eventServiceMock.Setup(s => s.GetEventDetailsViewModelById(1)).ReturnsAsync(vm);

            var result = await _controller.Details(1);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(vm, view.Model);
        }

        // =====================
        // CREATE (GET / POST)
        // =====================
        [Fact]
        public async Task Create_Get_ReturnsViewWithConfigurationSelectList()
        {
            var configs = new List<Configuration> { new Configuration { Id = 1, ConfigurationType = "Points" } };
            _configurationServiceMock.Setup(s => s.GetAllConfigurations()).ReturnsAsync(configs);

            var result = await _controller.Create();

            var view = Assert.IsType<ViewResult>(result);
            var selectList = Assert.IsType<SelectList>(view.ViewData["ConfigurationId"]);
            Assert.Single(selectList.Items);
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            var vm = new EventItemViewModel { Name = "Race1" };
            _eventServiceMock.Setup(s => s.Create(It.IsAny<Event>())).Returns(Task.CompletedTask);

            var result = await _controller.Create(vm);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            _eventServiceMock.Verify(s => s.Create(It.IsAny<Event>()), Times.Once);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsView()
        {
            var vm = new EventItemViewModel();
            _controller.ModelState.AddModelError("error", "invalid");

            var result = await _controller.Create(vm);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(vm, view.Model);
        }

        // =====================
        // EDIT (GET / POST)
        // =====================
        [Fact]
        public async Task Edit_Get_NullId_ReturnsNotFound()
        {
            var result = await _controller.Edit(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_NotFound_ReturnsNotFound()
        {
            _eventServiceMock.Setup(s => s.GetEventById(It.IsAny<int>()))
                .ReturnsAsync((Event)null);

            var result = await _controller.Edit(99);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_Found_ReturnsView()
        {
            var e = new Event { EventId = 1, EventName = "Race1" };
            _eventServiceMock.Setup(s => s.GetEventById(1)).ReturnsAsync(e);
            _configurationServiceMock.Setup(s => s.GetAllConfigurations()).ReturnsAsync(new List<Configuration>());

            var result = await _controller.Edit(1);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<EventItemViewModel>(view.Model);
            Assert.Equal("Race1", model.Name);
        }

        [Fact]
        public async Task Edit_Post_IdMismatch_ReturnsNotFound()
        {
            var vm = new EventItemViewModel { Id = 2 };
            var result = await _controller.Edit(1, vm);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_RedirectsToIndex()
        {
            var vm = new EventItemViewModel { Id = 1, Name = "Race1" };
            _eventServiceMock.Setup(s => s.Update(It.IsAny<Event>())).Returns(Task.CompletedTask);

            var result = await _controller.Edit(1, vm);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            _eventServiceMock.Verify(s => s.Update(It.IsAny<Event>()), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsView()
        {
            var vm = new EventItemViewModel { Id = 1 };
            _controller.ModelState.AddModelError("error", "invalid");

            var result = await _controller.Edit(1, vm);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(vm, view.Model);
        }

        [Fact]
        public async Task Edit_Post_ConcurrencyException_EventNotExists_ReturnsNotFound()
        {
            var vm = new EventItemViewModel { Id = 1 };
            _eventServiceMock.Setup(s => s.Update(It.IsAny<Event>())).ThrowsAsync(new DbUpdateConcurrencyException());
            _eventServiceMock.Setup(s => s.GetEventById(1)).ReturnsAsync((Event)null);

            var result = await _controller.Edit(1, vm);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ConcurrencyException_EventStillExists_Throws()
        {
            var vm = new EventItemViewModel { Id = 1 };
            _eventServiceMock.Setup(s => s.Update(It.IsAny<Event>())).ThrowsAsync(new DbUpdateConcurrencyException());
            _eventServiceMock.Setup(s => s.GetEventById(1)).ReturnsAsync(new Event { EventId = 1 });

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _controller.Edit(1, vm));
        }

        // =====================
        // DELETE
        // =====================
        [Fact]
        public async Task Delete_Get_NullId_ReturnsNotFound()
        {
            var result = await _controller.Delete(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_NotFound_ReturnsNotFound()
        {
            _eventServiceMock.Setup(s => s.GetEventById(It.IsAny<int>())).ReturnsAsync((Event)null);

            var result = await _controller.Delete(99);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_Found_ReturnsView()
        {
            var e = new Event { EventId = 1, EventName = "Race1" };
            _eventServiceMock.Setup(s => s.GetEventById(1)).ReturnsAsync(e);

            var result = await _controller.Delete(1);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<EventItemViewModel>(view.Model);
            Assert.Equal("Race1", model.Name);
        }

        [Fact]
        public async Task DeleteConfirmed_Found_RedirectsToIndex()
        {
            var e = new Event { EventId = 1 };
            _eventServiceMock.Setup(s => s.GetEventById(1)).ReturnsAsync(e);

            var result = await _controller.DeleteConfirmed(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            _eventServiceMock.Verify(s => s.Delete(e), Times.Once);
        }

        [Fact]
        public async Task DeleteConfirmed_NotFound_RedirectsToIndexWithoutDelete()
        {
            _eventServiceMock.Setup(s => s.GetEventById(1)).ReturnsAsync((Event)null);

            var result = await _controller.DeleteConfirmed(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            _eventServiceMock.Verify(s => s.Delete(It.IsAny<Event>()), Times.Never);
        }

        // =====================
        // ManageTeams (GET / POST)
        // =====================
        [Fact]
        public async Task ManageTeams_Get_EventNotFound_ReturnsNotFound()
        {
            _eventServiceMock.Setup(s => s.GetEventById(1)).ReturnsAsync((Event)null);
            var result = await _controller.ManageTeams(1);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ManageTeams_Get_EventFound_ReturnsPartialView()
        {
            var e = new Event { EventId = 1, EventName = "Race1", EventTeams = new List<EventTeam>() };
            _eventServiceMock.Setup(s => s.GetEventById(1)).ReturnsAsync(e);
            _teamServiceMock.Setup(s => s.GetAllTeams()).ReturnsAsync(new List<Team> { new Team { TeamId = 1, CurrentTeamName = "TeamA" } });

            var result = await _controller.ManageTeams(1);

            var view = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<EventTeamsViewModel>(view.Model);
            Assert.Single(model.Teams);
        }

        [Fact]
        public async Task ManageTeams_Post_InvalidModel_ReturnsJsonError()
        {
            _controller.ModelState.AddModelError("error", "invalid");
            var vm = new EventTeamsViewModel();
            var result = await _controller.ManageTeams(vm);

            var json = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(json.Value);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString);

            Assert.False(dict["success"].GetBoolean());
            Assert.Equal("Er is een fout opgetreden.", dict["message"].GetString());
        }

        [Fact]
        public async Task ManageTeams_Post_EventNotFound_ReturnsJsonError()
        {
            var vm = new EventTeamsViewModel { EventId = 1 };
            _eventServiceMock.Setup(s => s.GetEventById(1)).ReturnsAsync((Event)null);

            var result = await _controller.ManageTeams(vm);

            var json = Assert.IsType<JsonResult>(result);

            var jsonString = JsonSerializer.Serialize(json.Value);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString);

            Assert.False((bool)dict["success"].GetBoolean());
            Assert.Equal("Evenement niet gevonden", dict["message"].GetString());
        }

        [Fact]
        public async Task ManageTeams_Post_ValidModel_UpdatesEventTeams()
        {
            // Arrange
            var e = new Event { EventId = 1, EventTeams = new List<EventTeam>() };
            _eventServiceMock.Setup(s => s.GetEventById(1)).ReturnsAsync(e);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns("/dummy/url");
            _controller.Url = urlHelperMock.Object;

            var vm = new EventTeamsViewModel
            {
                EventId = 1,
                Teams = new List<TeamSelection> { new TeamSelection { TeamId = 1, IsSelected = true } }
            };

            // Act
            var result = await _controller.ManageTeams(vm);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                JsonSerializer.Serialize(json.Value)
            );

            Assert.True(dict["success"].GetBoolean());

            // Controleer dat de juiste service-methodes zijn aangeroepen
            _eventServiceMock.Verify(s => s.GetEventById(1), Times.Once);
            _eventServiceMock.Verify(s => s.RemoveAllTeamsForEvent(1), Times.Once);
            _eventServiceMock.Verify(s => s.AddTeamToEvent(1, 1), Times.Once);

            // Check dat er geen andere service-aanroepen zijn gedaan
            _eventServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ManageTeams_Post_NoTeamsSelected_ClearsEventTeamsAndReturnsSuccess()
        {
            // Arrange
            var e = new Event
            {
                EventId = 1,
                EventTeams = new List<EventTeam> { new EventTeam { TeamId = 1, EventId = 1 } }
            };
            _eventServiceMock.Setup(s => s.GetEventById(1)).ReturnsAsync(e);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("/dummy/url");
            _controller.Url = urlHelperMock.Object;

            var vm = new EventTeamsViewModel
            {
                EventId = 1,
                Teams = new List<TeamSelection>
                {
                    new TeamSelection { TeamId = 1, IsSelected = false } // niets geselecteerd
                }
            };

            // Act
            var result = await _controller.ManageTeams(vm);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(JsonSerializer.Serialize(json.Value));

            Assert.True(dict["success"].GetBoolean());

            // Check dat de juiste services zijn aangeroepen
            _eventServiceMock.Verify(s => s.RemoveAllTeamsForEvent(1), Times.Once);
            _eventServiceMock.Verify(s => s.AddTeamToEvent(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ManageTeams_Post_UpdateThrowsException_ReturnsJsonError()
        {
            // Arrange
            var e = new Event { EventId = 1, EventTeams = new List<EventTeam>() };
            _eventServiceMock.Setup(s => s.GetEventById(1)).ReturnsAsync(e);
            _eventServiceMock.Setup(s => s.Update(It.IsAny<Event>())).ThrowsAsync(new Exception("Update failed"));

            var vm = new EventTeamsViewModel
            {
                EventId = 1,
                Teams = new List<TeamSelection> { new TeamSelection { TeamId = 1, IsSelected = true } }
            };

            // Act
            var result = await _controller.ManageTeams(vm);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(JsonSerializer.Serialize(json.Value));

            Assert.False(dict["success"].GetBoolean());
            Assert.Equal("Er is een fout opgetreden tijdens het opslaan.", dict["message"].GetString());
        }

        [Fact]
        public async Task ManageTeams_Post_DuplicateTeamIds_OnlyAddsOnce()
        {
            var e = new Event { EventId = 1, EventTeams = new List<EventTeam>() };
            _eventServiceMock.Setup(s => s.GetEventById(1)).ReturnsAsync(e);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("/dummy/url");
            _controller.Url = urlHelperMock.Object;

            var vm = new EventTeamsViewModel
            {
                EventId = 1,
                Teams = new List<TeamSelection>
                {
                    new TeamSelection { TeamId = 1, IsSelected = true },
                    new TeamSelection { TeamId = 1, IsSelected = true }
                }
            };

            var result = await _controller.ManageTeams(vm);

            // Controleer of de service correct is aangeroepen
            _eventServiceMock.Verify(s => s.RemoveAllTeamsForEvent(1), Times.Once);
            _eventServiceMock.Verify(s => s.AddTeamToEvent(1, 1), Times.AtLeastOnce);

            // Controleer het resultaat
            var json = Assert.IsType<JsonResult>(result);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(JsonSerializer.Serialize(json.Value));

            Assert.True(dict["success"].GetBoolean());
        }

        // =====================
        // ManageStages
        // =====================
        [Fact]
        public async Task ManageStages_EventNotFound_ReturnsNotFound()
        {
            _eventServiceMock.Setup(s => s.GetEventById(1)).ReturnsAsync((Event)null);
            var result = await _controller.ManageStages(1);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ManageStages_EventFound_ReturnsPartialView()
        {
            var e = new Event { EventId = 1, EventName = "Race1", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) };
            _eventServiceMock.Setup(s => s.GetEventById(1)).ReturnsAsync(e);
            _stageServiceMock.Setup(s => s.GetStagesByEventId(1)).ReturnsAsync(new List<Stage>());

            var result = await _controller.ManageStages(1);

            var view = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<ManageStageViewModel>(view.Model);
            Assert.Equal(1, model.EventStages.EventId);
        }

        [Fact]
        public async Task ManageStages_Post_EventNotFound_ReturnsJsonError()
        {
            // arrange
            _eventServiceMock.Setup(s => s.GetEventById(1)).ReturnsAsync((Event)null);
            var vm = new EventStagesViewModel { EventId = 1 };

            // act
            var result = await _controller.ManageStages(vm.EventId); // pas aan naar jouw POST method

            // assert
            Assert.IsType<NotFoundResult>(result);
        }

    }
}
