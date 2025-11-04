using CycleManager.Domain.Models;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Tests.Integration.DataAccess
{
    public class ScoreRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public ScoreRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(_options);

        [Fact]
        public async Task GetScoresByEventIdAsync_ReturnsScoresForSpecificEvent()
        {
            using var context = CreateContext();
            var repo = new ScoreRepository(context);

            var stage1 = new Stage { Id = 1, EventId = 10 };
            var stage2 = new Stage { Id = 2, EventId = 20 };

            var score1 = new DeelnemerScore { Id = Guid.NewGuid(), Stage = stage1, StageId = stage1.Id };
            var score2 = new DeelnemerScore { Id = Guid.NewGuid(), Stage = stage2, StageId = stage2.Id };

            context.Stages.AddRange(stage1, stage2);
            context.DeelnemerScores.AddRange(score1, score2);
            await context.SaveChangesAsync();

            var results = await repo.GetScoresByEventIdAsync(10);

            results.Should().HaveCount(1);
            results.First().Stage.EventId.Should().Be(10);
        }

        [Fact]
        public async Task GetScoresByEventIdAsync_ReturnsEmpty_WhenNoScoresExist()
        {
            using var context = CreateContext();
            var repo = new ScoreRepository(context);

            var results = await repo.GetScoresByEventIdAsync(999);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPoolRankingForStage_ReturnsCorrectRanking()
        {
            using var context = CreateContext();
            var repo = new ScoreRepository(context);

            var evt = new Event { EventId = 1 };
            var stage1 = new Stage { Id = 1, EventId = 1, StageOrder = 1 };
            var stage2 = new Stage { Id = 2, EventId = 1, StageOrder = 2 };
            var user = new ApplicationUser { Id = "u1", FirstName = "Alice", LastName = "Smith" };
            var gameEvent = new GameCompetitorEvent { Id = 1, EventId = 1, TeamName = "DreamTeam", User = user };

            context.Events.Add(evt);
            context.Stages.AddRange(stage1, stage2);
            context.Users.Add(user);
            context.GameCompetitorsEvent.Add(gameEvent);

            context.DeelnemerScores.AddRange(
                new DeelnemerScore { Id = Guid.NewGuid(), Stage = stage1, StageId = 1, GameCompetitorEvent = gameEvent, GameCompetitorEventId = 1, TotalScore = 10 },
                new DeelnemerScore { Id = Guid.NewGuid(), Stage = stage2, StageId = 2, GameCompetitorEvent = gameEvent, GameCompetitorEventId = 1, TotalScore = 15 }
            );
            await context.SaveChangesAsync();

            var ranking = await repo.GetPoolRankingForStage(1, 2);

            ranking.Should().HaveCount(1);
            var deelnemer = ranking.First();
            deelnemer.DeelnemerNaam.Should().Be("Alice Smith");
            deelnemer.PoolNaam.Should().Be("DreamTeam");
            deelnemer.Punten.Should().Be(25);
        }

        [Fact]
        public async Task GetPoolRankingForStage_ThrowsException_WhenStageNotFound()
        {
            using var context = CreateContext();
            var repo = new ScoreRepository(context);

            Func<Task> act = async () => await repo.GetPoolRankingForStage(99, 1);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*bestaat niet voor event*");
        }

        [Fact]
        public async Task GetPoolRankingForStage_IgnoresScoresFromLaterStages()
        {
            using var context = CreateContext();
            var repo = new ScoreRepository(context);

            var evt = new Event { EventId = 1 };
            var stage1 = new Stage { Id = 1, EventId = 1, StageOrder = 1 };
            var stage2 = new Stage { Id = 2, EventId = 1, StageOrder = 2 };
            var stage3 = new Stage { Id = 3, EventId = 1, StageOrder = 3 };
            var user = new ApplicationUser { Id = "u1", FirstName = "Bob", LastName = "Jones" };
            var gameEvent = new GameCompetitorEvent { Id = 1, EventId = 1, TeamName = "FastRiders", User = user };

            context.Events.Add(evt);
            context.Stages.AddRange(stage1, stage2, stage3);
            context.Users.Add(user);
            context.GameCompetitorsEvent.Add(gameEvent);

            context.DeelnemerScores.AddRange(
                new DeelnemerScore { Id = Guid.NewGuid(), Stage = stage1, StageId = 1, GameCompetitorEvent = gameEvent, TotalScore = 5 },
                new DeelnemerScore { Id = Guid.NewGuid(), Stage = stage2, StageId = 2, GameCompetitorEvent = gameEvent, TotalScore = 5 },
                new DeelnemerScore { Id = Guid.NewGuid(), Stage = stage3, StageId = 3, GameCompetitorEvent = gameEvent, TotalScore = 5 }
            );
            await context.SaveChangesAsync();

            var ranking = await repo.GetPoolRankingForStage(1, 2);

            ranking.Should().HaveCount(1);
            ranking.First().Punten.Should().Be(10);
        }

        [Fact]
        public async Task GetPoolRankingForStage_ReturnsEmpty_WhenNoScoresExist()
        {
            using var context = CreateContext();
            var repo = new ScoreRepository(context);

            var evt = new Event { EventId = 1 };
            var stage = new Stage { Id = 1, EventId = 1, StageOrder = 1 };
            context.Events.Add(evt);
            context.Stages.Add(stage);
            await context.SaveChangesAsync();

            var ranking = await repo.GetPoolRankingForStage(1, 1);

            ranking.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPoolRankingForStage_HandlesMultipleParticipantsWithSameScore()
        {
            using var context = CreateContext();
            var repo = new ScoreRepository(context);

            var evt = new Event { EventId = 1 };
            var stage = new Stage { Id = 1, EventId = 1, StageOrder = 1 };
            var user1 = new ApplicationUser { Id = "u1", FirstName = "Tom", LastName = "White" };
            var user2 = new ApplicationUser { Id = "u2", FirstName = "Eva", LastName = "Green" };
            var g1 = new GameCompetitorEvent { Id = 1, EventId = 1, TeamName = "Alpha", User = user1 };
            var g2 = new GameCompetitorEvent { Id = 2, EventId = 1, TeamName = "Beta", User = user2 };

            context.Events.Add(evt);
            context.Stages.Add(stage);
            context.Users.AddRange(user1, user2);
            context.GameCompetitorsEvent.AddRange(g1, g2);

            context.DeelnemerScores.AddRange(
                new DeelnemerScore { Id = Guid.NewGuid(), Stage = stage, StageId = 1, GameCompetitorEvent = g1, TotalScore = 10 },
                new DeelnemerScore { Id = Guid.NewGuid(), Stage = stage, StageId = 1, GameCompetitorEvent = g2, TotalScore = 10 }
            );
            await context.SaveChangesAsync();

            var ranking = await repo.GetPoolRankingForStage(1, 1);

            ranking.Should().HaveCount(2);
            ranking.All(r => r.Punten == 10).Should().BeTrue();
        }

        [Fact]
        public async Task GetPoolRankingForStage_UserIsNull_SetsEmptyNames()
        {
            using var context = CreateContext();
            var repo = new ScoreRepository(context);

            var evt = new Event { EventId = 1 };
            var stage = new Stage { Id = 1, EventId = 1, StageOrder = 1 };
            var g1 = new GameCompetitorEvent { Id = 1, EventId = 1, TeamName = "GhostTeam", User = null };

            context.Events.Add(evt);
            context.Stages.Add(stage);
            context.GameCompetitorsEvent.Add(g1);

            context.DeelnemerScores.Add(
                new DeelnemerScore { Id = Guid.NewGuid(), Stage = stage, StageId = 1, GameCompetitorEvent = g1, TotalScore = 7 }
            );
            await context.SaveChangesAsync();

            var ranking = await repo.GetPoolRankingForStage(1, 1);

            ranking.Should().HaveCount(1);
            ranking.First().DeelnemerNaam.Should().Be(" ");
            ranking.First().PoolNaam.Should().Be("GhostTeam");
        }

        [Fact]
        public async Task GetPoolRankingForStage_ReturnsScoresForCorrectEventOnly()
        {
            using var context = CreateContext();
            var repo = new ScoreRepository(context);

            var evt1 = new Event { EventId = 1 };
            var evt2 = new Event { EventId = 2 };
            var stage1 = new Stage { Id = 1, EventId = 1, StageOrder = 1 };
            var stage2 = new Stage { Id = 2, EventId = 2, StageOrder = 1 };
            var user = new ApplicationUser { Id = "u1", FirstName = "Max", LastName = "Brown" };
            var g1 = new GameCompetitorEvent { Id = 1, EventId = 1, TeamName = "TeamA", User = user };
            var g2 = new GameCompetitorEvent { Id = 2, EventId = 2, TeamName = "TeamB", User = user };

            context.Events.AddRange(evt1, evt2);
            context.Stages.AddRange(stage1, stage2);
            context.Users.Add(user);
            context.GameCompetitorsEvent.AddRange(g1, g2);

            context.DeelnemerScores.AddRange(
                new DeelnemerScore { Id = Guid.NewGuid(), Stage = stage1, GameCompetitorEvent = g1, TotalScore = 8 },
                new DeelnemerScore { Id = Guid.NewGuid(), Stage = stage2, GameCompetitorEvent = g2, TotalScore = 99 }
            );
            await context.SaveChangesAsync();

            var ranking = await repo.GetPoolRankingForStage(1, 1);

            ranking.Should().HaveCount(1);
            ranking.First().PoolNaam.Should().Be("TeamA");
            ranking.First().Punten.Should().Be(8);
        }
    }
}
