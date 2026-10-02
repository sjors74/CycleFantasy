using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Domain.Dto;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Primitives;
using Moq;
using WebCycleManager.Controllers;
using WebCycleManager.Models;

namespace CycleManager.Tests.Unit.Manager
{
    public class CompetitorsInEventControllerTests
    {
        private readonly Mock<ICompetitorInEventService> _competitorInEventServiceMock;
        private readonly Mock<ICompetitorService> _competitorServiceMock;
        private readonly Mock<ITeamService> _teamServiceMock;
        private readonly Mock<IEventService> _eventServiceMock;
        private readonly Mock<ISeasonYearService> _seasonYearServiceMock;
        private readonly CompetitorsInEventsController _controller;

        public CompetitorsInEventControllerTests()
        {
            _competitorInEventServiceMock = new Mock<ICompetitorInEventService>();
            _competitorServiceMock = new Mock<ICompetitorService>();
            _teamServiceMock = new Mock<ITeamService>();
            _eventServiceMock = new Mock<IEventService>();
            _seasonYearServiceMock = new Mock<ISeasonYearService>();

            _controller = new CompetitorsInEventsController(
                _competitorInEventServiceMock.Object,
                _eventServiceMock.Object,
                _teamServiceMock.Object,
                _competitorServiceMock.Object,
                _seasonYearServiceMock.Object
            );
        }

