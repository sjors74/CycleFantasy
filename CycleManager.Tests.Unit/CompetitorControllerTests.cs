using CycleManager.Domain.Dto;
using CycleManager.Services.Interfaces;
using Domain.Dto;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebCycle.Controllers;

namespace CycleManager.Tests.Unit
{
    public class CompetitorControllerTests
    {
        private readonly Mock<ICompetitorService> _mockService;
        private readonly CompetitorController _controller;

        public CompetitorControllerTests()
        {
            _mockService = new Mock<ICompetitorService>();
            _controller = new CompetitorController(Mock.Of<ICompetitorRepository>(), _mockService.Object);
        }

        [Fact]
        public async Task GetCompetitors_ReturnsCompetitorList()
        {
            // Arrange
            var competitors = new List<CompetitorDto>
        {
            new CompetitorDto { CompetitorInTeamId = 1, FirstName = "Remco", LastName = "Evenepoel" },
            new CompetitorDto { CompetitorInTeamId = 2, FirstName = "Wout", LastName = "van Aert" }
        };

            _mockService.Setup(s => s.GetAllCompetitors(DateTime.Now.Year))
                        .ReturnsAsync(competitors);

            // Act
            var result = await _controller.GetCompetitors();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result); ;
            var data = Assert.IsAssignableFrom<IEnumerable<CompetitorDto>>(okResult.Value);
            Assert.Collection(data,
                c => Assert.Equal("Remco", c.FirstName),
                c => Assert.Equal("Wout", c.FirstName));
        }

        [Fact]
        public async Task GetById_ReturnsCompetitor()
        {
            // Arrange
            int competitorId = 5;
            var competitor = new Competitor { CompetitorId = competitorId, FirstName = "Jonas", LastName = "Vingegaard" };

            _mockService.Setup(s => s.GetCompetitorById(competitorId))
                        .ReturnsAsync(competitor);

            // Act
            var result = await _controller.GetById(competitorId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var c = Assert.IsType<Competitor>(okResult.Value);
            Assert.NotNull(result);
            Assert.Equal("Jonas", c.FirstName);
        }

        [Fact]
        public async Task GetByTeamId_ReturnsCompetitorsForTeam()
        {
            // Arrange
            int teamId = 12;
            int year = 2025;
            var teamCompetitors = new List<CompetitorInTeamDto>
            {
                new CompetitorInTeamDto { CompetitorInTeamId = 1, TeamId = teamId, Year = year },
                new CompetitorInTeamDto { CompetitorInTeamId = 2, TeamId = teamId, Year = year }
            };

            _mockService.Setup(s => s.GetByTeamId(teamId, year))
                        .ReturnsAsync(teamCompetitors);

            // Act
            var result = await _controller.GetByTeamId(teamId, year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<IEnumerable<CompetitorInTeamDto>>(okResult.Value);
            Assert.Equal(2, ((List<CompetitorInTeamDto>)data).Count);
            Assert.All(data, d => Assert.Equal(teamId, d.TeamId));
        }

        [Fact]
        public async Task GetCompetitors_Returns500_WhenServiceThrows()
        {
            // Arrange
            _mockService.Setup(s => s.GetAllCompetitors(It.IsAny<int>()))
                        .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _controller.GetCompetitors();

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenCompetitorIsNull()
        {
            // Arrange
            int id = 99;
            _mockService.Setup(s => s.GetCompetitorById(id)).ReturnsAsync((Competitor)null);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetById_Returns500_WhenServiceThrows()
        {
            // Arrange
            _mockService.Setup(s => s.GetCompetitorById(It.IsAny<int>()))
                        .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetByTeamId_ReturnsEmptyList_WhenServiceReturnsNull()
        {
            // Arrange
            int teamId = 10, year = 2025;
            _mockService.Setup(s => s.GetByTeamId(teamId, year))
                        .ReturnsAsync((List<CompetitorInTeamDto>)null);

            // Act
            var result = await _controller.GetByTeamId(teamId, year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<List<CompetitorInTeamDto>>(okResult.Value);
            Assert.Empty(data);
        }

        [Fact]
        public async Task GetByTeamId_Returns500_WhenServiceThrows()
        {
            // Arrange
            _mockService.Setup(s => s.GetByTeamId(It.IsAny<int>(), It.IsAny<int>()))
                        .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _controller.GetByTeamId(1, 2025);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }
    }
}
