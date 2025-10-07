using AutoMapper;
using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Domain.Dto;
using Domain.Interfaces;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebCycle.Controllers;

namespace CycleManager.Tests.Unit
{
    public class EventControllerTests
    {
        [Fact]
        public async Task GetEvent_ReturnsMappedEventsWithCalculatedScores()
        {
            // Arrange
            var mockEventService = new Mock<IEventService>();
            var mockResultService = new Mock<IResultService>();
            var mockMapper = new Mock<IMapper>();
            var mockDeelnemerService = new Mock<IGameCompetitorInEventService>();
            var mockTeamService = new Mock<ITeamService>();

            // 1️⃣ Actieve events uit service
            var activeEvents = new List<Event>
            {
                new Event { EventId = 1, EventName = "Tour de Test" }
            };

            mockEventService
                .Setup(s => s.GetActiveEvents())
                .ReturnsAsync(activeEvents);

            // 2️⃣ De gemapte EventDto’s (via AutoMapper)
            var mappedEvents = new List<EventDto>
            {
                new EventDto
                {
                    EventId = 1,
                    EventName = "Tour de Test",
                    Deelnemers = new List<DeelnemerDto>
                    {
                        new DeelnemerDto { Id = 10, DeelnemerNaam = "Annemiek" },
                        new DeelnemerDto { Id = 20, DeelnemerNaam = "Mathieu" }
                    }
                }
            };

            mockMapper
                .Setup(m => m.Map<List<EventDto>>(activeEvents))
                .Returns(mappedEvents);

            // 3️⃣ Scores voor dit event (zoals uit de resultservice komt)
            var scores = new List<DeelnemerScore>
            {
                new DeelnemerScore { GameCompetitorEventId = 10, StageId = 1, TotalScore = 5 },
                new DeelnemerScore { GameCompetitorEventId = 10, StageId = 2, TotalScore = 10 },
                new DeelnemerScore { GameCompetitorEventId = 20, StageId = 1, TotalScore = 7 }
            };

            mockResultService
                .Setup(s => s.GetScoresByEventIdAsync(1))
                .ReturnsAsync(scores);

            // 4️⃣ Maak controller aan
            var controller = new EventController(
                mockEventService.Object,
                mockDeelnemerService.Object,
                mockTeamService.Object,
                mockResultService.Object,
                mockMapper.Object
            );

            // Act
            var result = await controller.GetEvent();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedEvents = Assert.IsAssignableFrom<List<EventDto>>(okResult.Value);

            var returnedEvent = returnedEvents.First();
            Assert.Equal(1, returnedEvent.EventId);
            Assert.Equal("Tour de Test", returnedEvent.EventName);
            Assert.Equal(2, returnedEvent.Deelnemers.Count);
        }

            [Fact]
        public async Task GetEventById_ReturnsOkWithEvent()
        {
            //arrange
            var mockEventService = new Mock<IEventService>();
            var mockDeelnemerService = new Mock<IGameCompetitorInEventService>();
            var mockTeamService = new Mock<ITeamService>();
            var mockResultService = new Mock<IResultService>();
            var mockMapper = new Mock<IMapper>();


            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Tour de Test"
            };

            // Stel mapping in: domain → DTO
            var eventDto = new EventDto
            {
                EventId = 1,
                EventName = "Tour de Test",
                Deelnemers = new List<DeelnemerDto>
            {
                new DeelnemerDto { Id = 10, DeelnemerNaam = "Annemiek" },
                new DeelnemerDto { Id = 20, DeelnemerNaam = "Mathieu" }
            }
            };

            mockEventService
            .Setup(s => s.GetEventById(1))
            .ReturnsAsync(eventEntity);

            mockMapper
                .Setup(m => m.Map<EventDto>(eventEntity))
                .Returns(eventDto);

            mockDeelnemerService
                .Setup(s => s.GetAllPicks(10))
                .ReturnsAsync(new List<GameCompetitorEventPick>
                {
                    new GameCompetitorEventPick { CompetitorsInEventId = 100 },
                    new GameCompetitorEventPick { CompetitorsInEventId = 200 }
                });
            mockDeelnemerService
                .Setup(s => s.GetAllPicks(20))
                .ReturnsAsync(new List<GameCompetitorEventPick>
                {
                    new GameCompetitorEventPick { CompetitorsInEventId = 300 }
                });

            mockResultService
                .Setup(s => s.GetCompetitorResultsByEventId(1, 100))
                .ReturnsAsync(new CompetitorScoreDto { TotalScore = 5 });

