using CycleManager.Domain.Dto;
using CycleManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCycle.Controllers;

namespace CycleManager.Tests.Unit
{
    public class ResultsControllerTests
    {
        private readonly Mock<IResultService> _mockResultService;
        private readonly ResultsController _controller;

        public ResultsControllerTests()
        {
            _mockResultService = new Mock<IResultService>();
            _controller = new ResultsController(_mockResultService.Object);
        }

        [Fact]
        public async Task GetById_ReturnsIntResult()
        {
            // Arrange
            _mockResultService.Setup(s => s.GetResultsByStageId(10)).ReturnsAsync(42);

            // Act
            var result = await _controller.GetById(10);

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public async Task GetEtappeUitslag_ReturnsOk_WithResultList()
        {
            // Arrange
            var stageId = 5;
            var uitslag = new List<EtappeUitslagDto>
            {
                new EtappeUitslagDto { Positie = 1, CompetitorName = "Annemiek", Score = 50 },
                new EtappeUitslagDto { Positie = 2, CompetitorName = "Mathieu", Score = 30 }
            };

            _mockResultService.Setup(s => s.GetEtappeUitslag(stageId)).ReturnsAsync(uitslag);

            // Act
            var result = await _controller.GetEtappeUitslag(stageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<IEnumerable<EtappeUitslagDto>>(okResult.Value);
            Assert.Equal(2, ((List<EtappeUitslagDto>)data).Count);
        }

        [Fact]
        public async Task GetTop15_ReturnsOk_WithTop15Results()
        {
            // Arrange
            var eventId = 1;
            var top15 = new List<ResultDto>
            {
                new ResultDto { CompetitorName = "Pogacar",  Points = 150 },
                new ResultDto { CompetitorName = "Vingegaard", Points = 145 }
            };

            _mockResultService.Setup(s => s.GetResultsByEventId(eventId)).ReturnsAsync(top15);

            // Act
            var result = await _controller.GetTop15(eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<IEnumerable<ResultDto>>(okResult.Value);
            Assert.Equal(2, ((List<ResultDto>)data).Count);
        }

        [Fact]
        public async Task GetResultsByEventAndStageNumber_ReturnsOk_WithPoolRanking()
        {
            // Arrange
            var eventId = 3;
            var stageId = 8;
            var poolRanking = new List<DeelnemerDto>
            {
                new DeelnemerDto {  DeelnemerNaam = "Remco 1", Punten = 100 },
                new DeelnemerDto { DeelnemerNaam = "Remco 2", Punten = 90}
            };

            _mockResultService.Setup(s => s.GetPoolRankingForStage(eventId, stageId)).ReturnsAsync(poolRanking);

            // Act
            var result = await _controller.GetResultsByEventAndStageNumber(eventId, stageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<IEnumerable<DeelnemerDto>>(okResult.Value);
            Assert.Equal(2, ((List<DeelnemerDto>)data).Count);
            _mockResultService.Verify(s => s.GetPoolRankingForStage(eventId, stageId), Times.Once, "GetPoolRankingForStage should be called exactly once with the given event and stage IDs.");
        }

        [Fact]
        public async Task GetResultsByEventAndStageNumber_ReturnsOk_WithEmptyList_WhenNoResults()
        {
            // Arrange
            var eventId = 3;
            var stageId = 8;

            _mockResultService
                .Setup(s => s.GetPoolRankingForStage(eventId, stageId))
                .ReturnsAsync((List<DeelnemerDto>)null); // simuleer geen resultaten

            // Act
            var result = await _controller.GetResultsByEventAndStageNumber(eventId, stageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<IEnumerable<DeelnemerDto>>(okResult.Value);
            Assert.Empty(data); // verwacht lege collectie

            _mockResultService.Verify(s => s.GetPoolRankingForStage(eventId, stageId), Times.Once);
        }

        [Fact]
        public async Task GetResultsByEventAndStageNumber_Returns500_OnException()
        {
            // Arrange
            var eventId = 3;
            var stageId = 8;

            _mockResultService
                .Setup(s => s.GetPoolRankingForStage(eventId, stageId))
                .ThrowsAsync(new Exception("Database fout")); // simuleer fout

            // Act
            var result = await _controller.GetResultsByEventAndStageNumber(eventId, stageId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Fout bij ophalen van resultaten.", statusCodeResult.Value);

            _mockResultService.Verify(s => s.GetPoolRankingForStage(eventId, stageId), Times.Once);
        }
    }
}
