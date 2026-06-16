using AutoMapper;
using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using CycleManager.Tests.Helpers;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Tests.Integration.DataAccess
{
    public class GameCompetitorInEventRepositoryIntegrationTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly IMapper _mapper;

        public GameCompetitorInEventRepositoryIntegrationTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mapper = AutoMapperTestHelper.CreateMapper();
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(_options);

        [Fact]
        public async Task CreateGameCompetitorEventAsync_AddsEntityToDatabase()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new GameCompetitorInEventRepository(context, _mapper);

            var dto = new DeelnemerCreateDto { EventId = 1, UserId = "user123" };

            // Act
            var result = await repo.CreateGameCompetitorEventAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user123", result.UserId);
            Assert.Single(context.GameCompetitorsEvent);
        }

        [Fact]
        public async Task GetAllGameCompetitorsInEventByEventId_ReturnsCorrectList()
        {
            // Arrange
            using var context = CreateContext();

            var user = new ApplicationUser { Id = "u1", UserName = "User 1", FirstName = "Us", LastName = "Er1" };
            context.Users.Add(user);

            context.GameCompetitorsEvent.AddRange(
                new GameCompetitorEvent { Id = 1, EventId = 10, User = user },
                new GameCompetitorEvent { Id = 2, EventId = 10, User = user },
                new GameCompetitorEvent { Id = 3, EventId = 20, User = user }
            );
            await context.SaveChangesAsync();

            var repo = new GameCompetitorInEventRepository(context, _mapper);

            // Act
            var result = await repo.GetAllGameCompetitorsInEventByEventId(10);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, r => Assert.Equal(10, r.EventId));
        }

        [Fact]
        public async Task GetGameCompetitorInEventById_ReturnsEntityWithIncludes()
        {
            // Arrange
            using var context = CreateContext();
            var user = new ApplicationUser { Id = "u1", UserName = "Jan", FirstName = "Jan", LastName = "User1" };
            var evt = new Event { EventId = 99, EventName = "Test Event" };
            var gce = new GameCompetitorEvent { Id = 1, EventId = 99, Event = evt, User = user, UserId = "u1" };

            context.Users.Add(user);
            context.Events.Add(evt);
            context.GameCompetitorsEvent.Add(gce);
            await context.SaveChangesAsync();

            var repo = new GameCompetitorInEventRepository(context, _mapper);

            // Act
            var result = await repo.GetGameCompetitorInEventById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Jan", result.User.UserName);
            Assert.Equal("Test Event", result.Event.EventName);
        }

        [Fact]
        public async Task GetEventsByUserId_ReturnsEventsLinkedToUser()
        {
            // Arrange
            using var context = CreateContext();

            var user = new ApplicationUser { Id = "u123", UserName = "Klaas", FirstName = "Klaas", LastName = "User123" };
            var evt1 = new Event { EventId = 1, EventName = "Tour" };
            var evt2 = new Event { EventId = 2, EventName = "Giro" };
            var gce = new GameCompetitorEvent { Id = 1, Event = evt1, EventId = 1, User = user, UserId = user.Id };

            context.Users.Add(user);
            context.Events.AddRange(evt1, evt2);
            context.GameCompetitorsEvent.Add(gce);
            await context.SaveChangesAsync();

            var repo = new GameCompetitorInEventRepository(context, _mapper);

            // Act
            var result = await repo.GetEventsByUserId("u123");

            // Assert
            Assert.Single(result);
            Assert.Equal("Tour", result.First().EventName);
        }

        [Fact]
        public async Task GetyCompetitorWithPicksById_ReturnsCompetitorWithRenners()
        {
            // Arrange
            using var context = CreateContext();

            var gce = new GameCompetitorEvent
            {
                Id = 1,
                Renners = new List<GameCompetitorEventPick>
                {
                    new GameCompetitorEventPick { Id = 10 },
                    new GameCompetitorEventPick { Id = 11 }
                }
            };

            context.GameCompetitorsEvent.Add(gce);
            await context.SaveChangesAsync();

            var repo = new GameCompetitorInEventRepository(context, _mapper);

            // Act
            var result = await repo.GetyCompetitorWithPicksById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Renners.Count);
        }

        [Fact]
        public async Task GetyCompetitorWithPicksById_ReturnsNull_WhenNotFound()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new GameCompetitorInEventRepository(context, _mapper);

            // Act
            var result = await repo.GetyCompetitorWithPicksById(999);

            // Assert
            Assert.Null(result);
        }
    }
}
