using CycleManager.Domain.Enums;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using WebCycleManager.Controllers;
using WebCycleManager.Models;

namespace CycleManager.Tests.Unit.Manager
{
    public class StagesControllerTests
    {
        private readonly StagesController _controller;
        private readonly Mock<IStageService> _mockStageService;
        private readonly Mock<IEventService> _mockEventService;

        public StagesControllerTests()
        {
            _mockStageService = new Mock<IStageService>();
            _mockEventService = new Mock<IEventService>();

            _controller = new StagesController(_mockStageService.Object, _mockEventService.Object);
        }

        #region Helpers

        private static IUrlHelper CreateMockUrlHelper(string returnUrl = "/Events/Edit/1")
        {
            var mockUrl = new Mock<IUrlHelper>();
            mockUrl.Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                   .Returns((UrlActionContext ctx) => returnUrl);
            return mockUrl.Object;
        }

        private void SetupHttpContext()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_Get_ReturnsView_WithEvents()
        {
            // Arrange
            var events = TestDataFactory.CreateEvents(3);
            _mockEventService.Setup(s => s.GetAllEvents()).ReturnsAsync(events);

            // Act
            var result = await _controller.Create();

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<StageCreateViewModel>(view.Model);
            Assert.Equal(3, model.Events.Count());
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var vm = TestDataFactory.CreateStageCreateViewModel();
            _mockStageService.Setup(s => s.AddStage(It.IsAny<Stage>()))
                             .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(vm);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewWithCountries()
        {
            // Arrange
            var vm = TestDataFactory.CreateStageCreateViewModel();
            _controller.ModelState.AddModelError("StageName", "Required");

            _mockEventService.Setup(s => s.GetAllEvents())
                             .ReturnsAsync(TestDataFactory.CreateEvents(2));

            // Act
            var result = await _controller.Create(vm);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<StageCreateViewModel>(view.Model);
            Assert.NotEmpty(model.Events);
        }

        [Fact]
        public async Task CreateAjax_ValidModel_AddsStageAndReturnsPartialView()
        {
            // Arrange
            var vm = new ManageStageViewModel
            {
                NewStage = new StageCreateViewModel
                {
                    EventId = 10,
                    StageName = "Stage 1",
                    StageDate = new DateTime(2026, 7, 5),
                    StageOrder = 1,
                    StartLocation = "Arnhem",
                    FinishLocation = "Nijmegen"
                }
            };

            var eventEntity = TestDataFactory.CreateEvent(10);
            eventEntity.EventName = "Test Event";

            _mockEventService
                .Setup(s => s.GetEventById(10))
                .ReturnsAsync(eventEntity);

            _mockStageService
                .Setup(s => s.GetStagesByEventId(10))
                .ReturnsAsync(new List<Stage>());

            _mockStageService
                .Setup(s => s.AddStage(It.IsAny<Stage>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateAjax(vm);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);

            Assert.Equal(
                "~/Views/Events/_ManageStagesPartial.cshtml",
                partial.ViewName);

            _mockStageService.Verify(
                s => s.AddStage(It.Is<Stage>(x =>
                    x.EventId == 10 &&
                    x.StageName == "Stage 1" &&
                    x.StageOrder == 1 &&
                    x.StartLocation == "Arnhem" &&
                    x.FinishLocation == "Nijmegen")),
                Times.Once);
        }

        [Fact]
        public async Task CreateAjax_EmptyStage_ReturnsPartialViewWithError()
        {
            // Arrange
            var vm = new ManageStageViewModel
            {
                NewStage = new StageCreateViewModel
                {
                    EventId = 10
                }
            };

            var eventEntity = TestDataFactory.CreateEvent(10);

            _mockEventService
                .Setup(s => s.GetEventById(10))
                .ReturnsAsync(eventEntity);

            _mockStageService
                .Setup(s => s.GetStagesByEventId(10))
                .ReturnsAsync(new List<Stage>());

            // Act
            var result = await _controller.CreateAjax(vm);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);

            Assert.Equal(
                "~/Views/Events/_ManageStagesPartial.cshtml",
                partial.ViewName);

            var model = Assert.IsType<ManageStageViewModel>(partial.Model);

            Assert.Equal("Geen stage ingevoerd.", model.UiErrorMessage);

            _mockStageService.Verify(
                s => s.AddStage(It.IsAny<Stage>()),
                Times.Never);
        }

        #endregion

        #region Edit

        [Fact]
        public async Task EditStage_Get_ReturnsPartialViewWithModel()
        {
            // Arrange
            var stage = TestDataFactory.CreateStage(1);
            _mockStageService.Setup(s => s.GetStageById(1))
                             .ReturnsAsync(stage);

            // Act
            var result = await _controller.EditStage(1);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_EditStagePartial", partial.ViewName);
            Assert.IsType<StageViewModel>(partial.Model);
        }

        [Fact]
        public async Task EditAjax_ValidModel_ReturnsJsonSuccess()
        {
            // Arrange
            var stage = TestDataFactory.CreateStage(1);
            var vm = new StageViewModel
            {
                StageId = stage.Id,
                EventId = stage.EventId,
                StageName = stage.StageName,
                StageOrder = stage.StageOrder,
                StageDate = DateOnly.FromDateTime(stage.StageDate),
                StartLocation = stage.StartLocation,
                FinishLocation = stage.FinishLocation
            };

            _mockStageService
                .Setup(s => s.GetStageById(stage.Id))
                .ReturnsAsync(stage);

            _mockStageService
                .Setup(s => s.UpdateStage(It.IsAny<Stage>()))
                .Returns(Task.CompletedTask);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            
            _controller.Request.Headers["X-Requested-With"] = "XMLHttpRequest";

            // Act
            var result = await _controller.EditAjax(vm);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var obj = JObject.FromObject(json.Value!);

            Assert.True(obj.Value<bool?>("success") ?? obj.Value<bool>("Success"));
            Assert.Equal(vm.StageName, obj["stage"]?["name"]?.ToString());
        }

        [Fact]
        public async Task EditAjax_InvalidModel_ReturnsPartialView()
        {
            // Arrange
            var vm = new StageViewModel
            {
                StageId = 1,
                StageName = ""
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _controller.Request.Headers["X-Requested-With"] = "XMLHttpRequest";

            _controller.ModelState.AddModelError(
                "StageName",
                "Required");

            Assert.False(_controller.ModelState.IsValid);

            // Act
            var result = await _controller.EditAjax(vm);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);

            Assert.Equal("_EditStagePartial", partial.ViewName);
            Assert.Same(vm, partial.Model);

            _mockStageService.Verify(
                s => s.GetStageById(It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task EditStage_Get_ReturnsNotFound_WhenStageDoesNotExist()
        {
            // Arrange
            _mockStageService
                .Setup(s => s.GetStageById(999))
                .ReturnsAsync((Stage?)null);

            // Act
            var result = await _controller.EditStage(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditAjax_ValidModel_ReturnsNotFound_WhenStageDoesNotExist()
        {
            // Arrange
            var vm = new StageViewModel
            {
                StageId = 999,
                StageName = "Test"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _mockStageService
                .Setup(s => s.GetStageById(999))
                .ReturnsAsync((Stage?)null);

            // Act
            var result = await _controller.EditAjax(vm);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditAjax_ValidModel_NonAjax_ReturnsManageStagesPartial()
        {
            // Arrange
            var vm = new StageViewModel
            {
                StageId = 1,
                StageName = "Nieuwe stage",
                StageOrder = 1,
                StageDate = new DateOnly(2026, 7, 1),
                StartLocation = "Start",
                FinishLocation = "Finish",
                NoScore = false
            };

            var stage = new Stage
            {
                Id = 1,
                EventId = 2,
                StageName = "Oude stage",
                StageOrder = 1,
                StageDate = new DateTime(2026, 7, 1),
                StartLocation = "Oude start",
                FinishLocation = "Oude finish"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _mockStageService
                .Setup(s => s.GetStageById(1))
                .ReturnsAsync(stage);

            _mockEventService
                .Setup(s => s.GetEventById(2))
                .ReturnsAsync(new Event
                {
                    EventId = 2
                });

            // Act
            var result = await _controller.EditAjax(vm);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);

            Assert.Equal(
                "~/Views/Events/_ManageStagesPartial.cshtml",
                partialView.ViewName);

            Assert.NotNull(partialView.Model);

            _mockStageService.Verify(
                s => s.UpdateStage(It.IsAny<Stage>()),
                Times.Once);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_Get_ReturnsViewWithModel()
        {
            // Arrange
            var stage = TestDataFactory.CreateStage(1);
            _mockStageService.Setup(s => s.GetStageById(stage.Id))
                             .ReturnsAsync(stage);

            // Act
            var result = await _controller.Delete(stage.Id);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<StageDeleteViewModel>(view.Model);
            Assert.Equal(stage.StageName, model.StageName);
        }

        [Fact]
        public async Task DeleteConfirmed_DeletesAndRedirects()
        {
            // Arrange
            var stage = TestDataFactory.CreateStage(1);
            _mockStageService.Setup(s => s.DeleteStage(stage.Id))
                             .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteConfirmed(stage.Id, stage.EventId);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
            Assert.Equal("Events", redirect.ControllerName);
        }

        [Fact]
        public async Task DeleteAjax_ValidStage_DeletesStageAndReturnsPartialView()
        {
            // Arrange
            var stage = new Stage
            {
                Id = 1,
                EventId = 10
            };

            _mockStageService
                .Setup(s => s.GetStageById(1))
                .ReturnsAsync(stage);

            _mockStageService
                .Setup(s => s.DeleteStage(1))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteAjax(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            jsonResult.Value.Should().NotBeNull();

            _mockStageService.Verify(
                s => s.DeleteStage(1),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAjax_StageNotFound_ReturnsJsonFailure()
        {
            // Arrange
            _mockStageService
                .Setup(s => s.GetStageById(1))
                .ReturnsAsync((Stage?)null);

            // Act
            var result = await _controller.DeleteAjax(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            jsonResult.Value.Should().NotBeNull();

            _mockStageService.Verify(
                s => s.DeleteStage(It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task Delete_NullId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Delete(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);

            _mockStageService.Verify(
                s => s.GetStageById(It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task Delete_StageDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _mockStageService
                .Setup(s => s.GetStageById(999))
                .ReturnsAsync((Stage?)null);

            // Act
            var result = await _controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        #endregion

        #region CreateFromViewModel
        [Fact]
        public async Task CreateFromViewModel_ExistingStage_UpdatesStage()
        {
            // Arrange
            var stage = new Stage
            {
                Id = 10,
                StageName = "Oude naam"
            };

            var vm = new StageViewModel
            {
                StageId = 10,
                StageName = "Nieuwe naam",
                StageOrder = 3,
                StageDate = new DateOnly(2026, 7, 15),
                StartLocation = "Arnhem",
                FinishLocation = "Alpe d'Huez",
                NoScore = true,
                NoScoreDescription = "Geen score",
                EventId = 2,
                ScrapeStatus = ScrapeStatus.Completed
            };

            _mockStageService
                .Setup(s => s.GetStageById(10))
                .ReturnsAsync(stage);

            // Act
            var result = await _controller.CreateFromViewModel(vm);

            // Assert
            Assert.Same(stage, result);

            Assert.Equal(10, result.Id);
            Assert.Equal(new DateTime(2026, 7, 15), result.StageDate);
            Assert.Equal("Nieuwe naam", result.StageName);
            Assert.Equal(3, result.StageOrder);
            Assert.Equal("Arnhem", result.StartLocation);
            Assert.Equal("Alpe d'Huez", result.FinishLocation);
            Assert.True(result.NoScore);
            Assert.Equal("Geen score", result.NoScoreDescription);
            Assert.Equal(2, result.EventId);
            Assert.Equal(ScrapeStatus.Completed, result.ScrapeStatus);
        }

        [Fact]
        public async Task CreateFromViewModel_NonExistingStage_CreatesNewStage()
        {
            // Arrange
            var vm = new StageViewModel
            {
                StageId = 999,
                StageName = "Nieuwe stage",
                StageOrder = 2,
                StageDate = new DateOnly(2026, 8, 10),
                StartLocation = "Arnhem",
                FinishLocation = "Luik",
                NoScore = false,
                NoScoreDescription = null,
                EventId = 2,
                ScrapeStatus = ScrapeStatus.Pending
            };

            _mockStageService
                .Setup(s => s.GetStageById(999))
                .ReturnsAsync((Stage?)null);

            // Act
            var result = await _controller.CreateFromViewModel(vm);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(new DateTime(2026, 8, 10), result.StageDate);
            Assert.Equal("Nieuwe stage", result.StageName);
            Assert.Equal(2, result.StageOrder);
            Assert.Equal("Arnhem", result.StartLocation);
            Assert.Equal("Luik", result.FinishLocation);
            Assert.False(result.NoScore);
            Assert.Null(result.NoScoreDescription);
            Assert.Equal(2, result.EventId);
            Assert.Equal(ScrapeStatus.Pending, result.ScrapeStatus);
        }
        #endregion

        #region CreateViewModel
        [Fact]
        public void CreateViewModel_StageWithoutEvent_ReturnsEmptyEventValues()
        {
            // Arrange
            var stage = new Stage
            {
                Id = 10,
                StageDate = new DateTime(2026, 7, 15),
                StageName = "Bergstage",
                StageOrder = 3,
                StartLocation = "Arnhem",
                FinishLocation = "Alpe d'Huez",
                NoScore = true,
                NoScoreDescription = "Geen score",
                EventId = 2,
                Event = null!,
                ScrapeStatus = ScrapeStatus.Completed
            };

            // Act
            var result = _controller.CreateViewModel(stage);

            // Assert
            Assert.Equal(10, result.StageId);
            Assert.Equal(new DateOnly(2026, 7, 15), result.StageDate);
            Assert.Equal("Bergstage", result.StageName);
            Assert.Equal(3, result.StageOrder);
            Assert.Equal("Arnhem", result.StartLocation);
            Assert.Equal("Alpe d'Huez", result.FinishLocation);
            Assert.True(result.NoScore);
            Assert.Equal("Geen score", result.NoScoreDescription);
            Assert.Equal(2, result.EventId);

            Assert.Equal(string.Empty, result.EventName);
            Assert.Equal(int.MinValue, result.EventYear);

            Assert.Equal(ScrapeStatus.Completed, result.ScrapeStatus);
            Assert.NotNull(result.AvailableStatuses);
        }

        [Fact]
        public void CreateViewModel_StageWithEvent_ReturnsEventValues()
        {
            // Arrange
            var stage = new Stage
            {
                Id = 10,
                StageDate = new DateTime(2026, 7, 15),
                StageName = "Bergstage",
                StageOrder = 3,
                StartLocation = "Arnhem",
                FinishLocation = "Alpe d'Huez",
                NoScore = false,
                EventId = 2,
                Event = new Event
                {
                    EventId = 2,
                    EventName = "Tour de France",
                    EventYear = 2026
                },
                ScrapeStatus = ScrapeStatus.Pending
            };

            // Act
            var result = _controller.CreateViewModel(stage);

            // Assert
            Assert.Equal(10, result.StageId);
            Assert.Equal("Bergstage", result.StageName);
            Assert.Equal("Tour de France", result.EventName);
            Assert.Equal(2026, result.EventYear);
            Assert.Equal(ScrapeStatus.Pending, result.ScrapeStatus);
            Assert.NotNull(result.AvailableStatuses);
        }
        #endregion
    }
}
