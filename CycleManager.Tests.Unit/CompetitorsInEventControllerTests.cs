using AutoMapper;
using CycleManager.Services.Interfaces;
using Domain.Dto;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebCycle.Controllers;

namespace CycleManager.Tests.Unit
{
    public class CompetitorsInEventControllerTests
    {
        private readonly Mock<ICompetitorInEventService> _mockService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CompetitorsInEventController _controller;

        public CompetitorsInEventControllerTests()
        {
            _mockService = new Mock<ICompetitorInEventService>();
            _mockMapper = new Mock<IMapper>();

            _controller = new CompetitorsInEventController(
                _mockService.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task GetById_ReturnsOk_WithMappedCompetitor()
        {
            // Arrange
            int id = 1;
            var competitorInEvent = new CompetitorsInEvent
            {
                Id = id,
                CompetitorInTeamId = 10,
                EventId = 5,
                EventNumber = 22,
                InSelectie = true
            };

            var dto = new CompetitorDto
            {
                CompetitorInTeamId = 10,
                FirstName = "Remco",
                LastName = "Evenepoel"
            };

            _mockService.Setup(s => s.GetCompetitorById(id)).ReturnsAsync(competitorInEvent);
            _mockMapper.Setup(m => m.Map<CompetitorDto>(competitorInEvent)).Returns(dto);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<CompetitorDto>(okResult.Value);
            Assert.Equal(competitorInEvent.CompetitorInTeamId, data.CompetitorInTeamId);
            Assert.Equal("Remco", data.FirstName);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenCompetitorDoesNotExist()
        {
            // Arrange
            int id = 99;
            _mockService.Setup(s => s.GetCompetitorById(id))
                        .ReturnsAsync((CompetitorsInEvent)null);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetById_ReturnsStatus500_WhenExceptionThrown()
        {
            // Arrange
            int id = 1;
            _mockService.Setup(s => s.GetCompetitorById(id))
                        .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Er ging iets mis bij het ophalen van de renner.", objectResult.Value);
        }

        [Fact]
        public async Task GetById_DoesNotCallMapper_WhenCompetitorIsNull()
        {
            // Arrange
            int id = 42;
            _mockService.Setup(s => s.GetCompetitorById(id))
                        .ReturnsAsync((CompetitorsInEvent)null);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);

            // Controleer dat de mapper nooit werd aangeroepen
            _mockMapper.Verify(m => m.Map<CompetitorDto>(It.IsAny<CompetitorsInEvent>()), Times.Never);
        }

        [Fact]
        public async Task GetRandomById_ReturnsOk_WithMappedCompetitors()
        {
            // Arrange
            int eventId = 5;
            int number = 3;

            var competitors = new List<CompetitorsInEvent>
            {
                new CompetitorsInEvent { Id = 1 },
                new CompetitorsInEvent { Id = 2 },
                new CompetitorsInEvent { Id = 3 }
            };

            var competitorsDto = new List<CompetitorDto>
            {
                new CompetitorDto { CompetitorInTeamId = 1, FirstName = "Co", LastName = "mpetitor 1" },
                new CompetitorDto { CompetitorInTeamId = 2, FirstName = "Com", LastName = "petitor 2" },
                new CompetitorDto { CompetitorInTeamId = 3, FirstName = "Comp", LastName = "etitor 3" }
            };

            _mockService.Setup(s => s.GetRandomNumberofCompetitors(eventId, number))
                        .ReturnsAsync(competitors);

            _mockMapper.Setup(m => m.Map<List<CompetitorDto>>(competitors))
                       .Returns(competitorsDto);

            // Act
            var result = await _controller.GetRandomById(eventId, number);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<List<CompetitorDto>>(okResult.Value);
            Assert.Equal(3, data.Count);
            Assert.Equal("Co", data[0].FirstName);
        }

        [Fact]
        public async Task GetRandomById_ReturnsNotFound_WhenNoCompetitorsFound()
        {
            // Arrange
            int eventId = 2;
            int number = 5;

            _mockService.Setup(s => s.GetRandomNumberofCompetitors(eventId, number))
                        .ReturnsAsync((List<CompetitorsInEvent>)null);

            // Act
            var result = await _controller.GetRandomById(eventId, number);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Geen renners gevonden", notFound.Value.ToString());
        }

        [Fact]
        public async Task GetRandomById_ReturnsStatus500_WhenServiceThrowsException()
        {
            // Arrange
            int eventId = 10;
            int number = 2;

            _mockService.Setup(s => s.GetRandomNumberofCompetitors(eventId, number))
                        .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetRandomById(eventId, number);

            // Assert
            var status = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, status.StatusCode);
            Assert.Equal("Er is een fout opgetreden bij het ophalen van de renners.", status.Value);
        }
    }
}