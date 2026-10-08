using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebCycle.Services;

namespace CycleManager.Tests.Integration.Api
{
    public class SeedDataTests
    {
        private static ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task EnsureSeedAsync_DoesNothing_WhenEventsAlreadyExist()
        {
            await using var context = CreateContext();

            context.Events.Add(new Event
            {
                EventName = "Existing event"
            });

            await context.SaveChangesAsync();

            await SeedData.EnsureSeedAsync(context);

            context.Events.Should().ContainSingle();
            context.Events.Single().EventName.Should().Be("Existing event");

            context.Countries.Should().BeEmpty();
            context.Configurations.Should().BeEmpty();
            context.Teams.Should().BeEmpty();
        }

        [Fact]
        public async Task EnsureSeedAsync_SeedsDatabase_WhenEmpty()
        {
            await using var context = CreateContext();

            await SeedData.EnsureSeedAsync(context);

            // Country
            context.Countries.Should().ContainSingle();

            // Configurations
            context.Configurations.Should().HaveCount(2);
            context.Configurations
                .Select(c => c.ConfigurationType)
                .Should()
                .BeEquivalentTo("E2E Top20", "E2E Custom15");

            // Configuration items
            context.ConfigurationItems.Should().HaveCount(35);

            // Events
            context.Events.Should().HaveCount(2);

            // Teams and TeamYears
            context.Teams.Should().HaveCount(20);
            context.TeamYear.Should().HaveCount(20);

            // Competitors and team memberships
            context.Competitors.Should().HaveCount(200);
            context.CompetitorInTeams.Should().HaveCount(200);

            // Competitors in events
            context.CompetitorsInEvent.Should().HaveCount(400);

            // Event/team links
            context.EventTeam.Should().HaveCount(40);

            // Stages
            context.Stages.Should().HaveCount(42);

            // Results
            context.Results.Should().HaveCount(735);

            // Test users
            context.Users.Should().HaveCount(2);

            // Pools
            context.GameCompetitorsEvent.Should().HaveCount(2);

            // Picks
            context.GameCompetitorEventPicks.Should().HaveCount(16);

            // Calculated scores
            context.DeelnemerScores.Should().HaveCount(42);
            context.DeelnemerPickScores.Should().HaveCount(336);
        }
    }
}