        #region Index Tests
        [Fact]
        public async Task Index_ReturnsView_WithCorrectModel()
        {
            // Arrange
            int eventId = 1;
            var competitorsInEvent = new List<CompetitorsInEvent>
            {
                new CompetitorsInEvent
                {
                    Id = 1,
                    EventId = eventId,
                    EventNumber = 12,
                    CompetitorInTeam = new CompetitorInTeam
                    {
                        Competitor = new Competitor { FirstName = "Jan", LastName = "Jansen" },
                        TeamYear = new TeamYear { TeamId = 5, Name = "Team Jumbo" }
                    }
                }
            };

            var currentEvent = new Event { EventId = eventId, EventName = "Tour", EventYear = 2025 };

            _competitorInEventServiceMock.Setup(s => s.GetCompetitors(eventId))
                .ReturnsAsync(competitorsInEvent);
            _eventServiceMock.Setup(s => s.GetEventById(eventId))
                .ReturnsAsync(currentEvent);
            _teamServiceMock.Setup(s => s.GetTeamsForEvent(eventId))
                .ReturnsAsync(new List<Team> { new Team { TeamId = 5, CurrentTeamName = "Team Jumbo" } });

            // Act
            var result = await _controller.Index(eventId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CompetitorsInEventViewModel>(viewResult.Model);

            Assert.Equal("Tour", model.EventName);
            Assert.Single(model.Competitors);
            Assert.Equal("Jan", model.Competitors.First().FirstName);
        }

        [Fact]
        public async Task Index_ReturnsNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            int eventId = 1;
            _eventServiceMock.Setup(s => s.GetEventById(eventId)).ReturnsAsync((Event?)null);

            // Act
            var result = await _controller.Index(eventId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Index_FiltersByTeam_WhenFilterTeamIsSet()
        {
            int eventId = 1, filterTeam = 5;
            var allCompetitors = new List<CompetitorsInEvent>
            {
                new CompetitorsInEvent
                {
                    CompetitorInTeam = new CompetitorInTeam
                    {
                        TeamYear = new TeamYear { TeamId = 5, Name = "Team Jumbo" },
                        Competitor = new Competitor()
                    }
                },
                new CompetitorsInEvent
                {
                    CompetitorInTeam = new CompetitorInTeam
                    {
                        TeamYear = new TeamYear { TeamId = 9, Name = "Team B" },
                        Competitor = new Competitor()
                    }
                }
            };

            _competitorInEventServiceMock.Setup(s => s.GetCompetitors(eventId)).ReturnsAsync(allCompetitors);
            _eventServiceMock.Setup(s => s.GetEventById(eventId)).ReturnsAsync(new Event { EventId = eventId });
            _teamServiceMock.Setup(s => s.GetTeamsForEvent(eventId)).ReturnsAsync(new List<Team>());

            // Act
            var result = await _controller.Index(eventId, filterTeam);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CompetitorsInEventViewModel>(view.Model);
            Assert.Single(model.Competitors); // enkel team 5
        }

        [Fact]
        public async Task Index_HidesRemovedFromStartList_WhenShowRemovedFromStartListIsFalse()
        {
            // Arrange
            int eventId = 1;

            var competitors = new List<CompetitorsInEvent>
            {
                new CompetitorsInEvent
                {
                    Id = 1,
                    EventId = eventId,
                    EventNumber = 1,
                    RemovedFromStartList = false,
                    CompetitorInTeam = new CompetitorInTeam
                    {
                        Competitor = new Competitor
                        {
                            FirstName = "Jan",
                            LastName = "Jansen"
                        },
                        TeamYear = new TeamYear
                        {
                            TeamId = 5,
                            Team = new Team { TeamId = 5, CurrentTeamName = "Team A" }
                        }
                    }
                },
                new CompetitorsInEvent
                {
                    Id = 2,
                    EventId = eventId,
                    EventNumber = 2,
                    RemovedFromStartList = true,
                    CompetitorInTeam = new CompetitorInTeam
                    {
                        Competitor = new Competitor
                        {
                            FirstName = "Piet",
                            LastName = "Pietersen"
                        },
                        TeamYear = new TeamYear
                        {
                            TeamId = 5,
                            Team = new Team { TeamId = 5, CurrentTeamName = "Team A" }
                        }
                    }
                }
            };

            _competitorInEventServiceMock
                .Setup(s => s.GetCompetitors(eventId))
                .ReturnsAsync(competitors);

            _eventServiceMock
                .Setup(s => s.GetEventById(eventId))
                .ReturnsAsync(new Event
                {
                    EventId = eventId,
                    EventName = "Tour",
                    EventYear = 2025
                });

            _teamServiceMock
                .Setup(s => s.GetTeamsForEvent(eventId))
                .ReturnsAsync(new List<Team>());

            // Act
            var result = await _controller.Index(
                eventId,
                ShowRemovedFromStartList: false);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CompetitorsInEventViewModel>(view.Model);

            Assert.Single(model.Competitors);
            Assert.Equal("Jan", model.Competitors.First().FirstName);
        }

        [Fact]
        public async Task Index_HidesOutOfCompetition_WhenShowOutOfCompetitionIsFalse()
        {
            // Arrange
            int eventId = 1;

            var competitors = new List<CompetitorsInEvent>
            {
                new CompetitorsInEvent
                {
                    Id = 1,
                    EventId = eventId,
                    EventNumber = 1,
                    OutOfCompetition = false,
                    CompetitorInTeam = new CompetitorInTeam
                    {
                        Competitor = new Competitor
                        {
                            FirstName = "Jan",
                            LastName = "Jansen"
                        },
                        TeamYear = new TeamYear
                        {
                            TeamId = 5,
                            Team = new Team
                            {
                                TeamId = 5,
                                CurrentTeamName = "Team A"
                            }
                        }
                    }
                },
                new CompetitorsInEvent
                {
                    Id = 2,
                    EventId = eventId,
                    EventNumber = 2,
                    OutOfCompetition = true,
                    CompetitorInTeam = new CompetitorInTeam
                    {
                        Competitor = new Competitor
                        {
                            FirstName = "Piet",
                            LastName = "Pietersen"
                        },
                        TeamYear = new TeamYear
                        {
                            TeamId = 5,
                            Team = new Team
                            {
                                TeamId = 5,
                                CurrentTeamName = "Team A"
                            }
                        }
                    }
                }
            };

            _competitorInEventServiceMock
                .Setup(s => s.GetCompetitors(eventId))
                .ReturnsAsync(competitors);

            _eventServiceMock
                .Setup(s => s.GetEventById(eventId))
                .ReturnsAsync(new Event
                {
                    EventId = eventId,
                    EventName = "Tour",
                    EventYear = 2025
                });

            _teamServiceMock
                .Setup(s => s.GetTeamsForEvent(eventId))
                .ReturnsAsync(new List<Team>());

            // Act
            var result = await _controller.Index(
                eventId,
                ShowOutOfCompetition: false);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CompetitorsInEventViewModel>(view.Model);

            Assert.Single(model.Competitors);
            Assert.Equal("Jan", model.Competitors.First().FirstName);
        }

        [Fact]
        public async Task Index_ReturnsZeroRatingCount_WhenCompetitorHasNoRatings()
        {
            // Arrange
            var competitor = new Competitor
            {
                FirstName = "Jan",
                LastName = "Jansen",
                Ratings = null!
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Competitor = competitor,
                TeamYear = new TeamYear
                {
                    TeamId = 1,
                    Team = new Team
                    {
                        TeamId = 1,
                        CurrentTeamName = "Team A"
                    }
                }
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 10,
                EventId = 2,
                CompetitorInTeam = competitorInTeam,
                EventNumber = 25
            };

            _competitorInEventServiceMock
                .Setup(s => s.GetCompetitors(2))
                .ReturnsAsync(new List<CompetitorsInEvent> { competitorInEvent });

            _eventServiceMock
                .Setup(s => s.GetEventById(2))
                .ReturnsAsync(new Event
                {
                    EventId = 2,
                    EventName = "Tour Test",
                    EventYear = 2026
                });

            _teamServiceMock
                .Setup(s => s.GetTeamsForEvent(2))
                .ReturnsAsync(new List<Team>());

            // Act
            var result = await _controller.Index(2);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CompetitorsInEventViewModel>(view.Model);

            var competitorVm = Assert.Single(model.Competitors);
            Assert.Equal(0, competitorVm.RatingCount);
        }

        [Fact]
        public async Task Index_UsesEmptyCompetitorValues_WhenCompetitorIsNull()
        {
            // Arrange
            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 10,
                EventId = 2,
                EventNumber = 25,
                CompetitorInTeam = new CompetitorInTeam
                {
                    Competitor = null!,
                    TeamYear = new TeamYear
                    {
                        TeamId = 1,
                        Team = new Team
                        {
                            TeamId = 1,
                            CurrentTeamName = "Team A"
                        }
                    }
                }
            };

            _competitorInEventServiceMock
                .Setup(s => s.GetCompetitors(2))
                .ReturnsAsync(new List<CompetitorsInEvent> { competitorInEvent });

            _eventServiceMock
                .Setup(s => s.GetEventById(2))
                .ReturnsAsync(new Event
                {
                    EventId = 2,
                    EventName = "Tour Test",
                    EventYear = 2026
                });

            _teamServiceMock
                .Setup(s => s.GetTeamsForEvent(2))
                .ReturnsAsync(new List<Team>());

            // Act
            var result = await _controller.Index(2);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CompetitorsInEventViewModel>(view.Model);

            var competitorVm = Assert.Single(model.Competitors);

            Assert.Equal(string.Empty, competitorVm.FirstName);
            Assert.Equal(string.Empty, competitorVm.LastName);
            Assert.Equal(0, competitorVm.RatingCount);
        }

        [Fact]
        public async Task Index_UsesDefaultTeamValues_WhenTeamIsNull()
        {
            // Arrange
            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 10,
                EventId = 2,
                EventNumber = 25,
                CompetitorInTeam = new CompetitorInTeam
                {
                    Competitor = new Competitor
                    {
                        FirstName = "Jan",
                        LastName = "Jansen"
                    },
                    TeamYear = new TeamYear
                    {
                        TeamId = 1,
                        Team = null!
                    }
                }
            };

            _competitorInEventServiceMock
                .Setup(s => s.GetCompetitors(2))
                .ReturnsAsync(new List<CompetitorsInEvent> { competitorInEvent });

            _eventServiceMock
                .Setup(s => s.GetEventById(2))
                .ReturnsAsync(new Event
                {
                    EventId = 2,
                    EventName = "Tour Test",
                    EventYear = 2026
                });

            _teamServiceMock
                .Setup(s => s.GetTeamsForEvent(2))
                .ReturnsAsync(new List<Team>());

            // Act
            var result = await _controller.Index(2);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CompetitorsInEventViewModel>(view.Model);

            var competitorVm = Assert.Single(model.Competitors);

            Assert.Equal("onbekend", competitorVm.TeamName);
            Assert.Equal(0, competitorVm.TeamId);
        }
        #endregion

