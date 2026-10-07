using CycleManager.Services;
using Domain.Interfaces;
using Domain.Models;
using FluentAssertions;
using Moq;

namespace CycleManager.Tests.Unit.Services
{
    public class ConfigurationServiceTests
    {
        private readonly Mock<IConfigurationRepository> _configurationRepositoryMock;
        private readonly Mock<IConfigurationItemRepository> _configurationItemRepositoryMock;
        private readonly Mock<IConfigurationItemSpecialRepository> _configurationItemSpecialRepositoryMock;
        private readonly ConfigurationService _service;

        public ConfigurationServiceTests()
        {
            _configurationRepositoryMock = new Mock<IConfigurationRepository>();
            _configurationItemRepositoryMock = new Mock<IConfigurationItemRepository>();
            _configurationItemSpecialRepositoryMock = new Mock<IConfigurationItemSpecialRepository>();

            _service = new ConfigurationService(
                _configurationRepositoryMock.Object,
                _configurationItemRepositoryMock.Object,
                _configurationItemSpecialRepositoryMock.Object);
        }

        [Fact]
        public async Task Create_AddsAndSaves()
        {
            // Arrange
            var configuration = new Configuration();

            // Act
            await _service.Create(configuration);

            // Assert
            _configurationRepositoryMock.Verify(
                x => x.Add(configuration),
                Times.Once);

            _configurationRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task CreateItem_ReturnsRepositoryResult()
        {
            // Arrange
            var item = new ConfigurationItem();

            _configurationItemRepositoryMock
                .Setup(x => x.CreateItem(item))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CreateItem(item);

            // Assert
            result.Should().BeTrue();

            _configurationItemRepositoryMock.Verify(
                x => x.CreateItem(item),
                Times.Once);
        }

        [Fact]
        public async Task Delete_RemovesAndSaves()
        {
            // Arrange
            var configuration = new Configuration();

            // Act
            await _service.Delete(configuration);

            // Assert
            _configurationRepositoryMock.Verify(
                x => x.Remove(configuration),
                Times.Once);

            _configurationRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task DeleteItem_RemovesAndSaves()
        {
            // Arrange
            var item = new ConfigurationItem();

            // Act
            await _service.DeleteItem(item);

            // Assert
            _configurationItemRepositoryMock.Verify(
                x => x.Remove(item),
                Times.Once);

            _configurationItemRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task DeleteItemSpecial_RemovesAndSaves()
        {
            // Arrange
            var item = new ConfigurationItemSpecial();

            // Act
            await _service.DeleteItemSpecial(item);

            // Assert
            _configurationItemSpecialRepositoryMock.Verify(
                x => x.Remove(item),
                Times.Once);

            _configurationItemSpecialRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetAllConfigurationItems_ReturnsRepositoryResult()
        {
            // Arrange
            var expected = new List<ConfigurationItem>
            {
                new ConfigurationItem(),
                new ConfigurationItem()
            };

            _configurationItemRepositoryMock
                .Setup(x => x.GetAll())
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetAllConfigurationItems();

            // Assert
            result.Should().BeEquivalentTo(expected);

            _configurationItemRepositoryMock.Verify(
                x => x.GetAll(),
                Times.Once);
        }

        [Fact]
        public async Task GetAllConfigurationItemSpecials_ReturnsRepositoryResult()
        {
            // Arrange
            var expected = new List<ConfigurationItemSpecial>
            {
                new ConfigurationItemSpecial(),
                new ConfigurationItemSpecial()
            };

            _configurationItemSpecialRepositoryMock
                .Setup(x => x.GetAll())
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetAllConfigurationItemSpecials();

            // Assert
            result.Should().BeEquivalentTo(expected);

            _configurationItemSpecialRepositoryMock.Verify(
                x => x.GetAll(),
                Times.Once);
        }

        [Fact]
        public async Task GetAllConfigurations_ReturnsRepositoryResult()
        {
            // Arrange
            var expected = new List<Configuration>
            {
                new Configuration(),
                new Configuration()
            };

            _configurationRepositoryMock
                .Setup(x => x.GetAll())
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetAllConfigurations();

            // Assert
            result.Should().BeEquivalentTo(expected);

            _configurationRepositoryMock.Verify(
                x => x.GetAll(),
                Times.Once);
        }

        [Fact]
        public async Task GetConfigurationById_ReturnsRepositoryResult()
        {
            // Arrange
            const int configurationId = 5;
            var expected = new Configuration();

            _configurationRepositoryMock
                .Setup(x => x.GetConfigurationById(configurationId))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetConfigurationById(configurationId);

            // Assert
            result.Should().BeSameAs(expected);

            _configurationRepositoryMock.Verify(
                x => x.GetConfigurationById(configurationId),
                Times.Once);
        }

        [Fact]
        public async Task GetConfigurationItemSpecialById_ReturnsRepositoryResult()
        {
            // Arrange
            const int itemId = 5;
            var expected = new ConfigurationItemSpecial();

            _configurationItemSpecialRepositoryMock
                .Setup(x => x.GetById(itemId))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetConfigurationItemSpecialById(itemId);

            // Assert
            result.Should().BeSameAs(expected);

            _configurationItemSpecialRepositoryMock.Verify(
                x => x.GetById(itemId),
                Times.Once);
        }

        [Fact]
        public async Task Update_UpdatesAndSaves()
        {
            // Arrange
            var configuration = new Configuration();

            // Act
            await _service.Update(configuration);

            // Assert
            _configurationRepositoryMock.Verify(
                x => x.Update(configuration),
                Times.Once);

            _configurationRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task UpdateItem_ReturnsRepositoryResult()
        {
            // Arrange
            var item = new ConfigurationItem();

            _configurationItemRepositoryMock
                .Setup(x => x.UpdateItem(item))
                .ReturnsAsync(true);

            // Act
            var result = await _service.UpdateItem(item);

            // Assert
            result.Should().BeTrue();

            _configurationItemRepositoryMock.Verify(
                x => x.UpdateItem(item),
                Times.Once);
        }

        [Fact]
        public async Task UpdateItemSpecial_ReturnsRepositoryResult()
        {
            // Arrange
            var item = new ConfigurationItemSpecial();

            _configurationItemSpecialRepositoryMock
                .Setup(x => x.UpdateItemSpecial(item))
                .ReturnsAsync(true);

            // Act
            var result = await _service.UpdateItemSpecial(item);

            // Assert
            result.Should().BeTrue();

            _configurationItemSpecialRepositoryMock.Verify(
                x => x.UpdateItemSpecial(item),
                Times.Once);
        }
    }
}