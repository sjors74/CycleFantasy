using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebCycleManager.Controllers;
using WebCycleManager.Models;

namespace CycleManager.Tests.Unit.Manager
{
    public class ConfigurationItemsControllerTests
    {
        private readonly Mock<IConfigurationService> _configurationServiceMock;
        private readonly ConfigurationItemsController _controller;

        public ConfigurationItemsControllerTests()
        {
            _configurationServiceMock = new Mock<IConfigurationService>();
            _controller = new ConfigurationItemsController(_configurationServiceMock.Object);
        }

        [Fact]
        public async Task Index_ReturnsView_WithConfigurationItems()
        {
            // Arrange
            var items = new List<ConfigurationItem>
            {
                new ConfigurationItem { Id = 1, Score = 10, Position = 1, ConfigurationId = 2 }
            };
            _configurationServiceMock.Setup(s => s.GetAllConfigurationItems())
                .ReturnsAsync(items);

            // Act
            var result = await _controller.Index();

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ConfigurationItem>>(view.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Details_ValidId_ReturnsView()
        {
            // Arrange
            var item = new ConfigurationItem { Id = 5, Position = 2, Score = 100 };
            _configurationServiceMock.Setup(s => s.GetConfigurationItemById(5))
                .ReturnsAsync(item);

            // Act
            var result = await _controller.Details(5);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ConfigurationItem>(view.Model);
            Assert.Equal(5, model.Id);
        }

        [Fact]
        public async Task Details_InvalidId_ReturnsNotFound()
        {
            _configurationServiceMock.Setup(s => s.GetConfigurationItemById(It.IsAny<int>()))
                .ReturnsAsync((ConfigurationItem)null);

            var result = await _controller.Details(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_Get_ReturnsView_WithSelectList()
        {
            // Arrange
            _configurationServiceMock.Setup(s => s.GetAllConfigurations())
                .ReturnsAsync(new List<Configuration> { new Configuration { Id = 1, ConfigurationType = "Points" } });

            // Act
            var result = await _controller.Create(1);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.True(view.ViewData.ContainsKey("ConfigurationId"));
            var selectList = Assert.IsType<SelectList>(view.ViewData["ConfigurationId"]);
            Assert.Single(selectList.Items);
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToDetails()
        {
            // Arrange
            var vm = new ConfigurationItemViewModel { Id = 0, ConfigurationId = 3, Position = 1, Score = 50 };

            _configurationServiceMock.Setup(s => s.CreateItem(It.IsAny<ConfigurationItem>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Create(vm);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
            Assert.Equal("Configurations", redirect.ControllerName);
            Assert.Equal(3, redirect.RouteValues["id"]);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewWithVm()
        {
            // Arrange
            var vm = new ConfigurationItemViewModel { ConfigurationId = 3 };
            _controller.ModelState.AddModelError("Error", "Invalid");

            _configurationServiceMock.Setup(s => s.GetAllConfigurations())
                .ReturnsAsync(new List<Configuration>());

            // Act
            var result = await _controller.Create(vm);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(vm, view.Model);
        }

        [Fact]
        public async Task Edit_Get_ValidId_ReturnsView()
        {
            // Arrange
            var item = new ConfigurationItem { Id = 1, Position = 2, Score = 33, ConfigurationId = 10 };
            _configurationServiceMock.Setup(s => s.GetConfigurationItemById(1))
                .ReturnsAsync(item);
            _configurationServiceMock.Setup(s => s.GetAllConfigurations())
                .ReturnsAsync(new List<Configuration> { new Configuration { Id = 10, ConfigurationType = "GC" } });

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ConfigurationItemViewModel>(view.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal(33, model.Score);
        }

        [Fact]
        public async Task Edit_Get_InvalidId_ReturnsNotFound()
        {
            _configurationServiceMock.Setup(s => s.GetConfigurationItemById(It.IsAny<int>()))
                .ReturnsAsync((ConfigurationItem)null);

            var result = await _controller.Edit(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_UpdatesAndRedirects()
        {
            // Arrange
            var item = new ConfigurationItem { Id = 1, ConfigurationId = 3, Position = 1, Score = 10 };
            var vm = new ConfigurationItemViewModel { Id = 1, ConfigurationId = 3, Position = 2, Score = 20 };

            _configurationServiceMock.Setup(s => s.GetConfigurationItemById(1))
                .ReturnsAsync(item);
            _configurationServiceMock.Setup(s => s.UpdateItem(It.IsAny<ConfigurationItem>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Edit(1, vm);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
            Assert.Equal("Configurations", redirect.ControllerName);
            _configurationServiceMock.Verify(s => s.UpdateItem(It.IsAny<ConfigurationItem>()), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_IdMismatch_ReturnsNotFound()
        {
            var vm = new ConfigurationItemViewModel { Id = 99 };
            var result = await _controller.Edit(5, vm);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsView()
        {
            var vm = new ConfigurationItemViewModel { Id = 1, ConfigurationId = 3 };
            _controller.ModelState.AddModelError("Error", "Invalid");

            _configurationServiceMock.Setup(s => s.GetAllConfigurations())
                .ReturnsAsync(new List<Configuration>());

            var result = await _controller.Edit(1, vm);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(vm, view.Model);
        }

        [Fact]
        public async Task Delete_Get_ValidId_ReturnsView()
        {
            var item = new ConfigurationItem { Id = 2, Position = 1, Score = 5, ConfigurationId = 3 };
            _configurationServiceMock.Setup(s => s.GetConfigurationItemById(2))
                .ReturnsAsync(item);

            var result = await _controller.Delete(2);

            var view = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsType<ConfigurationItemViewModel>(view.Model);
            Assert.Equal(2, vm.Id);
        }

        [Fact]
        public async Task Delete_Get_InvalidId_ReturnsNotFound()
        {
            _configurationServiceMock.Setup(s => s.GetConfigurationItemById(It.IsAny<int>()))
                .ReturnsAsync((ConfigurationItem)null);

            var result = await _controller.Delete(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_DeletesAndRedirects()
        {
            var item = new ConfigurationItem { Id = 5, ConfigurationId = 8 };
            _configurationServiceMock.Setup(s => s.GetConfigurationItemById(5))
                .ReturnsAsync(item);

            var result = await _controller.DeleteConfirmed(5);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
            Assert.Equal("Configurations", redirect.ControllerName);
            Assert.Equal(8, redirect.RouteValues["id"]);
            _configurationServiceMock.Verify(s => s.DeleteItem(item), Times.Once);
        }

        [Fact]
        public async Task DeleteConfirmed_ItemNotFound_ReturnsNotFound()
        {
            _configurationServiceMock.Setup(s => s.GetConfigurationItemById(It.IsAny<int>()))
                .ReturnsAsync((ConfigurationItem)null);

            var result = await _controller.DeleteConfirmed(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ItemDoesNotExist_ReturnsViewWithModelError()
        {
            // Arrange
            var vm = new ConfigurationItemViewModel
            {
                Id = 1,
                ConfigurationId = 3,
                Position = 1,
                Score = 10
            };

            _configurationServiceMock
                .Setup(s => s.GetConfigurationItemById(vm.Id))
                .ReturnsAsync((ConfigurationItem)null); // item bestaat niet

            // Act
            var result = await _controller.Edit(vm.Id, vm);

            // Assert
            var viewResult = Assert.IsType<NotFoundResult>(result);
        }



        [Fact]
        public async Task Edit_Post_ConcurrencyException_ItemExists_Rethrows()
        {
            var vm = new ConfigurationItemViewModel { Id = 1, ConfigurationId = 3, Position = 1, Score = 10 };

            _configurationServiceMock.Setup(s => s.GetConfigurationItemById(vm.Id))
                .ReturnsAsync(new ConfigurationItem { Id = 1, ConfigurationId = 3 });

            _configurationServiceMock.Setup(s => s.UpdateItem(It.IsAny<ConfigurationItem>()))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            var controller = new ConfigurationItemsController(_configurationServiceMock.Object);

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => controller.Edit(1, vm));
        }

        [Fact]
        public async Task Edit_Post_ConcurrencyException_ItemStillExists_ThrowsException()
        {
            var vm = new ConfigurationItemViewModel
            {
                Id = 1,
                ConfigurationId = 3,
                Position = 1,
                Score = 10
            };

            // Eerste call: item bestaat (voor update)
            // Tweede call: item bestaat nog steeds (voor ConfigurationItemExists)
            _configurationServiceMock.SetupSequence(s => s.GetConfigurationItemById(It.IsAny<int>()))
                .ReturnsAsync(new ConfigurationItem { Id = 1, ConfigurationId = 3 })
                .ReturnsAsync(new ConfigurationItem { Id = 1, ConfigurationId = 3 });

            // UpdateItem gooit de concurrency exception
            _configurationServiceMock.Setup(s => s.UpdateItem(It.IsAny<ConfigurationItem>()))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _controller.Edit(1, vm));
        }

    }
}