        #region Create Tests
        [Fact]
        public async Task Create_Get_ReturnsView_WithLists()
        {
            // Arrange
            int eventId = 1;
            var eventObject = new Event
            {
                EventId = eventId,
                EventName = "Test Event",
                IsActive = true,
            };

            _seasonYearServiceMock
                .Setup(s => s.GetAllAsync())
                .ReturnsAsync(new List<SeasonYearDto>
                {
                    new SeasonYearDto
                    {
                        SeasonYearId = 1,
                        Year = 2024,
                        Active = true
                    }
                });

            _eventServiceMock.Setup(s => s.GetEventById(eventId))
                .ReturnsAsync(eventObject);

            var competitors = new List<CompetitorDto>
            {
                new CompetitorDto { CompetitorId = 1, FirstName = "Jonas", LastName = "Vingegaard", Teams = new List<CompetitorInTeamDto> { new CompetitorInTeamDto {  CompetitorInTeamId = 101 } } },
                new CompetitorDto { CompetitorId = 2, FirstName = "Remco", LastName = "Evenepoel", Teams = new List<CompetitorInTeamDto> { new CompetitorInTeamDto { CompetitorInTeamId = 102 } } }
            };

            _competitorServiceMock.Setup(s => s.GetAllCompetitors(It.IsAny<int>()))
                .ReturnsAsync(competitors);

            _teamServiceMock.Setup(s => s.GetTeamsForEvent(It.Is<int>(id => id == eventId)))
                .ReturnsAsync(new List<Team>
                {
            new Team { TeamId = 99, CurrentTeamName = "Team Test" }
                });

            // Act
            var result = await _controller.Create(eventId, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData["ListOfCompetitors"]);
            Assert.NotNull(viewResult.ViewData["ListOfTeams"]);
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            int eventId = 1;

            // Dit is wat uit je service komt (Competitors met Teams)
            var competitors = new List<CompetitorDto>
            {
                new CompetitorDto
                {
                    CompetitorId = 1,
                    FirstName = "Jonas",
                    LastName = "Vingegaard",
                    Teams = new List<CompetitorInTeamDto>
                    {
                        new CompetitorInTeamDto { CompetitorInTeamId = 5, TeamId = 1 }  
                    }
                },
                new CompetitorDto
                {
                    CompetitorId = 2,
                    FirstName = "Remco",
                    LastName = "Evenepoel",
                    Teams = new List<CompetitorInTeamDto>
                    {
                        new CompetitorInTeamDto { CompetitorInTeamId = 7, TeamId = 1 }
                    }
                }
            };
            _competitorServiceMock
                .Setup(s => s.GetCompetitorInTeamsByIdsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<CompetitorInTeam>
                {
                    new CompetitorInTeam { Id = 5, TeamYear = new TeamYear { TeamId = 1 } },
                    new CompetitorInTeam { Id = 7, TeamYear = new TeamYear { TeamId = 1 } }
                });

            // Mock de service call die de CompetitorsInEvent opslaat
            _competitorInEventServiceMock.Setup(s => s.Create(It.IsAny<List<CompetitorsInEvent>>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // FormCollection zoals door de controller ontvangen
            var form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "SelectCompetitorId", new[] { "5", "7" } }
            });

