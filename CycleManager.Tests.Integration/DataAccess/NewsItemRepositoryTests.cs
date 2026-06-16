using Domain.Context;
using Domain.Models;
using DataAccessEF.TypeRepository;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Tests.Integration.DataAccess
{
    public class NewsItemRepositoryIntegrationTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public NewsItemRepositoryIntegrationTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(_options);

        [Fact]
        public async Task CreateAsync_AddsNewsItemToDatabase()
        {
            using var context = CreateContext();
            var repo = new NewsItemRepository(context);

            var news = new NewsItem
            {
                Id = 1,
                Title = "Test News",
                Message = "This is a test",
                DatePosted = DateTime.UtcNow,
                IsActive = true
            };

            await repo.CreateAsync(news);

            var saved = await context.NewsItems.FindAsync(1);
            saved.Should().NotBeNull();
            saved.Title.Should().Be("Test News");
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectItem()
        {
            using var context = CreateContext();
            var news = new NewsItem { Id = 2, Title = "News2", Message = "Msg", DatePosted = DateTime.UtcNow, IsActive = true };
            context.NewsItems.Add(news);
            await context.SaveChangesAsync();

            var repo = new NewsItemRepository(context);
            var result = await repo.GetByIdAsync(2);

            result.Should().NotBeNull();
            result.Id.Should().Be(2);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            using var context = CreateContext();
            var repo = new NewsItemRepository(context);

            var result = await repo.GetByIdAsync(999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllActiveNewsItems_ReturnsOnlyActiveItems()
        {
            using var context = CreateContext();
            context.NewsItems.AddRange(
                new NewsItem { Id = 1, Title = "Active1", Message = "active message 1", IsActive = true },
                new NewsItem { Id = 2, Title = "Inactive", Message = "inactive message", IsActive = false },
                new NewsItem { Id = 3, Title = "Active2", Message = "active message 2", IsActive = true }
            );
            await context.SaveChangesAsync();

            var repo = new NewsItemRepository(context);
            var result = await repo.GetAllActiveNewsItems();

            result.Should().HaveCount(2);
            result.Should().OnlyContain(n => n.IsActive);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesExistingItem()
        {
            using var context = CreateContext();
            var news = new NewsItem { Id = 1, Title = "Old", Message = "Old Msg", DatePosted = DateTime.UtcNow, IsActive = true };
            context.NewsItems.Add(news);
            await context.SaveChangesAsync();

            var repo = new NewsItemRepository(context);

            news.Title = "Updated";
            news.Message = "Updated Msg";
            news.IsActive = false;

            await repo.UpdateAsync(news);

            var updated = await context.NewsItems.FindAsync(1);
            updated.Title.Should().Be("Updated");
            updated.Message.Should().Be("Updated Msg");
            updated.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenItemNotFound()
        {
            using var context = CreateContext();
            var repo = new NewsItemRepository(context);

            var news = new NewsItem { Id = 999, Title = "NotFound", Message = "Unfound message" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateAsync(news));
        }

        [Fact]
        public async Task DeleteAsync_RemovesItem()
        {
            using var context = CreateContext();
            var news = new NewsItem { Id = 1, Title = "ToDelete", Message = "Delete the news item" };
            context.NewsItems.Add(news);
            await context.SaveChangesAsync();

            var repo = new NewsItemRepository(context);
            await repo.DeleteAsync(1);

            (await context.NewsItems.FindAsync(1)).Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_DoesNothing_WhenItemNotFound()
        {
            using var context = CreateContext();
            var repo = new NewsItemRepository(context);

            await repo.DeleteAsync(999); // should not throw
            context.NewsItems.Should().BeEmpty();
        }

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenItemExists()
        {
            using var context = CreateContext();
            context.NewsItems.Add(new NewsItem { Id = 1, Title = "Exist", Message = "existing message" });
            await context.SaveChangesAsync();

            var repo = new NewsItemRepository(context);
            var exists = await repo.ExistsAsync(1);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenItemDoesNotExist()
        {
            using var context = CreateContext();
            var repo = new NewsItemRepository(context);

            var exists = await repo.ExistsAsync(999);
            exists.Should().BeFalse();
        }
    }
}
