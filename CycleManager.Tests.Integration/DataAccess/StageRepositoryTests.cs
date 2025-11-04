using CycleManager.Domain.Models;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Tests.Integration.DataAccess
{
    public class StageRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public StageRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(_options);

        [Fact]
        public async Task GetByEventId_ReturnsStagesOrderedByStageOrder()
        {
            using var context = CreateContext();
            var repo = new StageRepository(context);

            var event1 = new Event { EventId = 1 };
            var stages = new[]
            {
                new Stage { Id = 1, EventId = 1, StageName = "1", StageOrder = 2, Event = event1 },
                new Stage { Id = 2, EventId = 1, StageName = "2", StageOrder = 1, Event = event1 },
                new Stage { Id = 3, EventId = 2, StageName = "X", StageOrder = 3 } // andere event
            };

            context.Events.Add(event1);
            context.Stages.AddRange(stages);
            await context.SaveChangesAsync();

            var result = await repo.GetByEventId(1);

            result.Should().HaveCount(2);
            result.First().StageOrder.Should().Be(1);
            result.Last().StageOrder.Should().Be(2);
        }

        [Fact]
        public async Task GetStageId_ReturnsStageId_WhenExists()
        {
            using var context = CreateContext();
            var repo = new StageRepository(context);

            var evt = new Event { EventId = 1 };
            var stage = new Stage { Id = 42, EventId = 1, StageName = "3", Event = evt };

            context.Events.Add(evt);
            context.Stages.Add(stage);
            await context.SaveChangesAsync();

            var id = await repo.GetStageId(3, 1);

            id.Should().Be(42);
        }

        [Fact]
        public async Task GetStageId_ReturnsZero_WhenStageNotFound()
        {
            using var context = CreateContext();
            var repo = new StageRepository(context);

            var result = await repo.GetStageId(99, 5);

            result.Should().Be(0);
        }

        [Fact]
        public async Task GetStageNumber_ReturnsCorrectStageNumber()
        {
            using var context = CreateContext();
            var repo = new StageRepository(context);

            var evt = new Event { EventId = 1 };
            var date = new DateTime(2025, 6, 15);

            var stage = new Stage
            {
                Id = 1,
                EventId = 1,
                StageDate = date,
                StageName = "5"
            };

            context.Events.Add(evt);
            context.Stages.Add(stage);
            await context.SaveChangesAsync();

            var number = await repo.GetStageNumber(date, 1);

            number.Should().Be(5);
        }

        [Fact]
        public async Task GetStageNumber_ReturnsZero_WhenNoStageFound()
        {
            using var context = CreateContext();
            var repo = new StageRepository(context);

            var result = await repo.GetStageNumber(DateTime.Now, 999);

            result.Should().Be(0);
        }

        [Fact]
        public async Task GetStageNumber_ReturnsZero_WhenStageNameIsNotParsable()
        {
            using var context = CreateContext();
            var repo = new StageRepository(context);

            var evt = new Event { EventId = 1 };
            var stage = new Stage
            {
                Id = 1,
                EventId = 1,
                StageDate = DateTime.Today,
                StageName = "Proloog"
            };

            context.Events.Add(evt);
            context.Stages.Add(stage);
            await context.SaveChangesAsync();

            var result = await repo.GetStageNumber(DateTime.Today, 1);

            result.Should().Be(0);
        }

        [Fact]
        public async Task GetStagesResults_ReturnsCorrectCount()
        {
            using var context = CreateContext();
            var repo = new StageRepository(context);

            context.ScrapedStageResults.AddRange(
                new ScrapedStageResult { Id = 1, StageId = 10, EventId = 5 },
                new ScrapedStageResult { Id = 2, StageId = 10, EventId = 5 },
                new ScrapedStageResult { Id = 3, StageId = 11, EventId = 5 }
            );
            await context.SaveChangesAsync();

            var count = await repo.GetStagesResults(10, 5);

            count.Should().Be(2);
        }

        [Fact]
        public async Task GetStagesResults_ReturnsZero_WhenNoResults()
        {
            using var context = CreateContext();
            var repo = new StageRepository(context);

            var result = await repo.GetStagesResults(99, 1);

            result.Should().Be(0);
        }

        [Fact]
        public async Task GetStage_ReturnsStage_WhenExists()
        {
            using var context = CreateContext();
            var repo = new StageRepository(context);

            var stage = new Stage { Id = 1, EventId = 1, StageName = "2" };
            context.Stages.Add(stage);
            await context.SaveChangesAsync();

            var result = await repo.GetStage(2, 1);

            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task GetStage_ReturnsNewStage_WhenNotFound()
        {
            using var context = CreateContext();
            var repo = new StageRepository(context);

            var result = await repo.GetStage(5, 99);

            result.Should().NotBeNull();
            result.Id.Should().Be(0);
        }

        [Fact]
        public async Task GetStageById_ReturnsStageWithEvent()
        {
            using var context = CreateContext();
            var repo = new StageRepository(context);

            var evt = new Event { EventId = 1, EventName = "Tour de Test" };
            var stage = new Stage { Id = 123, EventId = 1, Event = evt };

            context.Events.Add(evt);
            context.Stages.Add(stage);
            await context.SaveChangesAsync();

            var result = await repo.GetStageById(123);

            result.Should().NotBeNull();
            result.Event.Should().NotBeNull();
            result.Event.EventName.Should().Be("Tour de Test");
        }

        [Fact]
        public async Task GetStageById_ReturnsNewStage_WhenNotFound()
        {
            using var context = CreateContext();
            var repo = new StageRepository(context);

            var result = await repo.GetStageById(999);

            result.Should().NotBeNull();
            result.Id.Should().Be(0);
        }
    }
}
