using CycleManager.Domain.Models;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Tests.Integration.DataAccess
{
    public class SpecialResultsRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public SpecialResultsRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(_options);

        [Fact]
        public async Task GetByIdAsync_ReturnsSpecialResultWithIncludes()
        {
            using var context = CreateContext();
            var repo = new SpecialResultsRepository(context);

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "John",
                LastName = "Doe"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = competitor.CompetitorId,
                Competitor = competitor
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 1,
                CompetitorInTeamId = competitorInTeam.Id,
                CompetitorInTeam = competitorInTeam
            };

            var stage = new Stage
            {
                Id = 1,
                StageName = "Stage 1",
                EventId = 1
            };

            var special = new ConfigurationItemSpecial
            {
                Id = 1,
                Score = 10
            };

            var specialResult = new SpecialResult
            {
                Id = 1,
                CompetitorInEventId = competitorInEvent.Id,
                CompetitorInEvent = competitorInEvent,
                StageId = stage.Id,
                Stage = stage,
                SpecialId = special.Id,
                Special = special
            };

            context.Competitors.Add(competitor);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(competitorInEvent);
            context.Stages.Add(stage);
            context.ConfigurationItemSpecials.Add(special);
            context.SpecialResults.Add(specialResult);

            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);

            result.Special.Should().NotBeNull();
            result.Special!.Id.Should().Be(1);

            result.Stage.Should().NotBeNull();
            result.Stage!.Id.Should().Be(1);

            result.CompetitorInEvent.Should().NotBeNull();
            result.CompetitorInEvent!.CompetitorInTeam.Should().NotBeNull();
            result.CompetitorInEvent.CompetitorInTeam!.Competitor.Should().NotBeNull();
            result.CompetitorInEvent.CompetitorInTeam.Competitor!.CompetitorId.Should().Be(1);
        }

        [Fact]
        public async Task DeleteAsync_DoesNothing_WhenResultDoesNotExist()
        {
            using var context = CreateContext();
            var repo = new SpecialResultsRepository(context);

            Func<Task> act = async () => await repo.DeleteAsync(999);

            await act.Should().NotThrowAsync();
            (await context.SpecialResults.FindAsync(999)).Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_RemovesExistingResult()
        {
            using var context = CreateContext();
            var repo = new SpecialResultsRepository(context);

            var result = new SpecialResult
            {
                Id = 1,
                StageId = 1,
                CompetitorInEventId = 1,
                SpecialId = 1
            };

            context.SpecialResults.Add(result);
            await context.SaveChangesAsync();

            await repo.DeleteAsync(1);

            var deleted = await context.SpecialResults.FindAsync(1);

            deleted.Should().BeNull();
        }

        [Fact]
        public async Task GetByEventId_ReturnsResultsForEvent()
        {
            using var context = CreateContext();
            var repo = new SpecialResultsRepository(context);

            var configuration = new Configuration
            {
                Id = 1
            };

            var evt = new Event
            {
                EventId = 1,
                ConfigurationId = 1,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                Event = evt,
                EventId = 1,
                NoScore = false
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen"
            };

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "TeamTest"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2024
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                SeasonYearId = 1,
                Team = team,
                SeasonYear = seasonYear,
                Name = "TeamTest"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor,
                TeamYearId = 1,
                TeamYear = teamYear
            };

            var cie = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 1,
                Event = evt,
                CompetitorInTeamId = 1,
                CompetitorInTeam = competitorInTeam
            };

            var special = new ConfigurationItemSpecial
            {
                Id = 1,
                ConfigurationId = 1,
                Score = 10
            };

            var specialResult = new SpecialResult
            {
                Id = 1,
                Stage = stage,
                StageId = stage.Id,
                CompetitorInEvent = cie,
                CompetitorInEventId = cie.Id,
                Special = special,
                SpecialId = special.Id
            };

            context.Events.Add(evt);
            context.Stages.Add(stage);
            context.Competitors.Add(competitor);
            context.Teams.Add(team);
            context.SeasonYears.Add(seasonYear);
            context.TeamYear.Add(teamYear);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(cie);
            context.ConfigurationItemSpecials.Add(special);
            context.SpecialResults.Add(specialResult);

            await context.SaveChangesAsync();

            var results = (await repo.GetByEventId(1)).ToList();

            results.Should().ContainSingle();
            results[0].Id.Should().Be(1);
            results[0].Stage.Should().NotBeNull();
            results[0].Stage.EventId.Should().Be(1);
            results[0].Special.Should().NotBeNull();
            results[0].Special.Id.Should().Be(1);
        }

        [Fact]
        public async Task GetByEventId_DoesNotReturnResultsFromOtherEvents()
        {
            using var context = CreateContext();
            var repo = new SpecialResultsRepository(context);

            var configuration = new Configuration
            {
                Id = 1
            };

            var evt = new Event
            {
                EventId = 1,
                ConfigurationId = 1,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                Event = evt,
                EventId = 1,
                NoScore = false
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen"
            };

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "TeamTest"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2024
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                SeasonYearId = 1,
                Team = team,
                SeasonYear = seasonYear,
                Name = "TeamTest"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor,
                TeamYearId = 1,
                TeamYear = teamYear
            };

            var cie = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 1,
                Event = evt,
                CompetitorInTeamId = 1,
                CompetitorInTeam = competitorInTeam
            };

            var special = new ConfigurationItemSpecial
            {
                Id = 1,
                ConfigurationId = 1,
                Score = 10
            };

            var specialResult = new SpecialResult
            {
                Id = 1,
                Stage = stage,
                StageId = stage.Id,
                CompetitorInEvent = cie,
                CompetitorInEventId = cie.Id,
                Special = special,
                SpecialId = special.Id
            };

            // Tweede event
            var configuration2 = new Configuration
            {
                Id = 2
            };

            var evt2 = new Event
            {
                EventId = 2,
                ConfigurationId = 2,
                Configuration = configuration2
            };

            var stage2 = new Stage
            {
                Id = 2,
                Event = evt2,
                EventId = 2,
                NoScore = false
            };

            var special2 = new ConfigurationItemSpecial
            {
                Id = 2,
                ConfigurationId = 2,
                Score = 8
            };

            var specialResult2 = new SpecialResult
            {
                Id = 2,
                Stage = stage2,
                StageId = stage2.Id,
                CompetitorInEvent = cie,
                CompetitorInEventId = cie.Id,
                Special = special2,
                SpecialId = special2.Id
            };

            context.Events.AddRange(evt, evt2);
            context.Stages.AddRange(stage, stage2);
            context.Competitors.Add(competitor);
            context.Teams.Add(team);
            context.SeasonYears.Add(seasonYear);
            context.TeamYear.Add(teamYear);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(cie);
            context.ConfigurationItemSpecials.AddRange(special, special2);
            context.SpecialResults.AddRange(specialResult, specialResult2);

            await context.SaveChangesAsync();

            var results = (await repo.GetByEventId(1)).ToList();

            results.Should().ContainSingle();
            results[0].Id.Should().Be(1);
            results[0].Stage.Should().NotBeNull();
            results[0].Stage.EventId.Should().Be(1);
        }

        [Fact]
        public async Task GetByStageAsync_ReturnsResultsForStage()
        {
            using var context = CreateContext();
            var repo = new SpecialResultsRepository(context);

            var configuration = new Configuration
            {
                Id = 1
            };

            var evt = new Event
            {
                EventId = 1,
                ConfigurationId = 1,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                Event = evt,
                EventId = 1,
                NoScore = false
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen"
            };

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "TeamTest"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2024
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                SeasonYearId = 1,
                Team = team,
                SeasonYear = seasonYear,
                Name = "TeamTest"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor,
                TeamYearId = 1,
                TeamYear = teamYear
            };

            var cie = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 1,
                Event = evt,
                CompetitorInTeamId = 1,
                CompetitorInTeam = competitorInTeam
            };

            var special = new ConfigurationItemSpecial
            {
                Id = 1,
                ConfigurationId = 1,
                Score = 10
            };

            var specialResult = new SpecialResult
            {
                Id = 1,
                Stage = stage,
                StageId = stage.Id,
                CompetitorInEvent = cie,
                CompetitorInEventId = cie.Id,
                Special = special,
                SpecialId = special.Id
            };

            context.Events.Add(evt);
            context.Stages.Add(stage);
            context.Competitors.Add(competitor);
            context.Teams.Add(team);
            context.SeasonYears.Add(seasonYear);
            context.TeamYear.Add(teamYear);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(cie);
            context.ConfigurationItemSpecials.Add(special);
            context.SpecialResults.Add(specialResult);

            await context.SaveChangesAsync();

            var results = await repo.GetByStageAsync(1);

            results.Should().ContainSingle();
            results[0].Id.Should().Be(1);
            results[0].Stage.Should().NotBeNull();
            results[0].Stage.Id.Should().Be(1);
            results[0].Special.Should().NotBeNull();
            results[0].Special.Id.Should().Be(1);
        }

        [Fact]
        public async Task GetByStageAsync_DoesNotReturnResultsFromOtherStages()
        {
            using var context = CreateContext();
            var repo = new SpecialResultsRepository(context);

            var configuration = new Configuration
            {
                Id = 1
            };

            var evt = new Event
            {
                EventId = 1,
                ConfigurationId = 1,
                Configuration = configuration
            };

            var stage1 = new Stage
            {
                Id = 1,
                Event = evt,
                EventId = 1,
                NoScore = false
            };

            var stage2 = new Stage
            {
                Id = 2,
                Event = evt,
                EventId = 1,
                NoScore = false
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen"
            };

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "TeamTest"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2024
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                SeasonYearId = 1,
                Team = team,
                SeasonYear = seasonYear,
                Name = "TeamTest"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor,
                TeamYearId = 1,
                TeamYear = teamYear
            };

            var cie = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 1,
                Event = evt,
                CompetitorInTeamId = 1,
                CompetitorInTeam = competitorInTeam
            };

            var special = new ConfigurationItemSpecial
            {
                Id = 1,
                ConfigurationId = 1,
                Score = 10
            };

            var result1 = new SpecialResult
            {
                Id = 1,
                Stage = stage1,
                StageId = stage1.Id,
                CompetitorInEvent = cie,
                CompetitorInEventId = cie.Id,
                Special = special,
                SpecialId = special.Id
            };

            var result2 = new SpecialResult
            {
                Id = 2,
                Stage = stage2,
                StageId = stage2.Id,
                CompetitorInEvent = cie,
                CompetitorInEventId = cie.Id,
                Special = special,
                SpecialId = special.Id
            };

            context.Events.Add(evt);
            context.Stages.AddRange(stage1, stage2);
            context.Competitors.Add(competitor);
            context.Teams.Add(team);
            context.SeasonYears.Add(seasonYear);
            context.TeamYear.Add(teamYear);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.CompetitorsInEvent.Add(cie);
            context.ConfigurationItemSpecials.Add(special);
            context.SpecialResults.AddRange(result1, result2);

            await context.SaveChangesAsync();

            var results = await repo.GetByStageAsync(1);

            results.Should().ContainSingle();
            results[0].Id.Should().Be(1);
            results[0].Stage.Id.Should().Be(1);
        }

        [Fact]
        public async Task AddResultsAsync_AddsResults()
        {
            using var context = CreateContext();
            var repo = new SpecialResultsRepository(context);

            var results = new[]
            {
                new SpecialResult
                {
                    Id = 1,
                    StageId = 1,
                    CompetitorInEventId = 1,
                    SpecialId = 1
                },
                new SpecialResult
                {
                    Id = 2,
                    StageId = 1,
                    CompetitorInEventId = 2,
                    SpecialId = 2
                }
            };

            await repo.AddResultsAsync(results);

            var savedResults = await context.SpecialResults.ToListAsync();

            savedResults.Should().HaveCount(2);
            savedResults.Should().Contain(x => x.Id == 1);
            savedResults.Should().Contain(x => x.Id == 2);
        }
    }
}