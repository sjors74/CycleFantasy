using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebCycleManager.Controllers;
using WebCycleManager.Models;
using CycleManager.Domain.Enums;

namespace CycleManager.Tests.Unit.Manager
{
    public class ConfigurationItemSpecialsControllerTests
    {
        private readonly Mock<IConfigurationService> _mockConfigurationService;
        private readonly ConfigurationItemSpecialsController _controller;

        public ConfigurationItemSpecialsControllerTests()
        {
            _mockConfigurationService = new Mock<IConfigurationService>();
            _controller = new ConfigurationItemSpecialsController(
                _mockConfigurationService.Object);
        }

        // -------------------------------------------
        // INDEX
        // -------------------------------------------
        [Fact]
        public async Task Index_ReturnsView_WithConfigurationItemSpecials()
        {
            var items = new List<ConfigurationItemSpecial>
            {
                new()
                {
                    Id = 1,
                    ConfigurationId = 10,
                    Question = QuestionType.GC,
                    Score = 5,
                    Color = "Red"
                },
                new()
                {
                    Id = 2,
                    ConfigurationId = 10,
                    Question = QuestionType.KOM,
                    Score = 10,
                    Color = "Blue"
                }
            };

            _mockConfigurationService
                .Setup(s => s.GetAllConfigurationItemSpecials())
                .ReturnsAsync(items);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ConfigurationItemSpecial>>(viewResult.Model);

            Assert.Equal(2, model.Count());
        }

