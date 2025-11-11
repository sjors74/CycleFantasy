using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Domain.Models;
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
        public async Task CreateAjax_ValidModel_ReturnsJsonSuccess()
        {
            // Arrange
            var vm = TestDataFactory.CreateStageCreateViewModel(1);
            var eventObj = TestDataFactory.CreateEvents(1).First();

            _mockEventService.Setup(s => s.GetEventById(vm.EventId))
                             .ReturnsAsync(eventObj);
            _mockStageService.Setup(s => s.AddStage(It.IsAny<Stage>()))
                             .Returns(Task.CompletedTask);

            _controller.Url = CreateMockUrlHelper($"/Events/Edit/{vm.EventId}");
            SetupHttpContext();

            // Act
            var result = await _controller.CreateAjax(vm);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var obj = JObject.FromObject(json.Value!);

            Assert.True(obj.Value<bool?>("success") ?? obj.Value<bool>("Success"));
            Assert.NotNull(obj.Value<string>("redirectUrl"));
        }

        [Fact]
        public async Task CreateAjax_InvalidModel_ReturnsJsonResult()
        {
            // Arrange
            var model = new StageCreateViewModel();
            _controller.ModelState.AddModelError("StageName", "Required");

            // Act
            var result = await _controller.CreateAjax(model);

            // Assert
            // Controleer eerst dat het resultaat een JsonResult is
            var jsonResult = Assert.IsType<JsonResult>(result);

            // Serialiseer en deserialiseer om Dictionary<string, JsonElement> te krijgen
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                JsonSerializer.Serialize(jsonResult.Value)
            );

            // Controleer dat success = false
            Assert.False(dict["success"].GetBoolean());

            // Controleer dat er een message aanwezig is
            var message = dict["message"].GetString();
            Assert.False(string.IsNullOrWhiteSpace(message));
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

            _mockStageService.Setup(s => s.GetStageById(stage.Id))
                             .ReturnsAsync(stage);
            _mockStageService.Setup(s => s.UpdateStage(It.IsAny<Stage>()))
                             .Returns(Task.CompletedTask);

            _controller.Url = CreateMockUrlHelper();
            SetupHttpContext();

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
            _controller.ModelState.AddModelError("StageName", "Required");

            // Act
            var result = await _controller.EditAjax(vm);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_EditStagePartial", partial.ViewName);
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
        public async Task DeleteAjax_Success_ReturnsJsonSuccess()
        {
            // Arrange
            _mockStageService.Setup(s => s.DeleteStage(1))
                             .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteAjax(1);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var obj = JObject.FromObject(json.Value!);

            Assert.True(obj.Value<bool?>("success") ?? obj.Value<bool>("Success"));
        }

        [Fact]
        public async Task DeleteAjax_Failure_ReturnsJsonFalse()
        {
            // Arrange
            _mockStageService.Setup(s => s.DeleteStage(1))
                             .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteAjax(1);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var obj = JObject.FromObject(json.Value!);

            Assert.False(obj.Value<bool?>("success") ?? obj.Value<bool>("Success"));
        }

        #endregion
    }
}
