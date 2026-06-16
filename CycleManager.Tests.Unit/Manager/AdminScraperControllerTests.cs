using CycleManager.Domain.Dto;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebCycleManager.Controllers;

namespace CycleManager.Tests.Unit.Manager
{
    public class AdminScraperControllerTests
    {
        private readonly Mock<IAdminScraperService> _adminScraperServiceMock;
        private readonly Mock<IScoreService> _scoreServiceMock;
        private readonly Mock<IScraperService> _scraperServiceMock;
        private readonly AdminScraperController _controller;

        public AdminScraperControllerTests()
        {
            _adminScraperServiceMock = new Mock<IAdminScraperService>();
            _scoreServiceMock = new Mock<IScoreService>();
            _scraperServiceMock = new Mock<IScraperService>();

            _controller = new AdminScraperController(
                _scraperServiceMock.Object,
                _scoreServiceMock.Object,
                _adminScraperServiceMock.Object
            );

            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()
            );
        }

        [Fact]
        public async Task ScrapeAndPair_StageNotFound_ReturnsRedirectWithError()
        {
            // Arrange
            _adminScraperServiceMock.Setup(s => s.GetStageByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Stage?)null);

            // Act
            var result = await _controller.ScrapeAndPair(1, 2, "Tour", 2025);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
            Assert.Equal("Events", redirect.ControllerName);
            Assert.Equal(2, redirect.RouteValues["eventId"]);
            Assert.Equal("Stage niet gevonden.", _controller.TempData["Error"]);
        }

        [Fact]
        public async Task ScrapeAndPair_ValidStage_CallsScraperAndScoreService_AndRedirects()
        {
            // Arrange
            var stage = new Stage
            {
                Id = 1,
                StageName = "1",
                Event = new Event { EventId = 2 }
            };

            _adminScraperServiceMock.Setup(s => s.GetStageByIdAsync(1)).ReturnsAsync(stage);
            _scraperServiceMock.Setup(s => s.RunAsync(2, "Tour", 1, 2025)).Returns(Task.CompletedTask);
            _scoreServiceMock.Setup(s => s.UpdateScoresForStageAsync(2, 1)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ScrapeAndPair(1, 2, "Tour", 2025);

            // Assert
            _scraperServiceMock.Verify(s => s.RunAsync(2, "Tour", 1, 2025), Times.Once);
            _scoreServiceMock.Verify(s => s.UpdateScoresForStageAsync(2, 1), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
            Assert.Equal("Events", redirect.ControllerName);
            Assert.Equal(2, redirect.RouteValues["id"]);
            Assert.Equal("Scrape voltooid.", _controller.TempData["Success"]);
        }

        [Fact]
        public async Task ScrapeDropouts_CallsScraperService_AndRedirects()
        {
            // Arrange
            _scraperServiceMock.Setup(s => s.RunDropoutsAsync(2, "Tour", 2025)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ScrapeDropouts(2, "Tour", 2025);

            // Assert
            _scraperServiceMock.Verify(s => s.RunDropoutsAsync(2, "Tour", 2025), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
            Assert.Equal("Events", redirect.ControllerName);
            Assert.Equal(2, redirect.RouteValues["id"]);
        }

        [Fact]
        public async Task ScrapeCompetitors_TeamNotFound_ThrowsException()
        {
            // Arrange
            var dto = new ScrapeRequestDto { TeamId = 5, Year = 2025 };
            _adminScraperServiceMock.Setup(s => s.GetTeamByIdAsync(5))
                .ReturnsAsync((Team?)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.ScrapeCompetitors(dto));
        }

        [Fact]
        public async Task ScrapeCompetitors_ValidTeam_CallsService_AndReturnsOk()
        {
            // Arrange
            var dto = new ScrapeRequestDto { TeamId = 5, Year = 2025 };
            _adminScraperServiceMock.Setup(s => s.GetTeamByIdAsync(5))
                .ReturnsAsync(new Team { TeamId = 5 });
            _scraperServiceMock.Setup(s => s.RunCompetitorsAsync(5, 2025))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ScrapeCompetitors(dto);

            // Assert
            _scraperServiceMock.Verify(s => s.RunCompetitorsAsync(5, 2025), Times.Once);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ImportScrapedCompetitors_CallsService_AndRedirects()
        {
            // Arrange
            _scraperServiceMock.Setup(s => s.ImportScrapedCompetitorsAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _controller.ImportScrapedCompetitors();

            // Assert
            _scraperServiceMock.Verify(s => s.ImportScrapedCompetitorsAsync(), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Teams", redirect.ControllerName);
        }
    }
}