            mockResultService
            .Setup(s => s.GetCompetitorResultsByEventId(1, 200))
            .ReturnsAsync(new CompetitorScoreDto { TotalScore = 10 });

            mockResultService
            .Setup(s => s.GetCompetitorResultsByEventId(1, 300))
            .ReturnsAsync(new CompetitorScoreDto { TotalScore = 20 });

            var controller = new EventController(
                mockEventService.Object,
                mockDeelnemerService.Object,
                mockTeamService.Object,
                mockResultService.Object,
                mockMapper.Object);

            //Act
            var result = await controller.GetEvent(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();

            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedEvent = okResult!.Value.Should().BeOfType<EventDto>().Subject;
            returnedEvent.Should().NotBeNull();
            returnedEvent.EventId.Should().Be(1);
            returnedEvent.Deelnemers.Should().HaveCount(2);

            var deelnemer1 = returnedEvent.Deelnemers.First(d => d.Id == 10);
            var deelnemer2 = returnedEvent.Deelnemers.First(d => d.Id == 20);

            deelnemer1.Punten.Should().Be(15, "Annemiek had 5 + 10 punten");
            deelnemer2.Punten.Should().Be(20, "Mathieu had 20 punten");

            mockEventService.Verify(s => s.GetEventById(1), Times.Once);
            mockDeelnemerService.Verify(s => s.GetAllPicks(It.IsAny<int>()), Times.Exactly(2));
            mockResultService.Verify(s => s.GetCompetitorResultsByEventId(1, It.IsAny<int>()), Times.Exactly(3));
        }

        [Fact]
        public async Task GetEventById_EventNotFound_ReturnsNotFound()
        {
            //arrange
            var mockEventService = new Mock<IEventService>();
            var mockDeelnemerService = new Mock<IGameCompetitorInEventService>();
            var mockTeamService = new Mock<ITeamService>();
            var mockResultService = new Mock<IResultService>();
            var mockMapper = new Mock<IMapper>();

            mockEventService.Setup(s => s.GetEventById(99))
                .ReturnsAsync((Event?)null);

            var controller = new EventController(mockEventService.Object, mockDeelnemerService.Object, mockTeamService.Object, mockResultService.Object, mockMapper.Object);

            //act
            var result = await controller.GetEvent(99);

            //assert
            Assert.IsType<NotFoundResult>(result);

        }

        [Fact]
        public async Task GetEventById_EventNotFound_DoesNotCallMapper_AndReturnsNotFound()
        {
            // Arrange
            var mockEventRepo = new Mock<IEventRepository>();
            var mockScoreRepo = new Mock<IScoreRepository>();
            var mockEventService = new Mock<IEventService>();
            var mockDeelnemerService = new Mock<IGameCompetitorInEventService>();
            var mockTeamService = new Mock<ITeamService>();
            var mockResultService = new Mock<IResultService>();
            var mockMapper = new Mock<IMapper>();

            mockEventService
                .Setup(s => s.GetEventById(99))
                .ReturnsAsync((Event?)null);

            var controller = new EventController(
                mockEventService.Object,
                mockDeelnemerService.Object,
                mockTeamService.Object,
                mockResultService.Object,
                mockMapper.Object
            );

            // Act
            var result = await controller.GetEvent(99);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            mockMapper.Verify(m => m.Map<EventDto>(It.IsAny<Event>()), Times.Never);
        }

        [Fact]
        public async Task GetStagesByEventId_ValidId_ReturnsOkWithStages()
        {
            // Arrange
            var mockEventService = new Mock<IEventService>();

            var sampleStages = new List<StageResultDto>
            {
            new StageResultDto { StageId = 1, StageNumber = "Proloog" },
            new StageResultDto { StageId = 2, StageNumber = "Etappe 1" }
            };

            mockEventService
                .Setup(s => s.GetStagesWithResultsForEvent(1))
                .ReturnsAsync(sampleStages);

            var controller = new EventController(
                mockEventService.Object,
                new Mock<IGameCompetitorInEventService>().Object,
                new Mock<ITeamService>().Object,
                new Mock<IResultService>().Object,
                new Mock<IMapper>().Object
            );

            // Act
            var result = await controller.GetStagesByEventId(1);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var stages = okResult.Value.Should().BeAssignableTo<IEnumerable<StageResultDto>>().Subject;

            stages.Should().HaveCount(2);
            stages.Should().ContainSingle(s => s.StageNumber == "Proloog");
            stages.Should().ContainSingle(s => s.StageNumber == "Etappe 1");

            mockEventService.Verify(s => s.GetStagesWithResultsForEvent(1), Times.Once);
        }

