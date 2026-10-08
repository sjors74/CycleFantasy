using CycleManager.Domain.Models;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

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
            var competitorInTeam = new CompetitorInTeam {Id = 1 };
            var seasonYear = new SeasonYear { SeasonYearId = 1, Year = 2025 };
            var teamYear = new TeamYear { TeamYearId = 1, Year = 2025, TeamId = 1, SeasonYearId = seasonYear.SeasonYearId, SeasonYear = seasonYear };
            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "TeamA",
                Country = country,
                CountryId = 1,
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
            fetched.TeamYears.Should().HaveCount(1);
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
            fetched.TeamYears.Any(ty => ty.SeasonYear.Year == 2025).Should().BeTrue();
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

        [Fact]
        public async Task GetTeamYears_ReturnsTeamYearsForSeasonOrderedByName()
        {
            using var context = CreateContext();
            var repo = new TeamRepository(context);

            context.TeamYear.AddRange(
                new TeamYear
                {
                    TeamYearId = 1,
                    TeamId = 1,
                    SeasonYearId = 2025,
                    Name = "Z-Team"
                },
                new TeamYear
                {
                    TeamYearId = 2,
                    TeamId = 2,
                    SeasonYearId = 2025,
                    Name = "A-Team"
                },
                new TeamYear
                {
                    TeamYearId = 3,
                    TeamId = 3,
                    SeasonYearId = 2024,
                    Name = "Other"
                });

            await context.SaveChangesAsync();

            var result = await repo.GetTeamYears(2025);

            result.Should().HaveCount(2);
            result[0].Name.Should().Be("A-Team");
            result[1].Name.Should().Be("Z-Team");
        }

        [Fact]
        public async Task GetTeamYearByIdAsync_ReturnsTeamYearWithIncludes()
        {
            using var context = CreateContext();
            var repo = new TeamRepository(context);

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "TeamA"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2025
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "John",
                LastName = "Doe"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor,
                TeamYearId = 1
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear,
                Name = "TeamA 2025",
                CompetitorInTeams = new List<CompetitorInTeam>
                {
                    competitorInTeam
                }
            };

            context.Teams.Add(team);
            context.SeasonYears.Add(seasonYear);
            context.Competitors.Add(competitor);
            context.TeamYear.Add(teamYear);

            await context.SaveChangesAsync();

            var result = await repo.GetTeamYearByIdAsync(1);

            result.Should().NotBeNull();
            result!.TeamYearId.Should().Be(1);
            result.Name.Should().Be("TeamA 2025");
            result.Team.Should().NotBeNull();
            result.Team.TeamId.Should().Be(1);
            result.SeasonYear.Should().NotBeNull();
            result.SeasonYear.Year.Should().Be(2025);
            result.CompetitorInTeams.Should().ContainSingle();
            result.CompetitorInTeams.First().Competitor.Should().NotBeNull();
            result.CompetitorInTeams.First().Competitor.CompetitorId.Should().Be(1);
        }

        [Fact]
        public async Task GetByTeamAndSeasonAsync_ReturnsNull_WhenNoMatch()
        {
            using var context = CreateContext();
            var repo = new TeamRepository(context);

            context.TeamYear.Add(new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                SeasonYearId = 2025,
                Name = "TeamA 2025"
            });

            await context.SaveChangesAsync();

            var result = await repo.GetByTeamAndSeasonAsync(1, 2024);

            result.Should().BeNull();
        }

        [Fact]
        public async Task CountUnprocessedScrapedCompetitors_ReturnsNumberOfUnprocessedCompetitors()
        {
            using var context = CreateContext();
            var repo = new TeamRepository(context);

            context.ScrapedCompetitors.AddRange(
                new ScrapedCompetitor
                {
                    Id = 1,
                    RiderName = "Rider 1",
                    TeamId = 1,
                    Year = 2025,
                    ProcessedAt = null
                },
                new ScrapedCompetitor
                {
                    Id = 2,
                    RiderName = "Rider 2",
                    TeamId = 1,
                    Year = 2025,
                    ProcessedAt = DateTime.UtcNow
                },
                new ScrapedCompetitor
                {
                    Id = 3,
                    RiderName = "Rider 3",
                    TeamId = 2,
                    Year = 2025,
                    ProcessedAt = null
                });

            await context.SaveChangesAsync();

            var result = await repo.CountUnprocessedScrapedCompetitors();

            result.Should().Be(2);
        }

        [Fact]
        public async Task CountUnprocessedScrapedCompetitors_ReturnsZero_WhenAllAreProcessed()
        {
            using var context = CreateContext();
            var repo = new TeamRepository(context);

            context.ScrapedCompetitors.Add(
                new ScrapedCompetitor
                {
                    Id = 1,
                    RiderName = "Rider 1",
                    TeamId = 1,
                    Year = 2025,
                    ProcessedAt = DateTime.UtcNow
                });

            await context.SaveChangesAsync();

            var result = await repo.CountUnprocessedScrapedCompetitors();

            result.Should().Be(0);
        }

        [Fact]
        public async Task HasUnprocessedScrapedCompetitors_ReturnsTrue_WhenUnprocessedCompetitorExists()
        {
            using var context = CreateContext();
            var repo = new TeamRepository(context);

            context.ScrapedCompetitors.Add(
                new ScrapedCompetitor
                {
                    Id = 1,
                    RiderName = "Rider 1",
                    TeamId = 1,
                    Year = 2025,
                    ProcessedAt = null
                });

            await context.SaveChangesAsync();

            var result = await repo.HasUnprocessedScrapedCompetitors();

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasUnprocessedScrapedCompetitors_ReturnsFalse_WhenNoUnprocessedCompetitorExists()
        {
            using var context = CreateContext();
            var repo = new TeamRepository(context);

            context.ScrapedCompetitors.AddRange(
                new ScrapedCompetitor
                {
                    Id = 1,
                    RiderName = "Rider 1",
                    TeamId = 1,
                    Year = 2025,
                    ProcessedAt = DateTime.UtcNow
                },
                new ScrapedCompetitor
                {
                    Id = 2,
                    RiderName = "Rider 2",
                    TeamId = 2,
                    Year = 2025,
                    ProcessedAt = DateTime.UtcNow
                });

            await context.SaveChangesAsync();

            var result = await repo.HasUnprocessedScrapedCompetitors();

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetByTeamAndSeasonAsync_ReturnsTeamYear_WhenMatchExists()
        {
            using var context = CreateContext();
            var repo = new TeamRepository(context);

            context.TeamYear.AddRange(
                new TeamYear
                {
                    TeamYearId = 1,
                    TeamId = 1,
                    SeasonYearId = 2025,
                    Name = "TeamA 2025"
                },
                new TeamYear
                {
                    TeamYearId = 2,
                    TeamId = 2,
                    SeasonYearId = 2025,
                    Name = "TeamB 2025"
                });

            await context.SaveChangesAsync();

            var result = await repo.GetByTeamAndSeasonAsync(1, 2025);

            result.Should().NotBeNull();
            result!.TeamYearId.Should().Be(1);
            result.Name.Should().Be("TeamA 2025");
        }
    }
}
