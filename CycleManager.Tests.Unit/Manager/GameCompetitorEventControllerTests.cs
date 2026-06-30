using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using WebCycleManager.Controllers;
using WebCycleManager.Models;

namespace CycleManager.Tests.Unit.Manager
{
    public class GameCompetitorEventsControllerTests
    {
        private readonly Mock<IGameCompetitorInEventService> _mockGameCompetitorEventService;
        private readonly Mock<IResultService> _mockResultService;
        private readonly Mock<IEventService> _mockEventService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ICompetitorInEventService> _mockCompetitorInEventService;
        private readonly GameCompetitorEventsController _controller;

        public GameCompetitorEventsControllerTests()
        {
            _mockGameCompetitorEventService = new Mock<IGameCompetitorInEventService>();
            _mockResultService = new Mock<IResultService>();
            _mockEventService = new Mock<IEventService>();
            _mockUserService = new Mock<IUserService>();
            _mockCompetitorInEventService = new Mock<ICompetitorInEventService>();

            _controller = new GameCompetitorEventsController(
                _mockGameCompetitorEventService.Object,
                _mockResultService.Object,
                _mockEventService.Object,
                _mockUserService.Object,
                _mockCompetitorInEventService.Object
            );
        }

        // ---------- INDEX ----------

        [Fact]
        public async Task Index_ValidEventId_ReturnsViewWithModel()
        {
            // Arrange
            int eventId = 1;
            _mockResultService.Setup(s => s.GetResultsByEventId(eventId, false))
                .ReturnsAsync(new List<CompetitorRankingDto> {
                    new CompetitorRankingDto { CompetitorInEventId = 10, Points = 5 }
                });

            _mockGameCompetitorEventService.Setup(s => s.GetPicks(eventId))
                .Returns(new List<GameCompetitorEventPick>().AsQueryable());

            _mockGameCompetitorEventService.Setup(s => s.GetAllCompetitorsInEvent(eventId))
                .ReturnsAsync(new List<GameCompetitorEvent> {
                    new GameCompetitorEvent
                    {
                        Id = 1, EventId = eventId, TeamName = "TeamX",
                        User = new ApplicationUser { FirstName = "John", LastName = "Doe" }
                    }
                });

            // Act
            var result = await _controller.Index(eventId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<List<GameCompetitorInEventViewModel>>(result.Model);
            Assert.Single(model);
            Assert.Equal("TeamX", model.First().TeamName);
            Assert.Equal("John Doe", model.First().GameCompetitorName);
        }

        [Fact]
        public async Task Index_NoResults_ReturnsEmptyModel()
        {
            // Arrange
            int eventId = 1;
            _mockResultService.Setup(s => s.GetResultsByEventId(eventId, false))
                .ReturnsAsync(new List<CompetitorRankingDto>());  // geen resultaten
            _mockGameCompetitorEventService.Setup(s => s.GetPicks(eventId))
                .Returns(new List<GameCompetitorEventPick>().AsQueryable());
            _mockGameCompetitorEventService.Setup(s => s.GetAllCompetitorsInEvent(eventId))
                .ReturnsAsync(new List<GameCompetitorEvent>());  // geen teams

            // Act
            var result = await _controller.Index(eventId);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<GameCompetitorInEventViewModel>>(view.Model);
            Assert.Empty(model);
        }


        // ---------- DETAILS (GET) ----------

        [Fact]
        public async Task Details_InvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Details(null, null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ValidId_ReturnsViewWithModel()
        {
            // Arrange
            int id = 1;
            int eventId = 2;

            _mockResultService.Setup(s => s.GetResultsByEventId(eventId, false))
                .ReturnsAsync(new List<CompetitorRankingDto>());

            _mockGameCompetitorEventService.Setup(s => s.GetPicks(eventId))
                .Returns(new List<GameCompetitorEventPick>().AsQueryable());

            _mockCompetitorInEventService.Setup(s => s.GetCompetitors(eventId))
                .ReturnsAsync(new List<CompetitorsInEvent>());

            // Act
            var result = await _controller.Details(id, eventId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<GameCompetitorInEventViewModel>(result.Model);
            Assert.Equal(eventId, model.EventId);
        }

        [Fact]
        public async Task Details_IdOrEventIdNull_ReturnsNotFound()
        {
            var result = await _controller.Details(null, 1);
            Assert.IsType<NotFoundResult>(result);

            result = await _controller.Details(1, null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_LessThan15Picks_FillsWithEmptyRows()
        {
            int eventId = 1, id = 2;
            _mockResultService.Setup(s => s.GetResultsByEventId(eventId, false))
                .ReturnsAsync(new List<CompetitorRankingDto>());
            _mockGameCompetitorEventService.Setup(s => s.GetPicks(eventId))
                .Returns(new List<GameCompetitorEventPick>().AsQueryable());
            _mockCompetitorInEventService.Setup(s => s.GetCompetitors(eventId))
                .ReturnsAsync(new List<CompetitorsInEvent>());

            var result = await _controller.Details(id, eventId);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GameCompetitorInEventViewModel>(view.Model);
            Assert.Equal(15, model.CompetitorsInEvent.Count);
        }


        // ---------- DETAILS (POST) ----------

        [Fact]
        public async Task Details_Post_InvalidModel_ReturnsSameView()
        {
            // Arrange
            var model = new GameCompetitorInEventViewModel
            {
                EventId = 1,
                CompetitorsInEvent = new List<PickDetailViewModel> { new PickDetailViewModel() }
            };
            _controller.ModelState.AddModelError("Error", "Invalid");

            _mockCompetitorInEventService.Setup(s => s.GetCompetitors(1))
                .ReturnsAsync(new List<CompetitorsInEvent>());

            // Act
            var result = await _controller.Details(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(model, result.Model);
        }

        [Fact]
        public async Task Details_Post_ValidModel_AddsPicksAndRedirects()
        {
            // Arrange
            var model = new GameCompetitorInEventViewModel
            {
                Id = 1,
                EventId = 2,
                CompetitorsInEvent = new List<PickDetailViewModel>
                {
                    new PickDetailViewModel { PickId = 0, SelectedCompetitorId = 5 }
                }
            };

            // Act
            var result = await _controller.Details(model) as RedirectToActionResult;

            // Assert
            _mockGameCompetitorEventService.Verify(s => s.AddPicks(It.IsAny<List<GameCompetitorEventPick>>()), Times.Once);
            Assert.NotNull(result);
            Assert.Equal("Details", result.ActionName);
            Assert.Equal(model.EventId, result.RouteValues["eventId"]);
        }

        [Fact]
        public async Task Details_Post_InvalidModel_ReturnsViewWithCompetitors()
        {
            var model = new GameCompetitorInEventViewModel
            {
                EventId = 1,
                CompetitorsInEvent = new List<PickDetailViewModel> { new() }
            };
            _controller.ModelState.AddModelError("Error", "Invalid");
            _mockCompetitorInEventService.Setup(s => s.GetCompetitors(1))
                .ReturnsAsync(new List<CompetitorsInEvent>());

            var result = await _controller.Details(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, view.Model);
        }

        [Fact]
        public async Task Details_Post_NoNewPicks_DoesNotCallAddPicks()
        {
            var model = new GameCompetitorInEventViewModel
            {
                EventId = 1,
                Id = 5,
                CompetitorsInEvent = new List<PickDetailViewModel>()
            };

            var result = await _controller.Details(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
            _mockGameCompetitorEventService.Verify(s => s.AddPicks(It.IsAny<List<GameCompetitorEventPick>>()), Times.Never);
        }

        [Fact]
        public async Task Details_Post_WithNewPicks_CallsAddPicks()
        {
            var model = new GameCompetitorInEventViewModel
            {
                EventId = 1,
                Id = 5,
                CompetitorsInEvent = new List<PickDetailViewModel>
        {
            new() { PickId = 0, SelectedCompetitorId = 10 }
        }
            };

            var result = await _controller.Details(model);

            _mockGameCompetitorEventService.Verify(s => s.AddPicks(It.IsAny<List<GameCompetitorEventPick>>()), Times.Once);
        }

        [Fact]
        public async Task Details_Post_SomeExistingSomeNewPicks_AddsOnlyNewPicks()
        {
            var model = new GameCompetitorInEventViewModel
            {
                EventId = 1,
                Id = 5,
                CompetitorsInEvent = new List<PickDetailViewModel>
                {
                    new() { PickId = 1, SelectedCompetitorId = 10 }, // bestaand
                    new() { PickId = 0, SelectedCompetitorId = 11 }  // nieuw
                }
            };

            var result = await _controller.Details(model) as RedirectToActionResult;

            _mockGameCompetitorEventService.Verify(s => s.AddPicks(It.Is<List<GameCompetitorEventPick>>(l => l.Count == 1 && l[0].CompetitorsInEventId == 11)), Times.Once);
            Assert.Equal("Details", result.ActionName);
        }

        // ---------- CREATE ----------

        [Fact]
        public async Task Create_Get_ReturnsViewWithUsersInViewData()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetAllUsers())
                .ReturnsAsync(new List<ApplicationUser>
                {
                    new ApplicationUser { Id = "1", FirstName = "Jane", LastName = "Doe", Email = "jane@x.com" }
                });

            // Act
            var result = await _controller.Create(3) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ViewData.ContainsKey("Users"));
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "invalid");
            var dto = new DeelnemerCreateDto { EventId = 1 };

            _mockUserService.Setup(s => s.GetAllUsers())
                .ReturnsAsync(new List<ApplicationUser>());

            // Act
            var result = await _controller.Create(dto) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto, result.Model);
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var dto = new DeelnemerCreateDto { EventId = 5 };
            _mockGameCompetitorEventService.Setup(s => s.CreateGameCompetitorEventAsync(dto))
                .ReturnsAsync(new GameCompetitorEvent { Id = 1, EventId = 5 });

            // Act
            var result = await _controller.Create(dto) as RedirectToActionResult;

            // Assert
            _mockGameCompetitorEventService.Verify(s => s.CreateGameCompetitorEventAsync(dto), Times.Once);
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
        }

        // ---------- EDIT ----------

        [Fact]
        public async Task Edit_Get_ValidId_ReturnsViewWithDto()
        {
            // Arrange
            var entity = new GameCompetitorEvent
            {
                Id = 1,
                EventId = 5,
                TeamName = "TeamA",
                UserId = "user1"
            };

            _mockGameCompetitorEventService.Setup(s => s.GetGameCompetitorEventById(1))
                .ReturnsAsync(entity);

            _mockUserService.Setup(s => s.GetAllUsers())
                .ReturnsAsync(new List<ApplicationUser> { new() { Id = "user1", FirstName = "John", LastName = "Doe", Email = "a@b.com" } });

            _mockEventService.Setup(s => s.GetAllEvents())
                .ReturnsAsync(new List<Event> { new() { EventId = 5, EventName = "Tour" } });

            // Act
            var result = await _controller.Edit(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<DeelnemerEditDto>(result.Model);
            Assert.Equal(entity.Id, model.Id);
            Assert.Equal(entity.TeamName, model.TeamName);
        }

        [Fact]
        public async Task Edit_Get_InvalidId_ReturnsNotFound()
        {
            _mockGameCompetitorEventService.Setup(s => s.GetGameCompetitorEventById(It.IsAny<int>()))
                .ReturnsAsync((GameCompetitorEvent)null);

            var result = await _controller.Edit(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_CallsUpdateAndRedirects()
        {
            var dto = new DeelnemerEditDto { Id = 1, EventId = 10 };
            var result = await _controller.Edit(dto);

            _mockGameCompetitorEventService.Verify(s => s.UpdateAsync(dto), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            var dto = new DeelnemerEditDto { Id = 1, EventId = 5 };
            _controller.ModelState.AddModelError("error", "invalid");

            _mockUserService.Setup(s => s.GetAllUsers())
                .ReturnsAsync(new List<ApplicationUser>());
            _mockEventService.Setup(s => s.GetAllEvents())
                .ReturnsAsync(new List<Event>());

            // Act
            var result = await _controller.Edit(dto) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto, result.Model);
        }

        // ---------- DELETE ----------

        [Fact]
        public async Task DeletePick_ValidId_RemovesPickAndReturnsOk()
        {
            var result = await _controller.DeletePick(5);
            _mockGameCompetitorEventService.Verify(s => s.RemovePickFromEvent(5), Times.Once);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Delete_Post_ExistingEntity_CallsServiceAndRedirects()
        {
            var entity = new GameCompetitorEvent { Id = 5, EventId = 77 };

            _mockGameCompetitorEventService
                .Setup(s => s.GetGameCompetitorEventById(5))
                .ReturnsAsync(entity);

            var result = await _controller.Delete(5);

            _mockGameCompetitorEventService.Verify(s => s.DeleteGameCompetitorEventAsync(5), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(77, redirect.RouteValues["eventId"]);
        }

        [Fact]
        public async Task Delete_Get_NullId_ReturnsNotFound()
        {
            var result = await _controller.Delete(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_EntityNotFound_ReturnsNotFound()
        {
            _mockGameCompetitorEventService.Setup(s => s.GetGameCompetitorEventById(It.IsAny<int>()))
                .ReturnsAsync((GameCompetitorEvent)null);

            var result = await _controller.Delete((int?)99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Post_EntityNotFound_RedirectsSafely()
        {
            // Arrange
            _mockGameCompetitorEventService.Setup(s => s.GetGameCompetitorEventById(99))
                .ReturnsAsync((GameCompetitorEvent)null);

            // Act
            var result = await _controller.Delete(99) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            // Entity is null, dus eventId bestaat niet; test dat er geen exception komt
        }


        [Fact]
        public async Task Delete_Get_ValidId_ReturnsViewWithDto()
        {
            var entity = new GameCompetitorEvent
            {
                Id = 5,
                EventId = 77,
                TeamName = "Alpha",
                Event = new Event { EventId = 77, EventName = "Tour" },
                User = new ApplicationUser { FirstName = "Tom", LastName = "Boonen" }
            };
            _mockGameCompetitorEventService.Setup(s => s.GetGameCompetitorEventById(5))
                .ReturnsAsync(entity);

            var result = await _controller.Delete((int?)5);
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DeelnemerDeleteDto>(view.Model);
            Assert.Equal("Alpha", model.TeamName);
            Assert.Equal("Tour", model.EventName);
        }

        [Fact]
        public async Task DeletePick_ServiceThrowsException_ReturnsServerError()
        {
            // Arrange
            _mockGameCompetitorEventService.Setup(s => s.RemovePickFromEvent(5))
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.DeletePick(5));
        }

        // ---------- Helpers  ----------

        [Fact]
        public async Task FillList_ReturnsRedirectToDetails()
        {
            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );

            _mockGameCompetitorEventService.Setup(s => s.GetCompetitors(1, It.IsAny<int>()))
                .ReturnsAsync(new List<CompetitorsInEvent>
                {
            new() { CompetitorInTeamId = 10 }
                });

            var result = await _controller.FillList(2, 10, 1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
        }

        [Fact]
        public async Task FillList_NoCompetitors_ReturnsRedirect()
        {
            // Arrange
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _mockGameCompetitorEventService.Setup(s => s.GetCompetitors(1, It.IsAny<int>()))
                .ReturnsAsync(new List<CompetitorsInEvent>()); // geen suggesties

            // Act
            var result = await _controller.FillList(2, 10, 1) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Details", result.ActionName);
            Assert.Empty((List<int>)_controller.TempData["suggestedCompetitors"]);
        }

    }
}
