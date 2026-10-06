using CycleManager.Domain.Enums;
using CycleManager.Domain.Models;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace CycleManager.Tests.Unit.Services
{
    public class EventScrapeSchedulerServiceTests
    {
        private readonly ApplicationDbContext _db;
        private readonly Mock<IScrapeOrchestratorService> _orchestratorMock;
        private readonly Mock<ILogger<EventScrapeSchedulerService>> _loggerMock;
        private readonly EventScrapeSchedulerService _service;

        public EventScrapeSchedulerServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);

            _orchestratorMock = new Mock<IScrapeOrchestratorService>();
            _loggerMock = new Mock<ILogger<EventScrapeSchedulerService>>();

            _service = new EventScrapeSchedulerService(
                _db,
                _orchestratorMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task RunEventScrapeAsync_WhenNoPendingStages_ReturnsWithoutCallingOrchestrator()
        {
            // Arrange
            // Geen stages in de database.

            // Act
            var act = async () =>
                await _service.RunEventScrapeAsync(1);

            // Assert
            await act.Should().NotThrowAsync();

            _orchestratorMock.Verify(
                x => x.RunStageScrapeAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task RunEventScrapeAsync_WhenStageHasNoScore_SkipsStage()
        {
            // Arrange
            var eventEntity = new Event
            {
                EventId = 1,
                EventCode = "TEST",
                EventYear = 2026
            };

            var stage = new Stage
            {
                Id = 10,
                EventId = 1,
                Event = eventEntity,
                StageName = "1",
                StageOrder = 1,
                ScrapeStatus = ScrapeStatus.Pending,
                NoScore = true
            };

            _db.Events.Add(eventEntity);
            _db.Stages.Add(stage);
            await _db.SaveChangesAsync();

            // Act
            await _service.RunEventScrapeAsync(1);

            // Assert
            var storedStage = await _db.Stages.SingleAsync();

            storedStage.ScrapeStatus.Should().Be(ScrapeStatus.Skipped);

            _orchestratorMock.Verify(
                x => x.RunStageScrapeAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task RunEventScrapeAsync_WhenScrapeSucceedsWithoutData_SetsStageToPending()
        {
            // Arrange
            var configuration = new Configuration
            {
                Id = 1,
                ConfigurationType = "Test"
            };

            var configurationItem = new ConfigurationItem
            {
                Id = 1,
                ConfigurationId = 1,
                Configuration = configuration,
                Score = 10
            };
            var eventEntity = new Event
            {
                EventId = 1,
                EventCode = "TEST",
                EventYear = 2026,
                ConfigurationId = 1,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 10,
                EventId = 1,
                Event = eventEntity,
                StageName = "1",
                StageOrder = 1,
                ScrapeStatus = ScrapeStatus.Pending,
                NoScore = false
            };

            _db.Configurations.Add(configuration);
            _db.ConfigurationItems.Add(configurationItem);
            _db.Events.Add(eventEntity);
            _db.Stages.Add(stage);
            await _db.SaveChangesAsync();

            // Act
            await _service.RunEventScrapeAsync(1);

            // Assert
            var storedStage = await _db.Stages.SingleAsync();

            storedStage.ScrapeStatus.Should().Be(ScrapeStatus.Pending);

            _orchestratorMock.Verify(
                x => x.RunStageScrapeAsync(
                    1,
                    "TEST",
                    10,
                    1,
                    2026),
                Times.Once);
        }

        [Fact]
        public async Task RunEventScrapeAsync_WhenAllExpectedDataExists_SetsStageToCompleted()
        {
            // Arrange
            var configuration = new Configuration
            {
                Id = 1,
                ConfigurationType = "Test"
            };

            var configurationItem = new ConfigurationItem
            {
                Id = 1,
                ConfigurationId = 1,
                Configuration = configuration,
                Score = 10
            };

            var special = new ConfigurationItemSpecial
            {
                Id = 1,
                ConfigurationId = 1,
                Configuration = configuration,
                Question = QuestionType.GC,
                Score = 7
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventCode = "TEST",
                EventYear = 2026,
                ConfigurationId = 1,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 10,
                EventId = 1,
                Event = eventEntity,
                StageName = "1",
                StageOrder = 1,
                ScrapeStatus = ScrapeStatus.Pending,
                NoScore = false
            };

            var result = new Result
            {
                Id = 1,
                StageId = 10
            };

            var specialResult = new ScrapedSpecialResult
            {
                Id = Guid.NewGuid(),
                StageId = 10,
                QuestionType = QuestionType.GC,
                BibNumber = 1,
                ImportedAt = DateTime.UtcNow
            };

            _db.Configurations.Add(configuration);
            _db.ConfigurationItems.Add(configurationItem);
            _db.ConfigurationItemSpecials.Add(special);
            _db.Events.Add(eventEntity);
            _db.Stages.Add(stage);
            _db.Results.Add(result);
            _db.ScrapedSpecialResults.Add(specialResult);

            await _db.SaveChangesAsync();

            // Act
            await _service.RunEventScrapeAsync(1);

            // Assert
            var storedStage = await _db.Stages.SingleAsync();

            storedStage.ScrapeStatus.Should().Be(ScrapeStatus.Completed);
            storedStage.LastSuccessfulScrape.Should().NotBeNull();

            _orchestratorMock.Verify(
                x => x.RunStageScrapeAsync(
                    1,
                    "TEST",
                    10,
                    1,
                    2026),
                Times.Once);
        }

        [Fact]
        public async Task RunEventScrapeAsync_WhenOnlyPartialDataExists_SetsStageToPartial()
        {
            // Arrange
            var configuration = new Configuration
            {
                Id = 1,
                ConfigurationType = "Test"
            };

            var configurationItem = new ConfigurationItem
            {
                Id = 1,
                ConfigurationId = 1,
                Configuration = configuration,
                Score = 10
            };

            var special = new ConfigurationItemSpecial
            {
                Id = 1,
                ConfigurationId = 1,
                Configuration = configuration,
                Question = QuestionType.GC,
                Score = 7
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventCode = "TEST",
                EventYear = 2026,
                ConfigurationId = 1,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 10,
                EventId = 1,
                Event = eventEntity,
                StageName = "1",
                StageOrder = 1,
                ScrapeStatus = ScrapeStatus.Pending,
                NoScore = false
            };

            // Wel een normaal resultaat, maar geen special result.
            var result = new Result
            {
                Id = 1,
                StageId = 10
            };

            _db.Configurations.Add(configuration);
            _db.ConfigurationItems.Add(configurationItem);
            _db.ConfigurationItemSpecials.Add(special);
            _db.Events.Add(eventEntity);
            _db.Stages.Add(stage);
            _db.Results.Add(result);

            await _db.SaveChangesAsync();

            // Act
            await _service.RunEventScrapeAsync(1);

            // Assert
            var storedStage = await _db.Stages.SingleAsync();

            storedStage.ScrapeStatus.Should().Be(ScrapeStatus.Partial);

            _orchestratorMock.Verify(
                x => x.RunStageScrapeAsync(
                    1,
                    "TEST",
                    10,
                    1,
                    2026),
                Times.Once);
        }

        [Fact]
        public async Task RunEventScrapeAsync_WhenEventCodeIsMissing_SetsStageToFailedAndThrows()
        {
            // Arrange
            var configuration = new Configuration
            {
                Id = 1,
                ConfigurationType = "Test"
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventCode = "",
                EventYear = 2026,
                ConfigurationId = 1,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 10,
                EventId = 1,
                Event = eventEntity,
                StageName = "1",
                StageOrder = 1,
                ScrapeStatus = ScrapeStatus.Pending,
                NoScore = false
            };

            _db.Configurations.Add(configuration);
            _db.Events.Add(eventEntity);
            _db.Stages.Add(stage);
            await _db.SaveChangesAsync();

            // Act
            var act = () => _service.RunEventScrapeAsync(1);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Event 1 has no EventCode.");

            var storedStage = await _db.Stages.SingleAsync();
            storedStage.ScrapeStatus.Should().Be(ScrapeStatus.Failed);

            _orchestratorMock.Verify(
                x => x.RunStageScrapeAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task RunEventScrapeAsync_WhenOrchestratorThrows_SetsStageToFailedAndRethrows()
        {
            // Arrange
            var configuration = new Configuration
            {
                Id = 1,
                ConfigurationType = "Test"
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventCode = "TEST",
                EventYear = 2026,
                ConfigurationId = 1,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 10,
                EventId = 1,
                Event = eventEntity,
                StageName = "1",
                StageOrder = 1,
                ScrapeStatus = ScrapeStatus.Pending,
                NoScore = false
            };

            _db.Configurations.Add(configuration);
            _db.Events.Add(eventEntity);
            _db.Stages.Add(stage);

            await _db.SaveChangesAsync();

            var exception = new InvalidOperationException("Scrape failed");

            _orchestratorMock
                .Setup(x => x.RunStageScrapeAsync(
                    1,
                    "TEST",
                    10,
                    1,
                    2026))
                .ThrowsAsync(exception);

            // Act
            var act = () => _service.RunEventScrapeAsync(1);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Scrape failed");

            var storedStage = await _db.Stages.SingleAsync();
            storedStage.ScrapeStatus.Should().Be(ScrapeStatus.Failed);

            _orchestratorMock.Verify(
                x => x.RunStageScrapeAsync(
                    1,
                    "TEST",
                    10,
                    1,
                    2026),
                Times.Once);
        }

        [Fact]
        public async Task RunStartlistSyncAsync_WhenSyncSucceeds_CallsOrchestrator()
        {
            // Arrange
            _orchestratorMock
                .Setup(x => x.RefreshStartlistAsync(1))
                .Returns(Task.CompletedTask);

            // Act
            var act = () => _service.RunStartlistSyncAsync(1);

            // Assert
            await act.Should().NotThrowAsync();

            _orchestratorMock.Verify(
                x => x.RefreshStartlistAsync(1),
                Times.Once);
        }

        [Fact]
        public async Task RunStartlistSyncAsync_WhenSyncThrowsWithInnerException_RethrowsException()
        {
            // Arrange
            var innerException = new InvalidOperationException("Database error");
            var exception = new Exception("Startlist sync failed", innerException);

            _orchestratorMock
                .Setup(x => x.RefreshStartlistAsync(1))
                .ThrowsAsync(exception);

            // Act
            var act = () => _service.RunStartlistSyncAsync(1);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Startlist sync failed");

            _orchestratorMock.Verify(
                x => x.RefreshStartlistAsync(1),
                Times.Once);
        }

        [Fact]
        public async Task RunStartlistSyncAsync_WhenSyncThrowsWithoutInnerException_RethrowsException()
        {
            // Arrange
            var exception = new InvalidOperationException("Startlist sync failed");

            _orchestratorMock
                .Setup(x => x.RefreshStartlistAsync(1))
                .ThrowsAsync(exception);

            // Act
            var act = () => _service.RunStartlistSyncAsync(1);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Startlist sync failed");

            _orchestratorMock.Verify(
                x => x.RefreshStartlistAsync(1),
                Times.Once);
        }
    }
}