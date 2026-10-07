using CycleManager.Domain.Models;
using CycleManager.Services;
using Domain.Interfaces;
using Domain.Models;
using FluentAssertions;
using Moq;

namespace CycleManager.Tests.Unit.Services
{
    public class AdminScraperServiceTests
    {
        private readonly Mock<IStageRepository> _stageRepositoryMock;
        private readonly Mock<ITeamRepository> _teamRepositoryMock;
        private readonly AdminScraperService _service;

        public AdminScraperServiceTests()
        {
            _stageRepositoryMock = new Mock<IStageRepository>();
            _teamRepositoryMock = new Mock<ITeamRepository>();

            _service = new AdminScraperService(
                _stageRepositoryMock.Object,
                _teamRepositoryMock.Object);
        }

        [Fact]
        public async Task GetStageByIdAsync_ReturnsRepositoryResult()
        {
            // Arrange
            var expected = new Stage
            {
                Id = 1
            };

            _stageRepositoryMock
                .Setup(x => x.GetStageById(1))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetStageByIdAsync(1);

            // Assert
            result.Should().BeSameAs(expected);

            _stageRepositoryMock.Verify(
                x => x.GetStageById(1),
                Times.Once);
        }

        [Fact]
        public async Task GetTeamByIdAsync_ReturnsRepositoryResult()
        {
            // Arrange
            var expected = new Team
            {
                TeamId = 1
            };

            _teamRepositoryMock
                .Setup(x => x.GetTeamById(1))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetTeamByIdAsync(1);

            // Assert
            result.Should().BeSameAs(expected);

            _teamRepositoryMock.Verify(
                x => x.GetTeamById(1),
                Times.Once);
        }

        [Fact]
        public async Task GetTeamYearByIdAsync_ReturnsRepositoryResult()
        {
            // Arrange
            var expected = new TeamYear
            {
                TeamYearId = 1
            };

            _teamRepositoryMock
                .Setup(x => x.GetTeamYearByIdAsync(1))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetTeamYearByIdAsync(1);

            // Assert
            result.Should().BeSameAs(expected);

            _teamRepositoryMock.Verify(
                x => x.GetTeamYearByIdAsync(1),
                Times.Once);
        }
    }
}