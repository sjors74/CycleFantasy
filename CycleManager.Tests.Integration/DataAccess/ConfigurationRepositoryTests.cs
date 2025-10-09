using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Tests.Integration.DataAccess
{
    public class ConfigurationRepositoryTests
    {
        private ApplicationDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetConfigurationById_ReturnsConfigurationWithItems()
        {
            using var context = GetInMemoryDbContext("ConfigTest1");

            // Arrange
            var configuration = new Configuration { Id = 1, ConfigurationType = "Config1" };
            var items = new List<ConfigurationItem>
            {
                new ConfigurationItem { Id = 1,  Position = 1, Score  = 25, Configuration = configuration },
                new ConfigurationItem { Id = 2, Position = 2, Score = 20, Configuration = configuration }
            };
            configuration.ConfigurationItems = items;

            context.Configurations.Add(configuration);
            await context.SaveChangesAsync();

            var repo = new ConfigurationRepository(context);

            // Act
            var result = await repo.GetConfigurationById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Config1", result.ConfigurationType);
            Assert.Equal(2, result.ConfigurationItems.Count);
        }

        [Fact]
        public async Task GetConfigurationById_ThrowsWhenNotFound()
        {
            using var context = GetInMemoryDbContext("ConfigTest2");
            var repo = new ConfigurationRepository(context);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await repo.GetConfigurationById(999));

            Assert.Equal("Configuration with id 999 not found.", ex.Message);
        }

        [Fact]
        public async Task GetConfigurationById_ReturnsCorrectConfigurationAmongMultiple()
        {
            using var context = GetInMemoryDbContext("ConfigTest3");

            // Arrange: meerdere configuraties
            var config1 = new Configuration { Id = 1, ConfigurationType = "Config1" };
            var config2 = new Configuration { Id = 2, ConfigurationType = "Config2" };
            var config3 = new Configuration { Id = 3, ConfigurationType = "Config3" };

            context.Configurations.AddRange(config1, config2, config3);
            await context.SaveChangesAsync();

            var repo = new ConfigurationRepository(context);

            // Act
            var result = await repo.GetConfigurationById(2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Id);
            Assert.Equal("Config2", result.ConfigurationType);
        }

        [Fact]
        public async Task GetConfigurationById_IncludesConfigurationItems()
        {
            using var context = GetInMemoryDbContext("ConfigTestWithItems");

            // Arrange: configuratie met items
            var config = new Configuration
            {
                Id = 1,
                ConfigurationType = "ConfigWithItems",
                ConfigurationItems = new List<ConfigurationItem>
                {
                   new ConfigurationItem { Id = 1, Position = 1, Score = 30 },
                    new ConfigurationItem { Id = 2, Position = 2, Score = 25 }
                }
            };

            context.Configurations.Add(config);
            await context.SaveChangesAsync();

            var repo = new ConfigurationRepository(context);

            // Act
            var result = await repo.GetConfigurationById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("ConfigWithItems", result.ConfigurationType);
            Assert.NotNull(result.ConfigurationItems);
            Assert.Equal(2, result.ConfigurationItems.Count);

            Assert.Contains(result.ConfigurationItems, ci => ci.Position == 1 && ci.Score == 30);
            Assert.Contains(result.ConfigurationItems, ci => ci.Position == 2 && ci.Score == 25);
        }

        [Fact]
        public async Task GetConfigurationById_Throws_WhenNotFound()
        {
            using var context = GetInMemoryDbContext("ConfigTest_NotFound");

            // Arrange: geen configuraties toegevoegd
            var repo = new ConfigurationRepository(context);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                repo.GetConfigurationById(999) // id die niet bestaat
            );

            Assert.Equal("Configuration with id 999 not found.", exception.Message);
        }
    }
}
