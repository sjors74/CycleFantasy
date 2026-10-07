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
    public class DropoutOrchestratorServiceTests
    {
        private readonly ApplicationDbContext _db;
        private readonly Mock<IScraperService> _scraperServiceMock;
        private readonly Mock<ILogger<DropoutOrchestratorService>> _loggerMock;
        private readonly DropoutOrchestratorService _service;

        public DropoutOrchestratorServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);

            _scraperServiceMock = new Mock<IScraperService>();
            _loggerMock = new Mock<ILogger<DropoutOrchestratorService>>();

            _service = new DropoutOrchestratorService(
                _db,
                _scraperServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task RunDailyDropoutScrapeAsync_WhenNoActiveEvents_ReturnsWithoutCallingScraper()
        {
            // Arrange
            _db.Events.Add(new Event
            {
                EventId = 1,
                IsActive = false
            });

            await _db.SaveChangesAsync();

            // Act
            await _service.RunDailyDropoutScrapeAsync(
                eventId: 1,
                eventName: "Test Event",
                year: 2026);

            // Assert
            _scraperServiceMock.Verify(
                x => x.RunDropoutsAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task RunDailyDropoutScrapeAsync_WithActiveEvent_CallsScraper()
        {
            // Arrange
            _db.Events.Add(new Event
            {
                EventId = 1,
                IsActive = true
            });

            await _db.SaveChangesAsync();

            // Act
            await _service.RunDailyDropoutScrapeAsync(
                eventId: 1,
                eventName: "Test Event",
                year: 2026);

            // Assert
            _scraperServiceMock.Verify(
                x => x.RunDropoutsAsync(
                    1,
                    "Test Event",
                    2026),
                Times.Once);
        }

        [Fact]
        public async Task RunDailyDropoutScrapeAsync_WhenScraperThrows_LogsErrorAndContinues()
        {
            // Arrange
            _db.Events.Add(new Event
            {
                EventId = 1,
                IsActive = true
            });

            await _db.SaveChangesAsync();

            var exception = new InvalidOperationException("Test error");

            _scraperServiceMock
                .Setup(x => x.RunDropoutsAsync(
                    1,
                    "Test Event",
                    2026))
                .ThrowsAsync(exception);

            // Act
            var act = async () =>
                await _service.RunDailyDropoutScrapeAsync(
                    1,
                    "Test Event",
                    2026);

            // Assert
            await act.Should().NotThrowAsync();

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()!.Contains("Dropout scrape mislukt")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}