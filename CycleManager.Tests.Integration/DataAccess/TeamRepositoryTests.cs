using CycleManager.Domain.Models;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CycleManager.Tests.Integration.DataAccess
{
    public class TeamRepositoryTests
    {
        private ApplicationDbContext CreateContext([System.Runtime.CompilerServices.CallerMemberName] string testName = "")
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"{Guid.NewGuid()}_{testName}")
                .Options;

            return new ApplicationDbContext(options);
        }

        private static Team CreateTestTeam()
        {
            var country = new Country { CountryId = 1, CountryNameLong = "Nederland" };
            var competitor = new Competitor { CompetitorId = 1, FirstName = "John", LastName = "Doe", Country = country };
            var competitorInTeam = new CompetitorInTeam { Id = 1, Competitor = competitor, CompetitorId = 1, TeamId = 1 };
            var teamYear = new TeamYear { TeamYearId = 1, Year = 2025, TeamId = 1 };
            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "TeamA",
                Country = country,
                CountryId = 1,
                CompetitorInTeams = new List<CompetitorInTeam> { competitorInTeam },
                TeamYears = new List<TeamYear> { teamYear }
            };
            return team;
        }

        [Fact]
        public async Task GetAllTeams_ReturnsTeamsWithIncludes()
        {
            using var context = CreateContext();
            var repo = new TeamRepository(context);

            var team = CreateTestTeam();
            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var result = await repo.GetAllTeams();

            result.Should().NotBeEmpty();
            var fetched = result.First();
            fetched.Country.Should().NotBeNull();
            fetched.CompetitorInTeams.Should().HaveCount(1);
            fetched.TeamYears.Should().HaveCount(1);
            fetched.CompetitorInTeams.First().Competitor.Should().NotBeNull();
            fetched.CompetitorInTeams.First().Competitor.Country.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllTeams_Empty_ReturnsEmpty()
        {
            using var context = CreateContext();
            var repo = new TeamRepository(context);

            var teams = await repo.GetAllTeams();

            teams.Should().BeEmpty();
        }

        [Fact]
        public async Task GetTeamById_ReturnsTeamWithIncludes()
        {
            using var context = CreateContext();
            var repo = new TeamRepository(context);

            var team = CreateTestTeam();
            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var fetched = await repo.GetTeamById(1);

            fetched.Should().NotBeNull();
            fetched.Country.Should().NotBeNull();
            fetched.CompetitorInTeams.Should().NotBeEmpty();
            fetched.TeamYears.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetTeamById_ReturnsNull_WhenNotFound()
        {
            using var context = CreateContext();
            var repo = new TeamRepository(context);

            var fetched = await repo.GetTeamById(999);

            fetched.Should().BeNull();
        }

        [Fact]
        public async Task GetTeamForCurrentYear_ReturnsMatchingTeam()
        {
            using var context = CreateContext();
            var repo = new TeamRepository(context);

            var team = CreateTestTeam();
            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var fetched = await repo.GetTeamForCurrentYear(1, 2025);

            fetched.Should().NotBeNull();
            fetched.TeamYears.Any(ty => ty.Year == 2025).Should().BeTrue();
        }

        [Fact]
        public async Task GetTeamForCurrentYear_ReturnsNull_WhenNoMatchingYear()
        {
            using var context = CreateContext();
            var repo = new TeamRepository(context);

            var team = CreateTestTeam();
            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var fetched = await repo.GetTeamForCurrentYear(1, 1999);

            fetched.Should().BeNull();
        }

        [Fact]
        public async Task GetTeamsForEvent_ReturnsTeamsLinkedToEvent()
        {
            using var context = CreateContext();
            var repo = new TeamRepository(context);

            var team = CreateTestTeam();
            team.EventTeams = new List<EventTeam> { new EventTeam { EventId = 100, TeamId = 1 } };
            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var fetched = await repo.GetTeamsForEvent(100);

            fetched.Should().HaveCount(1);
            fetched.First().EventTeams.Should().ContainSingle(et => et.EventId == 100);
        }

        [Fact]
        public async Task GetTeamsForEvent_ReturnsEmpty_WhenNoMatch()
        {
            using var context = CreateContext();
            var repo = new TeamRepository(context);

            var team = CreateTestTeam();
            team.EventTeams = new List<EventTeam> { new EventTeam { EventId = 200, TeamId = 1 } };
            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var fetched = await repo.GetTeamsForEvent(999);

            fetched.Should().BeEmpty();
        }
    }
}
