using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebCycleManager.Controllers;
using WebCycleManager.Helpers;
using WebCycleManager.Models;

namespace CycleManager.Tests.Unit.Manager
{
    public class ResultsControllerTests
    {
        private readonly Mock<IResultService> _resultsServiceMock;
        private readonly Mock<IApiClient> _apiClientMock;
        private readonly Mock<IScoreService> _scoreServiceMock;
        private readonly ResultsController _controller;

        public ResultsControllerTests()
        {
            _resultsServiceMock = new Mock<IResultService>();
            _apiClientMock = new Mock<IApiClient>();
            _scoreServiceMock = new Mock<IScoreService>();

            _controller = new ResultsController(_resultsServiceMock.Object, _apiClientMock.Object, _scoreServiceMock.Object);
        }

        [Fact]
        public async Task Index_StageNotFound_ReturnsNotFound()
        {
            // Arrange
            _resultsServiceMock.Setup(s => s.GetStageByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Stage?)null);

            // Act
            var result = await _controller.Index(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Index_ValidStage_ReturnsViewWithViewModel()
        {
            // Arrange
            var stage = new Stage
            {
                Id = 1,
                StageName = "Etappe 1",
                StartLocation = "Brugge",
                FinishLocation = "Gent",
                Event = new Event
                {
                    EventId = 10,
                    Configuration = new Configuration { Id = 20 }
                }
            };

            var configItems = new List<ConfigurationItem>
            {
                new ConfigurationItem { Id = 1, ConfigurationId = 20, Position = 1 },
                new ConfigurationItem { Id = 2, ConfigurationId = 20, Position = 2 }
            };

            var results = new List<Result>
            {
                new Result
                {
                    Id = 1,
                    StageId = 1,
                    ConfigurationItem = configItems.First(),
                    CompetitorInEvent = new CompetitorsInEvent
                    {
                        Id = 5,
                        CompetitorInTeam = new CompetitorInTeam
                        {
                            Competitor = new Competitor
                            {
                                CompetitorId = 99,
                                FirstName = "Remco",
                                LastName = "Evenepoel"
                            }
                        }
                    }
                }
            };

            var competitors = new List<CompetitorsInEvent>
            {
                new CompetitorsInEvent
                {
                    Id = 5,
                    EventId = 10,
                    CompetitorInTeam = new CompetitorInTeam
                    {
                        Competitor = new Competitor { CompetitorId = 99, FirstName = "Remco", LastName = "Evenepoel" }
                    }
                }
            };

            _resultsServiceMock.Setup(s => s.GetStageByIdAsync(1)).ReturnsAsync(stage);
            _resultsServiceMock.Setup(s => s.GetResultsByStageAsync(1)).ReturnsAsync(results);
            _resultsServiceMock
                .Setup(s => s.GetSpecialResultsByStageAsync(1))
                .ReturnsAsync(new List<SpecialResult>());
            _resultsServiceMock.Setup(s => s.GetCompetitorsInEventAsync(10)).ReturnsAsync(competitors);
            _resultsServiceMock.Setup(s => s.GetConfigurationItemsByConfigAsync(20)).ReturnsAsync(configItems);
            _resultsServiceMock
                .Setup(s => s.GetConfigurationItemSpecialsAsync(20))
                .ReturnsAsync(new List<ConfigurationItemSpecial>());
            _resultsServiceMock.Setup(s => s.GetCompetitorFullName(99)).Returns("Remco Evenepoel");

            // Act
            var result = await _controller.Index(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ResultViewModel>(viewResult.Model);
            Assert.Equal(stage.Id, model.StageId);
            Assert.Equal(2, model.Rows.Count);
            Assert.Contains(model.Rows, r => r.CompetitorName == "Remco Evenepoel");
        }

        [Fact]
        public async Task Index_Post_AddsResultsAndRedirects()
        {
            // Arrange
            var model = new ResultViewModel
            {
                EventId = 10,
                StageId = 1,
                ConfigurationId = 20,
                Rows = new List<StageResultRowViewModel>
                {
                    new StageResultRowViewModel { Position = 1, SelectedCompetitorId = 5 },
                    new StageResultRowViewModel { Position = 2, SelectedCompetitorId = 0 }
                }
            };

            var competitors = new List<CompetitorsInEvent>
            {
                new CompetitorsInEvent
                {
                    Id = 5,
                    EventId = 10,
                    CompetitorInTeam = new CompetitorInTeam
                    {
                        Competitor = new Competitor { CompetitorId = 5 }
                    }
                }
            };

            var configItems = new List<ConfigurationItem>
            {
                new ConfigurationItem { Id = 1, ConfigurationId = 20, Position = 1 },
                new ConfigurationItem { Id = 2, ConfigurationId = 20, Position = 2 }
            };

            var specialConfigItems = new List<ConfigurationItemSpecial>
            {
                new ConfigurationItemSpecial { Id = 1, ConfigurationId = 20, Question =  Domain.Enums.QuestionType.KOM, Score = 10 },
                new ConfigurationItemSpecial { Id = 2, ConfigurationId = 20, Question = Domain.Enums.QuestionType.Points, Score = 5 }
            };

            _resultsServiceMock
                .Setup(s => s.GetCompetitorsInEventAsync(10))
                .ReturnsAsync(competitors);

            _resultsServiceMock
                .Setup(s => s.GetConfigurationItemsByConfigAsync(20))
                .ReturnsAsync(configItems);

            _resultsServiceMock
                .Setup(s => s.GetConfigurationItemSpecialsAsync(2))
                .ReturnsAsync(specialConfigItems);

            _resultsServiceMock
                .Setup(s => s.SyncResultsAsync(
                    It.IsAny<int>(),
                    It.IsAny<IEnumerable<Result>>(), 
                    It.IsAny<IEnumerable<SpecialResult>>()))
                .Returns(Task.CompletedTask);

            _scoreServiceMock
                .Setup(s => s.UpdateScoresForStageAsync(10, 1))
                .Returns(Task.CompletedTask);

            _apiClientMock
                .Setup(c => c.PostToApiAsync(It.IsAny<string>()))
                .ReturnsAsync(
                    new System.Net.Http.HttpResponseMessage(
                        System.Net.HttpStatusCode.OK));

            // Act
            var result = await _controller.Index(model);

            // Assert
            _resultsServiceMock.Verify(
                s => s.SyncResultsAsync(
                    1,
                    It.IsAny<IEnumerable<Result>>(), 
                    It.IsAny<IEnumerable<SpecialResult>>()),
                Times.Once);

            _scoreServiceMock.Verify(
                 s => s.UpdateScoresForStageAsync(10, 1),
                Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(model.StageId, redirect.RouteValues?["stageId"]);
        }

        [Fact]
        public async Task Delete_ResultNotFound_ReturnsNotFound()
        {
            // Arrange
            _resultsServiceMock.Setup(s => s.GetResultByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Result?)null);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_RemovesResultAndRedirects()
        {
            // Arrange
            var resultEntity = new Result { Id = 1, StageId = 7 };
            _resultsServiceMock.Setup(s => s.GetResultByIdAsync(It.IsAny<int>())).ReturnsAsync(resultEntity);
            _resultsServiceMock.Setup(s => s.DeleteResultAsync(It.IsAny<Result>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            _resultsServiceMock.Verify(s => s.DeleteResultAsync(resultEntity), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(resultEntity.StageId, redirect.RouteValues?["stageId"]);
        }

        [Fact]
        public async Task Delete_NullId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Delete(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ValidId_ReturnsViewWithViewModel()
        {
            // Arrange
            var resultEntity = new Result
            {
                Id = 1,
                StageId = 7,
                ConfigurationItem = new ConfigurationItem { Position = 3 },
                CompetitorInEvent = new CompetitorsInEvent
                {
                    CompetitorInTeam = new CompetitorInTeam
                    {
                        Competitor = new Competitor
                        {
                            FirstName = "Remco",
                            LastName = "Evenepoel"
                        }
                    }
                }
            };
            _resultsServiceMock.Setup(s => s.GetResultByIdAsync(It.IsAny<int>())).ReturnsAsync(resultEntity);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ResultItemViewModel>(viewResult.Model);
            Assert.Equal("Remco Evenepoel", model.CompetitorName);
            Assert.Equal(3, model.Position);
        }
    }
}
