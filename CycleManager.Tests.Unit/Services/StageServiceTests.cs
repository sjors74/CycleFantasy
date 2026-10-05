using CycleManager.Services;
using Domain.Interfaces;
using Domain.Models;
using FluentAssertions;
using Moq;

namespace CycleManager.Tests.Unit.Services
{
    public class StageServiceTests
    {
        private readonly Mock<IStageRepository> _stageRepositoryMock;
        private readonly StageService _service;

        public StageServiceTests()
        {
            _stageRepositoryMock = new Mock<IStageRepository>();
            _service = new StageService(_stageRepositoryMock.Object);
        }

        [Fact]
        public async Task GetStagesByEventId_ReturnsRepositoryResult()
        {
            // Arrange
            const int eventId = 2;

            var expected = new List<Stage>
            {
                new Stage()
            };

            _stageRepositoryMock
                .Setup(x => x.GetByEventId(eventId))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetStagesByEventId(eventId);

            // Assert
            result.Should().BeEquivalentTo(expected);

            _stageRepositoryMock.Verify(
                x => x.GetByEventId(eventId),
                Times.Once);
        }

        [Fact]
        public async Task GetStageNumberForDateAsync_ReturnsRepositoryResult()
        {
            // Arrange
            var date = new DateTime(2026, 7, 10);
            const int eventId = 2;

            _stageRepositoryMock
                .Setup(x => x.GetStageNumber(date, eventId))
                .ReturnsAsync(5);

            // Act
            var result = await _service.GetStageNumberForDateAsync(date, eventId);

            // Assert
            result.Should().Be(5);

            _stageRepositoryMock.Verify(
                x => x.GetStageNumber(date, eventId),
                Times.Once);
        }

        [Fact]
        public async Task GetStageIdFromStageNumber_ReturnsRepositoryResult()
        {
            // Arrange
            const int stageNumber = 5;
            const int eventId = 2;

            _stageRepositoryMock
                .Setup(x => x.GetStageId(stageNumber, eventId))
                .ReturnsAsync(42);

            // Act
            var result = await _service.GetStageIdFromStageNumber(
                stageNumber,
                eventId);

            // Assert
            result.Should().Be(42);

            _stageRepositoryMock.Verify(
                x => x.GetStageId(stageNumber, eventId),
                Times.Once);
        }

        [Fact]
        public async Task GetStageResults_ReturnsRepositoryResult()
        {
            // Arrange
            const int stageNumber = 5;
            const int eventId = 2;

            _stageRepositoryMock
                .Setup(x => x.GetStagesResults(stageNumber, eventId))
                .ReturnsAsync(123);

            // Act
            var result = await _service.GetStageResults(
                stageNumber,
                eventId);

            // Assert
            result.Should().Be(123);

            _stageRepositoryMock.Verify(
                x => x.GetStagesResults(stageNumber, eventId),
                Times.Once);
        }

        [Fact]
        public async Task AddStage_AddsAndSaves()
        {
            // Arrange
            var stage = new Stage();

            // Act
            await _service.AddStage(stage);

            // Assert
            _stageRepositoryMock.Verify(
                x => x.Add(stage),
                Times.Once);

            _stageRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task DeleteStage_ReturnsFalse_WhenStageDoesNotExist()
        {
            // Arrange
            _stageRepositoryMock
                .Setup(x => x.GetById(999))
                .ReturnsAsync((Stage?)null);

            // Act
            var result = await _service.DeleteStage(999);

            // Assert
            result.Should().BeFalse();

            _stageRepositoryMock.Verify(
                x => x.GetById(999),
                Times.Once);

            _stageRepositoryMock.Verify(
                x => x.Remove(It.IsAny<Stage>()),
                Times.Never);

            _stageRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task DeleteStage_RemovesAndSaves_WhenStageExists()
        {
            // Arrange
            var stage = new Stage();

            _stageRepositoryMock
                .Setup(x => x.GetById(5))
                .ReturnsAsync(stage);

            // Act
            var result = await _service.DeleteStage(5);

            // Assert
            result.Should().BeTrue();

            _stageRepositoryMock.Verify(
                x => x.GetById(5),
                Times.Once);

            _stageRepositoryMock.Verify(
                x => x.Remove(stage),
                Times.Once);

            _stageRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetStageById_ReturnsRepositoryResult()
        {
            // Arrange
            const int stageId = 5;
            var expected = new Stage();

            _stageRepositoryMock
                .Setup(x => x.GetStageById(stageId))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetStageById(stageId);

            // Assert
            result.Should().BeSameAs(expected);

            _stageRepositoryMock.Verify(
                x => x.GetStageById(stageId),
                Times.Once);
        }

        [Fact]
        public async Task UpdateStage_UpdatesAndSaves()
        {
            // Arrange
            var stage = new Stage();

            // Act
            await _service.UpdateStage(stage);

            // Assert
            _stageRepositoryMock.Verify(
                x => x.Update(stage),
                Times.Once);

            _stageRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }
    }
}