using CycleManager.Domain.Models;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;

namespace CycleManager.Tests.Integration.DataAccess
{
    public class GameCompetitorEventPickRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public GameCompetitorEventPickRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(_options);

        [Fact]
        public async Task GetCompetitorEventPicksByEventId_ReturnsCorrectPicks()
        {
            using var context = CreateContext();

            // Seed
            var user = new ApplicationUser { Id = "abc", FirstName = "Test", LastName = "User" };
            var gameEvent = new GameCompetitorEvent { Id = 1, UserId = user.Id, User = user, Renners = new List<GameCompetitorEventPick>() };
            var competitor = new Competitor { CompetitorId = 1, FirstName = "Rider" };
            var team = new Team { TeamId = 1, CurrentTeamName = "TeamA" };
            var competitorInTeam = new CompetitorInTeam { CompetitorId = 1, TeamId = 1, Competitor = competitor, Team = team };
            var competitorsInEvent = new CompetitorsInEvent { Id = 1, EventId = 100, CompetitorInTeam = competitorInTeam };

            var pick = new GameCompetitorEventPick
            {
                Id = 1,
                GameCompetitorEventId = gameEvent.Id,
                GameCompetitorEvent = gameEvent,
                CompetitorsInEventId = competitorsInEvent.Id,
                CompetitorsInEvent = competitorsInEvent
            };

            gameEvent.Renners.Add(pick);

            context.Users.Add(user);
            context.Competitors.Add(competitor);
            context.Teams.Add(team);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(competitorsInEvent);
            context.GameCompetitorsEvent.Add(gameEvent);
            context.GameCompetitorEventPicks.Add(pick);
            await context.SaveChangesAsync();

            var repo = new GameCompetitorEventPickRepository(context);

            // Act
            var result = repo.GetCompetitorEventPicksByEventId(100).ToList();

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(pick.Id);
            result.First().CompetitorsInEvent.EventId.Should().Be(100);
        }

