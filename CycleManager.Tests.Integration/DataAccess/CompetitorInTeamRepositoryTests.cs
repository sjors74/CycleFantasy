using CycleManager.Domain.Models;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Tests.Integration.DataAccess
{
    public class CompetitorInTeamRepositoryTests
    {
        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task CheckCompetitorInTeam_ReturnsTrue_WhenCompetitorExistsInTeamForYear()
        {
            // Arrange
            using var context = CreateContext();

            var competitor = new Competitor { CompetitorId = 1, FirstName = "Remco", LastName = "Evenepoel" };
            var team = new Team { TeamId = 1, CurrentTeamName = "Soudal Quick-Step" };

            var cit = new CompetitorInTeam
            {
                CompetitorId = competitor.CompetitorId,
                TeamId = team.TeamId,
                Year = 2024
            };

            context.Competitors.Add(competitor);
            context.Teams.Add(team);
            context.CompetitorInTeams.Add(cit);
            await context.SaveChangesAsync();

            var repo = new CompetitorInTeamRepository(context);

            // Act
            var result = await repo.CheckCompetitorInTeam(1, 1, 2024);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CheckCompetitorInTeam_ReturnsFalse_WhenCompetitorNotInTeamForYear()
        {
            // Arrange
            using var context = CreateContext();

            // Competitor in ander jaar
            var competitor = new Competitor { CompetitorId = 1, FirstName = "Remco", LastName = "Evenepoel" };
            var team = new Team { TeamId = 1, CurrentTeamName = "Soudal Quick-Step" };

            context.Competitors.Add(competitor);
            context.Teams.Add(team);
            context.CompetitorInTeams.Add(new CompetitorInTeam
            {
                CompetitorId = 1,
                TeamId = 1,
                Year = 2023 // niet 2024
            });
            await context.SaveChangesAsync();

            var repo = new CompetitorInTeamRepository(context);

            // Act
            var result = await repo.CheckCompetitorInTeam(1, 1, 2024);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CheckCompetitorInTeam_ReturnsFalse_WhenNoRecordsExist()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new CompetitorInTeamRepository(context);

            // Act
            var result = await repo.CheckCompetitorInTeam(99, 99, 2025);

            // Assert
            Assert.False(result);
        }
    }
}