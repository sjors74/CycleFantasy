using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using WebCycleManager.Controllers;
using WebCycleManager.Models;

namespace CycleManager.Tests.Unit.Manager
{
    public class GameCompetitorEventsControllerTests
    {
        private readonly Mock<IGameCompetitorInEventService> _mockGameCompetitorEventService;
        private readonly Mock<IResultService> _mockResultService;
        private readonly Mock<IEventService> _mockEventService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IRatingService> _mockRatingService;
        private readonly Mock<ICompetitorInEventService> _mockCompetitorInEventService;
        private readonly GameCompetitorEventsController _controller;

        public GameCompetitorEventsControllerTests()
        {
            _mockGameCompetitorEventService = new Mock<IGameCompetitorInEventService>();
            _mockResultService = new Mock<IResultService>();
            _mockEventService = new Mock<IEventService>();
            _mockUserService = new Mock<IUserService>();
            _mockRatingService = new Mock<IRatingService>();
            _mockCompetitorInEventService = new Mock<ICompetitorInEventService>();

            _controller = new GameCompetitorEventsController(
                _mockGameCompetitorEventService.Object,
                _mockResultService.Object,
                _mockEventService.Object,
                _mockUserService.Object,
                _mockCompetitorInEventService.Object,
                _mockRatingService.Object
            );
        }

        #region Index Tests

