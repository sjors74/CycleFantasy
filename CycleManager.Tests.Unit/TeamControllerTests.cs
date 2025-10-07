using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebCycle.Controllers;

namespace CycleManager.Tests.Unit
{
    public class TeamControllerTests
    {
        [Fact]
        public async Task GetAllTeams_ReturnsListOfTeams()
        {
            // Arrange
            var mockService = new Mock<ITeamService>();
            var expectedTeams = new List<Team>
            {
                new Team { TeamId = 1, CurrentTeamName = "Team A" },
                new Team { TeamId = 2, CurrentTeamName = "Team B" }
            };

            mockService.Setup(s => s.GetAllTeams()).ReturnsAsync(expectedTeams);

            var controller = new TeamController(mockService.Object);

            // Act
            var result = await controller.GetAllTeams();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var teams = Assert.IsAssignableFrom<IEnumerable<Team>>(okResult.Value);
            Assert.Equal(2, ((List<Team>)teams).Count);
        }

        [Fact]
        public async Task GetTeam_ReturnsTeam_WhenExists()
        {
            // Arrange
            var mockService = new Mock<ITeamService>();
            var team = new Team { TeamId = 5, CurrentTeamName = "TestTeam" };

            mockService.Setup(s => s.GetTeamById(5)).ReturnsAsync(team);

            var controller = new TeamController(mockService.Object);

            // Act
            var result = await controller.GetTeam(5);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTeam = Assert.IsType<Team>(okResult.Value);
            Assert.Equal(5, returnedTeam.TeamId);
            Assert.Equal("TestTeam", returnedTeam.CurrentTeamName);
        }

        [Fact]
        public async Task GetTeam_ReturnsNotFound_WhenTeamDoesNotExist()
        {
            // Arrange
            var mockService = new Mock<ITeamService>();
            mockService.Setup(s => s.GetTeamById(It.IsAny<int>()))
                       .ReturnsAsync((Team)null);

            var controller = new TeamController(mockService.Object);

            // Act
            var result = await controller.GetTeam(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
