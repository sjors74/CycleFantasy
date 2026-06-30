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
            _mockResultsRepo.Setup(r => r.GetResultsByEventId(It.IsAny<int>()))
                .ReturnsAsync(new List<Result>());

            // Act
            var result = await _controller.Index(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<PointsCompetitorInEventViewModel>>(viewResult.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task Index_WithResults_CalculatesPointsAndRanking()
        {
            // Arrange
            var results = new List<Result>
            {
                new Result
                {
                    Stage = new Stage { EventId = 1 },
                    CompetitorInEventId = 10,
                    CompetitorInEvent = new CompetitorsInEvent
                    {
                        CompetitorInTeam = new CompetitorInTeam
                        {
                            Competitor = new Competitor { FirstName = "John", LastName = "Doe" }
                        }
                    },
                    ConfigurationItem = new ConfigurationItem { Score = 5 }
                },
                new Result
                {
                    Stage = new Stage { EventId = 1 },
                    CompetitorInEventId = 11,
                    CompetitorInEvent = new CompetitorsInEvent
                    {
                        CompetitorInTeam = new CompetitorInTeam
                        {
                            Competitor = new Competitor { FirstName = "Alice", LastName = "Smith" }
                        }
                    },
                    ConfigurationItem = new ConfigurationItem { Score = 10 }
                },
            };
            _mockResultsRepo.Setup(r => r.GetResultsByEventId(1)).ReturnsAsync(results);

            // Act
            var result = await _controller.Index(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<PointsCompetitorInEventViewModel>>(viewResult.Model);

            Assert.Equal(2, model.Count);

            // Check sorting (Alice first because 10 > 5)
            Assert.Equal("Alice", model[0].FirstName);
            Assert.Equal(1, model[0].Ranking);
            Assert.Equal("John", model[1].FirstName);
            Assert.Equal(2, model[1].Ranking);
        }

        [Fact]
        public async Task Index_WithTiedScores_AssignsSameRanking()
        {
            // Arrange
            var results = new List<Result>
            {
                new Result
                {
                    Stage = new Stage { EventId = 1 },
                    CompetitorInEventId = 1,
                    CompetitorInEvent = new CompetitorsInEvent
                    {
                        CompetitorInTeam = new CompetitorInTeam
                        {
                            Competitor = new Competitor { FirstName = "A", LastName = "One" }
                        }
                    },
                    ConfigurationItem = new ConfigurationItem { Score = 10 }
                },
                new Result
                {
                    Stage = new Stage { EventId = 1 },
                    CompetitorInEventId = 2,
                    CompetitorInEvent = new CompetitorsInEvent
                    {
                        CompetitorInTeam = new CompetitorInTeam
                        {
                            Competitor = new Competitor { FirstName = "B", LastName = "Two" }
                        }
                    },
                    ConfigurationItem = new ConfigurationItem { Score = 10 }
                }
            };
            _mockResultsRepo.Setup(r => r.GetResultsByEventId(1)).ReturnsAsync(results);

            // Act
            var result = await _controller.Index(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<PointsCompetitorInEventViewModel>>(viewResult.Model);

            Assert.Equal(2, model.Count);
            Assert.All(model, m => Assert.Equal(1, m.Ranking)); // beide 1e plaats
        }

        [Fact]
        public async Task Index_NullConfigurationItem_SetsScoreToZero()
        {
            // Arrange
            var results = new List<Result>
            {
                new Result
                {
                    Stage = new Stage { EventId = 1 },
                    CompetitorInEventId = 1,
                    CompetitorInEvent = new CompetitorsInEvent
                    {
                        CompetitorInTeam = new CompetitorInTeam
                        {
                            Competitor = new Competitor { FirstName = "Null", LastName = "Score" }
                        }
                    },
                    ConfigurationItem = null
                }
            };
            _mockResultsRepo.Setup(r => r.GetResultsByEventId(1)).ReturnsAsync(results);

            // Act
            var result = await _controller.Index(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<PointsCompetitorInEventViewModel>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal(0, model[0].Points);
        }

        [Fact]
        public async Task Index_FiltersOnlyMatchingEventId()
        {
            // Arrange
            var results = new List<Result>
            {
                new Result
                {
                    Stage = new Stage { EventId = 1 },
                    CompetitorInEventId = 1,
                    CompetitorInEvent = new CompetitorsInEvent
                    {
                        CompetitorInTeam = new CompetitorInTeam
                        {
                            Competitor = new Competitor { FirstName = "Keep", LastName = "Me" }
                        }
                    },
                    ConfigurationItem = new ConfigurationItem { Score = 5 }
                },
                new Result
                {
                    Stage = new Stage { EventId = 2 },
                    CompetitorInEventId = 2,
                    CompetitorInEvent = new CompetitorsInEvent
                    {
                        CompetitorInTeam = new CompetitorInTeam
                        {
                            Competitor = new Competitor { FirstName = "Wrong", LastName = "Event" }
                        }
                    },
                    ConfigurationItem = new ConfigurationItem { Score = 50 }
                }
            };
            _mockResultsRepo.Setup(r => r.GetResultsByEventId(1)).ReturnsAsync(results);

            // Act
            var result = await _controller.Index(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<PointsCompetitorInEventViewModel>>(viewResult.Model);

            Assert.Single(model);
            Assert.Equal("Keep", model[0].FirstName);
            Assert.Equal(1, model[0].EventId);
        }
    }
}
