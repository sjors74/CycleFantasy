using CycleManager.Domain.Enums;
using CycleManager.Domain.Models;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

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

        #region AddResultsAsync Tests
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

        #endregion

        #region DeleteResultAsync Tests
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

        #endregion

        #region GetCompetitorFullName Tests
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

        #endregion

        #region GetCompetitorLatestScore Tests
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
        public async Task GetCompetitorLatestScore_ReturnsZero_WhenNoResultsExist()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var score = await repo.GetCompetitorLatestScore(1, 1);

            score.Should().Be(0);
        }

        #endregion

        #region GetCompetitorResult by/for Event Tests
        [Fact]
        public async Task GetCompetitorResultsByEventId_CalculatesScore()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            // Setup related tables for join
            var gameEvent = new GameCompetitorEvent { Id = 1, EventId = 1, UserId = "abc" };
            var gcep = new GameCompetitorEventPick { Id = 1, GameCompetitorEventId = gameEvent.Id, CompetitorsInEventId = 1 };
            var normalScore = new DeelnemerStagePickScore
            {
                Id = Guid.NewGuid(),
                GameCompetitorEventPickId = gcep.Id,
                Score = 5,
            };

            context.GameCompetitorsEvent.Add(gameEvent);
            context.GameCompetitorEventPicks.Add(gcep);
            context.DeelnemerStagePickScores.Add(normalScore);

            await context.SaveChangesAsync();

            var score = await repo.GetCompetitorResultsByEventId(1, 1);

            score.Should().NotBeNull();
            score!.CompetitorInEventId.Should().Be(1);
            score.NormalScore.Should().Be(5);
            score.SpecialScore.Should().Be(0);
        }

        [Fact]
        public async Task GetCompetitorResultsForEvent_Returns_Special_Only_Competitor()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var gameEvent = new GameCompetitorEvent
            {
                Id = 1,
                EventId = 100,
                UserId = "user1"
            };

            var pick = new GameCompetitorEventPick
            {
                Id = 1,
                GameCompetitorEventId = 1,
                CompetitorsInEventId = 10
            };

            context.GameCompetitorsEvent.Add(gameEvent);
            context.GameCompetitorEventPicks.Add(pick);

            context.DeelnemerStagePickSpecialScores.Add(
                new DeelnemerStagePickSpecialScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventPickId = 1,
                    StageId = 1,
                    Score = 7
                });

            await context.SaveChangesAsync();

            var result = await repo.GetCompetitorResultsForEvent(100);

            result.Should().HaveCount(1);

            var score = result.Single();

            score.CompetitorInEventId.Should().Be(10);
            score.NormalScore.Should().Be(0);
            score.SpecialScore.Should().Be(7);
        }

        [Fact]
        public async Task GetCompetitorResultsForEvent_Returns_Normal_And_Special_Score()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var gameEvent = new GameCompetitorEvent
            {
                Id = 1,
                EventId = 100,
                UserId = "user1"
            };

            var pick = new GameCompetitorEventPick
            {
                Id = 1,
                GameCompetitorEventId = 1,
                CompetitorsInEventId = 10
            };

            context.GameCompetitorsEvent.Add(gameEvent);
            context.GameCompetitorEventPicks.Add(pick);

            context.DeelnemerStagePickScores.Add(
                new DeelnemerStagePickScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventPickId = 1,
                    StageId = 1,
                    Score = 10
                });

            context.DeelnemerStagePickSpecialScores.Add(
                new DeelnemerStagePickSpecialScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventPickId = 1,
                    StageId = 1,
                    Score = 7
                });

            await context.SaveChangesAsync();

            var result = await repo.GetCompetitorResultsForEvent(100);

            result.Should().HaveCount(1);

            var score = result.Single();

            score.CompetitorInEventId.Should().Be(10);
            score.NormalScore.Should().Be(10);
            score.SpecialScore.Should().Be(7);
        }

        [Fact]
        public async Task GetCompetitorResultsByEventId_ReturnsNull_WhenCompetitorHasNoPicks()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var gameEvent = new Event
            {
                EventId = 100
            };

            context.Events.Add(gameEvent);
            await context.SaveChangesAsync();

            var result = await repo.GetCompetitorResultsByEventId(
                eventId: 100,
                competitorInEventId: 10);

            result.Should().BeNull();
        }
        #endregion

        #region GetCompetitorsInEventAsync Tests
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
        #endregion

        #region GetConfigurationItemsByConfigAsync Tests
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
        #endregion

        #region GetEtappeUitslag Tests
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

            results.Should().NotBeNull();
            results.Uitslag.Should().NotBeNull();
            results.Uitslag.Should().HaveCount(1);
            results.Uitslag.First().NoScore.Should().BeTrue();
            results.Uitslag.First().NoScoreDescription.Should().Be("No results");
        }

        [Fact]
        public async Task GetEtappeUitslag_ReturnsTop15_WhenNoScoreIsFalse()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            // Arrange
            var configuration = new Configuration
            {
                Id = 1
            };
            var evt = new Event { EventId = 1, ConfigurationId = 1, Configuration = configuration };

            var stage = new Stage { Id = 1, Event = evt, EventId = 1, NoScore = false };

            var competitor = new Competitor { CompetitorId = 1, FirstName = "Jan", LastName = "Jansen" };

            var team = new Team { TeamId = 1, CurrentTeamName = "TeamTest" };

            var seasonYear = new SeasonYear { SeasonYearId = 1, Year = 2024 };

            var teamYear = new TeamYear { TeamYearId = 1, TeamId = 1, SeasonYearId = 1, Team = team, SeasonYear = seasonYear, Name = "TeamTest" };

            var competitorInTeam = new CompetitorInTeam { Id = 1, CompetitorId = 1, Competitor = competitor, TeamYearId = 1, TeamYear = teamYear };

            var cie = new CompetitorsInEvent { Id = 1, EventId = 1, Event = evt, CompetitorInTeamId = 1, CompetitorInTeam = competitorInTeam };

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

            context.Configurations.Add(configuration);
            context.Events.Add(evt);
            context.Stages.Add(stage);
            context.Competitors.Add(competitor);
            context.Teams.Add(team);
            context.SeasonYears.Add(seasonYear);
            context.TeamYear.Add(teamYear);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(cie);
            context.ConfigurationItems.AddRange(configItems);
            context.Results.AddRange(results);
            await context.SaveChangesAsync();

            // Act
            var uitslag = await repo.GetEtappeUitslag(1);

            // Assert
            uitslag.Should().NotBeNull();
            uitslag.Uitslag.Should().HaveCount(3);
            uitslag!.Uitslag.First().CompetitorName.Should().Be("Jan Jansen");
            uitslag.Uitslag.First().TeamName.Should().Be("TeamTest");
        }

        [Fact]
        public async Task GetEtappeUitslag_ReturnsNull_WhenStageDoesNotExist()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var result = await repo.GetEtappeUitslag(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetEtappeUitslag_Returns_NoScore_Result_When_Stage_Has_NoScore()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var gameEvent = new Event
            {
                EventId = 100
            };

            var stage = new Stage
            {
                Id = 1,
                EventId = 100,
                NoScore = true,
                NoScoreDescription = "Rustdag"
            };

            context.Events.Add(gameEvent);
            context.Stages.Add(stage);

            await context.SaveChangesAsync();

            var result = await repo.GetEtappeUitslag(1);

            result.Should().NotBeNull();
            result!.Uitslag.Should().HaveCount(1);
            result.Specials.Should().BeEmpty();

            var noScoreResult = result.Uitslag.Single();

            noScoreResult.NoScore.Should().BeTrue();
            noScoreResult.NoScoreDescription.Should().Be("Rustdag");
        }

        [Fact]
        public async Task GetEtappeUitslag_Skips_ConfigurationItem_When_No_Result_Exists()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var gameEvent = new Event
            {
                EventId = 100
            };

            var stage = new Stage
            {
                Id = 1,
                EventId = 100,
                NoScore = false
            };

            var configItem = new ConfigurationItem
            {
                Id = 10,
                ConfigurationId = 1,
                Position = 1,
                Score = 10
            };

            gameEvent.ConfigurationId = 1;

            context.Events.Add(gameEvent);
            context.Stages.Add(stage);
            context.ConfigurationItems.Add(configItem);

            await context.SaveChangesAsync();

            var result = await repo.GetEtappeUitslag(1);

            result.Should().NotBeNull();
            result!.Uitslag.Should().BeEmpty();
            result.Specials.Should().BeEmpty();
        }

        [Fact]
        public async Task GetEtappeUitslag_Skips_Result_When_Competitor_Is_Missing()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var gameEvent = new Event
            {
                EventId = 100,
                ConfigurationId = 1
            };

            var stage = new Stage
            {
                Id = 1,
                EventId = 100,
                NoScore = false
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
                Name = "Test Team 2026"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 999, // bestaat niet
                TeamYearId = 1
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 10,
                EventId = 100,
                CompetitorInTeamId = 1
            };

            var configItem = new ConfigurationItem
            {
                Id = 10,
                ConfigurationId = 1,
                Position = 1,
                Score = 10
            };

            var result = new Result
            {
                Id = 1,
                StageId = 1,
                CompetitorInEventId = 10,
                ConfigurationItemId = 10
            };

            context.Events.Add(gameEvent);
            context.Stages.Add(stage);
            context.Teams.Add(team);
            context.TeamYear.Add(teamYear);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(competitorInEvent);
            context.ConfigurationItems.Add(configItem);
            context.Results.Add(result);

            await context.SaveChangesAsync();

            var uitslag = await repo.GetEtappeUitslag(1);

            uitslag.Should().NotBeNull();
            uitslag!.Uitslag.Should().BeEmpty();
        }

        [Fact]
        public async Task GetEtappeUitslag_Skips_Special_When_No_SpecialResult_Exists()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var gameEvent = new Event
            {
                EventId = 100,
                ConfigurationId = 1
            };

            var stage = new Stage
            {
                Id = 1,
                EventId = 100,
                NoScore = false
            };

            var special = new ConfigurationItemSpecial
            {
                Id = 20,
                ConfigurationId = 1,
                Question = QuestionType.GC,
                Color = "red",
                Score = 5
            };

            context.Events.Add(gameEvent);
            context.Stages.Add(stage);
            context.ConfigurationItemSpecials.Add(special);

            await context.SaveChangesAsync();

            var result = await repo.GetEtappeUitslag(1);

            result.Should().NotBeNull();
            result!.Uitslag.Should().BeEmpty();
            result.Specials.Should().BeEmpty();
        }

        [Fact]
        public async Task GetEtappeUitslag_Skips_Special_When_Competitor_Is_Missing()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var gameEvent = new Event
            {
                EventId = 100,
                ConfigurationId = 1
            };

            var stage = new Stage
            {
                Id = 1,
                EventId = 100,
                NoScore = false
            };

            var special = new ConfigurationItemSpecial
            {
                Id = 20,
                ConfigurationId = 1,
                Question = QuestionType.GC,
                Color = "red",
                Score = 5
            };

            var specialResult = new SpecialResult
            {
                Id = 1,
                StageId = 1,
                CompetitorInEventId = 10,
                SpecialId = 20
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
                Name = "Test Team 2026"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 999,
                TeamYearId = 1
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 10,
                EventId = 100,
                CompetitorInTeamId = 1
            };

            context.Events.Add(gameEvent);
            context.Stages.Add(stage);
            context.Teams.Add(team);
            context.TeamYear.Add(teamYear);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(competitorInEvent);
            context.ConfigurationItemSpecials.Add(special);
            context.SpecialResults.Add(specialResult);

            await context.SaveChangesAsync();

            var result = await repo.GetEtappeUitslag(1);

            result.Should().NotBeNull();
            result!.Specials.Should().BeEmpty();
        }

        [Fact]
        public async Task GetEtappeUitslag_Returns_Special_When_SpecialResult_Has_Valid_Competitor()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var gameEvent = new Event
            {
                EventId = 100,
                ConfigurationId = 1
            };

            var stage = new Stage
            {
                Id = 1,
                EventId = 100,
                NoScore = false
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Test",
                LastName = "Rider"
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
                Name = "Test Team 2026"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                TeamYearId = 1
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 10,
                EventId = 100,
                CompetitorInTeamId = 1
            };

            var special = new ConfigurationItemSpecial
            {
                Id = 20,
                ConfigurationId = 1,
                Question = QuestionType.GC,
                Color = "red",
                Score = 5
            };

            var specialResult = new SpecialResult
            {
                Id = 1,
                StageId = 1,
                CompetitorInEventId = 10,
                SpecialId = 20
            };

            context.Events.Add(gameEvent);
            context.Stages.Add(stage);
            context.Competitors.Add(competitor);
            context.Teams.Add(team);
            context.TeamYear.Add(teamYear);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(competitorInEvent);
            context.ConfigurationItemSpecials.Add(special);
            context.SpecialResults.Add(specialResult);

            await context.SaveChangesAsync();

            var result = await repo.GetEtappeUitslag(1);

            result.Should().NotBeNull();
            result!.Specials.Should().HaveCount(1);

            var specialDto = result.Specials.Single();

            specialDto.Name.Should().Be("GC");
            specialDto.Color.Should().Be("red");
            specialDto.CompetitorName.Should().Be("Test Rider");
            specialDto.TeamName.Should().Be("Test Team 2026");
            specialDto.Score.Should().Be(5);
        }

        #endregion

        #region GetResultByIdAsync Tests

        [Fact]
        public async Task GetResultByIdAsync_ReturnsResultWithIncludes()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var competitor = new Competitor { CompetitorId = 1, FirstName = "John", LastName = "Doe" };
            var team = new Team { TeamId = 1, CurrentTeamName = "TeamA" };
            var competitorInTeam = new CompetitorInTeam { CompetitorId = 1 };
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

        #endregion

        #region GetPickDetailsAsync Tests
        [Fact]
        public async Task GetPickDetailsAsync_ReturnsEmptyList_WhenNoPicksExist()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var result = await repo.GetPickDetailsAsync(
                eventId: 100,
                gameCompetitorEventId: 1);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPickDetailsAsync_Returns_Normal_And_Special_Scores()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var gameEvent = new Event
            {
                EventId = 100,
                ConfigurationId = 1
            };

            var stage = new Stage
            {
                Id = 1,
                EventId = 100
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Test",
                LastName = "Rider"
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
                Name = "Test Team 2026"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                TeamYearId = 1
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 10,
                EventId = 100,
                CompetitorInTeamId = 1
            };

            var gameCompetitorEvent = new GameCompetitorEvent
            {
                Id = 1,
                EventId = 100,
                UserId = "user1"
            };

            var pick = new GameCompetitorEventPick
            {
                Id = 1,
                GameCompetitorEventId = 1,
                CompetitorsInEventId = 10
            };

            var configurationItem = new ConfigurationItem
            {
                Id = 20,
                ConfigurationId = 1,
                Position = 1,
                Score = 10
            };

            var special = new ConfigurationItemSpecial
            {
                Id = 30,
                ConfigurationId = 1,
                Question = QuestionType.GC,
                Color = "red",
                Score = 5
            };

            var result = new Result
            {
                Id = 1,
                StageId = 1,
                CompetitorInEventId = 10,
                ConfigurationItemId = 20
            };

            var specialResult = new SpecialResult
            {
                Id = 1,
                StageId = 1,
                CompetitorInEventId = 10,
                SpecialId = 30
            };

            context.Events.Add(gameEvent);
            context.Stages.Add(stage);
            context.Competitors.Add(competitor);
            context.Teams.Add(team);
            context.TeamYear.Add(teamYear);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(competitorInEvent);
            context.GameCompetitorsEvent.Add(gameCompetitorEvent);
            context.GameCompetitorEventPicks.Add(pick);
            context.ConfigurationItems.Add(configurationItem);
            context.ConfigurationItemSpecials.Add(special);
            context.Results.Add(result);
            context.SpecialResults.Add(specialResult);

            await context.SaveChangesAsync();

            var output = await repo.GetPickDetailsAsync(100, 1);

            output.Should().HaveCount(1);

            var detail = output.Single();

            detail.CompetitorInEventId.Should().Be(10);
            detail.CompetitorName.Should().Be("Test Rider");
            detail.NormalScore.Should().Be(10);
            detail.SpecialScore.Should().Be(5);
            detail.TotalScore.Should().Be(15);
            detail.LastScore.Should().Be(15);

            detail.Specials.Should().HaveCount(1);
            detail.Specials.Single().Name.Should().Be("GC");
            detail.Specials.Single().Score.Should().Be(5);
        }

        [Fact]
        public async Task GetPickDetailsAsync_Returns_Zero_Scores_When_Pick_Has_No_Results()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var gameEvent = new Event
            {
                EventId = 100,
                ConfigurationId = 1
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Test",
                LastName = "Rider"
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
                Name = "Test Team 2026"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                TeamYearId = 1
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 10,
                EventId = 100,
                CompetitorInTeamId = 1
            };

            var gameCompetitorEvent = new GameCompetitorEvent
            {
                Id = 1,
                EventId = 100,
                UserId = "user1"
            };

            var pick = new GameCompetitorEventPick
            {
                Id = 1,
                GameCompetitorEventId = 1,
                CompetitorsInEventId = 10
            };

            context.Events.Add(gameEvent);
            context.Competitors.Add(competitor);
            context.Teams.Add(team);
            context.TeamYear.Add(teamYear);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(competitorInEvent);
            context.GameCompetitorsEvent.Add(gameCompetitorEvent);
            context.GameCompetitorEventPicks.Add(pick);

            await context.SaveChangesAsync();

            var output = await repo.GetPickDetailsAsync(100, 1);

            output.Should().HaveCount(1);

            var detail = output.Single();

            detail.CompetitorInEventId.Should().Be(10);
            detail.CompetitorName.Should().Be("Test Rider");
            detail.NormalScore.Should().Be(0);
            detail.SpecialScore.Should().Be(0);
            detail.TotalScore.Should().Be(0);
            detail.LastScore.Should().Be(0);
            detail.Specials.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPickDetailsAsync_Orders_Picks_By_TotalScore_Descending()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var gameEvent = new Event
            {
                EventId = 100
            };

            var stage = new Stage
            {
                Id = 1,
                EventId = 100
            };

            var competitor1 = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Low",
                LastName = "Score"
            };

            var competitor2 = new Competitor
            {
                CompetitorId = 2,
                FirstName = "High",
                LastName = "Score"
            };

            var team1 = new Team { TeamId = 1, CurrentTeamName = "Team 1" };
            var team2 = new Team { TeamId = 2, CurrentTeamName = "Team 2" };

            var teamYear1 = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Name = "Team 1 2026"
            };

            var teamYear2 = new TeamYear
            {
                TeamYearId = 2,
                TeamId = 2,
                Name = "Team 2 2026"
            };

            var competitorInTeam1 = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                TeamYearId = 1
            };

            var competitorInTeam2 = new CompetitorInTeam
            {
                Id = 2,
                CompetitorId = 2,
                TeamYearId = 2
            };

            var competitorInEvent1 = new CompetitorsInEvent
            {
                Id = 10,
                EventId = 100,
                CompetitorInTeamId = 1
            };

            var competitorInEvent2 = new CompetitorsInEvent
            {
                Id = 20,
                EventId = 100,
                CompetitorInTeamId = 2
            };

            var user = new ApplicationUser
            {
                Id = "test-user",
                UserName = "test@example.com",
                NormalizedUserName = "TEST@EXAMPLE.COM",
                Email = "test@example.com",
                NormalizedEmail = "TEST@EXAMPLE.COM",
                EmailConfirmed = true,
                FirstName = "Test",
                LastName = "User"
            };

            var gameCompetitorEvent = new GameCompetitorEvent
            {
                Id = 1,
                EventId = 100,
                UserId = "test-user"
            };

            var pick1 = new GameCompetitorEventPick
            {
                Id = 1,
                GameCompetitorEventId = 1,
                CompetitorsInEventId = 10
            };

            var pick2 = new GameCompetitorEventPick
            {
                Id = 2,
                GameCompetitorEventId = 1,
                CompetitorsInEventId = 20
            };

            var configItem1 = new ConfigurationItem
            {
                Id = 1,
                Score = 5
            };

            var configItem2 = new ConfigurationItem
            {
                Id = 2,
                Score = 20
            };

            var result1 = new Result
            {
                Id = 1,
                StageId = 1,
                CompetitorInEventId = 10,
                ConfigurationItemId = 1
            };

            var result2 = new Result
            {
                Id = 2,
                StageId = 1,
                CompetitorInEventId = 20,
                ConfigurationItemId = 2
            };

            context.Events.Add(gameEvent);
            context.Stages.Add(stage);
            context.Competitors.AddRange(competitor1, competitor2);
            context.Teams.AddRange(team1, team2);
            context.TeamYear.AddRange(teamYear1, teamYear2);
            context.CompetitorInTeams.AddRange(competitorInTeam1, competitorInTeam2);
            context.CompetitorsInEvent.AddRange(competitorInEvent1, competitorInEvent2);
            context.Users.Add(user);
            context.GameCompetitorsEvent.Add(gameCompetitorEvent);
            context.GameCompetitorEventPicks.AddRange(pick1, pick2);
            context.ConfigurationItems.AddRange(configItem1, configItem2);
            context.Results.AddRange(result1, result2);

            await context.SaveChangesAsync();

            var result = await repo.GetPickDetailsAsync(100, 1);

            result.Should().HaveCount(2);

            result[0].CompetitorName.Should().Be("High Score");
            result[0].TotalScore.Should().Be(20);

            result[1].CompetitorName.Should().Be("Low Score");
            result[1].TotalScore.Should().Be(5);
        }
        #endregion

        #region GetResultsByEventId Tests
        [Fact]
        public async Task GetResultsByEventId_ReturnsOrderedResultsWithIncludes()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            // Arrange
            var eventId = 100;
            var configuration = new Configuration
            {
                Id = 1
            };

            var evt = new Event
            {
                EventId = eventId,
                ConfigurationId = 1,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                EventId = eventId,
                Event = evt,
                StageName = "Etappe 1"
            };

            var configurationItem = new ConfigurationItem
            {
                Id = 1,
                Position = 1,
                ConfigurationId = 1,
                Configuration = configuration
            };

            var competitor = new Competitor { CompetitorId = 1, FirstName = "John", LastName = "Doe" };
            var team = new Team { TeamId = 1, CurrentTeamName = "TeamX" };
            var seasonYear = new SeasonYear { SeasonYearId = 1, Year = 2024 };
            var teamYear = new TeamYear { TeamYearId = 1, TeamId = 1, SeasonYearId = 1, Team = team, SeasonYear = seasonYear, Name = "TeamX" };
            var competitorInTeam = new CompetitorInTeam { Id = 1, CompetitorId = 1, Competitor = competitor, TeamYearId = 1, TeamYear = teamYear };
            var competitorsInEvent = new CompetitorsInEvent
            {
                Id = 1,
                EventId = eventId,
                Event = evt,
                CompetitorInTeamId = 1,
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

            context.Configurations.Add(configuration);
            context.Events.Add(evt);
            context.Stages.Add(stage);
            context.Competitors.Add(competitor);
            context.Teams.Add(team);
            context.SeasonYears.Add(seasonYear);
            context.TeamYear.Add(teamYear);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(competitorsInEvent);
            context.ConfigurationItems.Add(configurationItem);
            context.Results.Add(result);

            await context.SaveChangesAsync();

            // Act
            var fetched = (await repo.GetResultsByEventId(eventId)).ToList();

            // Assert
            fetched.Should().NotBeNull();
            fetched.Should().BeAssignableTo<IEnumerable<Result>>();
            fetched.First().Stage.EventId.Should().Be(eventId);
            fetched.First().CompetitorInEvent.Should().NotBeNull();
            fetched.First().ConfigurationItem.Should().NotBeNull();
        }

        #endregion

        #region GetResultsByStage(Id) Tests
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
        public async Task GetResultsByStageAsync_ReturnsResultsWithIncludes()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var competitor = new Competitor { CompetitorId = 1, FirstName = "John", LastName = "Doe" };
            var team = new Team { TeamId = 1, CurrentTeamName = "TeamA" };
            var competitorInTeam = new CompetitorInTeam { CompetitorId = 1 };
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
            fetched.Should().NotBeNull();
            fetched.Should().BeAssignableTo<IEnumerable<Result>>();
            fetched.Cast<Result>().Should().HaveCount(1);
        }

        #endregion

        #region GetScoreBreakdownByEventIdAsync Tests
        [Fact]
        public async Task GetScoreBreakdownByEventIdAsync_Returns_Normal_And_Special_Scores()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var gameEvent = new GameCompetitorEvent
            {
                Id = 1,
                EventId = 100,
                UserId = "user1"
            };

            var pick = new GameCompetitorEventPick
            {
                Id = 1,
                GameCompetitorEventId = 1,
                CompetitorsInEventId = 10
            };

            context.GameCompetitorsEvent.Add(gameEvent);
            context.GameCompetitorEventPicks.Add(pick);

            context.DeelnemerStagePickScores.AddRange(
                new DeelnemerStagePickScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventPickId = 1,
                    StageId = 1,
                    Score = 10
                },
                new DeelnemerStagePickScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventPickId = 1,
                    StageId = 2,
                    Score = 8
                });

            context.DeelnemerStagePickSpecialScores.Add(
                new DeelnemerStagePickSpecialScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventPickId = 1,
                    StageId = 1,
                    Score = 7
                });

            await context.SaveChangesAsync();

            var result = await repo.GetScoreBreakdownByEventIdAsync(100);

            result.Should().HaveCount(1);

            var breakdown = result.Single();

            breakdown.GameCompetitorEventId.Should().Be(1);
            breakdown.NormalPoints.Should().Be(18);
            breakdown.SpecialPoints.Should().Be(7);
        }

        [Fact]
        public async Task GetScoreBreakdownByEventIdAsync_Returns_Zero_When_Competitor_Has_No_Scores()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var gameEvent = new GameCompetitorEvent
            {
                Id = 1,
                EventId = 100,
                UserId = "user1"
            };

            context.GameCompetitorsEvent.Add(gameEvent);

            await context.SaveChangesAsync();

            var result = await repo.GetScoreBreakdownByEventIdAsync(100);

            result.Should().HaveCount(1);

            var breakdown = result.Single();

            breakdown.GameCompetitorEventId.Should().Be(1);
            breakdown.NormalPoints.Should().Be(0);
            breakdown.SpecialPoints.Should().Be(0);
        }
        #endregion

        #region GetStageByIdAsync Tests
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

        #endregion

        #region GetTotalScoresByEventIdAsync Tests
        [Fact]
        public async Task GetTotalScoresByEventIdAsync_Returns_Scores_For_Event()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var event1 = new Event
            {
                EventId = 100
            };

            var gameCompetitorEvent = new GameCompetitorEvent
            {
                Id = 1,
                EventId = 100,
                UserId = "user1"
            };

            var score = new DeelnemerScore
            {
                Id = Guid.NewGuid(),
                GameCompetitorEventId = 1,
                TotalScore = 30,
                LaatsteStageScore = 10,
                LaatsteStageId = 5
            };

            context.Events.Add(event1);
            context.GameCompetitorsEvent.Add(gameCompetitorEvent);
            context.DeelnemerScores.Add(score);

            await context.SaveChangesAsync();

            var result = await repo.GetTotalScoresByEventIdAsync(100);

            result.Should().HaveCount(1);

            var returnedScore = result.Single();

            returnedScore.GameCompetitorEventId.Should().Be(1);
            returnedScore.TotalScore.Should().Be(30);
            returnedScore.LaatsteStageScore.Should().Be(10);
            returnedScore.LaatsteStageId.Should().Be(5);
        }

        [Fact]
        public async Task GetTotalScoresByEventIdAsync_Ignores_Scores_From_Other_Events()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var event1 = new Event
            {
                EventId = 100
            };

            var event2 = new Event
            {
                EventId = 200
            };

            var gameCompetitorEvent1 = new GameCompetitorEvent
            {
                Id = 1,
                EventId = 100,
                UserId = "user1"
            };

            var gameCompetitorEvent2 = new GameCompetitorEvent
            {
                Id = 2,
                EventId = 200,
                UserId = "user2"
            };

            var score1 = new DeelnemerScore
            {
                Id = Guid.NewGuid(),
                GameCompetitorEventId = 1,
                TotalScore = 30
            };

            var score2 = new DeelnemerScore
            {
                Id = Guid.NewGuid(),
                GameCompetitorEventId = 2,
                TotalScore = 50
            };

            context.Events.AddRange(event1, event2);
            context.GameCompetitorsEvent.AddRange(
                gameCompetitorEvent1,
                gameCompetitorEvent2);
            context.DeelnemerScores.AddRange(score1, score2);

            await context.SaveChangesAsync();

            var result = await repo.GetTotalScoresByEventIdAsync(100);

            result.Should().HaveCount(1);
            result.Single().GameCompetitorEventId.Should().Be(1);
            result.Single().TotalScore.Should().Be(30);
        }

        #endregion

        #region RecalculateEventScoresAsync Tests
        [Fact]
        public async Task RecalculateEventScoresAsync_Calculates_Normal_And_Special_Scores()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            // Arrange
            var configuration = new Configuration
            {
                Id = 1
            };

            var normalConfigItem = new ConfigurationItem
            {
                Id = 1,
                ConfigurationId = 1,
                Position = 1,
                Score = 10,
                Configuration = configuration
            };

            var specialConfigItem = new ConfigurationItemSpecial
            {
                Id = 1,
                ConfigurationId = 1,
                Score = 7,
                Configuration = configuration
            };

            configuration.ConfigurationItems = new List<ConfigurationItem>
            {
                normalConfigItem
            };

            configuration.Specials = new List<ConfigurationItemSpecial>
            {
                specialConfigItem
            };

            var evt = new Event
            {
                EventId = 100,
                ConfigurationId = 1,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                StageName = "Etappe 1"
            };

            var gameEvent = new GameCompetitorEvent
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                UserId = "user1",
                TeamName = "Team 1"
            };

            var pick = new GameCompetitorEventPick
            {
                Id = 1,
                GameCompetitorEventId = 1,
                GameCompetitorEvent = gameEvent,
                CompetitorsInEventId = 1
            };

            gameEvent.Renners.Add(pick);

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "John",
                LastName = "Doe"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                CompetitorInTeamId = 1,
                CompetitorInTeam = competitorInTeam
            };

            var result = new Result
            {
                Id = 1,
                StageId = 1,
                Stage = stage,
                CompetitorInEventId = 1,
                CompetitorInEvent = competitorInEvent,
                ConfigurationItemId = 1,
                ConfigurationItem = normalConfigItem
            };

            var specialResult = new SpecialResult
            {
                Id = 1,
                StageId = 1,
                Stage = stage,
                CompetitorInEventId = 1,
                CompetitorInEvent = competitorInEvent,
                SpecialId = 1,
                Special = specialConfigItem
            };

            context.Configurations.Add(configuration);
            context.ConfigurationItems.Add(normalConfigItem);
            context.ConfigurationItemSpecials.Add(specialConfigItem);
            context.Events.Add(evt);
            context.Stages.Add(stage);
            context.Competitors.Add(competitor);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(competitorInEvent);
            context.GameCompetitorsEvent.Add(gameEvent);
            context.GameCompetitorEventPicks.Add(pick);
            context.Results.Add(result);
            context.SpecialResults.Add(specialResult);

            await context.SaveChangesAsync();

            // Act
            await repo.RecalculateEventScoresAsync(100);

            // Assert
            var stagePickScore = await context.DeelnemerStagePickScores
                .SingleAsync();

            stagePickScore.GameCompetitorEventPickId.Should().Be(1);
            stagePickScore.StageId.Should().Be(1);
            stagePickScore.Score.Should().Be(10);

            var stagePickSpecialScore = await context.DeelnemerStagePickSpecialScores
                .SingleAsync();

            stagePickSpecialScore.GameCompetitorEventPickId.Should().Be(1);
            stagePickSpecialScore.StageId.Should().Be(1);
            stagePickSpecialScore.Score.Should().Be(7);

            var stageScore = await context.DeelnemerStageScores
                .SingleAsync();

            stageScore.GameCompetitorEventId.Should().Be(1);
            stageScore.StageId.Should().Be(1);
            stageScore.Score.Should().Be(17);

            var pickScore = await context.DeelnemerPickScores
                .SingleAsync();

            pickScore.GameCompetitorEventPickId.Should().Be(1);
            pickScore.TotalScore.Should().Be(17);

            var deelnemerScore = await context.DeelnemerScores
                .SingleAsync();

            deelnemerScore.GameCompetitorEventId.Should().Be(1);
            deelnemerScore.TotalScore.Should().Be(17);
            deelnemerScore.LaatsteStageId.Should().Be(1);
            deelnemerScore.LaatsteStageScore.Should().Be(17);
        }

        [Fact]
        public async Task RecalculateEventScoresAsync_Calculates_Totals_Across_Multiple_Stages()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            // Arrange
            var configuration = new Configuration
            {
                Id = 1
            };

            var normalConfigItem1 = new ConfigurationItem
            {
                Id = 1,
                ConfigurationId = 1,
                Position = 1,
                Score = 10,
                Configuration = configuration
            };

            var normalConfigItem2 = new ConfigurationItem
            {
                Id = 2,
                ConfigurationId = 1,
                Position = 2,
                Score = 8,
                Configuration = configuration
            };

            var specialConfigItem = new ConfigurationItemSpecial
            {
                Id = 1,
                ConfigurationId = 1,
                Score = 7,
                Configuration = configuration
            };

            configuration.ConfigurationItems = new List<ConfigurationItem>
            {
                normalConfigItem1,
                normalConfigItem2
            };

            configuration.Specials = new List<ConfigurationItemSpecial>
            {
                specialConfigItem
            };

            var evt = new Event
            {
                EventId = 100,
                ConfigurationId = 1,
                Configuration = configuration
            };

            var stage1 = new Stage
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                StageName = "Etappe 1"
            };

            var stage2 = new Stage
            {
                Id = 2,
                EventId = 100,
                Event = evt,
                StageName = "Etappe 2"
            };

            var gameEvent = new GameCompetitorEvent
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                UserId = "user1",
                TeamName = "Team 1"
            };

            var pick = new GameCompetitorEventPick
            {
                Id = 1,
                GameCompetitorEventId = 1,
                GameCompetitorEvent = gameEvent,
                CompetitorsInEventId = 1
            };

            gameEvent.Renners.Add(pick);

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "John",
                LastName = "Doe"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                CompetitorInTeamId = 1,
                CompetitorInTeam = competitorInTeam
            };

            // Stage 1: normal = 10, special = 7
            var result1 = new Result
            {
                Id = 1,
                StageId = 1,
                Stage = stage1,
                CompetitorInEventId = 1,
                CompetitorInEvent = competitorInEvent,
                ConfigurationItemId = 1,
                ConfigurationItem = normalConfigItem1
            };

            var specialResult1 = new SpecialResult
            {
                Id = 1,
                StageId = 1,
                Stage = stage1,
                CompetitorInEventId = 1,
                CompetitorInEvent = competitorInEvent,
                SpecialId = 1,
                Special = specialConfigItem
            };

            // Stage 2: normal = 8, no special
            var result2 = new Result
            {
                Id = 2,
                StageId = 2,
                Stage = stage2,
                CompetitorInEventId = 1,
                CompetitorInEvent = competitorInEvent,
                ConfigurationItemId = 2,
                ConfigurationItem = normalConfigItem2
            };

            context.Configurations.Add(configuration);
            context.ConfigurationItems.AddRange(
                normalConfigItem1,
                normalConfigItem2);

            context.ConfigurationItemSpecials.Add(specialConfigItem);

            context.Events.Add(evt);
            context.Stages.AddRange(stage1, stage2);

            context.Competitors.Add(competitor);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(competitorInEvent);

            context.GameCompetitorsEvent.Add(gameEvent);
            context.GameCompetitorEventPicks.Add(pick);

            context.Results.AddRange(result1, result2);
            context.SpecialResults.Add(specialResult1);

            await context.SaveChangesAsync();

            // Act
            await repo.RecalculateEventScoresAsync(100);

            // Assert - stage scores
            var stageScores = await context.DeelnemerStageScores
                .Where(x => x.GameCompetitorEventId == 1)
                .OrderBy(x => x.StageId)
                .ToListAsync();

            stageScores.Should().HaveCount(2);

            stageScores[0].StageId.Should().Be(1);
            stageScores[0].Score.Should().Be(17);

            stageScores[1].StageId.Should().Be(2);
            stageScores[1].Score.Should().Be(8);

            // Assert - pick total
            var pickScore = await context.DeelnemerPickScores
                .SingleAsync();

            pickScore.GameCompetitorEventPickId.Should().Be(1);
            pickScore.TotalScore.Should().Be(25);

            // Assert - deelnemer total
            var deelnemerScore = await context.DeelnemerScores
                .SingleAsync();

            deelnemerScore.GameCompetitorEventId.Should().Be(1);
            deelnemerScore.TotalScore.Should().Be(25);

            // Last stage must be stage 2
            deelnemerScore.LaatsteStageId.Should().Be(2);
            deelnemerScore.LaatsteStageScore.Should().Be(8);

            // Assert - individual stage/pick scores
            var stagePickScores = await context.DeelnemerStagePickScores
                .Where(x => x.GameCompetitorEventPickId == 1)
                .OrderBy(x => x.StageId)
                .ToListAsync();

            stagePickScores.Should().HaveCount(2);
            stagePickScores[0].Score.Should().Be(10);
            stagePickScores[1].Score.Should().Be(8);

            var stagePickSpecialScores = await context.DeelnemerStagePickSpecialScores
                .Where(x => x.GameCompetitorEventPickId == 1)
                .OrderBy(x => x.StageId)
                .ToListAsync();

            stagePickSpecialScores.Should().HaveCount(2);
            stagePickSpecialScores[0].Score.Should().Be(7);
            stagePickSpecialScores[1].Score.Should().Be(0);
        }

        [Fact]
        public async Task RecalculateEventScoresAsync_Removes_Existing_Scores_Before_Recalculating()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            // Arrange
            var configuration = new Configuration
            {
                Id = 1
            };

            var normalConfigItem = new ConfigurationItem
            {
                Id = 1,
                ConfigurationId = 1,
                Position = 1,
                Score = 10,
                Configuration = configuration
            };

            configuration.ConfigurationItems = new List<ConfigurationItem>
            {
                normalConfigItem
            };

            var evt = new Event
            {
                EventId = 100,
                ConfigurationId = 1,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                StageName = "Etappe 1"
            };

            var gameEvent = new GameCompetitorEvent
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                UserId = "user1",
                TeamName = "Team 1"
            };

            var pick = new GameCompetitorEventPick
            {
                Id = 1,
                GameCompetitorEventId = 1,
                GameCompetitorEvent = gameEvent,
                CompetitorsInEventId = 1
            };

            gameEvent.Renners.Add(pick);

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "John",
                LastName = "Doe"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                CompetitorInTeamId = 1,
                CompetitorInTeam = competitorInTeam
            };

            var result = new Result
            {
                Id = 1,
                StageId = 1,
                Stage = stage,
                CompetitorInEventId = 1,
                CompetitorInEvent = competitorInEvent,
                ConfigurationItemId = 1,
                ConfigurationItem = normalConfigItem
            };

            // Oude scoregegevens
            var oldStagePickScore = new DeelnemerStagePickScore
            {
                Id = Guid.NewGuid(),
                GameCompetitorEventPickId = 1,
                StageId = 1,
                Score = 999
            };

            var oldStagePickSpecialScore = new DeelnemerStagePickSpecialScore
            {
                Id = Guid.NewGuid(),
                GameCompetitorEventPickId = 1,
                StageId = 1,
                Score = 888
            };

            var oldStageScore = new DeelnemerStageScore
            {
                Id = Guid.NewGuid(),
                GameCompetitorEventId = 1,
                StageId = 1,
                Score = 777
            };

            var oldPickScore = new DeelnemerPickScore
            {
                Id = Guid.NewGuid(),
                GameCompetitorEventPickId = 1,
                TotalScore = 666,
                LastUpdate = DateTime.UtcNow
            };

            var oldDeelnemerScore = new DeelnemerScore
            {
                Id = Guid.NewGuid(),
                GameCompetitorEventId = 1,
                TotalScore = 555,
                LaatsteStageId = 99,
                LaatsteStageScore = 444
            };

            context.Configurations.Add(configuration);
            context.ConfigurationItems.Add(normalConfigItem);
            context.Events.Add(evt);
            context.Stages.Add(stage);
            context.Competitors.Add(competitor);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(competitorInEvent);
            context.GameCompetitorsEvent.Add(gameEvent);
            context.GameCompetitorEventPicks.Add(pick);
            context.Results.Add(result);

            context.DeelnemerStagePickScores.Add(oldStagePickScore);
            context.DeelnemerStagePickSpecialScores.Add(oldStagePickSpecialScore);
            context.DeelnemerStageScores.Add(oldStageScore);
            context.DeelnemerPickScores.Add(oldPickScore);
            context.DeelnemerScores.Add(oldDeelnemerScore);

            await context.SaveChangesAsync();

            // Act
            await repo.RecalculateEventScoresAsync(100);

            // Assert
            var stagePickScores = await context.DeelnemerStagePickScores
                .ToListAsync();

            stagePickScores.Should().HaveCount(1);
            stagePickScores.Single().Score.Should().Be(10);
            stagePickScores.Single().Id.Should().NotBe(oldStagePickScore.Id);

            var stagePickSpecialScores = await context.DeelnemerStagePickSpecialScores
                .ToListAsync();

            stagePickSpecialScores.Should().HaveCount(1);
            stagePickSpecialScores.Single().Score.Should().Be(0);
            stagePickSpecialScores.Single().Id.Should().NotBe(oldStagePickSpecialScore.Id);

            var stageScores = await context.DeelnemerStageScores
                .ToListAsync();

            stageScores.Should().HaveCount(1);
            stageScores.Single().Score.Should().Be(10);
            stageScores.Single().Id.Should().NotBe(oldStageScore.Id);

            var pickScores = await context.DeelnemerPickScores
                .ToListAsync();

            pickScores.Should().HaveCount(1);
            pickScores.Single().TotalScore.Should().Be(10);
            pickScores.Single().Id.Should().NotBe(oldPickScore.Id);

            var deelnemerScores = await context.DeelnemerScores
                .ToListAsync();

            deelnemerScores.Should().HaveCount(1);
            deelnemerScores.Single().TotalScore.Should().Be(10);
            deelnemerScores.Single().LaatsteStageId.Should().Be(1);
            deelnemerScores.Single().LaatsteStageScore.Should().Be(10);
            deelnemerScores.Single().Id.Should().NotBe(oldDeelnemerScore.Id);
        }

        [Fact]
        public async Task RecalculateEventScoresAsync_UsesZero_WhenPickHasNoResult()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            // Arrange
            var configuration = new Configuration
            {
                Id = 1
            };

            var normalConfigItem = new ConfigurationItem
            {
                Id = 1,
                ConfigurationId = 1,
                Position = 1,
                Score = 10,
                Configuration = configuration
            };

            configuration.ConfigurationItems = new List<ConfigurationItem>
            {
                normalConfigItem
            };

            var evt = new Event
            {
                EventId = 100,
                ConfigurationId = 1,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                StageName = "Etappe 1"
            };

            var gameEvent = new GameCompetitorEvent
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                UserId = "user1",
                TeamName = "Team 1"
            };

            var pick = new GameCompetitorEventPick
            {
                Id = 1,
                GameCompetitorEventId = 1,
                GameCompetitorEvent = gameEvent,
                CompetitorsInEventId = 1
            };

            gameEvent.Renners.Add(pick);

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "John",
                LastName = "Doe"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                CompetitorInTeamId = 1,
                CompetitorInTeam = competitorInTeam
            };

            // Bewust GEEN Result en GEEN SpecialResult

            context.Configurations.Add(configuration);
            context.ConfigurationItems.Add(normalConfigItem);
            context.Events.Add(evt);
            context.Stages.Add(stage);

            context.Competitors.Add(competitor);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(competitorInEvent);

            context.GameCompetitorsEvent.Add(gameEvent);
            context.GameCompetitorEventPicks.Add(pick);

            await context.SaveChangesAsync();

            // Act
            await repo.RecalculateEventScoresAsync(100);

            // Assert
            var stagePickScore = await context.DeelnemerStagePickScores
                .SingleAsync();

            stagePickScore.GameCompetitorEventPickId.Should().Be(1);
            stagePickScore.StageId.Should().Be(1);
            stagePickScore.Score.Should().Be(0);

            var stagePickSpecialScore = await context.DeelnemerStagePickSpecialScores
                .SingleAsync();

            stagePickSpecialScore.GameCompetitorEventPickId.Should().Be(1);
            stagePickSpecialScore.StageId.Should().Be(1);
            stagePickSpecialScore.Score.Should().Be(0);

            var stageScore = await context.DeelnemerStageScores
                .SingleAsync();

            stageScore.GameCompetitorEventId.Should().Be(1);
            stageScore.StageId.Should().Be(1);
            stageScore.Score.Should().Be(0);

            var pickScore = await context.DeelnemerPickScores
                .SingleAsync();

            pickScore.GameCompetitorEventPickId.Should().Be(1);
            pickScore.TotalScore.Should().Be(0);

            var deelnemerScore = await context.DeelnemerScores
                .SingleAsync();

            deelnemerScore.GameCompetitorEventId.Should().Be(1);
            deelnemerScore.TotalScore.Should().Be(0);
            deelnemerScore.LaatsteStageId.Should().Be(1);
            deelnemerScore.LaatsteStageScore.Should().Be(0);
        }

        [Fact]
        public async Task RecalculateEventScoresAsync_Throws_WhenEventDoesNotExist()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            Func<Task> act = () => repo.RecalculateEventScoresAsync(999);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Event 999 not found");
        }

        [Fact]
        public async Task RecalculateEventScoresAsync_Throws_WhenEventHasNoConfiguration()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            // Arrange
            var evt = new Event
            {
                EventId = 100,
                ConfigurationId = 999
            };

            context.Events.Add(evt);
            await context.SaveChangesAsync();

            // Act
            Func<Task> act = () => repo.RecalculateEventScoresAsync(100);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Event 100 has no configuration");
        }

        [Fact]
        public async Task RecalculateEventScoresAsync_Skips_Result_Without_ConfigurationItem()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            // Arrange
            var configuration = new Configuration
            {
                Id = 1
            };

            var configurationItem = new ConfigurationItem
            {
                Id = 1,
                ConfigurationId = 1,
                Position = 1,
                Score = 10,
                Configuration = configuration
            };

            configuration.ConfigurationItems = new List<ConfigurationItem>
            {
                configurationItem
            };

            var evt = new Event
            {
                EventId = 100,
                ConfigurationId = 1,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                StageName = "Etappe 1"
            };

            var gameEvent = new GameCompetitorEvent
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                UserId = "user1",
                TeamName = "Team 1"
            };

            var pick = new GameCompetitorEventPick
            {
                Id = 1,
                GameCompetitorEventId = 1,
                GameCompetitorEvent = gameEvent,
                CompetitorsInEventId = 1
            };

            gameEvent.Renners.Add(pick);

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "John",
                LastName = "Doe"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                CompetitorInTeamId = 1,
                CompetitorInTeam = competitorInTeam
            };

            // Result zonder ConfigurationItemId
            var result = new Result
            {
                Id = 1,
                StageId = 1,
                Stage = stage,
                CompetitorInEventId = 1,
                CompetitorInEvent = competitorInEvent,
                ConfigurationItemId = null
            };

            context.Configurations.Add(configuration);
            context.ConfigurationItems.Add(configurationItem);
            context.Events.Add(evt);
            context.Stages.Add(stage);

            context.Competitors.Add(competitor);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(competitorInEvent);

            context.GameCompetitorsEvent.Add(gameEvent);
            context.GameCompetitorEventPicks.Add(pick);
            context.Results.Add(result);

            await context.SaveChangesAsync();

            // Act
            await repo.RecalculateEventScoresAsync(100);

            // Assert
            var stagePickScore = await context.DeelnemerStagePickScores
                .SingleAsync();

            stagePickScore.Score.Should().Be(0);

            var stageScore = await context.DeelnemerStageScores
                .SingleAsync();

            stageScore.Score.Should().Be(0);

            var pickScore = await context.DeelnemerPickScores
                .SingleAsync();

            pickScore.TotalScore.Should().Be(0);

            var deelnemerScore = await context.DeelnemerScores
                .SingleAsync();

            deelnemerScore.TotalScore.Should().Be(0);
        }

        [Fact]
        public async Task RecalculateEventScoresAsync_Clears_ConfigurationItem_When_Position_No_Longer_Exists()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            // Arrange
            var configuration = new Configuration
            {
                Id = 1
            };

            // Huidige configuratie kent alleen positie 1.
            var currentConfigItem = new ConfigurationItem
            {
                Id = 1,
                ConfigurationId = 1,
                Position = 1,
                Score = 10,
                Configuration = configuration
            };

            configuration.ConfigurationItems = new List<ConfigurationItem>
            {
                currentConfigItem
            };

            var evt = new Event
            {
                EventId = 100,
                ConfigurationId = 1,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                StageName = "Etappe 1"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "John",
                LastName = "Doe"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                CompetitorInTeamId = 1,
                CompetitorInTeam = competitorInTeam
            };

            // Oude ConfigurationItem met positie 99.
            // Die positie bestaat niet meer in de huidige configuratie.
            var oldConfigItem = new ConfigurationItem
            {
                Id = 99,
                ConfigurationId = 999,
                Position = 99,
                Score = 5
            };

            var result = new Result
            {
                Id = 1,
                StageId = 1,
                Stage = stage,
                CompetitorInEventId = 1,
                CompetitorInEvent = competitorInEvent,
                ConfigurationItemId = 99,
                ConfigurationItem = oldConfigItem
            };

            context.Configurations.Add(configuration);
            context.ConfigurationItems.Add(currentConfigItem);
            context.ConfigurationItems.Add(oldConfigItem);

            context.Events.Add(evt);
            context.Stages.Add(stage);

            context.Competitors.Add(competitor);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(competitorInEvent);

            context.Results.Add(result);

            await context.SaveChangesAsync();

            // Act
            await repo.RecalculateEventScoresAsync(100);

            // Assert
            var updatedResult = await context.Results
                .SingleAsync(x => x.Id == 1);

            updatedResult.ConfigurationItemId.Should().BeNull();
            updatedResult.ConfigurationItem.Should().BeNull();
        }

        [Fact]
        public async Task RecalculateEventScoresAsync_Calculates_Scores_Independently_For_Multiple_Competitors()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            // Arrange
            var configuration = new Configuration
            {
                Id = 1
            };

            var configItem1 = new ConfigurationItem
            {
                Id = 1,
                ConfigurationId = 1,
                Position = 1,
                Score = 10,
                Configuration = configuration
            };

            var configItem2 = new ConfigurationItem
            {
                Id = 2,
                ConfigurationId = 1,
                Position = 2,
                Score = 8,
                Configuration = configuration
            };

            configuration.ConfigurationItems = new List<ConfigurationItem>
            {
                configItem1,
                configItem2
            };

            var evt = new Event
            {
                EventId = 100,
                ConfigurationId = 1,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                StageName = "Etappe 1"
            };

            var competitor1 = new Competitor
            {
                CompetitorId = 1,
                FirstName = "John",
                LastName = "Doe"
            };

            var competitor2 = new Competitor
            {
                CompetitorId = 2,
                FirstName = "Jane",
                LastName = "Doe"
            };

            var cit1 = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor1
            };

            var cit2 = new CompetitorInTeam
            {
                Id = 2,
                CompetitorId = 2,
                Competitor = competitor2
            };

            var cie1 = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                CompetitorInTeamId = 1,
                CompetitorInTeam = cit1
            };

            var cie2 = new CompetitorsInEvent
            {
                Id = 2,
                EventId = 100,
                Event = evt,
                CompetitorInTeamId = 2,
                CompetitorInTeam = cit2
            };

            var gameEvent1 = new GameCompetitorEvent
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                UserId = "user1",
                TeamName = "Team 1"
            };

            var gameEvent2 = new GameCompetitorEvent
            {
                Id = 2,
                EventId = 100,
                Event = evt,
                UserId = "user2",
                TeamName = "Team 2"
            };

            var pick1 = new GameCompetitorEventPick
            {
                Id = 1,
                GameCompetitorEventId = 1,
                GameCompetitorEvent = gameEvent1,
                CompetitorsInEventId = 1,
                CompetitorsInEvent = cie1
            };

            var pick2 = new GameCompetitorEventPick
            {
                Id = 2,
                GameCompetitorEventId = 2,
                GameCompetitorEvent = gameEvent2,
                CompetitorsInEventId = 2,
                CompetitorsInEvent = cie2
            };

            gameEvent1.Renners.Add(pick1);
            gameEvent2.Renners.Add(pick2);

            var result1 = new Result
            {
                Id = 1,
                StageId = 1,
                Stage = stage,
                CompetitorInEventId = 1,
                CompetitorInEvent = cie1,
                ConfigurationItemId = 1,
                ConfigurationItem = configItem1
            };

            var result2 = new Result
            {
                Id = 2,
                StageId = 1,
                Stage = stage,
                CompetitorInEventId = 2,
                CompetitorInEvent = cie2,
                ConfigurationItemId = 2,
                ConfigurationItem = configItem2
            };

            context.Configurations.Add(configuration);
            context.ConfigurationItems.AddRange(configItem1, configItem2);
            context.Events.Add(evt);
            context.Stages.Add(stage);

            context.Competitors.AddRange(competitor1, competitor2);
            context.CompetitorInTeams.AddRange(cit1, cit2);
            context.CompetitorsInEvent.AddRange(cie1, cie2);

            context.GameCompetitorsEvent.AddRange(gameEvent1, gameEvent2);
            context.GameCompetitorEventPicks.AddRange(pick1, pick2);

            context.Results.AddRange(result1, result2);

            await context.SaveChangesAsync();

            // Act
            await repo.RecalculateEventScoresAsync(100);

            // Assert
            var deelnemerScores = await context.DeelnemerScores
                .OrderBy(x => x.GameCompetitorEventId)
                .ToListAsync();

            deelnemerScores.Should().HaveCount(2);

            deelnemerScores[0].GameCompetitorEventId.Should().Be(1);
            deelnemerScores[0].TotalScore.Should().Be(10);
            deelnemerScores[0].LaatsteStageId.Should().Be(1);
            deelnemerScores[0].LaatsteStageScore.Should().Be(10);

            deelnemerScores[1].GameCompetitorEventId.Should().Be(2);
            deelnemerScores[1].TotalScore.Should().Be(8);
            deelnemerScores[1].LaatsteStageId.Should().Be(1);
            deelnemerScores[1].LaatsteStageScore.Should().Be(8);

            var pickScores = await context.DeelnemerPickScores
                .OrderBy(x => x.GameCompetitorEventPickId)
                .ToListAsync();

            pickScores.Should().HaveCount(2);

            pickScores[0].GameCompetitorEventPickId.Should().Be(1);
            pickScores[0].TotalScore.Should().Be(10);

            pickScores[1].GameCompetitorEventPickId.Should().Be(2);
            pickScores[1].TotalScore.Should().Be(8);
        }

        [Fact]
        public async Task RecalculateEventScoresAsync_Handles_Stage_With_No_Results()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            // Arrange
            var configuration = new Configuration
            {
                Id = 1
            };

            var configItem = new ConfigurationItem
            {
                Id = 1,
                ConfigurationId = 1,
                Position = 1,
                Score = 10,
                Configuration = configuration
            };

            configuration.ConfigurationItems = new List<ConfigurationItem>
            {
                configItem
            };

            var evt = new Event
            {
                EventId = 100,
                ConfigurationId = 1,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                StageName = "Etappe zonder uitslag"
            };

            var gameEvent = new GameCompetitorEvent
            {
                Id = 1,
                EventId = 100,
                Event = evt,
                UserId = "user1",
                TeamName = "Team 1"
            };

            var pick = new GameCompetitorEventPick
            {
                Id = 1,
                GameCompetitorEventId = 1,
                GameCompetitorEvent = gameEvent,
                CompetitorsInEventId = 1
            };

            gameEvent.Renners.Add(pick);

            context.Configurations.Add(configuration);
            context.ConfigurationItems.Add(configItem);
            context.Events.Add(evt);
            context.Stages.Add(stage);

            context.GameCompetitorsEvent.Add(gameEvent);
            context.GameCompetitorEventPicks.Add(pick);

            await context.SaveChangesAsync();

            // Act
            await repo.RecalculateEventScoresAsync(100);

            // Assert
            var stagePickScore = await context.DeelnemerStagePickScores
                .SingleAsync();

            stagePickScore.StageId.Should().Be(1);
            stagePickScore.GameCompetitorEventPickId.Should().Be(1);
            stagePickScore.Score.Should().Be(0);

            var stagePickSpecialScore = await context.DeelnemerStagePickSpecialScores
                .SingleAsync();

            stagePickSpecialScore.StageId.Should().Be(1);
            stagePickSpecialScore.GameCompetitorEventPickId.Should().Be(1);
            stagePickSpecialScore.Score.Should().Be(0);

            var stageScore = await context.DeelnemerStageScores
                .SingleAsync();

            stageScore.StageId.Should().Be(1);
            stageScore.GameCompetitorEventId.Should().Be(1);
            stageScore.Score.Should().Be(0);

            var pickScore = await context.DeelnemerPickScores
                .SingleAsync();

            pickScore.TotalScore.Should().Be(0);

            var deelnemerScore = await context.DeelnemerScores
                .SingleAsync();

            deelnemerScore.TotalScore.Should().Be(0);
            deelnemerScore.LaatsteStageId.Should().Be(1);
            deelnemerScore.LaatsteStageScore.Should().Be(0);
        }
        #endregion

        #region ResultExistsAsync Tests
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

        #endregion

        #region SyncResultsAsync Tests
        [Fact]
        public async Task SyncResultsAsync_Adds_New_Results_And_SpecialResults()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var stage = new Stage
            {
                Id = 1,
                EventId = 100
            };

            await context.Stages.AddAsync(stage);
            await context.SaveChangesAsync();

            var result = new Result
            {
                Id = 1,
                StageId = 1,
                CompetitorInEventId = 10
            };

            var specialResult = new SpecialResult
            {
                Id = 1,
                StageId = 1,
                CompetitorInEventId = 10,
                SpecialId = 20
            };

            await repo.SyncResultsAsync(
                stageId: 1,
                results: new[] { result },
                specialResults: new[] { specialResult });

            var savedResult = await context.Results
                .SingleAsync();

            savedResult.CompetitorInEventId.Should().Be(10);
            savedResult.StageId.Should().Be(1);

            var savedSpecialResult = await context.SpecialResults
                .SingleAsync();

            savedSpecialResult.CompetitorInEventId.Should().Be(10);
            savedSpecialResult.StageId.Should().Be(1);
            savedSpecialResult.SpecialId.Should().Be(20);
        }

        [Fact]
        public async Task SyncResultsAsync_Removes_Results_And_SpecialResults_No_Longer_Present()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var stage = new Stage
            {
                Id = 1,
                EventId = 100
            };

            var existingResult = new Result
            {
                Id = 1,
                StageId = 1,
                CompetitorInEventId = 10
            };

            var existingSpecialResult = new SpecialResult
            {
                Id = 1,
                StageId = 1,
                CompetitorInEventId = 10,
                SpecialId = 20
            };

            context.Stages.Add(stage);
            context.Results.Add(existingResult);
            context.SpecialResults.Add(existingSpecialResult);

            await context.SaveChangesAsync();

            // Nieuwe uitslag bevat deze resultaten niet meer.
            await repo.SyncResultsAsync(
                stageId: 1,
                results: Array.Empty<Result>(),
                specialResults: Array.Empty<SpecialResult>());

            (await context.Results.AnyAsync())
                .Should().BeFalse();

            (await context.SpecialResults.AnyAsync())
                .Should().BeFalse();
        }

        [Fact]
        public async Task SyncResultsAsync_Keeps_Existing_Results_And_Adds_New_Results()
        {
            using var context = CreateContext();
            var repo = new ResultsRepository(context);

            var stage = new Stage
            {
                Id = 1,
                EventId = 100
            };

            var existingResult = new Result
            {
                Id = 1,
                StageId = 1,
                CompetitorInEventId = 10
            };

            var existingSpecialResult = new SpecialResult
            {
                Id = 1,
                StageId = 1,
                CompetitorInEventId = 10,
                SpecialId = 20
            };

            context.Stages.Add(stage);
            context.Results.Add(existingResult);
            context.SpecialResults.Add(existingSpecialResult);

            await context.SaveChangesAsync();

            var newResult = new Result
            {
                Id = 2,
                StageId = 1,
                CompetitorInEventId = 11
            };

            var newSpecialResult = new SpecialResult
            {
                Id = 2,
                StageId = 1,
                CompetitorInEventId = 11,
                SpecialId = 21
            };

            await repo.SyncResultsAsync(
                stageId: 1,
                results: new[] { existingResult, newResult },
                specialResults: new[] { existingSpecialResult, newSpecialResult });

            var results = await context.Results
                .OrderBy(x => x.Id)
                .ToListAsync();

            results.Should().HaveCount(2);
            results.Select(x => x.CompetitorInEventId)
                .Should().BeEquivalentTo(new[] { 10, 11 });

            var specialResults = await context.SpecialResults
                .OrderBy(x => x.Id)
                .ToListAsync();

            specialResults.Should().HaveCount(2);
            specialResults.Select(x => x.SpecialId)
                .Should().BeEquivalentTo(new[] { 20, 21 });
        }
        #endregion
    }
}
