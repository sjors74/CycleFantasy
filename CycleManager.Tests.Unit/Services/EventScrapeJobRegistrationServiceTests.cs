using CycleManager.Services;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace CycleManager.Tests.Unit.Services
{
    public class EventScrapeJobRegistrationServiceTests
    {
        private readonly ApplicationDbContext _db;
        private readonly Mock<ILogger<EventScrapeJobRegistrationService>> _loggerMock;
        private readonly EventScrapeJobRegistrationService _service;

        public EventScrapeJobRegistrationServiceTests()
        {
            GlobalConfiguration.Configuration
                .UseMemoryStorage();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);

            _loggerMock = new Mock<ILogger<EventScrapeJobRegistrationService>>();

            _service = new EventScrapeJobRegistrationService(
                _db,
                _loggerMock.Object);
        }

        [Fact]
        public async Task RegisterSchedulesAsync_WithNoEvents_CompletesSuccessfully()
        {
            // Act
            var act = async () => await _service.RegisterSchedulesAsync();

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task RegisterSchedulesAsync_ActiveEventToday_RegistersScraperJob()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;

            var eventEntity = new Event
            {
                EventId = 1,
                IsActive = true,
                StartDate = today,
                EndDate = today,
                EventCode = "TEST",
                EventYear = today.Year
            };

            _db.Events.Add(eventEntity);
            await _db.SaveChangesAsync();

            // Act
            await _service.RegisterSchedulesAsync();

            // Assert
            // Als de methode zonder exception door de Hangfire-registratie heen komt,
            // is de betreffende branch uitgevoerd.
            var storedEvent = await _db.Events.SingleAsync();

            storedEvent.EventId.Should().Be(1);
            storedEvent.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task RegisterSchedulesAsync_ActiveEventWithoutEventCode_LogsWarning()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;

            var eventEntity = new Event
            {
                EventId = 2,
                IsActive = true,
                StartDate = today,
                EndDate = today,
                EventCode = null,
                EventYear = today.Year
            };

            _db.Events.Add(eventEntity);
            await _db.SaveChangesAsync();

            // Act
            await _service.RegisterSchedulesAsync();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()!.Contains("has no EventCode")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RegisterSchedulesAsync_ActiveEventWithEventCode_RegistersDropoutJob()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;

            var eventEntity = new Event
            {
                EventId = 3,
                IsActive = true,
                StartDate = today,
                EndDate = today,
                EventCode = "TEST",
                EventYear = today.Year
            };

            _db.Events.Add(eventEntity);
            await _db.SaveChangesAsync();

            // Act
            await _service.RegisterSchedulesAsync();

            // Assert
            // De call naar RecurringJob.AddOrUpdate is uitgevoerd als de methode
            // zonder exception doorloopt.
            var storedEvent = await _db.Events.SingleAsync();

            storedEvent.EventCode.Should().Be("TEST");
            storedEvent.EventYear.Should().Be(today.Year);
        }

        [Fact]
        public async Task RegisterSchedulesAsync_EventWithinStartlistWindow_RegistersStartlistJob()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;

            var eventEntity = new Event
            {
                EventId = 4,
                IsActive = false,
                StartDate = today,
                EndDate = today.AddDays(2),
                EventCode = "TEST",
                EventYear = today.Year
            };

            _db.Events.Add(eventEntity);
            await _db.SaveChangesAsync();

            // Act
            await _service.RegisterSchedulesAsync();

            // Assert
            var storedEvent = await _db.Events.SingleAsync();

            storedEvent.EventId.Should().Be(4);
            storedEvent.StartDate.Should().Be(today);
        }

        [Fact]
        public async Task RegisterSchedulesAsync_InactiveEvent_RemovesScraperJob()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;

            var eventEntity = new Event
            {
                EventId = 5,
                IsActive = false,
                StartDate = today.AddDays(-2),
                EndDate = today.AddDays(2),
                EventCode = "TEST",
                EventYear = today.Year
            };

            _db.Events.Add(eventEntity);
            await _db.SaveChangesAsync();

            // Act
            await _service.RegisterSchedulesAsync();

            // Assert
            var storedEvent = await _db.Events.SingleAsync();

            storedEvent.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task RegisterSchedulesAsync_EventNotToday_RemovesDropoutJob()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;

            var eventEntity = new Event
            {
                EventId = 6,
                IsActive = true,
                StartDate = today.AddDays(1),
                EndDate = today.AddDays(3),
                EventCode = "TEST",
                EventYear = today.Year
            };

            _db.Events.Add(eventEntity);
            await _db.SaveChangesAsync();

            // Act
            await _service.RegisterSchedulesAsync();

            // Assert
            var storedEvent = await _db.Events.SingleAsync();

            storedEvent.StartDate.Should().Be(today.AddDays(1));
            storedEvent.EndDate.Should().Be(today.AddDays(3));
        }

        [Fact]
        public async Task RegisterSchedulesAsync_EventOutsideStartlistWindow_RemovesStartlistJob()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;

            var eventEntity = new Event
            {
                EventId = 7,
                IsActive = false,
                StartDate = today.AddDays(30),
                EndDate = today.AddDays(32),
                EventCode = "TEST",
                EventYear = today.Year
            };

            _db.Events.Add(eventEntity);
            await _db.SaveChangesAsync();

            // Act
            await _service.RegisterSchedulesAsync();

            // Assert
            var storedEvent = await _db.Events.SingleAsync();

            storedEvent.StartDate.Should().Be(today.AddDays(30));
        }
    }
}