        [Fact]
        public async Task GetStagesByEventId_NoStages_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mockEventService = new Mock<IEventService>();
            mockEventService
                .Setup(s => s.GetStagesWithResultsForEvent(It.IsAny<int>()))
                .ReturnsAsync(new List<StageResultDto>());

            var controller = new EventController(
                mockEventService.Object,
                new Mock<IGameCompetitorInEventService>().Object,
                new Mock<ITeamService>().Object,
                new Mock<IResultService>().Object,
                new Mock<IMapper>().Object
            );

            // Act
            var result = await controller.GetStagesByEventId(1);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var stages = okResult.Value.Should().BeAssignableTo<IEnumerable<StageResultDto>>().Subject;

            stages.Should().BeEmpty();
        }

        [Fact]
        public async Task GetStagesByEventId_ServiceThrowsException_ThrowsException()
        {
            // Arrange
            var mockEventService = new Mock<IEventService>();
            mockEventService
                .Setup(s => s.GetStagesWithResultsForEvent(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database down"));

            var controller = new EventController(
                mockEventService.Object,
                new Mock<IGameCompetitorInEventService>().Object,
                new Mock<ITeamService>().Object,
                new Mock<IResultService>().Object,
                new Mock<IMapper>().Object
            );

            // Act & Assert
            await FluentActions
                .Invoking(() => controller.GetStagesByEventId(1))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Database down");
        }

        [Fact]
        public async Task GetEventByUserId_CorrectlyCategorizesEvents()
        {
            // Arrange
            var userid = "user1";
            var now = DateTime.UtcNow;

            // Alle events
            var allEvents = new List<Event>
            {
                new Event { EventId = 1, StartDate = now.AddDays(-2), EndDate = now.AddDays(2), IsActive = true }, // Actief
                new Event { EventId = 2, StartDate = now.AddDays(3), EndDate = now.AddDays(5) },  // Toekomstig
                new Event { EventId = 3, StartDate = now.AddDays(-10), EndDate = now.AddDays(-5) } // Historisch
            };

            // Events die de user heeft
            var eventsForUser = new List<EventForUserDto>
            {
                new EventForUserDto { EventId = 1, StartDate = (DateTime)allEvents[0].StartDate, EndDate = (DateTime)allEvents[0].EndDate, IsIngeschreven = true }, // Actief
                new EventForUserDto { EventId = 3, StartDate = (DateTime)allEvents[2].StartDate, EndDate = (DateTime)allEvents[2].EndDate, IsIngeschreven = true }  // Historisch
            };

            var mockEventService = new Mock<IEventService>();
            mockEventService.Setup(s => s.GetAllEvents()).ReturnsAsync(allEvents);
            mockEventService.Setup(s => s.GetEventsByUserId(userid)).ReturnsAsync(eventsForUser);

            var mockMapper = new Mock<IMapper>();
            // Mapping van Event → EventForUserDto
            mockMapper.Setup(m => m.Map<List<EventForUserDto>>(It.IsAny<List<Event>>()))
                      .Returns<List<Event>>(events => events.Select(e => new EventForUserDto
                      {
                          EventId = e.EventId,
                          StartDate = e.StartDate ?? DateTime.MinValue,
                          EndDate = e.EndDate ?? DateTime.MinValue
                      }).ToList());

            var controller = new EventController(
                mockEventService.Object,
                Mock.Of<IGameCompetitorInEventService>(),
                Mock.Of<ITeamService>(),
                Mock.Of<IResultService>(),
                mockMapper.Object
            );

            // Act
            var result = await controller.GetEventByUserId(userid);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<EventViewDto>(okResult.Value);

            // Controleer categorieën
            Assert.Single(dto.ActieveEvenementen);
            Assert.Equal(1, dto.ActieveEvenementen[0].EventId);

            Assert.Single(dto.HistorischeEvenementen);
            Assert.Equal(3, dto.HistorischeEvenementen[0].EventId);

            Assert.Single(dto.ToekomstigeEvenementen);
            Assert.Equal(2, dto.ToekomstigeEvenementen[0].EventId);

            // Controleer flags
            Assert.True(dto.ActieveEvenementen[0].IsIngeschreven);
            Assert.False(dto.ToekomstigeEvenementen[0].IsIngeschreven);
            Assert.True(dto.HistorischeEvenementen[0].IsIngeschreven);

            // Controleer UserId
            Assert.All(dto.ActieveEvenementen.Concat(dto.ToekomstigeEvenementen).Concat(dto.HistorischeEvenementen),
                e => Assert.Equal(userid, e.UserId));
        }

        [Fact]
        public async Task GetTeamsWithRennersForEvent_ReturnsOk_WhenTeamsExist()
        {
            // Arrange
            int eventId = 1;
            var mockEventService = new Mock<IEventService>();

            // Event bestaat
            mockEventService.Setup(s => s.GetEventById(eventId))
                            .ReturnsAsync(new Event { EventId = eventId });

            // Teams voor event
            var teams = new List<TeamDto>
            {
                new TeamDto
                {
                    Id = 1,
                    Naam = "Team A",
                    Renners = new List<CompetitorDto>
                    {
                        new CompetitorDto { CompetitorInTeamId = 1,  FirstName = "R", LastName = "Renner 1" },
                        new CompetitorDto { CompetitorInTeamId = 2, FirstName = "T", LastName = "Renner 2" }
                    }
                },
                new TeamDto
                {
                    Id = 2,
                    Naam = "Team B",
                    Renners = new List<CompetitorDto>
                    {
                        new CompetitorDto { CompetitorInTeamId = 3,  FirstName = "D", LastName = "Renner 3" },
                        new CompetitorDto { CompetitorInTeamId = 4, FirstName = "A", LastName = "Renner 4" }
                    }
                }
            };

            mockEventService.Setup(s => s.GetTeamsForEvent(eventId))
                            .ReturnsAsync(teams);

            var controller = new EventController(
                mockEventService.Object,
                Mock.Of<IGameCompetitorInEventService>(),
                Mock.Of<ITeamService>(),
                Mock.Of<IResultService>(),
                Mock.Of<IMapper>()
            );

            // Act
            var result = await controller.GetTeamsWithRennersForEvent(eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTeams = Assert.IsAssignableFrom<IEnumerable<TeamDto>>(okResult.Value);
            Assert.Equal(2, returnedTeams.Count());
        }

        [Fact]
        public async Task GetTeamsWithRennersForEvent_ReturnsNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            int eventId = 1;
            var mockEventService = new Mock<IEventService>();
            mockEventService.Setup(s => s.GetEventById(eventId))
                            .ReturnsAsync((Event)null);

            var controller = new EventController(
                mockEventService.Object,
                Mock.Of<IGameCompetitorInEventService>(),
                Mock.Of<ITeamService>(),
                Mock.Of<IResultService>(),
                Mock.Of<IMapper>()
            );

            // Act
            var result = await controller.GetTeamsWithRennersForEvent(eventId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetTeamsWithRennersForEvent_ReturnsNotFound_WhenNoTeamsExist()
        {
            // Arrange
            int eventId = 1;
            var mockEventService = new Mock<IEventService>();
            mockEventService.Setup(s => s.GetEventById(eventId))
                            .ReturnsAsync(new Event { EventId = eventId });
            mockEventService.Setup(s => s.GetTeamsForEvent(eventId))
                            .ReturnsAsync((List<TeamDto>)null);

            var controller = new EventController(
                mockEventService.Object,
                Mock.Of<IGameCompetitorInEventService>(),
                Mock.Of<ITeamService>(),
                Mock.Of<IResultService>(),
                Mock.Of<IMapper>()
            );

            // Act
            var result = await controller.GetTeamsWithRennersForEvent(eventId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetTeamsWithRennersFromTeam_TeamExists_ReturnsCompetitors()
        {
            // Arrange
            var teamId = 1;
            var year = DateTime.Now.Year;

            var team = new Team
            {
                TeamId = teamId,
                CurrentTeamName = "Team A",
                CompetitorInTeams = new List<CompetitorInTeam>
                {
                    new CompetitorInTeam
                    {
                        Id = 10,
                        Year = year,
                        Competitor = new Competitor
                        {
                            CompetitorId = 101,
                            FirstName = "John",
                            LastName = "Doe",
                            PcsName = "JD123"
                        }
                    },
                    new CompetitorInTeam
                    {
                        Id = 11,
                        Year = year,
                        Competitor = new Competitor
                        {
                            CompetitorId = 102,
                            FirstName = "Jane",
                            LastName = "Smith",
                            PcsName = "JS456"
                        }
                    }
                }
            };

            var mockTeamService = new Mock<ITeamService>();
            mockTeamService.Setup(s => s.GetTeamForCurrentYear(teamId, year))
                           .ReturnsAsync(team);

            var controller = new EventController(
                Mock.Of<IEventService>(),
                Mock.Of<IGameCompetitorInEventService>(),
                mockTeamService.Object,
                Mock.Of<IResultService>(),
                Mock.Of<IMapper>()
            );

            // Act
            var result = await controller.GetTeamsWithRennersFromTeam(teamId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var competitors = Assert.IsAssignableFrom<List<CompetitorInSelectieDto>>(okResult.Value);

            Assert.Equal(2, competitors.Count);
            Assert.Contains(competitors, c => c.FirstName == "John" && c.LastName == "Doe");
            Assert.Contains(competitors, c => c.FirstName == "Jane" && c.LastName == "Smith");
        }

        [Fact]
        public async Task GetTeamsWithRennersFromTeam_TeamDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var teamId = 1;
            var year = DateTime.Now.Year;

            var mockTeamService = new Mock<ITeamService>();
            mockTeamService.Setup(s => s.GetTeamForCurrentYear(teamId, year))
                           .ReturnsAsync((Team?)null);

            var controller = new EventController(
                Mock.Of<IEventService>(),
                Mock.Of<IGameCompetitorInEventService>(),
                mockTeamService.Object,
                Mock.Of<IResultService>(),
                Mock.Of<IMapper>()
            );

            // Act
            var result = await controller.GetTeamsWithRennersFromTeam(teamId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task SlaSelectieOp_DtoIsNull_ReturnsBadRequest()
        {
            // Arrange
            var mockEventService = new Mock<IEventService>();
            var controller = new EventController(
                mockEventService.Object,
                Mock.Of<IGameCompetitorInEventService>(),
                Mock.Of<ITeamService>(),
                Mock.Of<IResultService>(),
                Mock.Of<IMapper>()
            );

            // Act
            var result = await controller.SlaSelectieOp(null);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Selectie dto is null.", badRequest.Value);
        }

        [Fact]
        public async Task SlaSelectieOp_SaveThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var mockEventService = new Mock<IEventService>();
            mockEventService
                .Setup(s => s.SaveSelectie(It.IsAny<SelectieDto>()))
                .ThrowsAsync(new Exception("Database error"));

            var controller = new EventController(
                mockEventService.Object,
                Mock.Of<IGameCompetitorInEventService>(),
                Mock.Of<ITeamService>(),
                Mock.Of<IResultService>(),
                Mock.Of<IMapper>()
            );

            var dto = new SelectieDto(); // vul eventueel properties in

            // Act
            var result = await controller.SlaSelectieOp(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Er ging iets mis met het opslaan van je pool.", badRequest.Value);
        }

        [Fact]
        public async Task SlaSelectieOp_ValidDto_ReturnsOk()
        {
            // Arrange
            var mockEventService = new Mock<IEventService>();
            mockEventService
                .Setup(s => s.SaveSelectie(It.IsAny<SelectieDto>()))
                .Returns(Task.CompletedTask);

            var controller = new EventController(
                mockEventService.Object,
                Mock.Of<IGameCompetitorInEventService>(),
                Mock.Of<ITeamService>(),
                Mock.Of<IResultService>(),
                Mock.Of<IMapper>()
            );

            var dto = new SelectieDto(); // vul eventueel properties in

            // Act
            var result = await controller.SlaSelectieOp(dto);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task CreatePool_DtoIsNull_ReturnsBadRequest()
        {
            // Arrange
            var mockEventService = new Mock<IEventService>();
            var controller = new EventController(
                mockEventService.Object,
                Mock.Of<IGameCompetitorInEventService>(),
                Mock.Of<ITeamService>(),
                Mock.Of<IResultService>(),
                Mock.Of<IMapper>()
            );

            // Act
            var result = await controller.CreatePool(null);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Deelnemer dto is null.", badRequest.Value);
        }

        [Fact]
        public async Task CreatePool_ValidDto_ReturnsOk()
        {
            // Arrange
            var mockEventService = new Mock<IEventService>();
            var dto = new DeelnemerDto(); // eventueel properties invullen
            var createdPool = new DeelnemerDto { Id = 1 }; // simulate created pool

            mockEventService
                .Setup(s => s.CreatePoolAsync(dto))
                .ReturnsAsync(createdPool);

            var controller = new EventController(
                mockEventService.Object,
                Mock.Of<IGameCompetitorInEventService>(),
                Mock.Of<ITeamService>(),
                Mock.Of<IResultService>(),
                Mock.Of<IMapper>()
            );

            // Act
            var result = await controller.CreatePool(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(createdPool, okResult.Value);
        }

        [Fact]
        public async Task CreatePool_CreateFails_ReturnsBadRequest()
        {
            // Arrange
            var mockEventService = new Mock<IEventService>();
            var dto = new DeelnemerDto(); // eventueel properties invullen

            // Simuleer dat CreatePoolAsync null of Id <= 0 teruggeeft
            mockEventService
                .Setup(s => s.CreatePoolAsync(dto))
                .ReturnsAsync(new DeelnemerDto { Id = 0 });

            var controller = new EventController(
                mockEventService.Object,
                Mock.Of<IGameCompetitorInEventService>(),
                Mock.Of<ITeamService>(),
                Mock.Of<IResultService>(),
                Mock.Of<IMapper>()
            );

            // Act
            var result = await controller.CreatePool(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Er is iets misgegaan bij het aanmaken van de pool.", badRequest.Value);
        }

        [Fact]
        public async Task DeleteDeelnemer_Success_ReturnsNoContent()
        {
            // Arrange
            var mockEventService = new Mock<IEventService>();
            var controller = new EventController(
                mockEventService.Object,
                Mock.Of<IGameCompetitorInEventService>(),
                Mock.Of<ITeamService>(),
                Mock.Of<IResultService>(),
                Mock.Of<IMapper>()
            );

            var deelnemerId = 1;

            mockEventService.Setup(s => s.DeletePoolAsync(deelnemerId))
                            .Returns(Task.CompletedTask);

            // Act
            var result = await controller.DeleteDeelnemer(deelnemerId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteDeelnemer_Exception_ReturnsStatusCode500()
        {
            // Arrange
            var mockEventService = new Mock<IEventService>();
            var controller = new EventController(
                mockEventService.Object,
                Mock.Of<IGameCompetitorInEventService>(),
                Mock.Of<ITeamService>(),
                Mock.Of<IResultService>(),
                Mock.Of<IMapper>()
            );

            var deelnemerId = 1;

            mockEventService.Setup(s => s.DeletePoolAsync(deelnemerId))
                            .ThrowsAsync(new Exception("Fout"));

            // Act
            var result = await controller.DeleteDeelnemer(deelnemerId);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Equal("Fout bij verwijderen deelnemer.", statusResult.Value);
        }

        [Fact]
        public async Task GetDeelnemersAantal_ReturnsCorrectCount()
        {
            // Arrange
            var mockEventService = new Mock<IEventService>();
            var controller = new EventController(
                mockEventService.Object,
                Mock.Of<IGameCompetitorInEventService>(),
                Mock.Of<ITeamService>(),
                Mock.Of<IResultService>(),
                Mock.Of<IMapper>()
            );

            var eventId = 42;
            var expectedCount = 7;

            mockEventService.Setup(s => s.GetAantalDeelnemers(eventId))
                            .ReturnsAsync(expectedCount);

            // Act
            var result = await controller.GetDeelnemersAantal(eventId);

            // Assert
            Assert.Equal(expectedCount, result);
        }

        [Fact]
        public async Task GetDeelnemersAantal_ReturnsZero_WhenNoParticipants()
        {
            // Arrange
            var mockEventService = new Mock<IEventService>();
            var controller = new EventController(
                mockEventService.Object,
                Mock.Of<IGameCompetitorInEventService>(),
                Mock.Of<ITeamService>(),
                Mock.Of<IResultService>(),
                Mock.Of<IMapper>()
            );

            var eventId = 100;
            mockEventService.Setup(s => s.GetAantalDeelnemers(eventId))
                            .ReturnsAsync(0);

            // Act
            var result = await controller.GetDeelnemersAantal(eventId);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetDeelnemersAantal_ReturnsNegative_WhenServiceReturnsNegative()
        {
            // Arrange
            var mockEventService = new Mock<IEventService>();
            var controller = new EventController(
                mockEventService.Object,
                Mock.Of<IGameCompetitorInEventService>(),
                Mock.Of<ITeamService>(),
                Mock.Of<IResultService>(),
                Mock.Of<IMapper>()
            );

            var eventId = 101;
            mockEventService.Setup(s => s.GetAantalDeelnemers(eventId))
                            .ReturnsAsync(-5);

            // Act
            var result = await controller.GetDeelnemersAantal(eventId);

            // Assert
            Assert.Equal(-5, result);
        }

    }
    }