        [Fact]
        public async Task Index_ValidEventId_ReturnsViewWithModel()
        {
            // Arrange
            int eventId = 1;
            _mockResultService.Setup(s => s.GetResultsByEventId(eventId, false))
                .ReturnsAsync(new List<CompetitorRankingDto> {
                    new CompetitorRankingDto { CompetitorInEventId = 10, NormalPoints = 3, SpecialPoints = 2 }
                });

            _mockGameCompetitorEventService.Setup(s => s.GetPicks(eventId))
                .Returns(new List<GameCompetitorEventPick>().AsQueryable());

            _mockGameCompetitorEventService.Setup(s => s.GetAllCompetitorsInEvent(eventId))
                .ReturnsAsync(new List<GameCompetitorEvent> {
                    new GameCompetitorEvent
                    {
                        Id = 1, EventId = eventId, TeamName = "TeamX",
                        User = new ApplicationUser { FirstName = "John", LastName = "Doe" }
                    }
                });

            // Act
            var result = await _controller.Index(eventId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<List<GameCompetitorInEventViewModel>>(result.Model);
            Assert.Single(model);
            Assert.Equal("TeamX", model.First().TeamName);
            Assert.Equal("John Doe", model.First().GameCompetitorName);
        }

        [Fact]
        public async Task Index_NoResults_ReturnsEmptyModel()
        {
            // Arrange
            int eventId = 1;
            _mockResultService.Setup(s => s.GetResultsByEventId(eventId, false))
                .ReturnsAsync(new List<CompetitorRankingDto>());  // geen resultaten
            _mockGameCompetitorEventService.Setup(s => s.GetPicks(eventId))
                .Returns(new List<GameCompetitorEventPick>().AsQueryable());
            _mockGameCompetitorEventService.Setup(s => s.GetAllCompetitorsInEvent(eventId))
                .ReturnsAsync(new List<GameCompetitorEvent>());  // geen teams

            // Act
            var result = await _controller.Index(eventId);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<GameCompetitorInEventViewModel>>(view.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task Index_WithRatingsAndMissingRanking_ReturnsScoresAndRatings()
        {
            // Arrange
            int eventId = 1;

            _mockResultService
                .Setup(s => s.GetResultsByEventId(eventId))
                .ReturnsAsync(new List<CompetitorRankingDto>
                {
                    new CompetitorRankingDto
                    {
                        CompetitorInEventId = 10,
                        NormalPoints = 5,
                        SpecialPoints = 3
                    }
                });

            _mockGameCompetitorEventService
                .Setup(s => s.GetPicks(eventId))
                .Returns(new List<GameCompetitorEventPick>
                {
                    new GameCompetitorEventPick
                    {
                        GameCompetitorEventId = 1,
                        GameCompetitorEvent = new GameCompetitorEvent
                        {
                            Id = 1
                        },
                        CompetitorsInEventId = 10
                    },
                    new GameCompetitorEventPick
                    {
                        GameCompetitorEventId = 1,
                        GameCompetitorEvent = new GameCompetitorEvent
                        {
                            Id = 1
                        },
                        CompetitorsInEventId = 99 // bestaat niet in rankingLookup
                    }
                }.AsQueryable());

            _mockGameCompetitorEventService
                .Setup(s => s.GetAllCompetitorsInEvent(eventId))
                .ReturnsAsync(new List<GameCompetitorEvent>
                {
                    new GameCompetitorEvent
                    {
                        Id = 1,
                        EventId = eventId,
                        TeamName = "TeamX",
                        User = new ApplicationUser
                        {
                            FirstName = "John",
                            LastName = "Doe"
                        }
                    }
                });

            _mockRatingService
                .Setup(s => s.GetGameCompetitorRatings(eventId))
                .ReturnsAsync(new List<DeelnemerRatingDto>
                {
                    new DeelnemerRatingDto
                    {
                        GameCompetitorEventId = 1,
                        RatingCategoryId = 2,
                        RatingCategoryName = "GC",
                        Color = "red",
                        Rating = 8
                    }
                });

            // Act
            var result = await _controller.Index(eventId);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<GameCompetitorInEventViewModel>>(view.Model);

            var team = Assert.Single(model);

            Assert.Equal(5, team.NormalScore);
            Assert.Equal(3, team.SpecialScore);

            var rating = Assert.Single(team.Ratings);
            Assert.Equal(2, rating.RatingCategoryId);
            Assert.Equal("GC", rating.RatingCategoryName);
            Assert.Equal("red", rating.Color);
            Assert.Equal(8, rating.Rating);
        }

        [Fact]
        public async Task Index_GameCompetitorWithoutRatings_ReturnsEmptyRatings()
        {
            // Arrange
            int eventId = 1;

            _mockResultService
                .Setup(s => s.GetResultsByEventId(eventId))
                .ReturnsAsync(new List<CompetitorRankingDto>());

            _mockGameCompetitorEventService
                .Setup(s => s.GetPicks(eventId))
                .Returns(new List<GameCompetitorEventPick>().AsQueryable());

            _mockGameCompetitorEventService
                .Setup(s => s.GetAllCompetitorsInEvent(eventId))
                .ReturnsAsync(new List<GameCompetitorEvent>
                {
            new GameCompetitorEvent
            {
                Id = 1,
                EventId = eventId,
                TeamName = "TeamX",
                User = new ApplicationUser
                {
                    FirstName = "John",
                    LastName = "Doe"
                }
            }
                });

            _mockRatingService
                .Setup(s => s.GetGameCompetitorRatings(eventId))
                .ReturnsAsync(new List<DeelnemerRatingDto>());

            // Act
            var result = await _controller.Index(eventId);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<GameCompetitorInEventViewModel>>(view.Model);

            var team = Assert.Single(model);

            Assert.Empty(team.Ratings);
            Assert.Equal(0, team.NormalScore);
            Assert.Equal(0, team.SpecialScore);
        }

        #endregion
        #region Details Tests

        [Fact]
        public async Task Details_InvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Details(null, null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ValidId_ReturnsViewWithModel()
        {
            // Arrange
            int id = 1;
            int eventId = 2;

            var gameEvent = new Event
            {
                EventId = eventId,
                Configuration = new Configuration
                {
                    ConfigurationItems = new List<ConfigurationItem>
            {
                new ConfigurationItem()
            }
                }
            };

            _mockEventService
                .Setup(s => s.GetEventById(eventId))
                .ReturnsAsync(gameEvent);

            _mockResultService
                .Setup(s => s.GetResultsByEventId(eventId))
                .ReturnsAsync(new List<CompetitorRankingDto>());

            _mockGameCompetitorEventService
                .Setup(s => s.GetPicks(eventId))
                .Returns(new List<GameCompetitorEventPick>().AsQueryable());

            _mockCompetitorInEventService
                .Setup(s => s.GetCompetitors(eventId))
                .ReturnsAsync(new List<CompetitorsInEvent>());

            _mockRatingService
                .Setup(s => s.GetRatingsByCompetitorIds(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<CompetitorRating>());

            // Act
            var result = await _controller.Details(id, eventId) as ViewResult;

            // Assert
            Assert.NotNull(result);

            var model = Assert.IsAssignableFrom<GameCompetitorInEventViewModel>(result.Model);

            Assert.Equal(eventId, model.EventId);
        }

        [Fact]
        public async Task Details_IdOrEventIdNull_ReturnsNotFound()
        {
            var result = await _controller.Details(null, 1);
            Assert.IsType<NotFoundResult>(result);

            result = await _controller.Details(1, null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_LessThan15Picks_FillsWithEmptyRows()
        {
            int eventId = 1;
            int id = 2;

            var gameEvent = new Event
            {
                EventId = eventId,
                Configuration = new Configuration
                {
                    ConfigurationItems = Enumerable
                        .Range(1, 15)
                        .Select(i => new ConfigurationItem())
                        .ToList()
                }
            };

            _mockEventService
                .Setup(s => s.GetEventById(eventId))
                .ReturnsAsync(gameEvent);

            _mockResultService
                .Setup(s => s.GetResultsByEventId(eventId))
                .ReturnsAsync(new List<CompetitorRankingDto>());

            _mockGameCompetitorEventService
                .Setup(s => s.GetPicks(eventId))
                .Returns(new List<GameCompetitorEventPick>().AsQueryable());

            _mockCompetitorInEventService
                .Setup(s => s.GetCompetitors(eventId))
                .ReturnsAsync(new List<CompetitorsInEvent>());

            _mockRatingService
                .Setup(s => s.GetRatingsByCompetitorIds(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<CompetitorRating>());

            var result = await _controller.Details(id, eventId);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GameCompetitorInEventViewModel>(view.Model);

            Assert.Equal(15, model.CompetitorsInEvent.Count);
        }

        [Fact]
        public async Task Details_Post_InvalidModel_ReturnsSameView()
        {
            // Arrange
            var model = new GameCompetitorInEventViewModel
            {
                EventId = 1,
                CompetitorsInEvent = new List<PickDetailViewModel> { new PickDetailViewModel() }
            };
            _controller.ModelState.AddModelError("Error", "Invalid");

            _mockCompetitorInEventService.Setup(s => s.GetCompetitors(1))
                .ReturnsAsync(new List<CompetitorsInEvent>());

            // Act
            var result = await _controller.Details(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(model, result.Model);
        }

        [Fact]
        public async Task Details_Post_ValidModel_AddsPicksAndRedirects()
        {
            // Arrange
            var model = new GameCompetitorInEventViewModel
            {
                Id = 1,
                EventId = 2,
                CompetitorsInEvent = new List<PickDetailViewModel>
                {
                    new PickDetailViewModel { PickId = 0, SelectedCompetitorId = 5 }
                }
            };

            // Act
            var result = await _controller.Details(model) as RedirectToActionResult;

            // Assert
            _mockGameCompetitorEventService.Verify(s => s.AddPicks(It.IsAny<List<GameCompetitorEventPick>>()), Times.Once);
            Assert.NotNull(result);
            Assert.Equal("Details", result.ActionName);
            Assert.Equal(model.EventId, result.RouteValues?["eventId"]);
        }

        [Fact]
        public async Task Details_Post_InvalidModel_ReturnsViewWithCompetitors()
        {
            var model = new GameCompetitorInEventViewModel
            {
                EventId = 1,
                CompetitorsInEvent = new List<PickDetailViewModel> { new() }
            };
            _controller.ModelState.AddModelError("Error", "Invalid");
            _mockCompetitorInEventService.Setup(s => s.GetCompetitors(1))
                .ReturnsAsync(new List<CompetitorsInEvent>());

            var result = await _controller.Details(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, view.Model);
        }

        [Fact]
        public async Task Details_Post_NoNewPicks_DoesNotCallAddPicks()
        {
            var model = new GameCompetitorInEventViewModel
            {
                EventId = 1,
                Id = 5,
                CompetitorsInEvent = new List<PickDetailViewModel>()
            };

            var result = await _controller.Details(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
            _mockGameCompetitorEventService.Verify(s => s.AddPicks(It.IsAny<List<GameCompetitorEventPick>>()), Times.Never);
        }

        [Fact]
        public async Task Details_Post_WithNewPicks_CallsAddPicks()
        {
            var model = new GameCompetitorInEventViewModel
            {
                EventId = 1,
                Id = 5,
                CompetitorsInEvent = new List<PickDetailViewModel>
        {
            new() { PickId = 0, SelectedCompetitorId = 10 }
        }
            };

            var result = await _controller.Details(model);

            _mockGameCompetitorEventService.Verify(s => s.AddPicks(It.IsAny<List<GameCompetitorEventPick>>()), Times.Once);
        }

        [Fact]
        public async Task Details_Post_SomeExistingSomeNewPicks_AddsOnlyNewPicks()
        {
            var model = new GameCompetitorInEventViewModel
            {
                EventId = 1,
                Id = 5,
                CompetitorsInEvent = new List<PickDetailViewModel>
                {
                    new() { PickId = 1, SelectedCompetitorId = 10 }, // bestaand
                    new() { PickId = 0, SelectedCompetitorId = 11 }  // nieuw
                }
            };

            var result = await _controller.Details(model) as RedirectToActionResult;

            _mockGameCompetitorEventService.Verify(
                s => s.AddPicks(
                    It.Is<List<GameCompetitorEventPick>>(l => 
                        l.Count == 1 && 
                        l[0].CompetitorsInEventId == 11)), 
                Times.Once);

            Assert.NotNull(result);
            Assert.Equal("Details", result.ActionName);
        }

        [Fact]
        public async Task Details_EventNotFound_ReturnsNotFound()
        {
            _mockEventService
                .Setup(s => s.GetEventById(1))
                .ReturnsAsync((Event?)null);

            var result = await _controller.Details(5, 1);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Event 1 niet gevonden.", notFound.Value);
        }

        [Fact]
        public async Task Details_EventWithoutConfiguration_ReturnsBadRequest()
        {
            _mockEventService
                .Setup(s => s.GetEventById(1))
                .ReturnsAsync(new Event
                {
                    EventId = 1,
                    Configuration = null
                });

            var result = await _controller.Details(5, 1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Event 1 heeft geen configuratie.", badRequest.Value);
        }

        [Fact]
        public async Task Details_ValidPick_ReturnsViewWithScoresAndRatings()
        {
            int eventId = 1;
            int gameCompetitorEventId = 5;

            var gameEvent = new Event
            {
                EventId = eventId,
                Configuration = new Configuration
                {
                    ConfigurationItems = new List<ConfigurationItem>
                    {
                        new ConfigurationItem(),
                        new ConfigurationItem()
                    }
                }
            };

            var competitor = new Competitor
            {
                CompetitorId = 100,
                FirstName = "Wout",
                LastName = "van Aert"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                CompetitorId = competitor.CompetitorId,
                Competitor = competitor
            };

            var competitorsInEvent = new CompetitorsInEvent
            {
                Id = 10,
                CompetitorInTeam = competitorInTeam,
                OutOfCompetition = false
            };

            var pick = new GameCompetitorEventPick
            {
                Id = 20,
                GameCompetitorEventId = gameCompetitorEventId,
                CompetitorsInEventId = competitorsInEvent.Id,
                CompetitorsInEvent = competitorsInEvent,
                GameCompetitorEvent = new GameCompetitorEvent
                {
                    Id = gameCompetitorEventId,
                    EventId = eventId,
                    TeamName = "Team Sjors"
                }
            };

            var ratingCategory = new RatingCategory
            {
                RatingCategoryId = 1,
                Code = "GC",
                Color = "green",
                DisplayOrder = 1,
                IsActive = true
            };

            var rating = new CompetitorRating
            {
                CompetitorRatingId = 50,
                CompetitorId = competitor.CompetitorId,
                Competitor = competitor,
                RatingCategoryId = ratingCategory.RatingCategoryId,
                RatingCategory = ratingCategory,
                Rating = 8,
                RatingDate = new DateTime(2026, 9, 1)
            };

            _mockEventService
                .Setup(s => s.GetEventById(eventId))
                .ReturnsAsync(gameEvent);

            _mockResultService
                .Setup(s => s.GetResultsByEventId(eventId))
                .ReturnsAsync(new List<CompetitorRankingDto>
                {
                    new CompetitorRankingDto
                    {
                        CompetitorInEventId = competitorsInEvent.Id,
                        NormalPoints = 10,
                        SpecialPoints = 3
                    }
                });

            _mockGameCompetitorEventService
                .Setup(s => s.GetPicks(eventId))
                .Returns(new List<GameCompetitorEventPick>
                {
                    pick
                }.AsQueryable());

            _mockCompetitorInEventService
                .Setup(s => s.GetCompetitors(eventId))
                .ReturnsAsync(new List<CompetitorsInEvent>
                {
                    competitorsInEvent
                });

            _mockRatingService
                .Setup(s => s.GetRatingsByCompetitorIds(
                    It.Is<List<int>>(ids => ids.Contains(competitor.CompetitorId))))
                .ReturnsAsync(new List<CompetitorRating>
                {
                    rating
                });

            var result = await _controller.Details(gameCompetitorEventId, eventId);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GameCompetitorInEventViewModel>(view.Model);

            Assert.Equal(eventId, model.EventId);
            Assert.Equal(gameCompetitorEventId, model.Id);
            Assert.Equal("Team Sjors", model.TeamName);

            Assert.Equal(2, model.NumberOfPicks);
            Assert.Equal(10, model.NormalScore);

            Assert.Equal(2, model.CompetitorsInEvent.Count);

            var pickModel = model.CompetitorsInEvent.First(p => p.PickId == pick.Id);

            Assert.Equal(competitorsInEvent.Id, pickModel.CompetitorInEventId);
            Assert.Equal("Wout", pickModel.FirstName);
            Assert.Equal("van Aert", pickModel.LastName);
            Assert.Equal("Wout van Aert", pickModel.CompetitorName);

            Assert.False(pickModel.IsOutOfCompetition);

            Assert.Equal(10, pickModel.NormalScore);
            Assert.Equal(3, pickModel.SpecialQuestionScore);
            Assert.Equal(13, pickModel.TotalScore);

            Assert.Equal(pick.Id, pickModel.PickId);
            Assert.Equal(competitor.CompetitorId, pickModel.SelectedCompetitorId);

            var ratingModel = Assert.Single(pickModel.Ratings);
            Assert.Equal(1, ratingModel.RatingCategoryId);
            Assert.Equal("GC", ratingModel.Code);
            Assert.Equal("green", ratingModel.Color);
            Assert.Equal(1, ratingModel.DisplayOrder);
            Assert.Equal(8, ratingModel.Rating);
        }

        [Fact]
        public async Task Details_MultipleRatings_UsesLatestActiveRatingAndIgnoresInactive()
        {
            int eventId = 1;
            int gameCompetitorEventId = 5;
            int competitorId = 100;

            var competitor = new Competitor
            {
                CompetitorId = competitorId,
                FirstName = "Wout",
                LastName = "van Aert",
            };

            var competitorInTeam = new CompetitorInTeam
            {
                CompetitorId = competitorId,
                Competitor = competitor
            };

            var competitorsInEvent = new CompetitorsInEvent
            {
                Id = 10,
                CompetitorInTeam = competitorInTeam,
                OutOfCompetition = false
            };

            var pick = new GameCompetitorEventPick
            {
                Id = 20,
                GameCompetitorEventId = gameCompetitorEventId,
                CompetitorsInEventId = competitorsInEvent.Id,
                CompetitorsInEvent = competitorsInEvent,
                GameCompetitorEvent = new GameCompetitorEvent
                {
                    Id = gameCompetitorEventId,
                    EventId = eventId,
                    TeamName = "Team Sjors"
                }
            };

            var activeCategory = new RatingCategory
            {
                RatingCategoryId = 1,
                Code = "GC",
                Color = "green",
                DisplayOrder = 1,
                IsActive = true
            };

            var inactiveCategory = new RatingCategory
            {
                RatingCategoryId = 2,
                Code = "SPR",
                Color = "red",
                DisplayOrder = 2,
                IsActive = false
            };

            var oldRating = new CompetitorRating
            {
                CompetitorRatingId = 10,
                CompetitorId = competitorId,
                Competitor = competitor,
                RatingCategoryId = activeCategory.RatingCategoryId,
                RatingCategory = activeCategory,
                Rating = 5,
                RatingDate = new DateTime(2026, 8, 1)
            };

            var latestRating = new CompetitorRating
            {
                CompetitorRatingId = 20,
                CompetitorId = competitorId,
                Competitor = competitor,
                RatingCategoryId = activeCategory.RatingCategoryId,
                RatingCategory = activeCategory,
                Rating = 9,
                RatingDate = new DateTime(2026, 9, 1)
            };

            var inactiveRating = new CompetitorRating
            {
                CompetitorRatingId = 30,
                CompetitorId = competitorId,
                Competitor = competitor,
                RatingCategoryId = inactiveCategory.RatingCategoryId,
                RatingCategory = inactiveCategory,
                Rating = 7,
                RatingDate = new DateTime(2026, 9, 15)
            };

            _mockEventService
                .Setup(s => s.GetEventById(eventId))
                .ReturnsAsync(new Event
                {
                    EventId = eventId,
                    Configuration = new Configuration
                    {
                        ConfigurationItems = new List<ConfigurationItem>
                        {
                            new ConfigurationItem()
                        }
                    }
                });

            _mockResultService
                .Setup(s => s.GetResultsByEventId(eventId))
                .ReturnsAsync(new List<CompetitorRankingDto>());

            _mockGameCompetitorEventService
                .Setup(s => s.GetPicks(eventId))
                .Returns(new List<GameCompetitorEventPick>
                {
                    pick
                }.AsQueryable());

            _mockCompetitorInEventService
                .Setup(s => s.GetCompetitors(eventId))
                .ReturnsAsync(new List<CompetitorsInEvent>
                {
                    competitorsInEvent
                });

            _mockRatingService
                .Setup(s => s.GetRatingsByCompetitorIds(
                    It.Is<List<int>>(ids => ids.Contains(competitorId))))
                .ReturnsAsync(new List<CompetitorRating>
                {
                    oldRating,
                    latestRating,
                    inactiveRating
                });

            var result = await _controller.Details(gameCompetitorEventId, eventId);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GameCompetitorInEventViewModel>(view.Model);

            var pickModel = Assert.Single(model.CompetitorsInEvent);

            var rating = Assert.Single(pickModel.Ratings);

            Assert.Equal(activeCategory.RatingCategoryId, rating.RatingCategoryId);
            Assert.Equal("GC", rating.Code);
            Assert.Equal("green", rating.Color);
            Assert.Equal(1, rating.DisplayOrder);
            Assert.Equal(9, rating.Rating);
        }

        [Fact]
        public async Task Details_PickWithoutResult_ReturnsZeroScores()
        {
            int eventId = 1;
            int gameCompetitorEventId = 5;

            var competitor = new Competitor
            {
                CompetitorId = 100,
                FirstName = "Wout",
                LastName = "van Aert",
            };

            var competitorInTeam = new CompetitorInTeam
            {
                CompetitorId = competitor.CompetitorId,
                Competitor = competitor
            };

            var competitorsInEvent = new CompetitorsInEvent
            {
                Id = 10,
                CompetitorInTeam = competitorInTeam,
                OutOfCompetition = false
            };

            var pick = new GameCompetitorEventPick
            {
                Id = 20,
                GameCompetitorEventId = gameCompetitorEventId,
                CompetitorsInEventId = competitorsInEvent.Id,
                CompetitorsInEvent = competitorsInEvent,
                GameCompetitorEvent = new GameCompetitorEvent
                {
                    Id = gameCompetitorEventId,
                    EventId = eventId,
                    TeamName = "Team Sjors"
                }
            };

            _mockEventService
                .Setup(s => s.GetEventById(eventId))
                .ReturnsAsync(new Event
                {
                    EventId = eventId,
                    Configuration = new Configuration
                    {
                        ConfigurationItems = new List<ConfigurationItem>
                        {
                            new ConfigurationItem()
                        }
                    }
                });

            // Bewust geen resultaat voor CompetitorInEventId = 10.
            _mockResultService
                .Setup(s => s.GetResultsByEventId(eventId))
                .ReturnsAsync(new List<CompetitorRankingDto>());

            _mockGameCompetitorEventService
                .Setup(s => s.GetPicks(eventId))
                .Returns(new List<GameCompetitorEventPick>
                {
                    pick
                }.AsQueryable());

            _mockCompetitorInEventService
                .Setup(s => s.GetCompetitors(eventId))
                .ReturnsAsync(new List<CompetitorsInEvent>
                {
                    competitorsInEvent
                });

            _mockRatingService
                .Setup(s => s.GetRatingsByCompetitorIds(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<CompetitorRating>());

            var result = await _controller.Details(gameCompetitorEventId, eventId);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GameCompetitorInEventViewModel>(view.Model);

            var pickModel = Assert.Single(model.CompetitorsInEvent);

            Assert.Equal(0, pickModel.NormalScore);
            Assert.Equal(0, pickModel.SpecialQuestionScore);
            Assert.Equal(0, pickModel.TotalScore);

            Assert.Equal(0, model.NormalScore);
        }

        [Fact]
        public async Task Details_MorePicksThanConfigured_TakesOnlyConfiguredNumber()
        {
            int eventId = 1;
            int gameCompetitorEventId = 5;

            var competitor1 = new Competitor
            {
                CompetitorId = 100,
                FirstName = "Wout",
                LastName = "van Aert",
            };

            var competitor2 = new Competitor
            {
                CompetitorId = 200,
                FirstName = "Mathieu",
                LastName = "van der Poel",
            };

            var competitorInTeam1 = new CompetitorInTeam
            {
                CompetitorId = competitor1.CompetitorId,
                Competitor = competitor1
            };

            var competitorInTeam2 = new CompetitorInTeam
            {
                CompetitorId = competitor2.CompetitorId,
                Competitor = competitor2
            };

            var competitorsInEvent1 = new CompetitorsInEvent
            {
                Id = 10,
                CompetitorInTeam = competitorInTeam1
            };

            var competitorsInEvent2 = new CompetitorsInEvent
            {
                Id = 20,
                CompetitorInTeam = competitorInTeam2
            };

            var pick1 = new GameCompetitorEventPick
            {
                Id = 1,
                GameCompetitorEventId = gameCompetitorEventId,
                CompetitorsInEventId = competitorsInEvent1.Id,
                CompetitorsInEvent = competitorsInEvent1,
                GameCompetitorEvent = new GameCompetitorEvent
                {
                    Id = gameCompetitorEventId,
                    EventId = eventId,
                    TeamName = "Team Sjors"
                }
            };

            var pick2 = new GameCompetitorEventPick
            {
                Id = 2,
                GameCompetitorEventId = gameCompetitorEventId,
                CompetitorsInEventId = competitorsInEvent2.Id,
                CompetitorsInEvent = competitorsInEvent2,
                GameCompetitorEvent = pick1.GameCompetitorEvent
            };

            _mockEventService
                .Setup(s => s.GetEventById(eventId))
                .ReturnsAsync(new Event
                {
                    EventId = eventId,
                    Configuration = new Configuration
                    {
                        // Slechts één toegestane pick.
                        ConfigurationItems = new List<ConfigurationItem>
                        {
                            new ConfigurationItem()
                        }
                    }
                });

            _mockResultService
                .Setup(s => s.GetResultsByEventId(eventId))
                .ReturnsAsync(new List<CompetitorRankingDto>());

            _mockGameCompetitorEventService
                .Setup(s => s.GetPicks(eventId))
                .Returns(new List<GameCompetitorEventPick>
                {
                    pick1,
                    pick2
                }.AsQueryable());

            _mockCompetitorInEventService
                .Setup(s => s.GetCompetitors(eventId))
                .ReturnsAsync(new List<CompetitorsInEvent>
                {
                    competitorsInEvent1,
                    competitorsInEvent2
                });

            _mockRatingService
                .Setup(s => s.GetRatingsByCompetitorIds(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<CompetitorRating>());

            var result = await _controller.Details(gameCompetitorEventId, eventId);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GameCompetitorInEventViewModel>(view.Model);

            Assert.Single(model.CompetitorsInEvent);

            var selectedPick = model.CompetitorsInEvent[0];

            Assert.Equal(1, selectedPick.PickId);
            Assert.Equal("Wout", selectedPick.FirstName);
            Assert.Equal("van Aert", selectedPick.LastName);
        }

        [Fact]
        public async Task Details_PickWithNullNames_UsesOnbekendFallback()
        {
            int eventId = 1;
            int gameCompetitorEventId = 5;

            var competitor = new Competitor
            {
                CompetitorId = 100,
                FirstName = null!,
                LastName = null!,
            };

            var competitorInTeam = new CompetitorInTeam
            {
                CompetitorId = competitor.CompetitorId,
                Competitor = competitor
            };

            var competitorsInEvent = new CompetitorsInEvent
            {
                Id = 10,
                CompetitorInTeam = competitorInTeam,
                OutOfCompetition = true
            };

            var pick = new GameCompetitorEventPick
            {
                Id = 20,
                GameCompetitorEventId = gameCompetitorEventId,
                CompetitorsInEventId = competitorsInEvent.Id,
                CompetitorsInEvent = competitorsInEvent,

                // Bewust geen GameCompetitorEvent:
                // hierdoor moet TeamName "onbekend" worden.
                GameCompetitorEvent = null!
            };

            _mockEventService
                .Setup(s => s.GetEventById(eventId))
                .ReturnsAsync(new Event
                {
                    EventId = eventId,
                    Configuration = new Configuration
                    {
                        ConfigurationItems = new List<ConfigurationItem>
                        {
                            new ConfigurationItem()
                        }
                    }
                });

            _mockResultService
                .Setup(s => s.GetResultsByEventId(eventId))
                .ReturnsAsync(new List<CompetitorRankingDto>());

            _mockGameCompetitorEventService
                .Setup(s => s.GetPicks(eventId))
                .Returns(new List<GameCompetitorEventPick>
                {
                    pick
                }.AsQueryable());

            _mockCompetitorInEventService
                .Setup(s => s.GetCompetitors(eventId))
                .ReturnsAsync(new List<CompetitorsInEvent>
                {
                    competitorsInEvent
                });

            _mockRatingService
                .Setup(s => s.GetRatingsByCompetitorIds(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<CompetitorRating>());

            var result = await _controller.Details(gameCompetitorEventId, eventId);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GameCompetitorInEventViewModel>(view.Model);
            var pickModel = Assert.Single(model.CompetitorsInEvent);

            Assert.Equal("onbekend", pickModel.FirstName);
            Assert.Equal("onbekend", pickModel.LastName);
            Assert.True(pickModel.IsOutOfCompetition);
        }

        [Fact]
        public async Task Details_RatingsWithSameDate_UsesHighestRatingId()
        {
            int eventId = 1;
            int gameCompetitorEventId = 5;
            int competitorId = 100;

            var competitor = new Competitor
            {
                CompetitorId = competitorId,
                FirstName = "Wout",
                LastName = "van Aert"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                CompetitorId = competitorId,
                Competitor = competitor
            };

            var competitorsInEvent = new CompetitorsInEvent
            {
                Id = 10,
                CompetitorInTeam = competitorInTeam
            };

            var pick = new GameCompetitorEventPick
            {
                Id = 20,
                GameCompetitorEventId = gameCompetitorEventId,
                CompetitorsInEventId = competitorsInEvent.Id,
                CompetitorsInEvent = competitorsInEvent,
                GameCompetitorEvent = new GameCompetitorEvent
                {
                    Id = gameCompetitorEventId,
                    EventId = eventId,
                    TeamName = "Team Sjors"
                }
            };

            var ratingCategory = new RatingCategory
            {
                RatingCategoryId = 1,
                Code = "GC",
                Color = "green",
                DisplayOrder = 1,
                IsActive = true
            };

            var ratingDate = new DateTime(2026, 9, 1);

            var lowerIdRating = new CompetitorRating
            {
                CompetitorRatingId = 10,
                CompetitorId = competitorId,
                Competitor = competitor,
                RatingCategoryId = ratingCategory.RatingCategoryId,
                RatingCategory = ratingCategory,
                Rating = 5,
                RatingDate = ratingDate
            };

            var higherIdRating = new CompetitorRating
            {
                CompetitorRatingId = 20,
                CompetitorId = competitorId,
                Competitor = competitor,
                RatingCategoryId = ratingCategory.RatingCategoryId,
                RatingCategory = ratingCategory,
                Rating = 9,
                RatingDate = ratingDate
            };

            _mockEventService
                .Setup(s => s.GetEventById(eventId))
                .ReturnsAsync(new Event
                {
                    EventId = eventId,
                    Configuration = new Configuration
                    {
                        ConfigurationItems = new List<ConfigurationItem>
                        {
                            new ConfigurationItem()
                        }
                    }
                });

            _mockResultService
                .Setup(s => s.GetResultsByEventId(eventId))
                .ReturnsAsync(new List<CompetitorRankingDto>());

            _mockGameCompetitorEventService
                .Setup(s => s.GetPicks(eventId))
                .Returns(new List<GameCompetitorEventPick>
                {
                    pick
                }.AsQueryable());

            _mockCompetitorInEventService
                .Setup(s => s.GetCompetitors(eventId))
                .ReturnsAsync(new List<CompetitorsInEvent>
                {
                    competitorsInEvent
                });

            _mockRatingService
                .Setup(s => s.GetRatingsByCompetitorIds(
                    It.Is<List<int>>(ids => ids.Contains(competitorId))))
                .ReturnsAsync(new List<CompetitorRating>
                {
                    lowerIdRating,
                    higherIdRating
                });

            var result = await _controller.Details(gameCompetitorEventId, eventId);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GameCompetitorInEventViewModel>(view.Model);

            var pickModel = Assert.Single(model.CompetitorsInEvent);
            var rating = Assert.Single(pickModel.Ratings);

            Assert.Equal(1, rating.RatingCategoryId);
            Assert.Equal(9, rating.Rating);
        }
        #endregion
        #region Create Tests

        [Fact]
        public async Task Create_Get_ReturnsViewWithUsersInViewData()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetAllUsers())
                .ReturnsAsync(new List<ApplicationUser>
                {
                    new ApplicationUser { Id = "1", FirstName = "Jane", LastName = "Doe", Email = "jane@x.com" }
                });

            // Act
            var result = await _controller.Create(3) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ViewData.ContainsKey("Users"));
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "invalid");
            var dto = new DeelnemerCreateDto { EventId = 1 };

            _mockUserService.Setup(s => s.GetAllUsers())
                .ReturnsAsync(new List<ApplicationUser>());

            // Act
            var result = await _controller.Create(dto) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto, result.Model);
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var dto = new DeelnemerCreateDto { EventId = 5 };
            _mockGameCompetitorEventService.Setup(s => s.CreateGameCompetitorEventAsync(dto))
                .ReturnsAsync(new GameCompetitorEvent { Id = 1, EventId = 5 });

            // Act
            var result = await _controller.Create(dto) as RedirectToActionResult;

            // Assert
            _mockGameCompetitorEventService.Verify(s => s.CreateGameCompetitorEventAsync(dto), Times.Once);
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
        }

        #endregion
        #region Edit Tests

        [Fact]
        public async Task Edit_Get_ValidId_ReturnsViewWithDto()
        {
            // Arrange
            var entity = new GameCompetitorEvent
            {
                Id = 1,
                EventId = 5,
                TeamName = "TeamA",
                UserId = "user1"
            };

            _mockGameCompetitorEventService.Setup(s => s.GetGameCompetitorEventById(1))
                .ReturnsAsync(entity);

            _mockUserService.Setup(s => s.GetAllUsers())
                .ReturnsAsync(new List<ApplicationUser> { new() { Id = "user1", FirstName = "John", LastName = "Doe", Email = "a@b.com" } });

            _mockEventService.Setup(s => s.GetAllEvents())
                .ReturnsAsync(new List<Event> { new() { EventId = 5, EventName = "Tour" } });

            // Act
            var result = await _controller.Edit(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<DeelnemerEditDto>(result.Model);
            Assert.Equal(entity.Id, model.Id);
            Assert.Equal(entity.TeamName, model.TeamName);
        }

        [Fact]
        public async Task Edit_Get_InvalidId_ReturnsNotFound()
        {
            _mockGameCompetitorEventService
                .Setup(s => s.GetGameCompetitorEventById(It.IsAny<int>()))
                .ReturnsAsync((GameCompetitorEvent?)null);

            var result = await _controller.Edit(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_CallsUpdateAndRedirects()
        {
            var dto = new DeelnemerEditDto { Id = 1, EventId = 10 };
            var result = await _controller.Edit(dto);

            _mockGameCompetitorEventService.Verify(s => s.UpdateAsync(dto), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            var dto = new DeelnemerEditDto { Id = 1, EventId = 5 };
            _controller.ModelState.AddModelError("error", "invalid");

            _mockUserService.Setup(s => s.GetAllUsers())
                .ReturnsAsync(new List<ApplicationUser>());
            _mockEventService.Setup(s => s.GetAllEvents())
                .ReturnsAsync(new List<Event>());

            // Act
            var result = await _controller.Edit(dto) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto, result.Model);
        }

        #endregion
        #region Delete Tests

        [Fact]
        public async Task DeletePick_ValidId_RemovesPickAndReturnsOk()
        {
            var result = await _controller.DeletePick(5);
            _mockGameCompetitorEventService.Verify(s => s.RemovePickFromEvent(5), Times.Once);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Delete_Post_ExistingEntity_CallsServiceAndRedirects()
        {
            var entity = new GameCompetitorEvent { Id = 5, EventId = 77 };

            _mockGameCompetitorEventService
                .Setup(s => s.GetGameCompetitorEventById(5))
                .ReturnsAsync(entity);

            var result = await _controller.Delete(5);

            _mockGameCompetitorEventService.Verify(s => s.DeleteGameCompetitorEventAsync(5), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(77, redirect.RouteValues?["eventId"]);
        }

        [Fact]
        public async Task Delete_Get_NullId_ReturnsNotFound()
        {
            var result = await _controller.Delete(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_EntityNotFound_ReturnsNotFound()
        {
            _mockGameCompetitorEventService.Setup(s => s.GetGameCompetitorEventById(It.IsAny<int>()))
                .ReturnsAsync((GameCompetitorEvent?)null);

            var result = await _controller.Delete((int?)99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Post_EntityNotFound_RedirectsSafely()
        {
            // Arrange
            _mockGameCompetitorEventService.Setup(s => s.GetGameCompetitorEventById(99))
                .ReturnsAsync((GameCompetitorEvent?)null);

            // Act
            var result = await _controller.Delete(99) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            // Entity is null, dus eventId bestaat niet; test dat er geen exception komt
        }


        [Fact]
        public async Task Delete_Get_ValidId_ReturnsViewWithDto()
        {
            var entity = new GameCompetitorEvent
            {
                Id = 5,
                EventId = 77,
                TeamName = "Alpha",
                Event = new Event { EventId = 77, EventName = "Tour" },
                User = new ApplicationUser { FirstName = "Tom", LastName = "Boonen" }
            };
            _mockGameCompetitorEventService.Setup(s => s.GetGameCompetitorEventById(5))
                .ReturnsAsync(entity);

            var result = await _controller.Delete((int?)5);
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DeelnemerDeleteDto>(view.Model);
            Assert.Equal("Alpha", model.TeamName);
            Assert.Equal("Tour", model.EventName);
        }

        [Fact]
        public async Task DeletePick_ServiceThrowsException_ReturnsServerError()
        {
            // Arrange
            _mockGameCompetitorEventService.Setup(s => s.RemovePickFromEvent(5))
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.DeletePick(5));
        }
        #endregion
        #region Helper tests

        [Fact]
        public async Task FillList_ReturnsRedirectToDetails()
        {
            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );

            _mockGameCompetitorEventService.Setup(s => s.GetCompetitors(1, It.IsAny<int>()))
                .ReturnsAsync(new List<CompetitorsInEvent>
                {
            new() { CompetitorInTeamId = 10 }
                });

            var result = await _controller.FillList(2, 10, 1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
        }

        [Fact]
        public async Task FillList_NoCompetitors_ReturnsRedirect()
        {
            // Arrange
            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(), 
                Mock.Of<ITempDataProvider>());

            _mockGameCompetitorEventService
                .Setup(s => s.GetCompetitors(1, It.IsAny<int>()))
                .ReturnsAsync(new List<CompetitorsInEvent>()); // geen suggesties

            // Act
            var result = await _controller.FillList(2, 10, 1) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Details", result.ActionName);

            var suggestedCompetitors = Assert.IsType<List<int>>(
                _controller.TempData["suggestedCompetitors"]);

            Assert.Empty(suggestedCompetitors);
        }
        #endregion
    }
}
