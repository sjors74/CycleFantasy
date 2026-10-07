using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Moq;

namespace CycleManager.Tests.Unit.Services
{
    public class ScrapeOrchestratorServiceTests
    {
        private readonly Mock<IScraperService> _scraperServiceMock;
        private readonly Mock<IScoreService> _scoreServiceMock;
        private readonly ScrapeOrchestratorService _service;

        public ScrapeOrchestratorServiceTests()
        {
            _scraperServiceMock = new Mock<IScraperService>();
            _scoreServiceMock = new Mock<IScoreService>();

            _service = new ScrapeOrchestratorService(
                _scraperServiceMock.Object,
                _scoreServiceMock.Object);
        }

        [Fact]
        public async Task RunStageScrapeAsync_RunsScrapeAndUpdatesScores()
        {
            // Act
            await _service.RunStageScrapeAsync(
                eventId: 1,
                eventName: "Tour de Test",
                stageId: 10,
                stageNumber: 3,
                year: 2026);

            // Assert
            _scraperServiceMock.Verify(
                x => x.RunAsync(1, "Tour de Test", 3, 2026),
                Times.Once);

            _scoreServiceMock.Verify(
                x => x.UpdateScoresForStageAsync(1, 10),
                Times.Once);
        }

        [Fact]
        public async Task RefreshStartlistAsync_RefreshesStartlist()
        {
            // Act
            await _service.RefreshStartlistAsync(5);

            // Assert
            _scraperServiceMock.Verify(
                x => x.RefreshStartlistAsync(5),
                Times.Once);
        }
    }
}