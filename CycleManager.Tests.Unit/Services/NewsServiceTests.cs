using CycleManager.Services;
using Domain.Interfaces;
using Domain.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace CycleManager.Tests.Unit.Services
{
    public class NewsServiceTests
    {
        private readonly Mock<INewsItemRepository> _repositoryMock;
        private readonly NewsService _service;

        public NewsServiceTests()
        {
            _repositoryMock = new Mock<INewsItemRepository>();

            _service = new NewsService(
                _repositoryMock.Object);
        }

        [Fact]
        public async Task CreateAsync_CallsRepository()
        {
            // Arrange
            var item = new NewsItem
            {
                Title = "Test News Item",
                Message = "This is a test news item.",
            };

            // Act
            await _service.CreateAsync(item);

            // Assert
            _repositoryMock.Verify(
                x => x.CreateAsync(item),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_CallsRepository()
        {
            // Arrange
            const int id = 5;

            // Act
            await _service.DeleteAsync(id);

            // Assert
            _repositoryMock.Verify(
                x => x.DeleteAsync(id),
                Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsRepositoryResult()
        {
            // Arrange
            const int id = 5;

            _repositoryMock
                .Setup(x => x.ExistsAsync(id))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ExistsAsync(id);

            // Assert
            result.Should().BeTrue();

            _repositoryMock.Verify(
                x => x.ExistsAsync(id),
                Times.Once);
        }

        [Fact]
        public async Task GetAllActiveNewsItems_ReturnsRepositoryResult()
        {
            // Arrange
            var expected = new List<NewsItem>
            {
                new NewsItem
                {
                    Title = "Test News Item 1",
                Message = "This is test news item 1.",
                },
                new NewsItem
                {
                    Title = "Test News Item 2",
                    Message = "This is test news item 2.",
                }
            };

            _repositoryMock
                .Setup(x => x.GetAllActiveNewsItems())
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetAllActiveNewsItems();

            // Assert
            result.Should().BeSameAs(expected);

            _repositoryMock.Verify(
                x => x.GetAllActiveNewsItems(),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsRepositoryResult()
        {
            // Arrange
            const int id = 5;
            var expected = new NewsItem
            {
                Title = "Test News Item",
                Message = "This is a test news item."
            };

            _repositoryMock
                .Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            result.Should().BeSameAs(expected);

            _repositoryMock.Verify(
                x => x.GetByIdAsync(id),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_CallsRepository()
        {
            // Arrange
            var item = new NewsItem
            {
                Title = "Test News Item",
                Message = "This is a test news item."
            };

            // Act
            await _service.UpdateAsync(item);

            // Assert
            _repositoryMock.Verify(
                x => x.UpdateAsync(item),
                Times.Once);
        }
    }
}