            // Act
            var result = await _controller.Create(eventId, null, form);

            // Assert redirect
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(eventId, redirect.RouteValues?["eventId"]);

            // Assert dat de service werd aangeroepen met de juiste CompetitorInTeamIds
            _competitorInEventServiceMock.Verify(s => s.Create(It.Is<List<CompetitorsInEvent>>(list =>
                list.Count == 2 &&
                list.Any(c => c.CompetitorInTeamId == 5) &&
                list.Any(c => c.CompetitorInTeamId == 7)
            )), Times.Once);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("Error", "Invalid");

            var result = await _controller.Create(1, null, new FormCollection(new()));

            var view = Assert.IsType<ViewResult>(result);
            Assert.Null(view.ViewName); // standaardview
        }

        [Fact]
        public async Task Edit_Get_ValidId_ReturnsView()
        {
            // Arrange
            var entity = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 2,
                CompetitorInTeamId = 10,
                CompetitorInTeam = new CompetitorInTeam
                {
                    Competitor = new Competitor
                    {
                        FirstName = "Jan",
                        LastName = "Jansen",
                        CompetitorInTeams = new List<CompetitorInTeam>
                {
                    new CompetitorInTeam { TeamYear = new TeamYear { TeamId = 3, Name = "Jumbo" } }
                }
                    }
                }
            };

