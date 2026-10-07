using AutoMapper;
using CycleManager.Domain.Dto;
using CycleManager.Services;
using Domain.Context;
using Domain.Dto;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CycleManager.Tests.Unit.Services
{
    public class EventDashboardServiceTests
    {
        private readonly ApplicationDbContext _db;
        private readonly Mock<IEventService> _eventServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly EventDashboardService _service;

        public EventDashboardServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);

            _eventServiceMock = new Mock<IEventService>();
            _mapperMock = new Mock<IMapper>();

            _service = new EventDashboardService(
                _db,
                _mapperMock.Object,
                _eventServiceMock.Object);
        }

        [Fact]
        public async Task GetDashboardAsync_ReturnsCurrentFutureAndHistoricalEventsWithUserData()
        {
            // Arrange
            var now = DateTime.UtcNow;
            const string userId = "user-1";

            var actueel = new Event
            {
                EventId = 1,
                EventName = "Actueel",
                IsActive = true,
                StartDate = now.AddDays(-2),
                EndDate = now.AddDays(2)
            };

            var toekomst = new Event
            {
                EventId = 2,
                EventName = "Toekomst",
                IsActive = true,
                StartDate = now.AddDays(5),
                EndDate = now.AddDays(7)
            };

            var historisch = new Event
            {
                EventId = 3,
                EventName = "Historisch",
                IsActive = false,
                StartDate = now.AddDays(-10),
                EndDate = now.AddDays(-5)
            };

            historisch.GameCompetitorEvents.Add(new GameCompetitorEvent
            {
                Id = 1,
                EventId = historisch.EventId,
                UserId = userId,
                TeamName = "Mijn team"
            });

            // Een historisch evenement van een andere gebruiker.
            var historischAndereGebruiker = new Event
            {
                EventId = 4,
                EventName = "Historisch ander",
                IsActive = false,
                StartDate = now.AddDays(-10),
                EndDate = now.AddDays(-5)
            };

            historischAndereGebruiker.GameCompetitorEvents.Add(
                new GameCompetitorEvent
                {
                    Id = 2,
                    EventId = historischAndereGebruiker.EventId,
                    UserId = "other-user",
                    TeamName = "Ander team"
                });

            _db.Events.AddRange(
                actueel,
                toekomst,
                historisch,
                historischAndereGebruiker);

            await _db.SaveChangesAsync();

            var actueelDto = new EventForUserDto
            {
                EventId = actueel.EventId
            };

            var toekomstDto = new EventForUserDto
            {
                EventId = toekomst.EventId
            };

            var historischDto = new EventForUserDto
            {
                EventId = historisch.EventId
            };

            _mapperMock
                .Setup(x => x.Map<List<EventForUserDto>>(
                    It.IsAny<List<Event>>()))
                .Returns((List<Event> events) =>
                    events.Select(e => e.EventId switch
                    {
                        1 => actueelDto,
                        2 => toekomstDto,
                        3 => historischDto,
                        _ => new EventForUserDto
                        {
                            EventId = e.EventId
                        }
                    }).ToList());

            var userEvent = new EventForUserDto
            {
                EventId = actueel.EventId,
                Deelnemers = new List<DeelnemerDto>(),
                IsIngeschreven = true,
                CanSubscribe = false
            };

            var userFutureEvent = new EventForUserDto
            {
                EventId = toekomst.EventId,
                Deelnemers = new List<DeelnemerDto>(),
                IsIngeschreven = false,
                CanSubscribe = true
            };

            _eventServiceMock
                .Setup(x => x.GetEventsByUserId(userId))
                .ReturnsAsync(new[]
                {
                    userEvent,
                    userFutureEvent
                });

            // Act
            var result = await _service.GetDashboardAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Titel.Should().Be("Evenementen");

            // Actueel: alle actieve events, dus zowel actueel als toekomstig
            result.Actueel.Should().HaveCount(2);

            result.Actueel.Should().ContainSingle(x => x.EventId == actueel.EventId);
            result.Actueel.Single(x => x.EventId == actueel.EventId)
                .IsIngeschreven.Should().BeTrue();

            result.Actueel.Should().ContainSingle(x => x.EventId == toekomst.EventId);
            result.Actueel.Single(x => x.EventId == toekomst.EventId)
                .CanSubscribe.Should().BeTrue();

            // Toekomst: actieve events waarvan StartDate in de toekomst ligt
            result.Toekomst.Should().ContainSingle();
            result.Toekomst[0].EventId.Should().Be(toekomst.EventId);
            result.Toekomst[0].IsIngeschreven.Should().BeFalse();
            result.Toekomst[0].CanSubscribe.Should().BeTrue();

            // Historisch: afgelopen events waarvoor deze gebruiker een GameCompetitorEvent heeft
            result.Historisch.Should().ContainSingle();
            result.Historisch[0].EventId.Should().Be(historisch.EventId);

            _eventServiceMock.Verify(
                x => x.GetEventsByUserId(userId),
                Times.Once);

            _mapperMock.Verify(
                x => x.Map<List<EventForUserDto>>(It.IsAny<List<Event>>()),
                Times.Exactly(3));
        }
    }
}