        [Fact]
        public async Task GetCompetitorEventPicksById_ReturnsCorrectPickWithIncludes()
        {
            using var context = CreateContext();

            var user = new ApplicationUser { Id = "abc", FirstName = "Test", LastName = "User" };
            var gameEvent = new GameCompetitorEvent { Id = 1, UserId = user.Id, User = user, Renners = new List<GameCompetitorEventPick>() };

            var country = new Country { CountryId = 1, CountryNameShort = "NL" };
            var competitor = new Competitor { CompetitorId = 1, FirstName = "Rider", Country = country };
            var team = new Team { TeamId = 1, CurrentTeamName = "TeamA" };
            var competitorInTeam = new CompetitorInTeam { CompetitorId = competitor.CompetitorId, TeamId = team.TeamId, Competitor = competitor, Team = team };

            var eventEntity = new Event { EventId = 100, EventName = "Test Event" };

            var competitorsInEvent = new CompetitorsInEvent
            {
                Id = 1,
                EventId = eventEntity.EventId,
                Event = eventEntity,
                CompetitorInTeam = competitorInTeam
            };

            var pick = new GameCompetitorEventPick
            {
                Id = 1,
                GameCompetitorEventId = gameEvent.Id,
                GameCompetitorEvent = gameEvent,
                CompetitorsInEventId = competitorsInEvent.Id,
                CompetitorsInEvent = competitorsInEvent
            };

            gameEvent.Renners.Add(pick);

            context.Users.Add(user);
            context.Countries.Add(country);
            context.Competitors.Add(competitor);
            context.Teams.Add(team);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.Events.Add(eventEntity);
            context.CompetitorsInEvent.Add(competitorsInEvent);
            context.GameCompetitorsEvent.Add(gameEvent);
            context.GameCompetitorEventPicks.Add(pick);

            await context.SaveChangesAsync();
            var repo = new GameCompetitorEventPickRepository(context);

            var result = await repo.GetCompetitorEventPicksById(gameEvent.Id);

            result.Should().HaveCount(1);
            var returnedPick = result.First();
            returnedPick.Id.Should().Be(pick.Id);
            returnedPick.GameCompetitorEvent.User.Should().NotBeNull();
            returnedPick.CompetitorsInEvent.CompetitorInTeam.Competitor.Should().NotBeNull();
            returnedPick.CompetitorsInEvent.CompetitorInTeam.Team.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateGamePicksAsync_AddsAndRemovesCorrectly()
        {
            using var context = CreateContext();

            var gameEvent = new GameCompetitorEvent { Id = 1, UserId = "abc" };
            var competitor1 = new CompetitorsInEvent { Id = 1, EventId = 100 };
            var competitor2 = new CompetitorsInEvent { Id = 2, EventId = 100 };

            context.GameCompetitorsEvent.Add(gameEvent);
            context.CompetitorsInEvent.AddRange(competitor1, competitor2);
            context.GameCompetitorEventPicks.Add(new GameCompetitorEventPick { GameCompetitorEventId = 1, CompetitorsInEventId = 1 });
            await context.SaveChangesAsync();

            var repo = new GameCompetitorEventPickRepository(context);
            var newPicks = new List<GameCompetitorEventPick>
            {
                new GameCompetitorEventPick { GameCompetitorEventId = 1, CompetitorsInEventId = 2 } // nieuwe pick
            };

            await repo.CreateGamePicksAsync(gameEvent.Id, newPicks);

            var saved = context.GameCompetitorEventPicks.ToList();
            saved.Should().HaveCount(1);
            saved.First().CompetitorsInEventId.Should().Be(2);
        }

        [Fact]
        public async Task RemovePickFromEvent_DeletesPick()
        {
            using var context = CreateContext();

            var pick = new GameCompetitorEventPick { Id = 1, GameCompetitorEventId = 1, CompetitorsInEventId = 1 };
            context.GameCompetitorEventPicks.Add(pick);
            await context.SaveChangesAsync();

            var repo = new GameCompetitorEventPickRepository(context);

            await repo.RemovePickFromEvent(1);
            await context.SaveChangesAsync();

            context.GameCompetitorEventPicks.Should().BeEmpty();
        }

        [Fact]
        public async Task DeleteGameCompetitorEventAsync_DeletesEventAndPicks()
        {
            using var context = CreateContext();

            var gameEvent = new GameCompetitorEvent { Id = 1, UserId = "abc", Renners = new List<GameCompetitorEventPick>() };
            var pick1 = new GameCompetitorEventPick { Id = 1, GameCompetitorEventId = 1, CompetitorsInEventId = 1 };
            var pick2 = new GameCompetitorEventPick { Id = 2, GameCompetitorEventId = 1, CompetitorsInEventId = 2 };
            gameEvent.Renners.AddRange(new[] { pick1, pick2 });

            context.GameCompetitorsEvent.Add(gameEvent);
            context.GameCompetitorEventPicks.AddRange(pick1, pick2);
            await context.SaveChangesAsync();

            var repo = new GameCompetitorEventPickRepository(context);

            await repo.DeleteGameCompetitorEventAsync(1);
            await context.SaveChangesAsync();

            context.GameCompetitorsEvent.Should().BeEmpty();
            context.GameCompetitorEventPicks.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateGamePicksAsync_WithEmptyList_DoesNothing()
        {
            using var context = CreateContext();
            var repo = new GameCompetitorEventPickRepository(context);
            var gameCompetitorEvent = new GameCompetitorEvent { Id = 1, UserId = "abc" };

            // Act
            await repo.CreateGamePicksAsync(gameCompetitorEvent.Id, new List<GameCompetitorEventPick>());

            // Assert: er gebeurt niets, geen fout, geen nieuwe records
            context.GameCompetitorEventPicks.Should().BeEmpty();
        }

        [Fact]
        public async Task RemovePickFromEvent_NonExistentId_DoesNothing()
        {
            using var context = CreateContext();
            var repo = new GameCompetitorEventPickRepository(context);

            // Seed een andere pick
            context.GameCompetitorEventPicks.Add(new GameCompetitorEventPick { Id = 1, GameCompetitorEventId = 1, CompetitorsInEventId = 1 });
            await context.SaveChangesAsync();

            // Act: verwijder een pick die niet bestaat
            await repo.RemovePickFromEvent(999);
            await context.SaveChangesAsync();

            // Assert: bestaande pick blijft intact
            context.GameCompetitorEventPicks.Should().HaveCount(1);
        }

        [Fact]
        public async Task DeleteGameCompetitorEventAsync_NonExistentId_DoesNothing()
        {
            using var context = CreateContext();
            var repo = new GameCompetitorEventPickRepository(context);

            // Seed een andere event
            context.GameCompetitorsEvent.Add(new GameCompetitorEvent { Id = 1, UserId = "abc" });
            await context.SaveChangesAsync();

            // Act: verwijder een event dat niet bestaat
            await repo.DeleteGameCompetitorEventAsync(999);
            await context.SaveChangesAsync();

            // Assert: bestaande event blijft
            context.GameCompetitorsEvent.Should().HaveCount(1);
        }
    }
}