            _competitorInEventServiceMock.Setup(s => s.GetCompetitorById(1))
                .ReturnsAsync(entity);
            _competitorServiceMock.Setup(s => s.GetCompetitorById(10))
                .ReturnsAsync(entity.CompetitorInTeam.Competitor);
            _eventServiceMock.Setup(s => s.GetAllEvents())
                .ReturnsAsync(new List<Event> { new Event { EventId = 2, EventName = "Tour" } });

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsType<CompetitorInEventViewModel>(viewResult.Model);
            Assert.Equal("Jan", vm.FirstName);
        }

        [Fact]
        public async Task Create_Get_ReturnsNotFound_WhenEventIsInactive()
        {
            // Arrange
            int eventId = 1;

            _eventServiceMock
                .Setup(s => s.GetEventById(eventId))
                .ReturnsAsync(new Event
                {
                    EventId = eventId,
                    EventName = "Tour",
                    IsActive = false
                });

            // Act
            var result = await _controller.Create(eventId, null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_Get_ReturnsNotFound_WhenNoActiveSeasonExists()
        {
            // Arrange
            int eventId = 1;

            _eventServiceMock
                .Setup(s => s.GetEventById(eventId))
                .ReturnsAsync(new Event
                {
                    EventId = eventId,
                    EventName = "Tour",
                    IsActive = true
                });

            _teamServiceMock
                .Setup(s => s.GetTeamsForEvent(eventId))
                .ReturnsAsync(new List<Team>());

            _seasonYearServiceMock
                .Setup(s => s.GetAllAsync())
                .ReturnsAsync(new List<SeasonYearDto>
                {
                    new SeasonYearDto
                    {
                        SeasonYearId = 1,
                        Year = 2025,
                        Active = false
                    }
                });

            // Act
            var result = await _controller.Create(eventId, null);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(
                "Er is geen actief seizoen gevonden.",
                notFound.Value);
        }

        [Fact]
        public async Task Create_Post_Redirects_WhenNoCompetitorsAreSelected()
        {
            // Arrange
            int eventId = 1;
            int filterTeam = 5;

            var form = new FormCollection(
                new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
                {
                    { 
                        "SelectCompetitorId", 
                        new Microsoft.Extensions.Primitives.StringValues()
                    }
                });

            // Act
            var result = await _controller.Create(eventId, filterTeam, form);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(eventId, redirect.RouteValues?["eventId"]);
            Assert.Equal(filterTeam, redirect.RouteValues?["filterTeam"]);

            _competitorServiceMock.Verify(
                s => s.GetCompetitorInTeamsByIdsAsync(It.IsAny<List<int>>()),
                Times.Never);
        }

        [Fact]
        public async Task Create_Post_Redirects_WhenNoValidCompetitorsInTeamAreFound()
        {
            // Arrange
            int eventId = 1;
            int filterTeam = 5;

            var form = new FormCollection(
                new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
                {
                    {
                        "SelectCompetitorId",
                        new Microsoft.Extensions.Primitives.StringValues("10")
                    }
                });

            _competitorServiceMock
                .Setup(s => s.GetCompetitorInTeamsByIdsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<CompetitorInTeam>());

            // Act
            var result = await _controller.Create(eventId, filterTeam, form);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(eventId, redirect.RouteValues?["eventId"]);
            Assert.Equal(filterTeam, redirect.RouteValues?["filterTeam"]);

            _competitorInEventServiceMock.Verify(
                s => s.Create(It.IsAny<List<CompetitorsInEvent>>()),
                Times.Never);
        }

        [Fact]
        public async Task Create_Post_Redirects_WhenSelectedCompetitorIdIsInvalid()
        {
            // Arrange
            var form = new FormCollection(new Dictionary<string, StringValues>
            {
                ["SelectCompetitorId"] = new StringValues("abc")
            });

            // Act
            var result = await _controller.Create(1, null, form);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(1, redirect.RouteValues!["eventId"]);
        }

        [Fact]
        public async Task Create_Get_ReturnsNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            _eventServiceMock
                .Setup(s => s.GetEventById(1))
                .ReturnsAsync((Event)null!);

            // Act
            var result = await _controller.Create(1, null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Edit Tests
        [Fact]
        public async Task Edit_Post_UpdatesAndRedirects()
        {
            // Arrange
            var entity = new CompetitorsInEvent { Id = 1, EventId = 2 };
            var vm = new CompetitorInEventViewModel
            {
                CompetitorInEventId = 1,
                EventId = 2,
                EventNumber = 10,
                OutOfCompetition = false,
                InSelection = true
            };

            _competitorInEventServiceMock.Setup(s => s.GetCompetitorById(1)).ReturnsAsync(entity);

            // Act
            var result = await _controller.Edit(1, null, vm);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            _competitorInEventServiceMock.Verify(s => s.Update(It.IsAny<CompetitorsInEvent>()), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_UpdatesEntity_AndRedirects()
        {
            // Arrange
            int id = 1;
            var competitorInEvent = new CompetitorsInEvent
            {
                Id = id,
                EventId = 10,
                EventNumber = 5,
                OutOfCompetition = false,
                InSelectie = false
            };

            var vm = new CompetitorInEventViewModel
            {
                CompetitorInEventId = id,
                EventId = 10,
                EventNumber = 99,
                OutOfCompetition = true,
                InSelection = true
            };

            _competitorInEventServiceMock.Setup(s => s.GetCompetitorById(id))
                .ReturnsAsync(competitorInEvent);

            _competitorInEventServiceMock.Setup(s => s.Update(It.IsAny<CompetitorsInEvent>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Edit(id, 3, vm);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(vm.EventId, redirect.RouteValues?["eventId"]);
            Assert.Equal(3, redirect.RouteValues?["FilterTeam"]);
        }

        [Fact]
        public async Task Edit_Post_ExceptionThrown_WhenEntityNotExists_ReturnsNotFound()
        {
            // Arrange
            int id = 1;
            var vm = new CompetitorInEventViewModel
            {
                CompetitorInEventId = id,
                EventId = 10,
                EventNumber = 99
            };

            // Setup zodat GetCompetitorById eerst werkt, maar daarna null oplevert bij Exists-check
            _competitorInEventServiceMock.SetupSequence(s => s.GetCompetitorById(id))
                .ReturnsAsync(new CompetitorsInEvent { Id = id })
                .ReturnsAsync((CompetitorsInEvent?)null);

            _competitorInEventServiceMock.Setup(s => s.Update(It.IsAny<CompetitorsInEvent>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.Edit(id, null, vm);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            int id = 1;
            var vm = new CompetitorInEventViewModel
            {
                CompetitorInEventId = id,
                EventId = 10,
                FirstName = "Jan",
                LastName = "Jansen"
            };

            // Forceer ModelState error
            _controller.ModelState.AddModelError("EventNumber", "Required");

            // Act
            var result = await _controller.Edit(id, null, vm);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<CompetitorInEventViewModel>(viewResult.Model);
            Assert.Equal(vm, returnedModel); // Hetzelfde model wordt teruggegeven
        }

        [Fact]
        public async Task Edit_Post_CompetitorNotFound_ReturnsNotFound()
        {
            // Arrange
            int id = 1;
            var vm = new CompetitorInEventViewModel { CompetitorInEventId = id, EventId = 10 };

            _competitorInEventServiceMock.Setup(s => s.GetCompetitorById(id))
                .ReturnsAsync((CompetitorsInEvent?)null); // Simuleer niet gevonden

            // Act
            var result = await _controller.Edit(id, null, vm);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ReturnsNotFound_WhenCompetitorInEventDoesNotExist()
        {
            // Arrange
            int id = 1;

            var vm = new CompetitorInEventViewModel
            {
                CompetitorInEventId = id,
                EventId = 10,
                EventNumber = 99
            };

            _competitorInEventServiceMock
                .Setup(s => s.GetCompetitorById(id))
                .ReturnsAsync((CompetitorsInEvent?)null);

            // Act
            var result = await _controller.Edit(id, null, vm);

            // Assert
            Assert.IsType<NotFoundResult>(result);

            _competitorInEventServiceMock.Verify(
                s => s.Update(It.IsAny<CompetitorsInEvent>()),
                Times.Never);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenIdIsNull()
        {
            // Act
            var result = await _controller.Edit(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenCompetitorInEventDoesNotExist()
        {
            // Arrange
            _competitorInEventServiceMock
                .Setup(s => s.GetCompetitorById(1))
                .ReturnsAsync((CompetitorsInEvent)null!);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
public async Task Edit_Post_RethrowsException_WhenEntityStillExists()
{
    // Arrange
    var entity = new CompetitorsInEvent
    {
        Id = 1,
        EventId = 2,
        EventNumber = 10
    };

    var vm = new CompetitorInEventViewModel
    {
        CompetitorInEventId = 1,
        EventId = 2,
        EventNumber = 20
    };

    _competitorInEventServiceMock
        .Setup(s => s.GetCompetitorById(1))
        .ReturnsAsync(entity);

    _competitorInEventServiceMock
        .Setup(s => s.Update(It.IsAny<CompetitorsInEvent>()))
        .ThrowsAsync(new Exception("test"));

    // Act & Assert
    await Assert.ThrowsAsync<Exception>(() =>
        _controller.Edit(1, 0, vm));
}
        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_Get_ValidId_ReturnsView()
        {
            var entity = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 2,
                CompetitorInTeam = new CompetitorInTeam
                {
                    Competitor = new Competitor { FirstName = "Jan", LastName = "Jansen" },
                    TeamYear = new TeamYear { TeamId =10, Name  = "Team A" }
                }
            };

            _competitorInEventServiceMock.Setup(s => s.GetCompetitorById(1)).ReturnsAsync(entity);

            var result = await _controller.Delete(1, 2);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CompetitorInEventViewModel>(view.Model);
            Assert.Equal("Jan", model.FirstName);
        }

        [Fact]
        public async Task Delete_Get_InvalidId_ReturnsNotFound()
        {
            _competitorInEventServiceMock
                .Setup(s => s.GetCompetitorById(It.IsAny<int>()))
                .ReturnsAsync((CompetitorsInEvent?)null);

            var result = await _controller.Delete(99, 2);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_ReturnsRedirect_WhenSuccessful()
        {
            var entity = new CompetitorsInEvent { Id = 1, EventId = 9 };
            _competitorInEventServiceMock.Setup(s => s.GetCompetitorById(1)).ReturnsAsync(entity);

            // Act
            var result = await _controller.DeleteConfirmed(1, null);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            _competitorInEventServiceMock.Verify(s => s.Delete(entity), Times.Once);
        }

        [Fact]
        public async Task DeleteConfirmed_ValidId_RedirectsToIndex()
        {
            // Arrange
            var competitorInEvent = new CompetitorsInEvent { Id = 1, EventId = 10 };
            _competitorInEventServiceMock.Setup(s => s.GetCompetitorById(1))
                .ReturnsAsync(competitorInEvent);
            _competitorInEventServiceMock.Setup(s => s.Delete(competitorInEvent))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteConfirmed(1, null);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(10, redirect.RouteValues?["eventId"]);
        }

        [Fact]
        public async Task DeleteConfirmed_ReturnsRedirectWithError_WhenInvalidOperationException()
        {
            var entity = new CompetitorsInEvent { Id = 1, EventId = 5 };
            _competitorInEventServiceMock.Setup(s => s.GetCompetitorById(1)).ReturnsAsync(entity);
            _competitorInEventServiceMock.Setup(s => s.Delete(entity)).ThrowsAsync(new InvalidOperationException("Cannot delete"));
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            // Act
            var result = await _controller.DeleteConfirmed(1, null);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(5, redirect.RouteValues?["eventId"]);
            Assert.Equal("Cannot delete", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task DeleteConfirmed_InvalidId_ReturnsNotFound()
        {
            // Arrange
            _competitorInEventServiceMock.Setup(s => s.GetCompetitorById(It.IsAny<int>()))
                .ReturnsAsync((CompetitorsInEvent?)null);

            // Act
            var result = await _controller.DeleteConfirmed(99, null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_GenericException_WhenEntityRemovedDuringDelete_ReturnsNotFound()
        {
            var entity = new CompetitorsInEvent { Id = 1, EventId = 5 };
            _competitorInEventServiceMock.SetupSequence(s => s.GetCompetitorById(1))
                .ReturnsAsync(entity)
                .ReturnsAsync((CompetitorsInEvent?)null); // tweede keer null

            _competitorInEventServiceMock.Setup(s => s.Delete(entity))
                .ThrowsAsync(new Exception("DB failure"));

            var result = await _controller.DeleteConfirmed(1, null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_RethrowsException_WhenEntityStillExists()
        {
            // Arrange
            var entity = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 5
            };

            _competitorInEventServiceMock
                .Setup(s => s.GetCompetitorById(1))
                .ReturnsAsync(entity);

            _competitorInEventServiceMock
                .Setup(s => s.Delete(entity))
                .ThrowsAsync(new Exception("DB failure"));

            // CompetitorsInEventExists moet true opleveren
            // doordat dezelfde entity nog steeds bestaat.
            // Daarom is één setup voldoende.

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _controller.DeleteConfirmed(1, null));

            Assert.Equal("DB failure", exception.Message);
        }

        [Fact]
        public async Task Delete_Get_ReturnsViewWithEmptyTeam_WhenCompetitorHasNoTeam()
        {
            // Arrange
            var entity = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 2,
                CompetitorInTeam = new CompetitorInTeam
                {
                    Competitor = new Competitor
                    {
                        FirstName = "Jan",
                        LastName = "Jansen",
                        CompetitorInTeams = new List<CompetitorInTeam>()
                    }
                }
            };

            _competitorInEventServiceMock
                .Setup(s => s.GetCompetitorById(1))
                .ReturnsAsync(entity);

            // Act
            var result = await _controller.Delete(1, 2);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CompetitorInEventViewModel>(view.Model);

            Assert.Equal("Jan", model.FirstName);
            Assert.Equal("Jansen", model.LastName);
            Assert.Equal(string.Empty, model.TeamName);
            Assert.Equal(0, model.TeamId);
        }

        [Fact]
        public async Task Delete_Get_ReturnsViewWithEmptyCompetitorFields_WhenCompetitorIsNull()
        {
            // Arrange
            var entity = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 2,
                CompetitorInTeam = new CompetitorInTeam
                {
                    Competitor = null!
                }
            };

            _competitorInEventServiceMock
                .Setup(s => s.GetCompetitorById(1))
                .ReturnsAsync(entity);

            // Act
            var result = await _controller.Delete(1, 2);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CompetitorInEventViewModel>(view.Model);

            Assert.Equal(string.Empty, model.FirstName);
            Assert.Equal(string.Empty, model.LastName);
            Assert.Equal(string.Empty, model.TeamName);
            Assert.Equal(0, model.TeamId);
        }
        #endregion

        #region Helpers Tests
        [Fact]
        public async Task GetCompetitorForEvent_ReturnsJson_WithExpectedValues()
        {
            // Arrange
            int teamYearId = 1;
            int seasonYearId = 1;
            _competitorServiceMock
                .Setup(s => s.GetByTeamId(teamYearId))
                .ReturnsAsync(new List<CompetitorInTeamDto>
            {
                new CompetitorInTeamDto
                {
                    CompetitorInTeamId = 1,
                    FirstName = "Jan",
                    LastName = "Jansen",
                    TeamId = 1,
                    TeamName = "Team A",
                    SeasonYearId = 1,
                    Year = 2025
                },
                new CompetitorInTeamDto
                {
                    CompetitorInTeamId = 2,
                    FirstName = "Piet",
                    LastName = "Pietersen",
                    TeamId = 2,
                    TeamName = "Team A",
                    SeasonYearId = 1,
                    Year = 2025
                }
            });

            // Act
            var result = await _controller.GetCompetitorForEvent(teamYearId, seasonYearId);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<object>>(json.Value);
            Assert.Contains(list, i => i.ToString()?.Contains("Jansen, Jan") == true);
        }

        [Fact]
        public async Task GetCompetitorForEvent_ReturnsEmptyJson_WhenNoCompetitorsMatchSeason()
        {
            // Arrange
            int teamId = 1;
            int seasonYearId = 2025;

            _competitorServiceMock
                .Setup(s => s.GetByTeamId(teamId))
                .ReturnsAsync(new List<CompetitorInTeamDto>
                {
                    new CompetitorInTeamDto
                    {
                        CompetitorInTeamId = 1,
                        FirstName = "Jan",
                        LastName = "Jansen",
                        TeamId = teamId,
                        SeasonYearId = 2024,
                        Year = 2024
                    }
                });

            // Act
            var result = await _controller.GetCompetitorForEvent(
                teamId,
                seasonYearId);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var competitors = Assert.IsAssignableFrom<IEnumerable<object>>(json.Value);

            Assert.Empty(competitors);
        }

        [Fact]
        public void FilterCompetitors_ReturnsEmptyList_WhenCompetitorsIsNull()
        {
            // Act
            var result = CompetitorsInEventsController.FilterCompetitors(
                null,
                new List<int> { 1, 2 },
                null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void FilterCompetitors_ExcludesCompetitor_WhenTeamsIsNull()
        {
            // Arrange
            var competitors = new List<CompetitorDto>
            {
                new CompetitorDto
                {
                    CompetitorId = 1,
                    FirstName = "Jan",
                    LastName = "Jansen",
                    Teams = null!
                },
                new CompetitorDto
                {
                    CompetitorId = 2,
                    FirstName = "Piet",
                    LastName = "Pietersen",
                    Teams = new List<CompetitorInTeamDto>
                    {
                        new CompetitorInTeamDto
                        {
                            CompetitorInTeamId = 20,
                            TeamId = 5
                        }
                    }
                }
            };

            // Act
            var result = CompetitorsInEventsController.FilterCompetitors(
                competitors,
                new List<int> { 5 },
                null);

            // Assert
            Assert.Single(result);
            Assert.Equal(2, result[0].CompetitorId);
        }

        [Fact]
        public void FilterCompetitors_FiltersBySpecificTeam_WhenFilterTeamIsSet()
        {
            // Arrange
            var competitors = new List<CompetitorDto>
            {
                new CompetitorDto
                {
                    CompetitorId = 1,
                    FirstName = "Jan",
                    LastName = "Jansen",
                    Teams = new List<CompetitorInTeamDto>
                    {
                        new CompetitorInTeamDto
                        {
                            CompetitorInTeamId = 101,
                            TeamId = 5
                        }
                    }
                },
                new CompetitorDto
                {
                    CompetitorId = 2,
                    FirstName = "Piet",
                    LastName = "Pietersen",
                    Teams = new List<CompetitorInTeamDto>
                    {
                        new CompetitorInTeamDto
                        {
                            CompetitorInTeamId = 102,
                            TeamId = 9
                        }
                    }
                }
            };

            // Act
            var result = CompetitorsInEventsController.FilterCompetitors(
                competitors,
                new List<int> { 5, 9 },
                5);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].CompetitorId);
        }
        #endregion

    }
}
