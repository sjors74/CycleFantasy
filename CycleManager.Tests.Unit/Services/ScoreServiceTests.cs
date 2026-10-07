using CycleManager.Domain.Enums;
using CycleManager.Domain.Models;
using CycleManager.Services;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Tests.Unit.Services
{
    public class ScoreServiceTests
    {
        private static (ApplicationDbContext Context, SqliteConnection Connection) CreateContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new ApplicationDbContext(options);

            context.Database.EnsureCreated();

            return (context, connection);
        }

        [Fact]
        public async Task UpdateScoresForStageAsync_WhenThereAreNoParticipants_CompletesSuccessfully()
        {
            var (context, connection) = CreateContext();

            await using (context)
            await using (connection)
            {
                var service = new ScoreService(context);

                await service.UpdateScoresForStageAsync(
                    eventId: 1,
                    stageId: 1);

                var stageScores = await context.DeelnemerStageScores.ToListAsync();
                var pickScores = await context.DeelnemerStagePickScores.ToListAsync();
                var totals = await context.DeelnemerScores.ToListAsync();

                stageScores.Should().BeEmpty();
                pickScores.Should().BeEmpty();
                totals.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task UpdateScoresForStageAsync_WithNormalResult_CreatesAllScoreRecords()
        {
            var (context, connection) = CreateContext();

            await using (context)
            await using (connection)
            {
                var eventEntity = new Event
                {
                    EventId = 1,
                    EventName = "Test Event",
                    EventYear = 2026
                };

                var seasonYear = new SeasonYear
                {
                    SeasonYearId = 1,
                    Year = 2026
                };

                var team = new Team
                {
                    TeamId = 1,
                    CurrentTeamName = "Test Team"
                };

                var teamYear = new TeamYear
                {
                    TeamYearId = 1,
                    TeamId = 1,
                    Team = team,
                    SeasonYearId = 1,
                    SeasonYear = seasonYear,
                    Year = 2026,
                    Name = "Test Team"
                };

                var country = new Country
                {
                    CountryId = 1,
                    CountryNameLong = "Test Country",
                    CountryNameShort = "TST"
                };

                var competitor = new Competitor
                {
                    CompetitorId = 1,
                    FirstName = "Test",
                    LastName = "Rider",
                    PcsName = "Test Rider",
                    CountryId = 1,
                    Country = country
                };

                var competitorInTeam = new CompetitorInTeam
                {
                    Id = 1,
                    Competitor = competitor,
                    TeamYear = teamYear
                };

                var competitorInEvent = new CompetitorsInEvent
                {
                    Id = 10,
                    Event = eventEntity,
                    CompetitorInTeam = competitorInTeam,
                    CompetitorInTeamId = 1,
                    EventId = 1
                };

                var applicationUser = new ApplicationUser
                {
                    Id = "user-1",
                    FirstName = "Piet",
                    LastName = "de Boer"
                };
                context.Users.Add(applicationUser);

                var gameCompetitorEvent = new GameCompetitorEvent
                {
                    Id = 1,
                    Event = eventEntity,
                    EventId = 1,
                    TeamName = "Mijn Team",
                    UserId = "user-1"
                };

                var pick = new GameCompetitorEventPick
                {
                    Id = 1,
                    GameCompetitorEvent = gameCompetitorEvent,
                    GameCompetitorEventId = 1,
                    CompetitorsInEvent = competitorInEvent,
                    CompetitorsInEventId = 10
                };

                gameCompetitorEvent.Renners.Add(pick);

                var stage = new Stage
                {
                    Id = 1,
                    Event = eventEntity,
                    EventId = 1,
                    StageName = "Stage 1",
                    StageOrder = 1
                };

                context.Events.Add(eventEntity);
                context.SeasonYears.Add(seasonYear);
                context.Teams.Add(team);
                context.TeamYear.Add(teamYear);
                context.Add(country);
                context.Competitors.Add(competitor);
                context.CompetitorInTeams.Add(competitorInTeam);
                context.CompetitorsInEvent.Add(competitorInEvent);
                context.GameCompetitorsEvent.Add(gameCompetitorEvent);
                context.Stages.Add(stage);

                var configuration = new Configuration
                {
                    Id = 1,
                    ConfigurationType = "Test Configuration"
                };

                context.Configurations.Add(configuration);

                var configurationItem = new ConfigurationItem
                {
                    Id = 1,
                    Score = 25,
                    ConfigurationId = 1,
                    Configuration = configuration
                };

                context.ConfigurationItems.Add(configurationItem);

                context.Results.Add(new Result
                {
                    Id = 1,
                    StageId = 1,
                    CompetitorInEventId = 10,
                    ConfigurationItemId = 1
                });

                await context.SaveChangesAsync();

                var service = new ScoreService(context);

                await service.UpdateScoresForStageAsync(
                    eventId: 1,
                    stageId: 1);

                var stagePickScore = await context.DeelnemerStagePickScores
                    .SingleAsync();

                stagePickScore.GameCompetitorEventPickId.Should().Be(1);
                stagePickScore.StageId.Should().Be(1);
                stagePickScore.Score.Should().Be(25);

                var pickScore = await context.DeelnemerPickScores
                    .SingleAsync();

                pickScore.GameCompetitorEventPickId.Should().Be(1);
                pickScore.TotalScore.Should().Be(25);

                var stageScore = await context.DeelnemerStageScores
                    .SingleAsync();

                stageScore.GameCompetitorEventId.Should().Be(1);
                stageScore.StageId.Should().Be(1);
                stageScore.Score.Should().Be(25);

                var total = await context.DeelnemerScores
                    .SingleAsync();

                total.GameCompetitorEventId.Should().Be(1);
                total.TotalScore.Should().Be(25);
                total.LaatsteStageScore.Should().Be(25);
                total.LaatsteStageId.Should().Be(1);
            }
        }

        [Fact]
        public async Task UpdateScoresForStageAsync_WhenPickHasNoNormalResult_GivesZeroScore()
        {
            var (context, connection) = CreateContext();

            await using (context)
            await using (connection)
            {
                var eventEntity = new Event
                {
                    EventId = 1,
                    EventName = "Test Event",
                    EventYear = 2026
                };

                var country = new Country
                {
                    CountryId = 1,
                    CountryNameLong = "Nederland",
                    CountryNameShort = "NL"
                };

                var seasonYear = new SeasonYear
                {
                    SeasonYearId = 1,
                    Year = 2026
                };

                var team = new Team
                {
                    TeamId = 1,
                    CurrentTeamName = "Test Team"
                };

                var teamYear = new TeamYear
                {
                    TeamYearId = 1,
                    Team = team,
                    SeasonYear = seasonYear,
                    Year = 2026,
                    Name = "Test Team"
                };

                var competitor = new Competitor
                {
                    CompetitorId = 1,
                    FirstName = "Test",
                    LastName = "Rider",
                    PcsName = "Test Rider",
                    Country = country,
                    CountryId = 1
                };

                var competitorInTeam = new CompetitorInTeam
                {
                    Id = 1,
                    Competitor = competitor,
                    TeamYear = teamYear
                };

                var competitorInEvent = new CompetitorsInEvent
                {
                    Id = 10,
                    Event = eventEntity,
                    CompetitorInTeam = competitorInTeam,
                    CompetitorInTeamId = 1,
                    EventId = 1
                };

                var user = new ApplicationUser
                {
                    Id = "user-1",
                    UserName = "test@test.nl",
                    NormalizedUserName = "TEST@TEST.NL",
                    Email = "test@test.nl",
                    NormalizedEmail = "TEST@TEST.NL",
                    EmailConfirmed = true,
                    FirstName = "Piet",
                    LastName = "de Boer"
                };

                var gameCompetitorEvent = new GameCompetitorEvent
                {
                    Id = 1,
                    Event = eventEntity,
                    EventId = 1,
                    TeamName = "Mijn Team",
                    UserId = "user-1"
                };

                var pick = new GameCompetitorEventPick
                {
                    Id = 1,
                    GameCompetitorEvent = gameCompetitorEvent,
                    GameCompetitorEventId = 1,
                    CompetitorsInEvent = competitorInEvent,
                    CompetitorsInEventId = 10
                };

                gameCompetitorEvent.Renners.Add(pick);

                var stage = new Stage
                {
                    Id = 1,
                    Event = eventEntity,
                    EventId = 1,
                    StageName = "Stage 1",
                    StageOrder = 1
                };

                context.Users.Add(user);
                context.Events.Add(eventEntity);
                context.Countries.Add(country);
                context.SeasonYears.Add(seasonYear);
                context.Teams.Add(team);
                context.TeamYear.Add(teamYear);
                context.Competitors.Add(competitor);
                context.CompetitorInTeams.Add(competitorInTeam);
                context.CompetitorsInEvent.Add(competitorInEvent);
                context.GameCompetitorsEvent.Add(gameCompetitorEvent);
                context.Stages.Add(stage);

                await context.SaveChangesAsync();

                var service = new ScoreService(context);

                await service.UpdateScoresForStageAsync(
                    eventId: 1,
                    stageId: 1);

                var stagePickScore = await context.DeelnemerStagePickScores
                    .SingleAsync();

                stagePickScore.Score.Should().Be(0);

                var pickScore = await context.DeelnemerPickScores
                    .SingleAsync();

                pickScore.TotalScore.Should().Be(0);

                var stageScore = await context.DeelnemerStageScores
                    .SingleAsync();

                stageScore.Score.Should().Be(0);

                var total = await context.DeelnemerScores
                    .SingleAsync();

                total.TotalScore.Should().Be(0);
                total.LaatsteStageScore.Should().Be(0);
            }
        }

        [Fact]
        public async Task UpdateScoresForStageAsync_WithSpecialResult_AddsSpecialScoreToTotal()
        {
            var (context, connection) = CreateContext();

            await using (context)
            await using (connection)
            {
                var eventEntity = new Event
                {
                    EventId = 1,
                    EventName = "Test Event",
                    EventYear = 2026
                };

                var country = new Country
                {
                    CountryId = 1,
                    CountryNameLong = "Nederland",
                    CountryNameShort = "NL"
                };

                var seasonYear = new SeasonYear
                {
                    SeasonYearId = 1,
                    Year = 2026
                };

                var team = new Team
                {
                    TeamId = 1,
                    CurrentTeamName = "Test Team"
                };

                var teamYear = new TeamYear
                {
                    TeamYearId = 1,
                    Team = team,
                    SeasonYear = seasonYear,
                    Year = 2026,
                    Name = "Test Team"
                };

                var competitor = new Competitor
                {
                    CompetitorId = 1,
                    FirstName = "Test",
                    LastName = "Rider",
                    PcsName = "Test Rider",
                    Country = country,
                    CountryId = 1
                };

                var competitorInTeam = new CompetitorInTeam
                {
                    Id = 1,
                    Competitor = competitor,
                    TeamYear = teamYear
                };

                var competitorInEvent = new CompetitorsInEvent
                {
                    Id = 10,
                    Event = eventEntity,
                    CompetitorInTeam = competitorInTeam,
                    CompetitorInTeamId = 1,
                    EventId = 1
                };

                var user = new ApplicationUser
                {
                    Id = "user-1",
                    UserName = "test@test.nl",
                    NormalizedUserName = "TEST@TEST.NL",
                    Email = "test@test.nl",
                    NormalizedEmail = "TEST@TEST.NL",
                    EmailConfirmed = true,
                    FirstName = "Piet",
                    LastName = "de Boer"
                };

                var gameCompetitorEvent = new GameCompetitorEvent
                {
                    Id = 1,
                    Event = eventEntity,
                    EventId = 1,
                    TeamName = "Mijn Team",
                    UserId = "user-1"
                };

                var pick = new GameCompetitorEventPick
                {
                    Id = 1,
                    GameCompetitorEvent = gameCompetitorEvent,
                    GameCompetitorEventId = 1,
                    CompetitorsInEvent = competitorInEvent,
                    CompetitorsInEventId = 10
                };

                gameCompetitorEvent.Renners.Add(pick);

                var stage = new Stage
                {
                    Id = 1,
                    Event = eventEntity,
                    EventId = 1,
                    StageName = "Stage 1",
                    StageOrder = 1
                };

                var configuration = new Configuration
                {
                    Id = 1,
                    ConfigurationType = "Test"
                };

                var configurationItem = new ConfigurationItem
                {
                    Id = 1,
                    Score = 10,
                    ConfigurationId = 1,
                    Configuration = configuration
                };

                var special = new ConfigurationItemSpecial
                {
                    Id = 1,
                    Question = QuestionType.GC,
                    Score = 7,
                    ConfigurationId = 1,
                    Configuration = configuration
                };

                var specialResult = new SpecialResult
                {
                    Id = 1,
                    StageId = 1,
                    CompetitorInEventId = 10,
                    SpecialId = 1,
                    Special = special
                };

                context.Users.Add(user);
                context.Events.Add(eventEntity);
                context.Countries.Add(country);
                context.SeasonYears.Add(seasonYear);
                context.Teams.Add(team);
                context.TeamYear.Add(teamYear);
                context.Competitors.Add(competitor);
                context.CompetitorInTeams.Add(competitorInTeam);
                context.CompetitorsInEvent.Add(competitorInEvent);
                context.GameCompetitorsEvent.Add(gameCompetitorEvent);
                context.Stages.Add(stage);
                context.Configurations.Add(configuration);
                context.ConfigurationItems.Add(configurationItem);

                context.Results.Add(new Result
                {
                    Id = 1,
                    StageId = 1,
                    CompetitorInEventId = 10,
                    ConfigurationItemId = 1
                });

                context.SpecialResults.Add(specialResult);

                await context.SaveChangesAsync();

                var service = new ScoreService(context);

                await service.UpdateScoresForStageAsync(
                    eventId: 1,
                    stageId: 1);

                var specialScore = await context.DeelnemerStagePickSpecialScores
                    .SingleAsync();

                specialScore.GameCompetitorEventPickId.Should().Be(1);
                specialScore.StageId.Should().Be(1);
                specialScore.QuestionType.Should().Be(QuestionType.GC);
                specialScore.Score.Should().Be(7);

                var pickScore = await context.DeelnemerPickScores
                    .SingleAsync();

                pickScore.TotalScore.Should().Be(17);

                var stageScore = await context.DeelnemerStageScores
                    .SingleAsync();

                stageScore.Score.Should().Be(17);

                var total = await context.DeelnemerScores
                    .SingleAsync();

                total.TotalScore.Should().Be(17);
                total.LaatsteStageScore.Should().Be(17);
            }
        }

        [Fact]
        public async Task UpdateScoresForStageAsync_WhenNormalStageScoreExists_UpdatesExistingScore()
        {
            var (context, connection) = CreateContext();

            try
            {
                // Arrange
                var user = new ApplicationUser
                {
                    Id = "user-1",
                    UserName = "test@test.nl",
                    NormalizedUserName = "TEST@TEST.NL",
                    Email = "test@test.nl",
                    NormalizedEmail = "TEST@TEST.NL",
                    FirstName = "Piet",
                    LastName = "de Boer"
                };

                var country = new Country
                {
                    CountryId = 1,
                    CountryNameLong = "Nederland",
                    CountryNameShort = "NL"
                };

                var seasonYear = new SeasonYear
                {
                    SeasonYearId = 1,
                    Year = 2026
                };

                var team = new Team
                {
                    TeamId = 1,
                     CurrentTeamName = "Test Team"
                };

                var teamYear = new TeamYear
                {
                    TeamYearId = 1,
                    TeamId = 1,
                    Team = team,
                    SeasonYearId = 1,
                    SeasonYear = seasonYear
                };

                var competitor = new Competitor
                {
                    CompetitorId = 1,
                    FirstName = "Test",
                    LastName = "Rider",
                    PcsName = "Test Rider",
                    CountryId = 1,
                    Country = country
                };

                var competitorInTeam = new CompetitorInTeam
                {
                    Id = 1,
                    CompetitorId = 1,
                    Competitor = competitor,
                    TeamYearId = 1,
                    TeamYear = teamYear
                };

                var competitorInEvent = new CompetitorsInEvent
                {
                    Id = 10,
                    CompetitorInTeamId = 1,
                    CompetitorInTeam = competitorInTeam,
                    EventId = 1
                };

                var ev = new Event
                {
                    EventId = 1,
                    EventName = "Test Event"
                };

                var participant = new GameCompetitorEvent
                {
                    Id = 1,
                    EventId = 1,
                    Event = ev,
                    UserId = "user-1",
                    User = user,
                    TeamName = "Test Team"
                };

                var pick = new GameCompetitorEventPick
                {
                    Id = 1,
                    GameCompetitorEventId = 1,
                    GameCompetitorEvent = participant,
                    CompetitorsInEventId = 10,
                    CompetitorsInEvent = competitorInEvent
                };

                participant.Renners.Add(pick);

                var stage = new Stage
                {
                    Id = 1,
                    EventId = 1,
                    Event = ev,
                    StageOrder = 1
                };

                var configuration = new Configuration
                {
                    Id = 1,
                    ConfigurationType = "Test"
                };

                var configurationItem = new ConfigurationItem
                {
                    Id = 1,
                    Score = 10,
                    ConfigurationId = 1,
                    Configuration = configuration
                };

                var result = new Result
                {
                    Id = 1,
                    StageId = 1,
                    Stage = stage,
                    CompetitorInEventId = 10,
                    CompetitorInEvent = competitorInEvent,
                    ConfigurationItemId = 1,
                    ConfigurationItem = configurationItem
                };

                var existingStageScore = new DeelnemerStageScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventId = 1,
                    StageId = 1,
                    Score = 999,
                    LastUpdated = DateTime.UtcNow.AddDays(-1)
                };

                context.Users.Add(user);
                context.Countries.Add(country);
                context.SeasonYears.Add(seasonYear);
                context.Teams.Add(team);
                context.TeamYear.Add(teamYear);
                context.Competitors.Add(competitor);
                context.CompetitorInTeams.Add(competitorInTeam);
                context.Events.Add(ev);
                context.Stages.Add(stage);
                context.CompetitorsInEvent.Add(competitorInEvent);
                context.GameCompetitorsEvent.Add(participant);
                context.GameCompetitorEventPicks.Add(pick);
                context.Configurations.Add(configuration);
                context.ConfigurationItems.Add(configurationItem);
                context.Results.Add(result);
                context.DeelnemerStageScores.Add(existingStageScore);

                await context.SaveChangesAsync();

                var originalId = existingStageScore.Id;
                var originalUpdated = existingStageScore.LastUpdated;

                var service = new ScoreService(context);

                // Act
                await service.UpdateScoresForStageAsync(1, 1);

                // Assert
                var updated = await context.DeelnemerStageScores
                    .SingleAsync(x =>
                        x.GameCompetitorEventId == 1 &&
                        x.StageId == 1);

                updated.Id.Should().Be(originalId);
                updated.Score.Should().Be(10);
                updated.LastUpdated.Should().BeAfter(originalUpdated);
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }

        [Fact]
        public async Task UpdateScoresForStageAsync_WhenNormalPickScoreExists_UpdatesExistingScore()
        {
            var (context, connection) = CreateContext();

            try
            {
                // Arrange
                var user = new ApplicationUser
                {
                    Id = "user-1",
                    UserName = "test@test.nl",
                    NormalizedUserName = "TEST@TEST.NL",
                    Email = "test@test.nl",
                    NormalizedEmail = "TEST@TEST.NL",
                    FirstName = "Piet",
                    LastName = "de Boer"
                };

                var country = new Country
                {
                    CountryId = 1,
                    CountryNameLong = "Nederland",
                    CountryNameShort = "NL"
                };

                var seasonYear = new SeasonYear
                {
                    SeasonYearId = 1,
                    Year = 2026
                };

                var team = new Team
                {
                    TeamId = 1,
                    CurrentTeamName = "Test Team"
                };

                var teamYear = new TeamYear
                {
                    TeamYearId = 1,
                    TeamId = 1,
                    Team = team,
                    SeasonYearId = 1,
                    SeasonYear = seasonYear
                };

                var competitor = new Competitor
                {
                    CompetitorId = 1,
                    FirstName = "Test",
                    LastName = "Rider",
                    PcsName = "Test Rider",
                    CountryId = 1,
                    Country = country
                };

                var competitorInTeam = new CompetitorInTeam
                {
                    Id = 1,
                    CompetitorId = 1,
                    Competitor = competitor,
                    TeamYearId = 1,
                    TeamYear = teamYear
                };

                var competitorInEvent = new CompetitorsInEvent
                {
                    Id = 10,
                    CompetitorInTeamId = 1,
                    CompetitorInTeam = competitorInTeam,
                    EventId = 1
                };

                var ev = new Event
                {
                    EventId = 1,
                    EventName = "Test Event"
                };

                var participant = new GameCompetitorEvent
                {
                    Id = 1,
                    EventId = 1,
                    Event = ev,
                    UserId = "user-1",
                    User = user,
                    TeamName = "Test Team"
                };

                var pick = new GameCompetitorEventPick
                {
                    Id = 1,
                    GameCompetitorEventId = 1,
                    GameCompetitorEvent = participant,
                    CompetitorsInEventId = 10,
                    CompetitorsInEvent = competitorInEvent
                };

                participant.Renners.Add(pick);

                var stage = new Stage
                {
                    Id = 1,
                    EventId = 1,
                    Event = ev,
                    StageOrder = 1
                };

                var configuration = new Configuration
                {
                    Id = 1,
                    ConfigurationType = "Test"
                };

                var configurationItem = new ConfigurationItem
                {
                    Id = 1,
                    Score = 10,
                    ConfigurationId = 1,
                    Configuration = configuration
                };

                var result = new Result
                {
                    Id = 1,
                    StageId = 1,
                    Stage = stage,
                    CompetitorInEventId = 10,
                    CompetitorInEvent = competitorInEvent,
                    ConfigurationItemId = 1,
                    ConfigurationItem = configurationItem
                };

                var existingPickScore = new DeelnemerStagePickScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventPickId = 1,
                    StageId = 1,
                    Score = 999,
                    LastUpdated = DateTime.UtcNow.AddDays(-1)
                };

                context.Users.Add(user);
                context.Countries.Add(country);
                context.SeasonYears.Add(seasonYear);
                context.Teams.Add(team);
                context.TeamYear.Add(teamYear);
                context.Competitors.Add(competitor);
                context.CompetitorInTeams.Add(competitorInTeam);
                context.Events.Add(ev);
                context.Stages.Add(stage);
                context.CompetitorsInEvent.Add(competitorInEvent);
                context.GameCompetitorsEvent.Add(participant);
                context.GameCompetitorEventPicks.Add(pick);
                context.Configurations.Add(configuration);
                context.ConfigurationItems.Add(configurationItem);
                context.Results.Add(result);
                context.DeelnemerStagePickScores.Add(existingPickScore);

                await context.SaveChangesAsync();

                var originalId = existingPickScore.Id;
                var originalUpdated = existingPickScore.LastUpdated;

                var service = new ScoreService(context);

                // Act
                await service.UpdateScoresForStageAsync(1, 1);

                // Assert
                var updated = await context.DeelnemerStagePickScores
                    .SingleAsync(x =>
                        x.GameCompetitorEventPickId == 1 &&
                        x.StageId == 1);

                updated.Id.Should().Be(originalId);
                updated.Score.Should().Be(10);
                updated.LastUpdated.Should().BeAfter(originalUpdated);
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }

        [Fact]
        public async Task UpdateScoresForStageAsync_WhenPickTotalExists_UpdatesExistingPickTotal()
        {
            var (context, connection) = CreateContext();

            try
            {
                // Arrange
                var user = new ApplicationUser
                {
                    Id = "user-1",
                    UserName = "test@test.nl",
                    NormalizedUserName = "TEST@TEST.NL",
                    Email = "test@test.nl",
                    NormalizedEmail = "TEST@TEST.NL",
                    FirstName = "Piet",
                    LastName = "de Boer"
                };

                var country = new Country
                {
                    CountryId = 1,
                    CountryNameLong = "Nederland",
                    CountryNameShort = "NL"
                };

                var seasonYear = new SeasonYear
                {
                    SeasonYearId = 1,
                    Year = 2026
                };

                var team = new Team
                {
                    TeamId = 1,
                    CurrentTeamName = "Test Team"
                };

                var teamYear = new TeamYear
                {
                    TeamYearId = 1,
                    TeamId = 1,
                    Team = team,
                    SeasonYearId = 1,
                    SeasonYear = seasonYear
                };

                var competitor = new Competitor
                {
                    CompetitorId = 1,
                    FirstName = "Test",
                    LastName = "Rider",
                    PcsName = "Test Rider",
                    CountryId = 1,
                    Country = country
                };

                var competitorInTeam = new CompetitorInTeam
                {
                    Id = 1,
                    CompetitorId = 1,
                    Competitor = competitor,
                    TeamYearId = 1,
                    TeamYear = teamYear
                };

                var competitorInEvent = new CompetitorsInEvent
                {
                    Id = 10,
                    CompetitorInTeamId = 1,
                    CompetitorInTeam = competitorInTeam,
                    EventId = 1
                };

                var ev = new Event
                {
                    EventId = 1,
                    EventName = "Test Event"
                };

                var participant = new GameCompetitorEvent
                {
                    Id = 1,
                    EventId = 1,
                    Event = ev,
                    UserId = "user-1",
                    User = user,
                    TeamName = "Test Team"
                };

                var pick = new GameCompetitorEventPick
                {
                    Id = 1,
                    GameCompetitorEventId = 1,
                    GameCompetitorEvent = participant,
                    CompetitorsInEventId = 10,
                    CompetitorsInEvent = competitorInEvent
                };

                participant.Renners.Add(pick);

                var stage = new Stage
                {
                    Id = 1,
                    EventId = 1,
                    Event = ev,
                    StageOrder = 1
                };

                var configuration = new Configuration
                {
                    Id = 1,
                    ConfigurationType = "Test"
                };

                var configurationItem = new ConfigurationItem
                {
                    Id = 1,
                    Score = 10,
                    ConfigurationId = 1,
                    Configuration = configuration
                };

                var result = new Result
                {
                    Id = 1,
                    StageId = 1,
                    Stage = stage,
                    CompetitorInEventId = 10,
                    CompetitorInEvent = competitorInEvent,
                    ConfigurationItemId = 1,
                    ConfigurationItem = configurationItem
                };

                var existingPickTotal = new DeelnemerPickScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventPickId = 1,
                    TotalScore = 999,
                    LastUpdate = DateTime.UtcNow.AddDays(-1)
                };

                context.Users.Add(user);
                context.Countries.Add(country);
                context.SeasonYears.Add(seasonYear);
                context.Teams.Add(team);
                context.TeamYear.Add(teamYear);
                context.Competitors.Add(competitor);
                context.CompetitorInTeams.Add(competitorInTeam);
                context.Events.Add(ev);
                context.Stages.Add(stage);
                context.CompetitorsInEvent.Add(competitorInEvent);
                context.GameCompetitorsEvent.Add(participant);
                context.GameCompetitorEventPicks.Add(pick);
                context.Configurations.Add(configuration);
                context.ConfigurationItems.Add(configurationItem);
                context.Results.Add(result);
                context.DeelnemerPickScores.Add(existingPickTotal);

                await context.SaveChangesAsync();

                var originalId = existingPickTotal.Id;
                var originalUpdated = existingPickTotal.LastUpdate;

                var service = new ScoreService(context);

                // Act
                await service.UpdateScoresForStageAsync(1, 1);

                // Assert
                var updated = await context.DeelnemerPickScores
                    .SingleAsync(x => x.GameCompetitorEventPickId == 1);

                updated.Id.Should().Be(originalId);
                updated.TotalScore.Should().Be(10);
                updated.LastUpdate.Should().BeAfter(originalUpdated);
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }

        [Fact]
        public async Task UpdateScoresForStageAsync_WhenParticipantTotalExists_UpdatesExistingTotal()
        {
            var (context, connection) = CreateContext();

            try
            {
                // Arrange
                var user = new ApplicationUser
                {
                    Id = "user-1",
                    UserName = "test@test.nl",
                    NormalizedUserName = "TEST@TEST.NL",
                    Email = "test@test.nl",
                    NormalizedEmail = "TEST@TEST.NL",
                    FirstName = "Piet",
                    LastName = "de Boer"
                };

                var country = new Country
                {
                    CountryId = 1,
                    CountryNameLong = "Nederland",
                    CountryNameShort = "NL"
                };

                var seasonYear = new SeasonYear
                {
                    SeasonYearId = 1,
                    Year = 2026
                };

                var team = new Team
                {
                    TeamId = 1,
                    CurrentTeamName = "Test Team"
                };

                var teamYear = new TeamYear
                {
                    TeamYearId = 1,
                    TeamId = 1,
                    Team = team,
                    SeasonYearId = 1,
                    SeasonYear = seasonYear
                };

                var competitor = new Competitor
                {
                    CompetitorId = 1,
                    FirstName = "Test",
                    LastName = "Rider",
                    PcsName = "Test Rider",
                    CountryId = 1,
                    Country = country
                };

                var competitorInTeam = new CompetitorInTeam
                {
                    Id = 1,
                    CompetitorId = 1,
                    Competitor = competitor,
                    TeamYearId = 1,
                    TeamYear = teamYear
                };

                var competitorInEvent = new CompetitorsInEvent
                {
                    Id = 10,
                    CompetitorInTeamId = 1,
                    CompetitorInTeam = competitorInTeam,
                    EventId = 1
                };

                var ev = new Event
                {
                    EventId = 1,
                    EventName = "Test Event"
                };

                var participant = new GameCompetitorEvent
                {
                    Id = 1,
                    EventId = 1,
                    Event = ev,
                    UserId = "user-1",
                    User = user,
                    TeamName = "Test Team"
                };

                var pick = new GameCompetitorEventPick
                {
                    Id = 1,
                    GameCompetitorEventId = 1,
                    GameCompetitorEvent = participant,
                    CompetitorsInEventId = 10,
                    CompetitorsInEvent = competitorInEvent
                };

                participant.Renners.Add(pick);

                var stage = new Stage
                {
                    Id = 1,
                    EventId = 1,
                    Event = ev,
                    StageOrder = 1
                };

                var configuration = new Configuration
                {
                    Id = 1,
                    ConfigurationType = "Test"
                };

                var configurationItem = new ConfigurationItem
                {
                    Id = 1,
                    Score = 10,
                    ConfigurationId = 1,
                    Configuration = configuration
                };

                var result = new Result
                {
                    Id = 1,
                    StageId = 1,
                    Stage = stage,
                    CompetitorInEventId = 10,
                    CompetitorInEvent = competitorInEvent,
                    ConfigurationItemId = 1,
                    ConfigurationItem = configurationItem
                };

                var existingTotal = new DeelnemerScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventId = 1,
                    TotalScore = 999,
                    LaatsteStageScore = 999,
                    LaatsteStageId = 99,
                    LastUpdated = DateTime.UtcNow.AddDays(-1)
                };

                context.Users.Add(user);
                context.Countries.Add(country);
                context.SeasonYears.Add(seasonYear);
                context.Teams.Add(team);
                context.TeamYear.Add(teamYear);
                context.Competitors.Add(competitor);
                context.CompetitorInTeams.Add(competitorInTeam);
                context.Events.Add(ev);
                context.Stages.Add(stage);
                context.CompetitorsInEvent.Add(competitorInEvent);
                context.GameCompetitorsEvent.Add(participant);
                context.GameCompetitorEventPicks.Add(pick);
                context.Configurations.Add(configuration);
                context.ConfigurationItems.Add(configurationItem);
                context.Results.Add(result);
                context.DeelnemerScores.Add(existingTotal);

                await context.SaveChangesAsync();

                var originalId = existingTotal.Id;
                var originalUpdated = existingTotal.LastUpdated;

                var service = new ScoreService(context);

                // Act
                await service.UpdateScoresForStageAsync(1, 1);

                // Assert
                var updated = await context.DeelnemerScores
                    .SingleAsync(x => x.GameCompetitorEventId == 1);

                updated.Id.Should().Be(originalId);
                updated.TotalScore.Should().Be(10);
                updated.LaatsteStageScore.Should().Be(10);
                updated.LaatsteStageId.Should().Be(1);
                updated.LastUpdated.Should().BeAfter(originalUpdated);
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }

        [Fact]
        public async Task UpdateScoresForStageAsync_WhenSpecialPickScoreExists_UpdatesExistingScore()
        {
            var (context, connection) = CreateContext();

            try
            {
                // Arrange
                var user = new ApplicationUser
                {
                    Id = "user-1",
                    UserName = "test@test.nl",
                    NormalizedUserName = "TEST@TEST.NL",
                    Email = "test@test.nl",
                    NormalizedEmail = "TEST@TEST.NL",
                    FirstName = "Piet",
                    LastName = "de Boer"
                };

                var country = new Country
                {
                    CountryId = 1,
                    CountryNameLong = "Nederland",
                    CountryNameShort = "NL"
                };

                var seasonYear = new SeasonYear
                {
                    SeasonYearId = 1,
                    Year = 2026
                };

                var team = new Team
                {
                    TeamId = 1,
                    CurrentTeamName = "Test Team"
                };

                var teamYear = new TeamYear
                {
                    TeamYearId = 1,
                    TeamId = 1,
                    Team = team,
                    SeasonYearId = 1,
                    SeasonYear = seasonYear
                };

                var competitor = new Competitor
                {
                    CompetitorId = 1,
                    FirstName = "Test",
                    LastName = "Rider",
                    PcsName = "Test Rider",
                    CountryId = 1,
                    Country = country
                };

                var competitorInTeam = new CompetitorInTeam
                {
                    Id = 1,
                    CompetitorId = 1,
                    Competitor = competitor,
                    TeamYearId = 1,
                    TeamYear = teamYear
                };

                var competitorInEvent = new CompetitorsInEvent
                {
                    Id = 10,
                    CompetitorInTeamId = 1,
                    CompetitorInTeam = competitorInTeam,
                    EventId = 1
                };

                var ev = new Event
                {
                    EventId = 1,
                    EventName = "Test Event"
                };

                var participant = new GameCompetitorEvent
                {
                    Id = 1,
                    EventId = 1,
                    Event = ev,
                    UserId = "user-1",
                    User = user,
                    TeamName = "Test Team"
                };

                var pick = new GameCompetitorEventPick
                {
                    Id = 1,
                    GameCompetitorEventId = 1,
                    GameCompetitorEvent = participant,
                    CompetitorsInEventId = 10,
                    CompetitorsInEvent = competitorInEvent
                };

                participant.Renners.Add(pick);

                var stage = new Stage
                {
                    Id = 1,
                    EventId = 1,
                    Event = ev,
                    StageOrder = 1
                };

                var configuration = new Configuration
                {
                    Id = 1,
                    ConfigurationType = "Test"
                };

                var configurationItem = new ConfigurationItem
                {
                    Id = 1,
                    Score = 10,
                    ConfigurationId = 1,
                    Configuration = configuration
                };

                var special = new ConfigurationItemSpecial
                {
                    Id = 1,
                    Question = QuestionType.GC,
                    Score = 7,
                    ConfigurationId = 1,
                    Configuration = configuration
                };

                var specialResult = new SpecialResult
                {
                    Id = 1,
                    StageId = 1,
                    Stage = stage,
                    CompetitorInEventId = 10,
                    CompetitorInEvent = competitorInEvent,
                    SpecialId = 1,
                    Special = special
                };

                var existingSpecialScore = new DeelnemerStagePickSpecialScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventPickId = 1,
                    StageId = 1,
                    QuestionType = QuestionType.GC,
                    Score = 999,
                    LastUpdated = DateTime.UtcNow.AddDays(-1)
                };

                context.Users.Add(user);
                context.Countries.Add(country);
                context.SeasonYears.Add(seasonYear);
                context.Teams.Add(team);
                context.TeamYear.Add(teamYear);
                context.Competitors.Add(competitor);
                context.CompetitorInTeams.Add(competitorInTeam);
                context.Events.Add(ev);
                context.Stages.Add(stage);
                context.CompetitorsInEvent.Add(competitorInEvent);
                context.GameCompetitorsEvent.Add(participant);
                context.GameCompetitorEventPicks.Add(pick);
                context.Configurations.Add(configuration);
                context.ConfigurationItems.Add(configurationItem);
                context.ConfigurationItemSpecials.Add(special);
                context.SpecialResults.Add(specialResult);
                context.DeelnemerStagePickSpecialScores.Add(existingSpecialScore);

                await context.SaveChangesAsync();

                var originalId = existingSpecialScore.Id;
                var originalUpdated = existingSpecialScore.LastUpdated;

                var service = new ScoreService(context);

                // Act
                await service.UpdateScoresForStageAsync(1, 1);

                // Assert
                var updated = await context.DeelnemerStagePickSpecialScores
                    .SingleAsync(x =>
                        x.GameCompetitorEventPickId == 1 &&
                        x.StageId == 1 &&
                        x.QuestionType == QuestionType.GC);

                updated.Id.Should().Be(originalId);
                updated.Score.Should().Be(7);
                updated.LastUpdated.Should().BeAfter(originalUpdated);
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }

        [Fact]
        public async Task RecalculateEventScoresAsync_WhenEventDoesNotExist_ReturnsWithoutChanges()
        {
            var (context, connection) = CreateContext();

            try
            {
                // Arrange
                var service = new ScoreService(context);

                // Act
                var act = async () => await service.RecalculateEventScoresAsync(999);

                // Assert
                await act.Should().NotThrowAsync();

                (await context.DeelnemerStagePickScores.CountAsync())
                    .Should().Be(0);

                (await context.DeelnemerStagePickSpecialScores.CountAsync())
                    .Should().Be(0);

                (await context.DeelnemerPickScores.CountAsync())
                    .Should().Be(0);

                (await context.DeelnemerStageScores.CountAsync())
                    .Should().Be(0);

                (await context.DeelnemerScores.CountAsync())
                    .Should().Be(0);
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }

        [Fact]
        public async Task RecalculateEventScoresAsync_WhenEventHasNoStages_ThrowsInvalidOperationException()
        {
            var (context, connection) = CreateContext();

            try
            {
                // Arrange
                var ev = new Event
                {
                    EventId = 1,
                    EventName = "Test Event"
                };

                context.Events.Add(ev);
                await context.SaveChangesAsync();

                var service = new ScoreService(context);

                // Act
                var act = async () => await service.RecalculateEventScoresAsync(1);

                // Assert
                var exception = await act.Should()
                    .ThrowAsync<InvalidOperationException>();

                exception.Which.Message
                    .Should()
                    .Be("Er zijn geen stages gevonden voor event 1.");
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }

        [Fact]
        public async Task RecalculateEventScoresAsync_WithNormalResult_RecalculatesScores()
        {
            var (context, connection) = CreateContext();

            try
            {
                // Arrange
                var user = new ApplicationUser
                {
                    Id = "user-1",
                    UserName = "test@test.nl",
                    NormalizedUserName = "TEST@TEST.NL",
                    Email = "test@test.nl",
                    NormalizedEmail = "TEST@TEST.NL",
                    FirstName = "Piet",
                    LastName = "de Boer"
                };

                var country = new Country
                {
                    CountryId = 1,
                    CountryNameLong = "Nederland",
                    CountryNameShort = "NL"
                };

                var seasonYear = new SeasonYear
                {
                    SeasonYearId = 1,
                    Year = 2026
                };

                var team = new Team
                {
                    TeamId = 1,
                    CurrentTeamName = "Test Team"
                };

                var teamYear = new TeamYear
                {
                    TeamYearId = 1,
                    TeamId = 1,
                    Team = team,
                    SeasonYearId = 1,
                    SeasonYear = seasonYear
                };

                var competitor = new Competitor
                {
                    CompetitorId = 1,
                    FirstName = "Test",
                    LastName = "Rider",
                    PcsName = "Test Rider",
                    CountryId = 1,
                    Country = country
                };

                var competitorInTeam = new CompetitorInTeam
                {
                    Id = 1,
                    CompetitorId = 1,
                    Competitor = competitor,
                    TeamYearId = 1,
                    TeamYear = teamYear
                };

                var competitorInEvent = new CompetitorsInEvent
                {
                    Id = 10,
                    CompetitorInTeamId = 1,
                    CompetitorInTeam = competitorInTeam,
                    EventId = 1
                };

                var ev = new Event
                {
                    EventId = 1,
                    EventName = "Test Event"
                };

                var participant = new GameCompetitorEvent
                {
                    Id = 1,
                    EventId = 1,
                    Event = ev,
                    UserId = "user-1",
                    User = user,
                    TeamName = "Test Team"
                };

                var pick = new GameCompetitorEventPick
                {
                    Id = 1,
                    GameCompetitorEventId = 1,
                    GameCompetitorEvent = participant,
                    CompetitorsInEventId = 10,
                    CompetitorsInEvent = competitorInEvent
                };

                participant.Renners.Add(pick);

                var stage = new Stage
                {
                    Id = 1,
                    EventId = 1,
                    Event = ev,
                    StageOrder = 1
                };

                var configuration = new Configuration
                {
                    Id = 1,
                    ConfigurationType = "Test"
                };

                var configurationItem = new ConfigurationItem
                {
                    Id = 1,
                    Score = 25,
                    ConfigurationId = 1,
                    Configuration = configuration
                };

                var result = new Result
                {
                    Id = 1,
                    StageId = 1,
                    Stage = stage,
                    CompetitorInEventId = 10,
                    CompetitorInEvent = competitorInEvent,
                    ConfigurationItemId = 1,
                    ConfigurationItem = configurationItem
                };

                context.Users.Add(user);
                context.Countries.Add(country);
                context.SeasonYears.Add(seasonYear);
                context.Teams.Add(team);
                context.TeamYear.Add(teamYear);
                context.Competitors.Add(competitor);
                context.CompetitorInTeams.Add(competitorInTeam);
                context.Events.Add(ev);
                context.Stages.Add(stage);
                context.CompetitorsInEvent.Add(competitorInEvent);
                context.GameCompetitorsEvent.Add(participant);
                context.GameCompetitorEventPicks.Add(pick);
                context.Configurations.Add(configuration);
                context.ConfigurationItems.Add(configurationItem);
                context.Results.Add(result);

                await context.SaveChangesAsync();

                var service = new ScoreService(context);

                // Act
                await service.RecalculateEventScoresAsync(1);

                // Assert
                var stagePickScore = await context.DeelnemerStagePickScores
                    .SingleAsync(x =>
                        x.GameCompetitorEventPickId == 1 &&
                        x.StageId == 1);

                stagePickScore.Score.Should().Be(25);

                var pickTotal = await context.DeelnemerPickScores
                    .SingleAsync(x => x.GameCompetitorEventPickId == 1);

                pickTotal.TotalScore.Should().Be(25);

                var stageScore = await context.DeelnemerStageScores
                    .SingleAsync(x =>
                        x.GameCompetitorEventId == 1 &&
                        x.StageId == 1);

                stageScore.Score.Should().Be(25);

                var total = await context.DeelnemerScores
                    .SingleAsync(x => x.GameCompetitorEventId == 1);

                total.TotalScore.Should().Be(25);
                total.LaatsteStageScore.Should().Be(25);
                total.LaatsteStageId.Should().Be(1);
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }

        [Fact]
        public async Task RecalculateEventScoresAsync_WithSpecialResult_AddsSpecialScoreToTotals()
        {
            var (context, connection) = CreateContext();

            try
            {
                // Arrange
                var user = new ApplicationUser
                {
                    Id = "user-1",
                    UserName = "test@test.nl",
                    NormalizedUserName = "TEST@TEST.NL",
                    Email = "test@test.nl",
                    NormalizedEmail = "TEST@TEST.NL",
                    FirstName = "Piet",
                    LastName = "de Boer"
                };

                var country = new Country
                {
                    CountryId = 1,
                    CountryNameLong = "Nederland",
                    CountryNameShort = "NL"
                };

                var seasonYear = new SeasonYear
                {
                    SeasonYearId = 1,
                    Year = 2026
                };

                var team = new Team
                {
                    TeamId = 1,
                    CurrentTeamName = "Test Team"
                };

                var teamYear = new TeamYear
                {
                    TeamYearId = 1,
                    TeamId = 1,
                    Team = team,
                    SeasonYearId = 1,
                    SeasonYear = seasonYear
                };

                var competitor = new Competitor
                {
                    CompetitorId = 1,
                    FirstName = "Test",
                    LastName = "Rider",
                    PcsName = "Test Rider",
                    CountryId = 1,
                    Country = country
                };

                var competitorInTeam = new CompetitorInTeam
                {
                    Id = 1,
                    CompetitorId = 1,
                    Competitor = competitor,
                    TeamYearId = 1,
                    TeamYear = teamYear
                };

                var competitorInEvent = new CompetitorsInEvent
                {
                    Id = 10,
                    CompetitorInTeamId = 1,
                    CompetitorInTeam = competitorInTeam,
                    EventId = 1
                };

                var ev = new Event
                {
                    EventId = 1,
                    EventName = "Test Event"
                };

                var participant = new GameCompetitorEvent
                {
                    Id = 1,
                    EventId = 1,
                    Event = ev,
                    UserId = "user-1",
                    User = user,
                    TeamName = "Test Team"
                };

                var pick = new GameCompetitorEventPick
                {
                    Id = 1,
                    GameCompetitorEventId = 1,
                    GameCompetitorEvent = participant,
                    CompetitorsInEventId = 10,
                    CompetitorsInEvent = competitorInEvent
                };

                participant.Renners.Add(pick);

                var stage = new Stage
                {
                    Id = 1,
                    EventId = 1,
                    Event = ev,
                    StageOrder = 1
                };

                var configuration = new Configuration
                {
                    Id = 1,
                    ConfigurationType = "Test"
                };

                var configurationItem = new ConfigurationItem
                {
                    Id = 1,
                    Score = 10,
                    ConfigurationId = 1,
                    Configuration = configuration
                };

                var special = new ConfigurationItemSpecial
                {
                    Id = 1,
                    Question = QuestionType.GC,
                    Score = 7,
                    ConfigurationId = 1,
                    Configuration = configuration
                };

                var result = new Result
                {
                    Id = 1,
                    StageId = 1,
                    Stage = stage,
                    CompetitorInEventId = 10,
                    CompetitorInEvent = competitorInEvent,
                    ConfigurationItemId = 1,
                    ConfigurationItem = configurationItem
                };

                var specialResult = new SpecialResult
                {
                    Id = 1,
                    StageId = 1,
                    Stage = stage,
                    CompetitorInEventId = 10,
                    CompetitorInEvent = competitorInEvent,
                    SpecialId = 1,
                    Special = special
                };

                context.Users.Add(user);
                context.Countries.Add(country);
                context.SeasonYears.Add(seasonYear);
                context.Teams.Add(team);
                context.TeamYear.Add(teamYear);
                context.Competitors.Add(competitor);
                context.CompetitorInTeams.Add(competitorInTeam);
                context.Events.Add(ev);
                context.Stages.Add(stage);
                context.CompetitorsInEvent.Add(competitorInEvent);
                context.GameCompetitorsEvent.Add(participant);
                context.GameCompetitorEventPicks.Add(pick);
                context.Configurations.Add(configuration);
                context.ConfigurationItems.Add(configurationItem);
                context.ConfigurationItemSpecials.Add(special);
                context.Results.Add(result);
                context.SpecialResults.Add(specialResult);

                await context.SaveChangesAsync();

                var service = new ScoreService(context);

                // Act
                await service.RecalculateEventScoresAsync(1);

                // Assert
                var specialScore = await context.DeelnemerStagePickSpecialScores
                    .SingleAsync(x =>
                        x.GameCompetitorEventPickId == 1 &&
                        x.StageId == 1 &&
                        x.QuestionType == QuestionType.GC);

                specialScore.Score.Should().Be(7);

                var stagePickScore = await context.DeelnemerStagePickScores
                    .SingleAsync(x =>
                        x.GameCompetitorEventPickId == 1 &&
                        x.StageId == 1);

                stagePickScore.Score.Should().Be(10);

                var pickTotal = await context.DeelnemerPickScores
                    .SingleAsync(x => x.GameCompetitorEventPickId == 1);

                pickTotal.TotalScore.Should().Be(17);

                var stageScore = await context.DeelnemerStageScores
                    .SingleAsync(x =>
                        x.GameCompetitorEventId == 1 &&
                        x.StageId == 1);

                stageScore.Score.Should().Be(17);

                var total = await context.DeelnemerScores
                    .SingleAsync(x => x.GameCompetitorEventId == 1);

                total.TotalScore.Should().Be(17);
                total.LaatsteStageScore.Should().Be(17);
                total.LaatsteStageId.Should().Be(1);
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }

        [Fact]
        public async Task RecalculateEventScoresAsync_WhenExistingScoresExist_ReplacesExistingScores()
        {
            var (context, connection) = CreateContext();

            try
            {
                // Arrange
                var user = new ApplicationUser
                {
                    Id = "user-1",
                    UserName = "test@test.nl",
                    NormalizedUserName = "TEST@TEST.NL",
                    Email = "test@test.nl",
                    NormalizedEmail = "TEST@TEST.NL",
                    FirstName = "Piet",
                    LastName = "de Boer"
                };

                var country = new Country
                {
                    CountryId = 1,
                    CountryNameLong = "Nederland",
                    CountryNameShort = "NL"
                };

                var seasonYear = new SeasonYear
                {
                    SeasonYearId = 1,
                    Year = 2026
                };

                var team = new Team
                {
                    TeamId = 1,
                    CurrentTeamName = "Test Team"
                };

                var teamYear = new TeamYear
                {
                    TeamYearId = 1,
                    TeamId = 1,
                    Team = team,
                    SeasonYearId = 1,
                    SeasonYear = seasonYear
                };

                var competitor = new Competitor
                {
                    CompetitorId = 1,
                    FirstName = "Test",
                    LastName = "Rider",
                    PcsName = "Test Rider",
                    CountryId = 1,
                    Country = country
                };

                var competitorInTeam = new CompetitorInTeam
                {
                    Id = 1,
                    CompetitorId = 1,
                    Competitor = competitor,
                    TeamYearId = 1,
                    TeamYear = teamYear
                };

                var competitorInEvent = new CompetitorsInEvent
                {
                    Id = 10,
                    CompetitorInTeamId = 1,
                    CompetitorInTeam = competitorInTeam,
                    EventId = 1
                };

                var ev = new Event
                {
                    EventId = 1,
                    EventName = "Test Event"
                };

                var participant = new GameCompetitorEvent
                {
                    Id = 1,
                    EventId = 1,
                    Event = ev,
                    UserId = "user-1",
                    User = user,
                    TeamName = "Test Team"
                };

                var pick = new GameCompetitorEventPick
                {
                    Id = 1,
                    GameCompetitorEventId = 1,
                    GameCompetitorEvent = participant,
                    CompetitorsInEventId = 10,
                    CompetitorsInEvent = competitorInEvent
                };

                participant.Renners.Add(pick);

                var stage = new Stage
                {
                    Id = 1,
                    EventId = 1,
                    Event = ev,
                    StageOrder = 1
                };

                var configuration = new Configuration
                {
                    Id = 1,
                    ConfigurationType = "Test"
                };

                var configurationItem = new ConfigurationItem
                {
                    Id = 1,
                    Score = 25,
                    ConfigurationId = 1,
                    Configuration = configuration
                };

                var result = new Result
                {
                    Id = 1,
                    StageId = 1,
                    Stage = stage,
                    CompetitorInEventId = 10,
                    CompetitorInEvent = competitorInEvent,
                    ConfigurationItemId = 1,
                    ConfigurationItem = configurationItem
                };

                context.Users.Add(user);
                context.Countries.Add(country);
                context.SeasonYears.Add(seasonYear);
                context.Teams.Add(team);
                context.TeamYear.Add(teamYear);
                context.Competitors.Add(competitor);
                context.CompetitorInTeams.Add(competitorInTeam);
                context.Events.Add(ev);
                context.Stages.Add(stage);
                context.CompetitorsInEvent.Add(competitorInEvent);
                context.GameCompetitorsEvent.Add(participant);
                context.GameCompetitorEventPicks.Add(pick);
                context.Configurations.Add(configuration);
                context.ConfigurationItems.Add(configurationItem);
                context.Results.Add(result);

                await context.SaveChangesAsync();

                // Bestaande, verouderde scoredata
                context.DeelnemerStagePickScores.Add(new DeelnemerStagePickScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventPickId = 1,
                    StageId = 1,
                    Score = 999,
                    LastUpdated = DateTime.UtcNow.AddDays(-1)
                });

                context.DeelnemerStagePickSpecialScores.Add(new DeelnemerStagePickSpecialScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventPickId = 1,
                    StageId = 1,
                    QuestionType = QuestionType.GC,
                    Score = 999,
                    LastUpdated = DateTime.UtcNow.AddDays(-1)
                });

                context.DeelnemerPickScores.Add(new DeelnemerPickScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventPickId = 1,
                    TotalScore = 999,
                    LastUpdate = DateTime.UtcNow.AddDays(-1)
                });

                context.DeelnemerStageScores.Add(new DeelnemerStageScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventId = 1,
                    StageId = 1,
                    Score = 999,
                    LastUpdated = DateTime.UtcNow.AddDays(-1)
                });

                context.DeelnemerScores.Add(new DeelnemerScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventId = 1,
                    TotalScore = 999,
                    LaatsteStageScore = 999,
                    LaatsteStageId = 1,
                    LastUpdated = DateTime.UtcNow.AddDays(-1)
                });

                await context.SaveChangesAsync();

                var service = new ScoreService(context);

                // Act
                await service.RecalculateEventScoresAsync(1);

                // Assert
                var stagePickScore = await context.DeelnemerStagePickScores
                    .SingleAsync(x =>
                        x.GameCompetitorEventPickId == 1 &&
                        x.StageId == 1);

                stagePickScore.Score.Should().Be(25);

                var pickTotal = await context.DeelnemerPickScores
                    .SingleAsync(x => x.GameCompetitorEventPickId == 1);

                pickTotal.TotalScore.Should().Be(25);

                var stageScore = await context.DeelnemerStageScores
                    .SingleAsync(x =>
                        x.GameCompetitorEventId == 1 &&
                        x.StageId == 1);

                stageScore.Score.Should().Be(25);

                var total = await context.DeelnemerScores
                    .SingleAsync(x => x.GameCompetitorEventId == 1);

                total.TotalScore.Should().Be(25);
                total.LaatsteStageScore.Should().Be(25);
                total.LaatsteStageId.Should().Be(1);
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }

        [Fact]
        public async Task RecalculateEventScoresAsync_WithMultipleStages_CalculatesCumulativeTotals()
        {
            var (context, connection) = CreateContext();

            try
            {
                // Arrange
                var user = new ApplicationUser
                {
                    Id = "user-1",
                    UserName = "test@test.nl",
                    NormalizedUserName = "TEST@TEST.NL",
                    Email = "test@test.nl",
                    NormalizedEmail = "TEST@TEST.NL",
                    FirstName = "Piet",
                    LastName = "de Boer"
                };

                var country = new Country
                {
                    CountryId = 1,
                    CountryNameLong = "Nederland",
                    CountryNameShort = "NL"
                };

                var seasonYear = new SeasonYear
                {
                    SeasonYearId = 1,
                    Year = 2026
                };

                var team = new Team
                {
                    TeamId = 1,
                    CurrentTeamName = "Test Team"
                };

                var teamYear = new TeamYear
                {
                    TeamYearId = 1,
                    TeamId = 1,
                    Team = team,
                    SeasonYearId = 1,
                    SeasonYear = seasonYear
                };

                var competitor = new Competitor
                {
                    CompetitorId = 1,
                    FirstName = "Test",
                    LastName = "Rider",
                    PcsName = "Test Rider",
                    CountryId = 1,
                    Country = country
                };

                var competitorInTeam = new CompetitorInTeam
                {
                    Id = 1,
                    CompetitorId = 1,
                    Competitor = competitor,
                    TeamYearId = 1,
                    TeamYear = teamYear
                };

                var competitorInEvent = new CompetitorsInEvent
                {
                    Id = 10,
                    CompetitorInTeamId = 1,
                    CompetitorInTeam = competitorInTeam,
                    EventId = 1
                };

                var ev = new Event
                {
                    EventId = 1,
                    EventName = "Test Event"
                };

                var participant = new GameCompetitorEvent
                {
                    Id = 1,
                    EventId = 1,
                    Event = ev,
                    UserId = "user-1",
                    User = user,
                    TeamName = "Test Team"
                };

                var pick = new GameCompetitorEventPick
                {
                    Id = 1,
                    GameCompetitorEventId = 1,
                    GameCompetitorEvent = participant,
                    CompetitorsInEventId = 10,
                    CompetitorsInEvent = competitorInEvent
                };

                participant.Renners.Add(pick);

                var stage1 = new Stage
                {
                    Id = 1,
                    EventId = 1,
                    Event = ev,
                    StageOrder = 1
                };

                var stage2 = new Stage
                {
                    Id = 2,
                    EventId = 1,
                    Event = ev,
                    StageOrder = 2
                };

                var configuration1 = new Configuration
                {
                    Id = 1,
                    ConfigurationType = "Test"
                };

                var configuration2 = new Configuration
                {
                    Id = 2,
                    ConfigurationType = "Test"
                };

                var configurationItem1 = new ConfigurationItem
                {
                    Id = 1,
                    Score = 10,
                    ConfigurationId = 1,
                    Configuration = configuration1
                };

                var configurationItem2 = new ConfigurationItem
                {
                    Id = 2,
                    Score = 20,
                    ConfigurationId = 2,
                    Configuration = configuration2
                };

                var result1 = new Result
                {
                    Id = 1,
                    StageId = 1,
                    Stage = stage1,
                    CompetitorInEventId = 10,
                    CompetitorInEvent = competitorInEvent,
                    ConfigurationItemId = 1,
                    ConfigurationItem = configurationItem1
                };

                var result2 = new Result
                {
                    Id = 2,
                    StageId = 2,
                    Stage = stage2,
                    CompetitorInEventId = 10,
                    CompetitorInEvent = competitorInEvent,
                    ConfigurationItemId = 2,
                    ConfigurationItem = configurationItem2
                };

                context.Users.Add(user);
                context.Countries.Add(country);
                context.SeasonYears.Add(seasonYear);
                context.Teams.Add(team);
                context.TeamYear.Add(teamYear);
                context.Competitors.Add(competitor);
                context.CompetitorInTeams.Add(competitorInTeam);
                context.Events.Add(ev);
                context.Stages.AddRange(stage1, stage2);
                context.CompetitorsInEvent.Add(competitorInEvent);
                context.GameCompetitorsEvent.Add(participant);
                context.GameCompetitorEventPicks.Add(pick);
                context.Configurations.AddRange(configuration1, configuration2);
                context.ConfigurationItems.AddRange(configurationItem1, configurationItem2);
                context.Results.AddRange(result1, result2);

                await context.SaveChangesAsync();

                var service = new ScoreService(context);

                // Act
                await service.RecalculateEventScoresAsync(1);

                // Assert
                var stageScores = await context.DeelnemerStageScores
                    .Where(x => x.GameCompetitorEventId == 1)
                    .OrderBy(x => x.StageId)
                    .ToListAsync();

                stageScores.Should().HaveCount(2);
                stageScores[0].Score.Should().Be(10);
                stageScores[1].Score.Should().Be(20);

                var pickScores = await context.DeelnemerStagePickScores
                    .Where(x => x.GameCompetitorEventPickId == 1)
                    .OrderBy(x => x.StageId)
                    .ToListAsync();

                pickScores.Should().HaveCount(2);
                pickScores[0].Score.Should().Be(10);
                pickScores[1].Score.Should().Be(20);

                var pickTotal = await context.DeelnemerPickScores
                    .SingleAsync(x => x.GameCompetitorEventPickId == 1);

                pickTotal.TotalScore.Should().Be(30);

                var total = await context.DeelnemerScores
                    .SingleAsync(x => x.GameCompetitorEventId == 1);

                total.TotalScore.Should().Be(30);
                total.LaatsteStageScore.Should().Be(20);
                total.LaatsteStageId.Should().Be(2);
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }

        [Fact]
        public async Task RecalculateEventScoresAsync_WhenParticipantHasNoPicks_CreatesZeroScores()
        {
            var (context, connection) = CreateContext();

            try
            {
                // Arrange
                var user = new ApplicationUser
                {
                    Id = "user-1",
                    UserName = "test@test.nl",
                    NormalizedUserName = "TEST@TEST.NL",
                    Email = "test@test.nl",
                    NormalizedEmail = "TEST@TEST.NL",
                    FirstName = "Piet",
                    LastName = "de Boer"
                };

                var ev = new Event
                {
                    EventId = 1,
                    EventName = "Test Event"
                };

                var participant = new GameCompetitorEvent
                {
                    Id = 1,
                    EventId = 1,
                    Event = ev,
                    UserId = "user-1",
                    User = user,
                    TeamName = "Test Team"
                };

                var stage = new Stage
                {
                    Id = 1,
                    EventId = 1,
                    Event = ev,
                    StageOrder = 1
                };

                context.Users.Add(user);
                context.Events.Add(ev);
                context.Stages.Add(stage);
                context.GameCompetitorsEvent.Add(participant);

                await context.SaveChangesAsync();

                var service = new ScoreService(context);

                // Act
                await service.RecalculateEventScoresAsync(1);

                // Assert
                var stageScore = await context.DeelnemerStageScores
                    .SingleAsync(x =>
                        x.GameCompetitorEventId == 1 &&
                        x.StageId == 1);

                stageScore.Score.Should().Be(0);

                var total = await context.DeelnemerScores
                    .SingleAsync(x => x.GameCompetitorEventId == 1);

                total.TotalScore.Should().Be(0);
                total.LaatsteStageScore.Should().Be(0);
                total.LaatsteStageId.Should().Be(1);

                (await context.DeelnemerStagePickScores.CountAsync())
                    .Should().Be(0);

                (await context.DeelnemerPickScores.CountAsync())
                    .Should().Be(0);
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }
    }
}