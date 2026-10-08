using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Tests.Integration.DataAccess
{
    public class ConfigurationItemSpecialRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public ConfigurationItemSpecialRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(_options);

        [Fact]
        public async Task CreateItemSpecial_AddsItem_WhenItemDoesNotExist()
        {
            using var context = CreateContext();
            var repo = new ConfigurationItemSpecialRepository(context);

            var item = new ConfigurationItemSpecial
            {
                Id = 1,
                ConfigurationId = 1,
                Question = Domain.Enums.QuestionType.KOM,
                Score = 10
            };

            var result = await repo.CreateItemSpecial(item);

            result.Should().BeTrue();

            var saved = await context.ConfigurationItemSpecials
                .SingleOrDefaultAsync(x => x.Id == 1);

            saved.Should().NotBeNull();
            saved!.ConfigurationId.Should().Be(1);
            saved.Question.Should().Be(Domain.Enums.QuestionType.KOM);
            saved.Score.Should().Be(10);
        }

        [Fact]
        public async Task CreateItemSpecial_ReturnsFalse_WhenItemAlreadyExists()
        {
            using var context = CreateContext();
            var repo = new ConfigurationItemSpecialRepository(context);

            context.ConfigurationItemSpecials.Add(new ConfigurationItemSpecial
            {
                Id = 1,
                ConfigurationId = 1,
                Question = Domain.Enums.QuestionType.KOM,
                Score = 10
            });

            await context.SaveChangesAsync();

            var item = new ConfigurationItemSpecial
            {
                Id = 2,
                ConfigurationId = 1,
                Question = Domain.Enums.QuestionType.KOM,
                Score = 8
            };

            var result = await repo.CreateItemSpecial(item);

            result.Should().BeFalse();

            var count = await context.ConfigurationItemSpecials.CountAsync();
            count.Should().Be(1);
        }

        [Fact]
        public async Task CreateItemSpecial_AllowsSameQuestionForDifferentConfiguration()
        {
            using var context = CreateContext();
            var repo = new ConfigurationItemSpecialRepository(context);

            context.ConfigurationItemSpecials.Add(new ConfigurationItemSpecial
            {
                Id = 1,
                ConfigurationId = 1,
                Question = Domain.Enums.QuestionType.KOM,
                Score = 10
            });

            await context.SaveChangesAsync();

            var item = new ConfigurationItemSpecial
            {
                Id = 2,
                ConfigurationId = 2,
                Question = Domain.Enums.QuestionType.KOM,
                Score = 8
            };

            var result = await repo.CreateItemSpecial(item);

            result.Should().BeTrue();

            var count = await context.ConfigurationItemSpecials.CountAsync();
            count.Should().Be(2);
        }

        [Fact]
        public async Task UpdateItemSpecial_UpdatesItem_WhenNoDuplicateExists()
        {
            using var context = CreateContext();
            var repo = new ConfigurationItemSpecialRepository(context);

            context.ConfigurationItemSpecials.Add(new ConfigurationItemSpecial
            {
                Id = 1,
                ConfigurationId = 1,
                Question = Domain.Enums.QuestionType.KOM,
                Score = 10
            });

            await context.SaveChangesAsync();

            var item = await context.ConfigurationItemSpecials.FindAsync(1);
            item.Should().NotBeNull();

            item!.Question = Domain.Enums.QuestionType.Points;
            item.Score = 20;

            var result = await repo.UpdateItemSpecial(item);

            result.Should().BeTrue();

            var updated = await context.ConfigurationItemSpecials
                .SingleAsync(x => x.Id == 1);

            updated.Question.Should().Be(Domain.Enums.QuestionType.Points);
            updated.Score.Should().Be(20);
        }

        [Fact]
        public async Task UpdateItemSpecial_ReturnsFalse_WhenAnotherItemHasSameQuestion()
        {
            using var context = CreateContext();
            var repo = new ConfigurationItemSpecialRepository(context);

            context.ConfigurationItemSpecials.AddRange(
                new ConfigurationItemSpecial
                {
                    Id = 1,
                    ConfigurationId = 1,
                    Question = Domain.Enums.QuestionType.KOM,
                    Score = 10
                },
                new ConfigurationItemSpecial
                {
                    Id = 2,
                    ConfigurationId = 1,
                    Question = Domain.Enums.QuestionType.Points,
                    Score = 8
                });

            await context.SaveChangesAsync();

            var item = await context.ConfigurationItemSpecials.FindAsync(2);
            item.Should().NotBeNull();

            item!.Question = Domain.Enums.QuestionType.KOM;

            var result = await repo.UpdateItemSpecial(item);

            result.Should().BeFalse();

            using var verificationContext = CreateContext();

            var unchanged = await verificationContext.ConfigurationItemSpecials
                .SingleAsync(x => x.Id == 2);

            unchanged.Question.Should().Be(Domain.Enums.QuestionType.Points);
            unchanged.Score.Should().Be(8);
        }
        [Fact]
        public async Task UpdateItemSpecial_AllowsKeepingOwnQuestion()
        {
            using var context = CreateContext();
            var repo = new ConfigurationItemSpecialRepository(context);

            context.ConfigurationItemSpecials.Add(new ConfigurationItemSpecial
            {
                Id = 1,
                ConfigurationId = 1,
                Question = Domain.Enums.QuestionType.KOM,
                Score = 10
            });

            await context.SaveChangesAsync();

            var item = await context.ConfigurationItemSpecials.FindAsync(1);
            item.Should().NotBeNull();

            item!.Score = 20;

            var result = await repo.UpdateItemSpecial(item);

            result.Should().BeTrue();

            var updated = await context.ConfigurationItemSpecials
                .SingleAsync(x => x.Id == 1);

            updated.Question.Should().Be(Domain.Enums.QuestionType.KOM);
            updated.Score.Should().Be(20);
        }
    }
}