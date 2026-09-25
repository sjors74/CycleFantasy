using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using DataAccessEF.Migrations;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCycleManager.Controllers;
using WebCycleManager.Models;

namespace CycleManager.Tests.Unit.Manager
{
    public class PointsControllerTests
    {
        private readonly Mock<IResultsRepository> _mockResultsRepo;
        private readonly Mock<IResultService> _mockResultService;
        private readonly PointsController _controller;

        public PointsControllerTests()
        {
            _mockResultsRepo = new Mock<IResultsRepository>();
            _mockResultService = new Mock<IResultService>();
            _controller = new PointsController(_mockResultsRepo.Object, _mockResultService.Object);
        }

        [Fact]
        public async Task Index_NoResults_ReturnsEmptyList()
        {
            // Arrange
            _mockResultService
                .Setup(s => s.GetResultsByEventId(It.IsAny<int>()))
                .ReturnsAsync(new List<CompetitorRankingDto>());

            // Act
            var result = await _controller.Index(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<CompetitorRankingDto>>(viewResult.Model);

            Assert.Empty(model);
        }

        [Fact]
        public async Task Index_WithResults_ReturnsResultsWithRanking()
        {
            // Arrange
            const int eventId = 1;

            var rankings = new List<CompetitorRankingDto>
            {
                new CompetitorRankingDto
                {
                    EventId = eventId,
                    CompetitorInEventId = 11,
                    CompetitorName = "Alice Smith",
                    NormalPoints = 10,
                    SpecialPoints = 0,
                    Position = 1
                },
                new CompetitorRankingDto
                {
                    EventId = eventId,
                    CompetitorInEventId = 10,
                    CompetitorName = "John Doe",
                    NormalPoints = 5,
                    SpecialPoints = 0,
                    Position = 2
                }
            };

            _mockResultService
                .Setup(s => s.GetResultsByEventId(eventId))
                .ReturnsAsync(rankings);

            // Act
            var result = await _controller.Index(eventId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<CompetitorRankingDto>>(viewResult.Model)
                .ToList();

            Assert.Equal(2, model.Count);

            Assert.Equal("Alice Smith", model[0].CompetitorName);
            Assert.Equal(1, model[0].Position);

            Assert.Equal("John Doe", model[1].CompetitorName);
            Assert.Equal(2, model[1].Position);
        }

        [Fact]
        public async Task Index_WithTiedScores_ReturnsResultsWithSameRanking()
        {
            // Arrange
            const int eventId = 1;

            var rankings = new List<CompetitorRankingDto>
            {
                new CompetitorRankingDto
                {
                    EventId = eventId,
                    CompetitorInEventId = 1,
                    CompetitorName = "A One",
                    NormalPoints = 10,
                    SpecialPoints = 0,
                    Position = 1
                },
                new CompetitorRankingDto
                {
                    EventId = eventId,
                    CompetitorInEventId = 2,
                    CompetitorName = "B Two",
                    NormalPoints = 10,
                    SpecialPoints = 0,
                    Position = 1
                }
            };

            _mockResultService
                .Setup(s => s.GetResultsByEventId(eventId))
                .ReturnsAsync(rankings);

            // Act
            var result = await _controller.Index(eventId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<CompetitorRankingDto>>(viewResult.Model)
                .ToList();

            Assert.Equal(2, model.Count);
            Assert.All(model, m => Assert.Equal(1, m.Position));
        }

        [Fact]
        public async Task Index_ReturnsResultsForEvent()
        {
            // Arrange
            int eventId = 1;

            var rankings = new List<CompetitorRankingDto>
            {
                new CompetitorRankingDto
                {
                    EventId = eventId,
                    CompetitorInEventId = 1,
                    CompetitorName = "Keep Me",
                    NormalPoints = 5
                }
            };

            _mockResultService
                .Setup(s => s.GetResultsByEventId(eventId))
                .ReturnsAsync(rankings);

            // Act
            var result = await _controller.Index(eventId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<CompetitorRankingDto>>(viewResult.Model);

            Assert.Single(model);
            Assert.Equal(eventId, model[0].EventId);
            Assert.Equal("Keep Me", model[0].CompetitorName);

            _mockResultService.Verify(
                s => s.GetResultsByEventId(eventId),
                Times.Once);
        }
    }
}