        // -------------------------------------------
        // DETAILS
        // -------------------------------------------
        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            var result = await _controller.Details(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenItemNotFound()
        {
            _mockConfigurationService
                .Setup(s => s.GetConfigurationItemSpecialById(1))
                .ReturnsAsync((ConfigurationItemSpecial?)null);

            var result = await _controller.Details(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsView_WhenItemExists()
        {
            var item = new ConfigurationItemSpecial
            {
                Id = 1,
                ConfigurationId = 10,
                Question = QuestionType.GC,
                Score = 5,
                Color = "Red"
            };

            _mockConfigurationService
                .Setup(s => s.GetConfigurationItemSpecialById(1))
                .ReturnsAsync(item);

            var result = await _controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ConfigurationItemSpecial>(viewResult.Model);

            Assert.Equal(1, model.Id);
            Assert.Equal(QuestionType.GC, model.Question);
        }

        // -------------------------------------------
        // CREATE (GET)
        // -------------------------------------------
        [Fact]
        public async Task Create_Get_ReturnsView_WithConfigurationList()
        {
            var configurations = new List<Configuration>
            {
                new()
                {
                    Id = 1,
                    ConfigurationType = "GC"
                },
                new()
                {
                    Id = 2,
                    ConfigurationType = "KOM"
                }
            };

            _mockConfigurationService
                .Setup(s => s.GetAllConfigurations())
                .ReturnsAsync(configurations);

            var result = await _controller.Create(1);

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.NotNull(viewResult.ViewData["ConfigurationId"]);
            Assert.IsType<Microsoft.AspNetCore.Mvc.Rendering.SelectList>(
                viewResult.ViewData["ConfigurationId"]);
        }

        // -------------------------------------------
        // CREATE (POST)
        // -------------------------------------------
        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("Question", "Required");

            var vm = new ConfigurationItemsSpecialViewModel
            {
                ConfigurationId = 1,
                Score = 5,
                ColorName = "Red"
            };

            _mockConfigurationService
                .Setup(s => s.GetAllConfigurations())
                .ReturnsAsync(new List<Configuration>());

            var result = await _controller.Create(vm);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(vm, viewResult.Model);

            _mockConfigurationService.Verify(
                s => s.CreateItemSpecial(It.IsAny<ConfigurationItemSpecial>()),
                Times.Never);
        }

        [Fact]
        public async Task Create_Post_ReturnsView_WhenItemAlreadyExists()
        {
            var vm = new ConfigurationItemsSpecialViewModel
            {
                ConfigurationId = 1,
                Question = QuestionType.GC,
                Score = 5,
                ColorName = "Red"
            };

            _mockConfigurationService
                .Setup(s => s.CreateItemSpecial(It.IsAny<ConfigurationItemSpecial>()))
                .ReturnsAsync(false);

            _mockConfigurationService
                .Setup(s => s.GetAllConfigurations())
                .ReturnsAsync(new List<Configuration>());

            var result = await _controller.Create(vm);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(vm, viewResult.Model);

            Assert.Contains(
                _controller.ModelState["Question"]!.Errors,
                error => error.ErrorMessage ==
                          "Deze vraag bestaat al binnen deze configuratie.");

            _mockConfigurationService.Verify(
                s => s.CreateItemSpecial(It.IsAny<ConfigurationItemSpecial>()),
                Times.Once);
        }

        [Fact]
        public async Task Create_Post_ValidModel_CreatesAndRedirects()
        {
            var vm = new ConfigurationItemsSpecialViewModel
            {
                ConfigurationId = 1,
                Question = QuestionType.Points,
                Score = 10,
                ColorName = "Green"
            };

            _mockConfigurationService
                .Setup(s => s.CreateItemSpecial(It.IsAny<ConfigurationItemSpecial>()))
                .ReturnsAsync(true);

            var result = await _controller.Create(vm);

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Details", redirect.ActionName);
            Assert.Equal("Configurations", redirect.ControllerName);
            Assert.Equal(1, redirect.RouteValues!["id"]);

            _mockConfigurationService.Verify(
                s => s.CreateItemSpecial(It.Is<ConfigurationItemSpecial>(x =>
                    x.ConfigurationId == 1 &&
                    x.Question == QuestionType.Points &&
                    x.Score == 10 &&
                    x.Color == "Green")),
                Times.Once);
        }

        // -------------------------------------------
        // EDIT (GET)
        // -------------------------------------------
        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenIdIsNull()
        {
            var result = await _controller.Edit(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenItemNotFound()
        {
            _mockConfigurationService
                .Setup(s => s.GetConfigurationItemSpecialById(1))
                .ReturnsAsync((ConfigurationItemSpecial?)null);

            var result = await _controller.Edit(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsView_WithCorrectModel()
        {
            var item = new ConfigurationItemSpecial
            {
                Id = 1,
                ConfigurationId = 10,
                Question = QuestionType.KOM,
                Score = 5,
                Color = "Red"
            };

            _mockConfigurationService
                .Setup(s => s.GetConfigurationItemSpecialById(1))
                .ReturnsAsync(item);

            _mockConfigurationService
                .Setup(s => s.GetAllConfigurations())
                .ReturnsAsync(new List<Configuration>());

            var result = await _controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ConfigurationItemsSpecialViewModel>(viewResult.Model);

            Assert.Equal(1, model.Id);
            Assert.Equal(10, model.ConfigurationId);
            Assert.Equal(QuestionType.KOM, model.Question);
            Assert.Equal(5, model.Score);
            Assert.Equal("Red", model.ColorName);
        }

        // -------------------------------------------
        // EDIT (POST)
        // -------------------------------------------
        [Fact]
        public async Task Edit_Post_ReturnsNotFound_WhenIdsDoNotMatch()
        {
            var vm = new ConfigurationItemsSpecialViewModel
            {
                Id = 2,
                ConfigurationId = 1
            };

            var result = await _controller.Edit(1, vm);

            Assert.IsType<NotFoundResult>(result);

            _mockConfigurationService.Verify(
                s => s.UpdateItemSpecial(It.IsAny<ConfigurationItemSpecial>()),
                Times.Never);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("Question", "Required");

            var vm = new ConfigurationItemsSpecialViewModel
            {
                Id = 1,
                ConfigurationId = 1,
                Score = 5,
                ColorName = "Red"
            };

            _mockConfigurationService
                .Setup(s => s.GetAllConfigurations())
                .ReturnsAsync(new List<Configuration>());

            var result = await _controller.Edit(1, vm);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(vm, viewResult.Model);

            _mockConfigurationService.Verify(
                s => s.UpdateItemSpecial(It.IsAny<ConfigurationItemSpecial>()),
                Times.Never);
        }

        [Fact]
        public async Task Edit_Post_ReturnsNotFound_WhenItemNotFound()
        {
            var vm = new ConfigurationItemsSpecialViewModel
            {
                Id = 1,
                ConfigurationId = 1,
                Question = QuestionType.GC,
                Score = 5,
                ColorName = "Red"
            };

            _mockConfigurationService
                .Setup(s => s.GetConfigurationItemSpecialById(1))
                .ReturnsAsync((ConfigurationItemSpecial?)null);

            var result = await _controller.Edit(1, vm);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ReturnsView_WhenUpdateFails()
        {
            var vm = new ConfigurationItemsSpecialViewModel
            {
                Id = 1,
                ConfigurationId = 1,
                Question = QuestionType.GC,
                Score = 10,
                ColorName = "Blue"
            };

            var item = new ConfigurationItemSpecial
            {
                Id = 1,
                ConfigurationId = 1,
                Question = QuestionType.KOM,
                Score = 5,
                Color = "Red"
            };

            _mockConfigurationService
                .Setup(s => s.GetConfigurationItemSpecialById(1))
                .ReturnsAsync(item);

            _mockConfigurationService
                .Setup(s => s.UpdateItemSpecial(It.IsAny<ConfigurationItemSpecial>()))
                .ReturnsAsync(false);

            _mockConfigurationService
                .Setup(s => s.GetAllConfigurations())
                .ReturnsAsync(new List<Configuration>());

            var result = await _controller.Edit(1, vm);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(vm, viewResult.Model);

            Assert.Contains(
                _controller.ModelState["Question"]!.Errors,
                error => error.ErrorMessage ==
                          "Deze vraag bestaat al binnen deze configuratie.");
        }

        [Fact]
        public async Task Edit_Post_ValidModel_UpdatesAndRedirects()
        {
            var vm = new ConfigurationItemsSpecialViewModel
            {
                Id = 1,
                ConfigurationId = 2,
                Question = QuestionType.Points,
                Score = 15,
                ColorName = "Green"
            };

            var item = new ConfigurationItemSpecial
            {
                Id = 1,
                ConfigurationId = 1,
                Question = QuestionType.KOM,
                Score = 5,
                Color = "Red"
            };

            _mockConfigurationService
                .Setup(s => s.GetConfigurationItemSpecialById(1))
                .ReturnsAsync(item);

            _mockConfigurationService
                .Setup(s => s.UpdateItemSpecial(It.IsAny<ConfigurationItemSpecial>()))
                .ReturnsAsync(true);

            var result = await _controller.Edit(1, vm);

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Details", redirect.ActionName);
            Assert.Equal("Configurations", redirect.ControllerName);
            Assert.Equal(2, redirect.RouteValues!["id"]);

            _mockConfigurationService.Verify(
                s => s.UpdateItemSpecial(It.Is<ConfigurationItemSpecial>(x =>
                    x.Id == 1 &&
                    x.ConfigurationId == 2 &&
                    x.Question == QuestionType.Points &&
                    x.Score == 15 &&
                    x.Color == "Green")),
                Times.Once);
        }

        // -------------------------------------------
        // DELETE (GET)
        // -------------------------------------------
        [Fact]
        public async Task Delete_Get_ReturnsNotFound_WhenIdIsNull()
        {
            var result = await _controller.Delete(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_ReturnsNotFound_WhenItemNotFound()
        {
            _mockConfigurationService
                .Setup(s => s.GetConfigurationItemSpecialById(1))
                .ReturnsAsync((ConfigurationItemSpecial?)null);

            var result = await _controller.Delete(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_ReturnsView_WithCorrectModel()
        {
            var item = new ConfigurationItemSpecial
            {
                Id = 1,
                ConfigurationId = 10,
                Question = QuestionType.Points,
                Score = 5,
                Color = "Red"
            };

            _mockConfigurationService
                .Setup(s => s.GetConfigurationItemSpecialById(1))
                .ReturnsAsync(item);

            var result = await _controller.Delete(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ConfigurationItemsSpecialViewModel>(viewResult.Model);

            Assert.Equal(1, model.Id);
            Assert.Equal(10, model.ConfigurationId);
            Assert.Equal(QuestionType.Points, model.Question);
            Assert.Equal(5, model.Score);
            Assert.Equal("Red", model.ColorName);
        }

        // -------------------------------------------
        // DELETE CONFIRMED
        // -------------------------------------------
        [Fact]
        public async Task DeleteConfirmed_ReturnsNotFound_WhenItemNotFound()
        {
            _mockConfigurationService
                .Setup(s => s.GetConfigurationItemSpecialById(1))
                .ReturnsAsync((ConfigurationItemSpecial?)null);

            var result = await _controller.DeleteConfirmed(1);

            Assert.IsType<NotFoundResult>(result);

            _mockConfigurationService.Verify(
                s => s.DeleteItemSpecial(It.IsAny<ConfigurationItemSpecial>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteConfirmed_DeletesItem_AndRedirects()
        {
            var item = new ConfigurationItemSpecial
            {
                Id = 1,
                ConfigurationId = 10,
                Question = QuestionType.Points,
                Score = 5,
                Color = "Red"
            };

            _mockConfigurationService
                .Setup(s => s.GetConfigurationItemSpecialById(1))
                .ReturnsAsync(item);

            _mockConfigurationService
                .Setup(s => s.DeleteItemSpecial(item))
                .Returns(Task.CompletedTask);

            var result = await _controller.DeleteConfirmed(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Details", redirect.ActionName);
            Assert.Equal("Configurations", redirect.ControllerName);
            Assert.Equal(10, redirect.RouteValues!["id"]);

            _mockConfigurationService.Verify(
                s => s.DeleteItemSpecial(item),
                Times.Once);
        }
    }
}