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

            _resultsServiceMock.Setup(s => s.GetStageByIdAsync(It.IsAny<int>())).ReturnsAsync(stage);
            _resultsServiceMock.Setup(s => s.GetResultsByStageAsync(It.IsAny<int>())).ReturnsAsync(results);
            _resultsServiceMock.Setup(s => s.GetCompetitorsInEventAsync(It.IsAny<int>())).ReturnsAsync(competitors);
            _resultsServiceMock.Setup(s => s.GetConfigurationItemsByConfigAsync(It.IsAny<int>())).ReturnsAsync(configItems);
            _resultsServiceMock.Setup(s => s.GetCompetitorFullName(It.IsAny<int>())).Returns("Remco Evenepoel");

            // Act
            var result = await _controller.Index(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ResultViewModel>(viewResult.Model);
            Assert.Equal(stage.Id, model.StageId);
            Assert.Equal(2, model.Results.Count);
            Assert.Contains(model.Results, r => r.CompetitorName == "Remco Evenepoel");
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
                Results = new List<ResultItemViewModel>
                {
                    new ResultItemViewModel { Position = 1, SelectedCompetitorId = 5 },
                    new ResultItemViewModel { Position = 2, SelectedCompetitorId = 0 }
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

            _resultsServiceMock.Setup(s => s.GetCompetitorsInEventAsync(It.IsAny<int>())).ReturnsAsync(competitors);
            _resultsServiceMock.Setup(s => s.GetConfigurationItemsByConfigAsync(It.IsAny<int>())).ReturnsAsync(configItems);
            _resultsServiceMock.Setup(s => s.AddResultsAsync(It.IsAny<IEnumerable<Result>>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _apiClientMock.Setup(c => c.PostToApiAsync(It.IsAny<string>()))
                .ReturnsAsync(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK));

            _scoreServiceMock.Setup(s => s.UpdateScoresForStageAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Index(model);

            // Assert
            _resultsServiceMock.Verify(s => s.AddResultsAsync(It.IsAny<IEnumerable<Result>>()), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(model.StageId, redirect.RouteValues["stageId"]);
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
            Assert.Equal(resultEntity.StageId, redirect.RouteValues["stageId"]);
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
