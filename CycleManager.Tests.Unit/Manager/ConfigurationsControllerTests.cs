using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebCycleManager.Controllers;
using WebCycleManager.Models;

namespace CycleManager.Tests.Unit.Manager
{
    public class ConfigurationsControllerTests
    {
        private readonly Mock<IConfigurationService> _configurationServiceMock;
        private readonly ConfigurationsController _controller;

        public ConfigurationsControllerTests()
        {
            _configurationServiceMock = new Mock<IConfigurationService>();
            _controller = new ConfigurationsController(_configurationServiceMock.Object);
        }

        [Fact]
        public async Task Index_ReturnsView_WithConfigurations()
        {
            var configs = new List<Configuration>
            {
                new Configuration { Id = 1, ConfigurationType = "GC" }
            };

            _configurationServiceMock.Setup(s => s.GetAllConfigurations())
                .ReturnsAsync(configs);

            var result = await _controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ConfigurationViewModel>>(view.Model);
            Assert.Single(model);
            Assert.Equal("GC", model.First().ConfigurationName);
        }

        [Fact]
        public async Task Details_ValidId_ReturnsView()
        {
            var config = new Configuration
            {
                Id = 1,
                ConfigurationType = "Points",
                ConfigurationItems = new List<ConfigurationItem>
                {
                    new ConfigurationItem { Id = 10, Position = 1, Score = 100, ConfigurationId = 1 }
                }
            };

            _configurationServiceMock.Setup(s => s.GetConfigurationById(1))
                .ReturnsAsync(config);

            var result = await _controller.Details(1);

            var view = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsType<ConfigurationViewModel>(view.Model);
            Assert.Equal("Points", vm.ConfigurationName);
            Assert.Single(vm.ConfigurationItems);
        }

        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
            var result = await _controller.Details(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_NotFound_ReturnsNotFound()
        {
            _configurationServiceMock.Setup(s => s.GetConfigurationById(It.IsAny<int>()))
                .ReturnsAsync((Configuration)null);

            var result = await _controller.Details(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Create_Get_ReturnsView()
        {
            var result = _controller.Create();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            var vm = new ConfigurationViewModel { ConfigurationName = "GC" };

            _configurationServiceMock.Setup(s => s.Create(It.IsAny<Configuration>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.Create(vm);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ConfigurationsController.Index), redirect.ActionName);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsView()
        {
            var vm = new ConfigurationViewModel();
            _controller.ModelState.AddModelError("Error", "Invalid");

            var result = await _controller.Create(vm);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(vm, view.Model);
        }

        [Fact]
        public async Task Edit_Get_ValidId_ReturnsView()
        {
            var config = new Configuration
            {
                Id = 2,
                ConfigurationType = "Mountains",
                ConfigurationItems = new List<ConfigurationItem>()
            };

            _configurationServiceMock.Setup(s => s.GetConfigurationById(2))
                .ReturnsAsync(config);

            var result = await _controller.Edit(2);

            var view = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsType<ConfigurationViewModel>(view.Model);
            Assert.Equal("Mountains", vm.ConfigurationName);
        }

        [Fact]
        public async Task Edit_Get_NullId_ReturnsNotFound()
        {
            var result = await _controller.Edit(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_NotFound_ReturnsNotFound()
        {
            _configurationServiceMock.Setup(s => s.GetConfigurationById(It.IsAny<int>()))
                .ReturnsAsync((Configuration)null);

            var result = await _controller.Edit(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_RedirectsToIndex()
        {
            var config = new Configuration { Id = 3, ConfigurationType = "Old" };
            var vm = new ConfigurationViewModel { Id = 3, ConfigurationName = "New" };

            _configurationServiceMock.Setup(s => s.GetConfigurationById(3))
                .ReturnsAsync(config);

            var result = await _controller.Edit(3, vm);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ConfigurationsController.Index), redirect.ActionName);
            _configurationServiceMock.Verify(s => s.Update(It.Is<Configuration>(c => c.ConfigurationType == "New")), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_IdMismatch_ReturnsNotFound()
        {
            var vm = new ConfigurationViewModel { Id = 5 };
            var result = await _controller.Edit(10, vm);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsView()
        {
            var vm = new ConfigurationViewModel { Id = 1 };
            _controller.ModelState.AddModelError("Error", "Invalid");

            var result = await _controller.Edit(1, vm);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(vm, view.Model);
        }

        [Fact]
        public async Task Edit_Post_ConcurrencyException_ItemNotExists_ReturnsNotFound()
        {
            var vm = new ConfigurationViewModel { Id = 1, ConfigurationName = "GC" };

            _configurationServiceMock.Setup(s => s.GetConfigurationById(1))
                .ThrowsAsync(new DbUpdateConcurrencyException());
            _configurationServiceMock.Setup(s => s.GetConfigurationById(1))
                .ReturnsAsync((Configuration)null);

            var controller = new ConfigurationsController(_configurationServiceMock.Object);

            var result = await controller.Edit(1, vm);
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task Delete_Get_ValidId_ReturnsView()
        {
            var config = new Configuration { Id = 7, ConfigurationType = "Time Trial" };
            _configurationServiceMock.Setup(s => s.GetConfigurationById(7))
                .ReturnsAsync(config);

            var result = await _controller.Delete(7);

            var view = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsType<ConfigurationViewModel>(view.Model);
            Assert.Equal(7, vm.Id);
        }

        [Fact]
        public async Task Delete_Get_NullId_ReturnsNotFound()
        {
            var result = await _controller.Delete(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_NotFound_ReturnsNotFound()
        {
            _configurationServiceMock.Setup(s => s.GetConfigurationById(It.IsAny<int>()))
                .ReturnsAsync((Configuration)null);

            var result = await _controller.Delete(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_ValidId_DeletesAndRedirects()
        {
            var config = new Configuration { Id = 8, ConfigurationType = "Sprint" };
            _configurationServiceMock.Setup(s => s.GetConfigurationById(8))
                .ReturnsAsync(config);

            var result = await _controller.DeleteConfirmed(8);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ConfigurationsController.Index), redirect.ActionName);
            _configurationServiceMock.Verify(s => s.Delete(config), Times.Once);
        }

        [Fact]
        public async Task DeleteConfirmed_ItemNotFound_RedirectsToIndex()
        {
            _configurationServiceMock.Setup(s => s.GetConfigurationById(It.IsAny<int>()))
                .ReturnsAsync((Configuration)null);

            var result = await _controller.DeleteConfirmed(123);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ConfigurationsController.Index), redirect.ActionName);
        }

        [Fact]
        public async Task Edit_Post_ConcurrencyException_ConfigurationStillExists_Throws()
        {
            // Arrange
            var vm = new ConfigurationViewModel { Id = 1, ConfigurationName = "Test Config" };
            var configuration = new Configuration { Id = 1, ConfigurationType = "Old Config" };

            _configurationServiceMock
                .Setup(s => s.GetConfigurationById(1))
                .ReturnsAsync(configuration); // komt wél goed terug

            _configurationServiceMock
                .Setup(s => s.Update(It.IsAny<Configuration>()))
                .ThrowsAsync(new DbUpdateConcurrencyException()); // concurrency conflict hier

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _controller.Edit(1, vm));
        }
    }
}
