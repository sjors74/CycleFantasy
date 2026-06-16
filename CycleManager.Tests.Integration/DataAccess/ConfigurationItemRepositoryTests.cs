using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Tests.Integration.DataAccess
{
    public class ConfigurationItemRepositoryTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ConfigurationItemRepository _repository;

        public ConfigurationItemRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"ConfigurationItemRepo_{System.Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new ConfigurationItemRepository(_context);
        }

        [Fact]
        public async Task Add_AddsConfigurationItemToDatabase()
        {
            // Arrange
            var item = new ConfigurationItem
            {
                Id = 1,
                Position = 1,
                Score = 100,
                ConfigurationId = 10
            };

            // Act
            _repository.Add(item);
            await _context.SaveChangesAsync();

            // Assert
            var stored = await _context.ConfigurationItems.FirstOrDefaultAsync(ci => ci.Id == 1);
            stored.Should().NotBeNull();
            stored!.Position.Should().Be(1);
            stored.Score.Should().Be(100);
            stored.ConfigurationId.Should().Be(10);
        }

        [Fact]
        public async Task GetById_ReturnsCorrectItem()
        {
            // Arrange
            var item = new ConfigurationItem
            {
                Position = 2,
                Score = 50,
                ConfigurationId = 20
            };
            await _context.ConfigurationItems.AddAsync(item);
            await _context.SaveChangesAsync();

            // Act
            var fetched = await _repository.GetById(item.Id);

            // Assert
            fetched.Should().NotBeNull();
            fetched!.Score.Should().Be(50);
            fetched.Position.Should().Be(2);
        }

        [Fact]
        public async Task Remove_RemovesItemFromDatabase()
        {
            // Arrange
            var item = new ConfigurationItem
            {
                Position = 3,
                Score = 70,
                ConfigurationId = 30
            };
            await _context.ConfigurationItems.AddAsync(item);
            await _context.SaveChangesAsync();

            // Act
            _repository.Remove(item);
            await _context.SaveChangesAsync();

            // Assert
            var exists = await _context.ConfigurationItems.AnyAsync(ci => ci.Id == item.Id);
            exists.Should().BeFalse();
        }

        [Fact]
        public void Constructor_CreatesInstance()
        {
            // Act
            var repo = new ConfigurationItemRepository(_context);

            // Assert
            repo.Should().NotBeNull();
        }
    }
}
