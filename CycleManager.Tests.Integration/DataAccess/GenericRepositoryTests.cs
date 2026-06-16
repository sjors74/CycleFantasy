using DataAccessEF;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Tests.Integration.DataAccess
{
    public class GenericRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public GenericRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(_options);

        [Fact]
        public async Task Add_ShouldAddEntity()
        {
            using var context = CreateContext();
            var repository = new GenericRepository<ConfigurationItem>(context);

            var item = new ConfigurationItem { Id = 1, Position = 1, Score = 10, ConfigurationId = 100 };

            repository.Add(item);
            await repository.SaveChangesAsync();

            var saved = await context.ConfigurationItems.FindAsync(1);
            saved.Should().NotBeNull();
            saved.Position.Should().Be(1);
        }

        [Fact]
        public async Task AddRange_ShouldAddMultipleEntities()
        {
            using var context = CreateContext();
            var repository = new GenericRepository<ConfigurationItem>(context);

            var items = new List<ConfigurationItem>
            {
                new ConfigurationItem { Id = 1, Position = 1, Score = 10, ConfigurationId = 100 },
                new ConfigurationItem { Id = 2, Position = 2, Score = 20, ConfigurationId = 100 }
            };

            repository.AddRange(items);
            await repository.SaveChangesAsync();

            var count = await context.ConfigurationItems.CountAsync();
            count.Should().Be(2);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllEntities()
        {
            using var context = CreateContext();
            var repository = new GenericRepository<ConfigurationItem>(context);

            context.ConfigurationItems.AddRange(
                new ConfigurationItem { Id = 1, Position = 1 },
                new ConfigurationItem { Id = 2, Position = 2 }
            );
            await context.SaveChangesAsync();

            var all = await repository.GetAll();
            all.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetById_ShouldReturnEntity_WhenExists()
        {
            using var context = CreateContext();
            var repository = new GenericRepository<ConfigurationItem>(context);

            context.ConfigurationItems.Add(new ConfigurationItem { Id = 1, Position = 1 });
            await context.SaveChangesAsync();

            var item = await repository.GetById(1);
            item.Should().NotBeNull();
            item.Position.Should().Be(1);
        }

        [Fact]
        public async Task Find_ShouldReturnMatchingEntities()
        {
            using var context = CreateContext();
            var repository = new GenericRepository<ConfigurationItem>(context);

            context.ConfigurationItems.AddRange(
                new ConfigurationItem { Id = 1, Position = 1 },
                new ConfigurationItem { Id = 2, Position = 2 }
            );
            await context.SaveChangesAsync();

            var result = await repository.Find(x => x.Position == 2);
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(2);
        }

        [Fact]
        public async Task Remove_ShouldRemoveEntity_WhenExists()
        {
            using var context = CreateContext();
            var repository = new GenericRepository<ConfigurationItem>(context);

            var item = new ConfigurationItem { Id = 1, Position = 1 };
            context.ConfigurationItems.Add(item);
            await context.SaveChangesAsync();

            repository.Remove(item);
            await repository.SaveChangesAsync();

            var count = await context.ConfigurationItems.CountAsync();
            count.Should().Be(0);
        }

        [Fact]
        public async Task RemoveRange_ShouldRemoveMultipleEntities()
        {
            using var context = CreateContext();
            var repository = new GenericRepository<ConfigurationItem>(context);

            var items = new List<ConfigurationItem>
            {
                new ConfigurationItem { Id = 1, Position = 1 },
                new ConfigurationItem { Id = 2, Position = 2 }
            };
            context.ConfigurationItems.AddRange(items);
            await context.SaveChangesAsync();

            repository.RemoveRange(items);
            await repository.SaveChangesAsync();

            var count = await context.ConfigurationItems.CountAsync();
            count.Should().Be(0);
        }

        [Fact]
        public async Task Update_ShouldModifyEntity()
        {
            using var context = CreateContext();
            var repository = new GenericRepository<ConfigurationItem>(context);

            var item = new ConfigurationItem { Id = 1, Position = 1, Score = 10 };
            context.ConfigurationItems.Add(item);
            await context.SaveChangesAsync();

            item.Score = 99;
            repository.Update(item);
            await repository.SaveChangesAsync();

            var saved = await context.ConfigurationItems.FindAsync(1);
            saved.Score.Should().Be(99);
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldPersistChanges()
        {
            using var context = CreateContext();
            var repository = new GenericRepository<ConfigurationItem>(context);

            repository.Add(new ConfigurationItem { Id = 1, Position = 1 });
            await repository.SaveChangesAsync();

            var saved = await context.ConfigurationItems.FindAsync(1);
            saved.Should().NotBeNull();
        }

        [Fact]
        public async Task Remove_ShouldNotThrow_WhenEntityDoesNotExist()
        {
            using var context = CreateContext();
            var repository = new GenericRepository<ConfigurationItem>(context);

            // Maak een entity aan die nog niet in de database staat
            var item = new ConfigurationItem { Id = 999, Position = 1 };

            // Act & Assert
            Func<Task> act = async () =>
            {
                repository.Remove(item);      // probeert te verwijderen
                await repository.SaveChangesAsync(); // slaat op, entity bestaat niet
            };

            await act.Should().NotThrowAsync();

            // Controleer dat de database nog steeds leeg is
            var count = await context.ConfigurationItems.CountAsync();
            count.Should().Be(0);
        }

    }
}
