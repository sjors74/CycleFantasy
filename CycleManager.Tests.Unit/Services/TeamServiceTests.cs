using CycleManager.Domain.Dto;
using CycleManager.Domain.Interfaces;
using CycleManager.Domain.Models;
using CycleManager.Services;
using Domain.Interfaces;
using Domain.Models;
using FluentAssertions;
using Moq;

namespace CycleManager.Tests.Unit.Services
{
    public class TeamServiceTests
    {
        private readonly Mock<ITeamRepository> _teamRepositoryMock;
        private readonly Mock<ISeasonYearRepository> _seasonYearRepositoryMock;
        private readonly TeamService _service;

        public TeamServiceTests()
        {
            _teamRepositoryMock = new Mock<ITeamRepository>();
            _seasonYearRepositoryMock = new Mock<ISeasonYearRepository>();

            _service = new TeamService(
                _teamRepositoryMock.Object,
                _seasonYearRepositoryMock.Object);
        }

        [Fact]
        public async Task Add_CreatesTeamYearsForAllSeasonYears()
        {
            // Arrange
            var seasonYears = new List<SeasonYear>
            {
                new SeasonYear
                {
                    SeasonYearId = 1,
                    Year = 2025,
                    Active = true
                },
                new SeasonYear
                {
                    SeasonYearId = 2,
                    Year = 2026,
                    Active = true
                }
            };

            var team = new Team
            {
                CurrentTeamName = "Test Team"
            };

            _seasonYearRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(seasonYears);

            // Act
            await _service.Add(team);

            // Assert
            team.TeamYears.Should().HaveCount(2);

            team.TeamYears.Should().ContainSingle(x =>
                x.SeasonYearId == 1 &&
                x.Year == 2025 &&
                x.Name == "Test Team");

            team.TeamYears.Should().ContainSingle(x =>
                x.SeasonYearId == 2 &&
                x.Year == 2026 &&
                x.Name == "Test Team");

            _teamRepositoryMock.Verify(
                x => x.Add(team),
                Times.Once);

            _teamRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task CountUnprocessedScrapedCompetitors_ReturnsRepositoryResult()
        {
            // Arrange
            _teamRepositoryMock
                .Setup(x => x.CountUnprocessedScrapedCompetitors())
                .ReturnsAsync(7);

            // Act
            var result = await _service.CountUnprocessedScrapedCompetitors();

            // Assert
            result.Should().Be(7);

            _teamRepositoryMock.Verify(
                x => x.CountUnprocessedScrapedCompetitors(),
                Times.Once);
        }

        [Fact]
        public async Task Delete_RemovesAndSaves()
        {
            // Arrange
            var team = new Team();

            // Act
            await _service.Delete(team);

            // Assert
            _teamRepositoryMock.Verify(
                x => x.Remove(team),
                Times.Once);

            _teamRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetAllTeams_ReturnsRepositoryResult()
        {
            // Arrange
            var expected = new List<Team>
            {
                new Team(),
                new Team()
            };

            _teamRepositoryMock
                .Setup(x => x.GetAllTeams())
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetAllTeams();

            // Assert
            result.Should().BeEquivalentTo(expected);

            _teamRepositoryMock.Verify(
                x => x.GetAllTeams(),
                Times.Once);
        }

        [Fact]
        public async Task GetTeamForCurrentYear_ReturnsRepositoryResult()
        {
            // Arrange
            const int teamId = 5;
            const int year = 2026;

            var expected = new Team();

            _teamRepositoryMock
                .Setup(x => x.GetTeamForCurrentYear(teamId, year))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetTeamForCurrentYear(teamId, year);

            // Assert
            result.Should().BeSameAs(expected);

            _teamRepositoryMock.Verify(
                x => x.GetTeamForCurrentYear(teamId, year),
                Times.Once);
        }

        [Fact]
        public async Task GetTeamsAsSelectList_MapsTeamsAndSelectedTeam()
        {
            // Arrange
            var teams = new List<Team>
            {
                new Team
                {
                    TeamId = 1,
                    CurrentTeamName = "Team One"
                },
                new Team
                {
                    TeamId = 2,
                    CurrentTeamName = "Team Two"
                }
            };

            _teamRepositoryMock
                .Setup(x => x.GetAll())
                .ReturnsAsync(teams);

            // Act
            var result = (await _service.GetTeamsAsSelectList(2)).ToList();

            // Assert
            result.Should().HaveCount(2);

            result[0].Value.Should().Be("1");
            result[0].Text.Should().Be("Team One");
            result[0].Selected.Should().BeFalse();

            result[1].Value.Should().Be("2");
            result[1].Text.Should().Be("Team Two");
            result[1].Selected.Should().BeTrue();

            _teamRepositoryMock.Verify(
                x => x.GetAll(),
                Times.Once);
        }

        [Fact]
        public async Task GetTeamsForEvent_ReturnsRepositoryResult()
        {
            // Arrange
            const int eventId = 2;

            var expected = new List<Team>
            {
                new Team()
            };

            _teamRepositoryMock
                .Setup(x => x.GetTeamsForEvent(eventId))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetTeamsForEvent(eventId);

            // Assert
            result.Should().BeEquivalentTo(expected);

            _teamRepositoryMock.Verify(
                x => x.GetTeamsForEvent(eventId),
                Times.Once);
        }

        [Fact]
        public async Task GetTeamYears_ReturnsRepositoryResult()
        {
            // Arrange
            const int seasonYearId = 2;

            var expected = new List<TeamYearDto>
            {
                new TeamYearDto()
            };

            _teamRepositoryMock
                .Setup(x => x.GetTeamYears(seasonYearId))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetTeamYears(seasonYearId);

            // Assert
            result.Should().BeEquivalentTo(expected);

            _teamRepositoryMock.Verify(
                x => x.GetTeamYears(seasonYearId),
                Times.Once);
        }

        [Fact]
        public async Task HasUnprocessedScrapedTeams_ReturnsRepositoryResult()
        {
            // Arrange
            _teamRepositoryMock
                .Setup(x => x.HasUnprocessedScrapedCompetitors())
                .ReturnsAsync(true);

            // Act
            var result = await _service.HasUnprocessedScrapedTeams();

            // Assert
            result.Should().BeTrue();

            _teamRepositoryMock.Verify(
                x => x.HasUnprocessedScrapedCompetitors(),
                Times.Once);
        }

        [Fact]
        public async Task Update_UpdatesAndSaves()
        {
            // Arrange
            var team = new Team();

            // Act
            await _service.Update(team);

            // Assert
            _teamRepositoryMock.Verify(
                x => x.Update(team),
                Times.Once);

            _teamRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetByTeamAndSeasonAsync_ReturnsRepositoryResult()
        {
            // Arrange
            const int teamId = 5;
            const int seasonYearId = 2;

            var expected = new TeamYearDto();

            _teamRepositoryMock
                .Setup(x => x.GetByTeamAndSeasonAsync(teamId, seasonYearId))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetByTeamAndSeasonAsync(
                teamId,
                seasonYearId);

            // Assert
            result.Should().BeSameAs(expected);

            _teamRepositoryMock.Verify(
                x => x.GetByTeamAndSeasonAsync(teamId, seasonYearId),
                Times.Once);
        }

        [Fact]
        public async Task GetTeamYearById_ReturnsTeamYear_WhenFound()
        {
            // Arrange
            const int teamYearId = 5;
            var expected = new TeamYear();

            _teamRepositoryMock
                .Setup(x => x.GetTeamYearByIdAsync(teamYearId))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetTeamYearById(teamYearId);

            // Assert
            result.Should().BeSameAs(expected);

            _teamRepositoryMock.Verify(
                x => x.GetTeamYearByIdAsync(teamYearId),
                Times.Once);
        }

        [Fact]
        public async Task GetTeamYearById_ReturnsNull_WhenNotFound()
        {
            // Arrange
            const int teamYearId = 999;

            _teamRepositoryMock
                .Setup(x => x.GetTeamYearByIdAsync(teamYearId))
                .ReturnsAsync((TeamYear?)null);

            // Act
            var result = await _service.GetTeamYearById(teamYearId);

            // Assert
            result.Should().BeNull();

            _teamRepositoryMock.Verify(
                x => x.GetTeamYearByIdAsync(teamYearId),
                Times.Once);
        }
    }
}