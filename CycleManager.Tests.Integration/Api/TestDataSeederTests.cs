using Domain.Context;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebCycle.Services;

namespace CycleManager.Tests.Integration.Api
{
    public class TestDataSeederTests
    {
        private static ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task SeedAsync_DoesNothing_WhenEnvironmentIsNotTest()
        {
            await using var context = CreateContext();

            var environment = new Mock<IWebHostEnvironment>();

            environment
                .SetupGet(e => e.EnvironmentName)
                .Returns("Development");

            await TestDataSeeder.SeedAsync(context, environment.Object);

            context.Users.Should().BeEmpty();
            context.Events.Should().BeEmpty();
            context.Countries.Should().BeEmpty();
        }

        [Fact]
        public async Task SeedAsync_SeedsDatabase_WhenEnvironmentIsTest()
        {
            await using var context = CreateContext();

            var environment = new Mock<IWebHostEnvironment>();
            environment
                .SetupGet(e => e.EnvironmentName)
                .Returns("Test");

            await TestDataSeeder.SeedAsync(context, environment.Object);

            context.Users.Should().ContainSingle(u => u.UserName == "testuser");

            context.Countries.Should().HaveCount(2);
            context.Competitors.Should().HaveCount(2);
            context.TeamYear.Should().ContainSingle();
            context.CompetitorInTeams.Should().HaveCount(2);

            context.Events.Should().HaveCount(2);
            context.Stages.Should().HaveCount(3);

            context.GameCompetitorsEvent.Should().ContainSingle();

            context.Configurations.Should().ContainSingle();
            context.ConfigurationItems.Should().HaveCount(2);

            context.Results.Should().ContainSingle();
        }
    }
}