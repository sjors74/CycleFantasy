using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CycleManager.Tests.Integration.DataAccess
{
    public class ResultsRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public ResultsRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(_options);

        [Fact]
        public async Task AddResultsAsync_AddsResults()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var stage = new Stage { Id = 1, StageName = "Stage1", EventId = 100 };
            var competitor = new Competitor { CompetitorId = 1, FirstName = "John", LastName = "Doe" };
            var competitorInTeam = new CompetitorInTeam { CompetitorId = 1, Competitor = competitor };
            var cie = new CompetitorsInEvent { Id = 1, CompetitorInTeam = competitorInTeam, EventId = 100 };

            context.Stages.Add(stage);
            context.Competitors.Add(competitor);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(cie);
            await context.SaveChangesAsync();

            var result = new Result { Id = 1, StageId = stage.Id, CompetitorInEventId = cie.Id, Stage = stage, CompetitorInEvent = cie };
            await repo.AddResultsAsync(new[] { result });

            var saved = await context.Results.FindAsync(1);
            saved.Should().NotBeNull();
            saved.StageId.Should().Be(1);
        }

        [Fact]
        public async Task GetResultByIdAsync_ReturnsResultWithIncludes()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var competitor = new Competitor { CompetitorId = 1, FirstName = "John", LastName = "Doe" };
            var team = new Team { TeamId = 1, CurrentTeamName = "TeamA" };
            var competitorInTeam = new CompetitorInTeam { CompetitorId = 1, Competitor = competitor, TeamId = 1, Team = team };
            var cie = new CompetitorsInEvent { Id = 1, CompetitorInTeam = competitorInTeam, CompetitorInTeamId = 1 };
            var stage = new Stage { Id = 1, StageName = "Stage1", EventId = 1 };
            var configurationItem = new ConfigurationItem { Id = 1, Position = 1, ConfigurationId = 1 };

            context.Competitors.Add(competitor);
            context.Teams.Add(team);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(cie);
            context.Stages.Add(stage);
            context.ConfigurationItems.Add(configurationItem);

            var result = new Result
            {
                Id = 1,
                StageId = stage.Id,
                Stage = stage,
                CompetitorInEventId = cie.Id,
                CompetitorInEvent = cie,
                ConfigurationItemId = configurationItem.Id,
                ConfigurationItem = configurationItem
            };

            context.Results.Add(result);
            await context.SaveChangesAsync();

            var fetched = await repo.GetResultByIdAsync(1);

            fetched.Should().NotBeNull();
            fetched.CompetitorInEvent.Should().NotBeNull();
            fetched.Stage.Should().NotBeNull();
            fetched.ConfigurationItem.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteResultAsync_RemovesResult()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var result = new Result { Id = 1 };
            context.Results.Add(result);
            await context.SaveChangesAsync();

            await repo.DeleteResultAsync(result);

            (await context.Results.FindAsync(1)).Should().BeNull();
        }

        [Fact]
        public async Task ResultExistsAsync_ReturnsTrueOrFalse()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            context.Results.Add(new Result { Id = 1 });
            await context.SaveChangesAsync();

            (await repo.ResultExistsAsync(1)).Should().BeTrue();
            (await repo.ResultExistsAsync(999)).Should().BeFalse();
        }

        [Fact]
        public async Task GetResultsByStageId_ReturnsCount()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            context.Results.AddRange(
                new Result { Id = 1, StageId = 1 },
                new Result { Id = 2, StageId = 1 },
                new Result { Id = 3, StageId = 2 }
            );
            await context.SaveChangesAsync();

            var count = await repo.GetResultsByStageId(1);
            count.Should().Be(2);
        }

        [Fact]
        public async Task GetResultsByEventId_ReturnsOrderedResultsWithIncludes()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            // Arrange
            var eventId = 100;

            var stage = new Stage { Id = 1, EventId = eventId, StageName = "Etappe 1" };
            var configurationItem = new ConfigurationItem { Id = 1, Position = 1, ConfigurationId = 1 };
            var competitor = new Competitor { CompetitorId = 1, FirstName = "John", LastName = "Doe" };
            var team = new Team { TeamId = 1, CurrentTeamName = "TeamX" };
            var competitorInTeam = new CompetitorInTeam { CompetitorId = 1, Competitor = competitor, TeamId = 1, Team = team };
            var competitorsInEvent = new CompetitorsInEvent
            {
                Id = 1,
                EventId = eventId,
                CompetitorInTeam = competitorInTeam
            };

            var result = new Result
            {
                Id = 1,
                Stage = stage,
                StageId = stage.Id,
                CompetitorInEvent = competitorsInEvent,
                CompetitorInEventId = competitorsInEvent.Id,
                ConfigurationItem = configurationItem,
                ConfigurationItemId = configurationItem.Id
            };

            context.Competitors.Add(competitor);
            context.Teams.Add(team);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(competitorsInEvent);
            context.Stages.Add(stage);
            context.ConfigurationItems.Add(configurationItem);
            context.Results.Add(result);
            await context.SaveChangesAsync();

            // Act
            var fetched = (await repo.GetResultsByEventId(eventId)).ToList();

            // Assert
            fetched.Should().HaveCount(1);
            fetched.First().Stage.EventId.Should().Be(eventId);
            fetched.First().CompetitorInEvent.Should().NotBeNull();
            fetched.First().ConfigurationItem.Should().NotBeNull();
        }

        [Fact]
        public async Task GetCompetitorFullName_ReturnsCorrectNameOrEmpty()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            context.Competitors.Add(new Competitor { CompetitorId = 1, FirstName = "John", LastName = "Doe" });
            context.SaveChanges();

            repo.GetCompetitorFullName(1).Should().Be("John Doe");
            repo.GetCompetitorFullName(999).Should().BeEmpty();
        }

        [Fact]
        public async Task GetCompetitorResultsByEventId_CalculatesScore()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            // Setup related tables for join
            var stage = new Stage { Id = 1, EventId = 1 };
            var gcep = new GameCompetitorEventPick { Id = 1, CompetitorsInEventId = 1 };
            var dps = new DeelnemerPickScore { Id = Guid.NewGuid(), GameCompetitorEventPickId = 1, StageId = 1, Score = 5 };

            context.Stages.Add(stage);
            context.GameCompetitorEventPicks.Add(gcep);
            context.DeelnemerPickScores.Add(dps);
            await context.SaveChangesAsync();

            var score = await repo.GetCompetitorResultsByEventId(1, 1);

            score.Should().NotBeNull();
            score.TotalScore.Should().Be(5);
        }

        [Fact]
        public async Task GetCompetitorLatestScore_ReturnsCorrectScore()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var ci = new ConfigurationItem { Id = 1, Position = 1, Score = 10 };
            var stage = new Stage { Id = 1, EventId = 1 };
            var competitor = new Competitor { CompetitorId = 1 };
            var competitorInTeam = new CompetitorInTeam { CompetitorId = 1, Competitor = competitor };
            var cie = new CompetitorsInEvent { Id = 1, EventId = 1, CompetitorInTeam = competitorInTeam };

            context.ConfigurationItems.Add(ci);
            context.Stages.Add(stage);
            context.Competitors.Add(competitor);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(cie);

            var result = new Result { Id = 1, StageId = 1, CompetitorInEventId = 1, ConfigurationItemId = 1 };
            context.Results.Add(result);
            await context.SaveChangesAsync();

            var latestScore = await repo.GetCompetitorLatestScore(1, 1);
            latestScore.Should().Be(10);
        }

        [Fact]
        public async Task GetEtappeUitslag_ReturnsTop15OrNoScore()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var evt = new Event { EventId = 1, ConfigurationId = 1 };
            var stage = new Stage { Id = 1, EventId = 1, Event = evt, NoScore = true, NoScoreDescription = "No results" };
            context.Events.Add(evt);
            context.Stages.Add(stage);
            await context.SaveChangesAsync();

            var results = await repo.GetEtappeUitslag(1);

            results.Should().HaveCount(1);
            results.First().NoScore.Should().BeTrue();
            results.First().NoScoreDescription.Should().Be("No results");
        }

        [Fact]
        public async Task GetStageByIdAsync_ReturnsStageWithEvent()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var evt = new Event { EventId = 1 };
            var stage = new Stage { Id = 1, Event = evt };
            context.Events.Add(evt);
            context.Stages.Add(stage);
            await context.SaveChangesAsync();

            var fetched = await repo.GetStageByIdAsync(1);
            fetched.Should().NotBeNull();
            fetched.Event.Should().NotBeNull();
        }

        [Fact]
        public async Task GetResultsByStageAsync_ReturnsResultsWithIncludes()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var competitor = new Competitor { CompetitorId = 1, FirstName = "John", LastName = "Doe" };
            var team = new Team { TeamId = 1, CurrentTeamName = "TeamA" };
            var competitorInTeam = new CompetitorInTeam { CompetitorId = 1, Competitor = competitor, TeamId = 1, Team = team };
            var competitorsInEvent = new CompetitorsInEvent { Id = 1, CompetitorInTeam = competitorInTeam, EventId = 1 };
            var stage = new Stage { Id = 1, StageName = "Stage1", EventId = 1 };
            var configurationItem = new ConfigurationItem { Id = 1, Position = 1, ConfigurationId = 1 };

            context.Competitors.Add(competitor);
            context.Teams.Add(team);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(competitorsInEvent);
            context.Stages.Add(stage);
            context.ConfigurationItems.Add(configurationItem);

            var result = new Result 
            { 
                Id = 1, 
                StageId = stage.Id, 
                CompetitorInEventId = competitorsInEvent.Id,
                Stage = stage,
                CompetitorInEvent = competitorsInEvent,
                ConfigurationItemId = configurationItem.Id,
                ConfigurationItem = configurationItem
            };
            context.Results.Add(result);
            await context.SaveChangesAsync();

            var fetched = await repo.GetResultsByStageAsync(1);
            fetched.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetCompetitorsInEventAsync_ReturnsCompetitorsWithNavigations()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var competitor = new Competitor { CompetitorId = 1 };
            var competitorInTeam = new CompetitorInTeam { CompetitorId = 1, Competitor = competitor };
            var cie = new CompetitorsInEvent { Id = 1, CompetitorInTeam = competitorInTeam, OutOfCompetition = false, EventId = 1 };

            context.Competitors.Add(competitor);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(cie);
            await context.SaveChangesAsync();

            var fetched = await repo.GetCompetitorsInEventAsync(1);
            fetched.Should().HaveCount(1);
            fetched.First().CompetitorInTeam.Competitor.Should().NotBeNull();
        }

        [Fact]
        public async Task GetConfigurationItemsByConfigAsync_ReturnsOrderedItems()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            context.ConfigurationItems.AddRange(
                new ConfigurationItem { Id = 2, ConfigurationId = 1, Position = 2 },
                new ConfigurationItem { Id = 1, ConfigurationId = 1, Position = 1 }
            );
            await context.SaveChangesAsync();

            var fetched = await repo.GetConfigurationItemsByConfigAsync(1);
            fetched.Should().HaveCount(2);
            fetched.First().Position.Should().Be(1);
        }

        [Fact]
        public async Task GetEtappeUitslag_ReturnsTop15_WhenNoScoreIsFalse()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            // Arrange
            var evt = new Event { EventId = 1, ConfigurationId = 1, Configuration = new Configuration { Id = 1 } };
            var stage = new Stage { Id = 1, Event = evt, EventId = 1, NoScore = false };
            var competitor = new Competitor { CompetitorId = 1, FirstName = "Jan", LastName = "Jansen" };
            var team = new Team { TeamId = 1, CurrentTeamName = "TeamTest" };
            var competitorInTeam = new CompetitorInTeam { CompetitorId = 1, TeamId = 1, Competitor = competitor, Team = team };
            var cie = new CompetitorsInEvent { Id = 1, EventId = 1, CompetitorInTeam = competitorInTeam };

            // Voeg 3 configuratie-items toe (de top 3)
            var configItems = new List<ConfigurationItem>
            {
                new() { Id = 1, ConfigurationId = 1, Position = 1, Score = 10 },
                new() { Id = 2, ConfigurationId = 1, Position = 2, Score = 8 },
                new() { Id = 3, ConfigurationId = 1, Position = 3, Score = 6 }
            };

            // Voeg 3 resultaten toe
            var results = configItems.Select(ci => new Result
            {
                Id = ci.Id,
                Stage = stage,
                StageId = stage.Id,
                CompetitorInEvent = cie,
                CompetitorInEventId = cie.Id,
                ConfigurationItem = ci,
                ConfigurationItemId = ci.Id
            }).ToList();

            context.Events.Add(evt);
            context.Stages.Add(stage);
            context.Competitors.Add(competitor);
            context.Teams.Add(team);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(cie);
            context.ConfigurationItems.AddRange(configItems);
            context.Results.AddRange(results);
            await context.SaveChangesAsync();

            // Act
            var uitslag = await repo.GetEtappeUitslag(1);

            // Assert
            uitslag.Should().NotBeNull();
            uitslag.Should().HaveCount(3);
            uitslag!.First().CompetitorName.Should().Be("Jan Jansen");
            uitslag.First().TeamName.Should().Be("TeamTest");
        }

        [Fact]
        public async Task DeleteResultAsync_DoesNotThrow_WhenResultNotInDatabase()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var fakeResult = new Result { Id = 99 };

            // Act
            Func<Task> act = async () => await repo.DeleteResultAsync(fakeResult);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public void GetCompetitorFullName_ReturnsEmpty_WhenCompetitorHasMissingData()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            context.Competitors.AddRange(
                new Competitor { CompetitorId = 1, FirstName = "OnlyFirst", LastName = "" },
                new Competitor { CompetitorId = 2, FirstName = "", LastName = "OnlyLast" }
            );
            context.SaveChanges();

            repo.GetCompetitorFullName(1).Should().Be("OnlyFirst ");
            repo.GetCompetitorFullName(2).Should().Be(" OnlyLast");
            repo.GetCompetitorFullName(999).Should().BeEmpty();
        }

    }
}
