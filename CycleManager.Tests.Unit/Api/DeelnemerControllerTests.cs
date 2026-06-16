using AutoMapper;
using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using WebCycle.Controllers;

namespace CycleManager.Tests.Unit.Api
{
    public class DeelnemerControllerTests
    {
        private readonly Mock<IGameCompetitorInEventService> _mockDeelnemerService;
        private readonly Mock<IEventService> _mockEventService;
        private readonly Mock<IResultService> _mockResultService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly IMemoryCache _memoryCache;
        private readonly DeelnemerController _controller;
        
        public DeelnemerControllerTests()
        {
            _mockDeelnemerService = new Mock<IGameCompetitorInEventService>();
            _mockEventService = new Mock<IEventService>();
            _mockResultService = new Mock<IResultService>();
            _mockMapper = new Mock<IMapper>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _controller = new DeelnemerController(
                _mockDeelnemerService.Object,
                _mockEventService.Object,
                _mockResultService.Object,
                _mockMapper.Object,
                _memoryCache
            );

        }
        [Fact]
        public async Task GetDeelnemerListByEventId_ReturnsOk_WithMappedDeelnemersAndScores()
        {
            // Arrange
            int eventId = 1;
            var gameCompetitorEvents = new List<GameCompetitorEvent>
            {
                new GameCompetitorEvent { Id = 1 },
                new GameCompetitorEvent { Id = 2 }
            };

            _mockDeelnemerService.Setup(s => s.GetAllCompetitorsInEvent(eventId))
                                 .ReturnsAsync(gameCompetitorEvents);

            _mockDeelnemerService.Setup(s => s.GetAllPicks(1))
                                 .ReturnsAsync(new List<GameCompetitorEventPick>
                                 {
                                     new GameCompetitorEventPick  {
                                            CompetitorsInEvent = new CompetitorsInEvent { Id = 1 } 
                                     }
                                 });


            _mockDeelnemerService.Setup(s => s.GetAllPicks(2))
                                 .ReturnsAsync(new List<GameCompetitorEventPick>
                                 {
                                     new GameCompetitorEventPick  {
                                            CompetitorsInEvent = new CompetitorsInEvent { Id = 2 }
                                     }
                                 });

            _mockResultService.Setup(s => s.GetCompetitorResultsByEventId(eventId, 1))
            .ReturnsAsync(new CompetitorScoreDto { TotalScore = 10 });

            _mockResultService.Setup(s => s.GetCompetitorResultsByEventId(eventId, 2))
            .ReturnsAsync(new CompetitorScoreDto { TotalScore = 0 });

            _mockMapper.Setup(m => m.Map<List<DeelnemerDto>>(It.IsAny<List<GameCompetitorEvent>>()))
            .Returns((List<GameCompetitorEvent> src) =>
                src.ConvertAll(c => new DeelnemerDto { Id = c.Id, DeelnemerNaam = $"Deelnemer {c.Id}" }));

            // Act
            var result = await _controller.GetDeelnemerListByEventId(eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<List<DeelnemerDto>>(okResult.Value);

            Assert.Equal(2, data.Count);
            Assert.Equal(10, data[0].Punten);
            Assert.Equal(0, data[1].Punten);
        }

        [Fact]
        public async Task GetDeelnemerListByEventId_ReturnsOk_WithEmptyList_WhenNoDeelnemers()
        {
            // Arrange
            int eventId = 1;
            _mockDeelnemerService.Setup(s => s.GetAllCompetitorsInEvent(eventId))
                .ReturnsAsync((List<GameCompetitorEvent>?)null);

            // Mapper moet null kunnen verwerken
            _mockMapper.Setup(m => m.Map<List<DeelnemerDto>>(It.IsAny<List<GameCompetitorEvent>>()))
                .Returns(new List<DeelnemerDto>());

            // Act
            var result = await _controller.GetDeelnemerListByEventId(eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<List<DeelnemerDto>>(okResult.Value);
            Assert.Empty(data);
        }

        [Fact]
        public async Task GetDeelnemerListByEventId_ReturnsOk_WhenExceptionInService()
        {
            // Arrange
            int eventId = 1;
            _mockDeelnemerService.Setup(s => s.GetAllCompetitorsInEvent(eventId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetDeelnemerListByEventId(eventId));
        }

        [Fact]
        public async Task GetDeelnemerListByEventId_UsesCacheOnSecondCall()
        {
            // Arrange
            int eventId = 1;
            var deelnemersInEvent = new List<GameCompetitorEvent>
            {
                new GameCompetitorEvent { Id = 1 }
            };

            var mappedDeelnemers = new List<DeelnemerDto>
            {
                new DeelnemerDto { Id = 1, DeelnemerNaam = "Deelnemer 1", Punten = 0 }
            };

            _mockDeelnemerService.Setup(s => s.GetAllCompetitorsInEvent(eventId))
                .ReturnsAsync(deelnemersInEvent);

            _mockDeelnemerService.Setup(s => s.GetAllPicks(It.IsAny<int>()))
                .ReturnsAsync(new List<GameCompetitorEventPick>());

            _mockMapper.Setup(m => m.Map<List<DeelnemerDto>>(It.IsAny<List<GameCompetitorEvent>>()))
                .Returns(mappedDeelnemers);

            // Act - eerste call (cache is leeg)
            var result1 = await _controller.GetDeelnemerListByEventId(eventId);
            var okResult1 = Assert.IsType<OkObjectResult>(result1);
            var data1 = Assert.IsAssignableFrom<List<DeelnemerDto>>(okResult1.Value);

            // Assert - eerste call haalt data van service
            _mockDeelnemerService.Verify(s => s.GetAllCompetitorsInEvent(eventId), Times.Once);

            // Act - tweede call (cache gevuld)
            var result2 = await _controller.GetDeelnemerListByEventId(eventId);
            var okResult2 = Assert.IsType<OkObjectResult>(result2);
            var data2 = Assert.IsAssignableFrom<List<DeelnemerDto>>(okResult2.Value);

            // Assert - tweede call haalt NIET opnieuw van service
            _mockDeelnemerService.Verify(s => s.GetAllCompetitorsInEvent(eventId), Times.Once);

            // Data moet gelijk zijn
            Assert.Equal(data1.Count, data2.Count);
            Assert.Equal(data1[0].Id, data2[0].Id);
        }

        [Fact]
        public async Task GetDeelnemerListByEventId_SetsCacheEntry()
        {
            // Arrange
            int eventId = 1;
            var deelnemersInEvent = new List<GameCompetitorEvent>
            {
                new GameCompetitorEvent { Id = 1 }
            };

            _mockDeelnemerService.Setup(s => s.GetAllCompetitorsInEvent(eventId))
                .ReturnsAsync(deelnemersInEvent);

            _mockDeelnemerService.Setup(s => s.GetAllPicks(It.IsAny<int>()))
                .ReturnsAsync(new List<GameCompetitorEventPick>());

            _mockMapper.Setup(m => m.Map<List<DeelnemerDto>>(It.IsAny<List<GameCompetitorEvent>>()))
                .Returns(new List<DeelnemerDto> { new DeelnemerDto { Id = 1, DeelnemerNaam = "Deelnemer 1" } });

            // Act
            await _controller.GetDeelnemerListByEventId(eventId);

            // Assert cache
            string cacheKey = $"deelnemers_{eventId}";
            Assert.True(_memoryCache.TryGetValue(cacheKey, out List<DeelnemerDto> cachedValue));
            Assert.Single(cachedValue);
            Assert.Equal(1, cachedValue[0].Id);
        }

        [Fact]
        public async Task GetDeelnemerListByEventId_CalculatesScoresAndUsesCache()
        {
            // Arrange
            int eventId = 1;

            var deelnemersInEvent = new List<GameCompetitorEvent>
            {
                new GameCompetitorEvent { Id = 1 },
                new GameCompetitorEvent { Id = 2 }
            };

            _mockDeelnemerService.Setup(s => s.GetAllCompetitorsInEvent(eventId))
                .ReturnsAsync(deelnemersInEvent);

            // Picks per deelnemer
            _mockDeelnemerService.Setup(s => s.GetAllPicks(1))
                .ReturnsAsync(new List<GameCompetitorEventPick>
                {
                new GameCompetitorEventPick { CompetitorsInEvent = new CompetitorsInEvent { Id = 1 } },
                new GameCompetitorEventPick { CompetitorsInEvent = new CompetitorsInEvent { Id = 1 } }
                });

            _mockDeelnemerService.Setup(s => s.GetAllPicks(2))
                .ReturnsAsync(new List<GameCompetitorEventPick>
                {
                new GameCompetitorEventPick { CompetitorsInEvent = new CompetitorsInEvent { Id = 2 } }
                });

            // Resultaten per pick
            _mockResultService.Setup(s => s.GetCompetitorResultsByEventId(eventId, 1))
                .ReturnsAsync(new CompetitorScoreDto { TotalScore = 5 }); // elke pick voor deelnemer 1 = 5 punten

            _mockResultService.Setup(s => s.GetCompetitorResultsByEventId(eventId, 2))
                .ReturnsAsync(new CompetitorScoreDto { TotalScore = 10 }); // deelnemer 2 = 10 punten

            // Mapper
            _mockMapper.Setup(m => m.Map<List<DeelnemerDto>>(It.IsAny<List<GameCompetitorEvent>>()))
                .Returns((List<GameCompetitorEvent> src) =>
                    src.ConvertAll(c => new DeelnemerDto { Id = c.Id, DeelnemerNaam = $"Deelnemer {c.Id}" }));

            // Act - eerste call (cache leeg)
            var result1 = await _controller.GetDeelnemerListByEventId(eventId);
            var okResult1 = Assert.IsType<OkObjectResult>(result1);
            var data1 = Assert.IsAssignableFrom<List<DeelnemerDto>>(okResult1.Value);

            // Assert scores
            Assert.Equal(10, data1[0].Punten); // 5 + 5 van 2 picks
            Assert.Equal(10, data1[1].Punten); // 10 van 1 pick

            // Cache moet gevuld zijn
            string cacheKey = $"deelnemers_{eventId}";
            Assert.True(_memoryCache.TryGetValue(cacheKey, out List<DeelnemerDto> cachedValue));
            Assert.Equal(2, cachedValue.Count);

            // Act - tweede call (moet cache gebruiken, service NIET opnieuw aanroepen)
            var result2 = await _controller.GetDeelnemerListByEventId(eventId);
            var okResult2 = Assert.IsType<OkObjectResult>(result2);
            var data2 = Assert.IsAssignableFrom<List<DeelnemerDto>>(okResult2.Value);

            _mockDeelnemerService.Verify(s => s.GetAllCompetitorsInEvent(eventId), Times.Once); // tweede call gebruikt cache
            _mockDeelnemerService.Verify(s => s.GetAllPicks(1), Times.Once);
            _mockDeelnemerService.Verify(s => s.GetAllPicks(2), Times.Once);

            // Data uit cache gelijk
            Assert.Equal(data1[0].Punten, data2[0].Punten);
            Assert.Equal(data1[1].Punten, data2[1].Punten);
        }

        [Fact]
        public async Task GetDeelnemerListByEventId_ReturnsEmptyList_WhenServiceReturnsNull()
        {
            // Arrange
            int eventId = 1;
            _mockDeelnemerService.Setup(s => s.GetAllCompetitorsInEvent(eventId))
                                 .ReturnsAsync((List<GameCompetitorEvent>)null);

            _mockMapper.Setup(m => m.Map<List<DeelnemerDto>>(It.IsAny<List<GameCompetitorEvent>>()))
               .Returns((List<GameCompetitorEvent> src) => src?.ConvertAll(c => new DeelnemerDto { Id = c.Id })
                                                          ?? new List<DeelnemerDto>());
            
            // Act
            var result = await _controller.GetDeelnemerListByEventId(eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<List<DeelnemerDto>>(okResult.Value);
            Assert.Empty(data); // moet een lege lijst teruggeven
        }

        [Fact]
        public async Task GetDeelnemerListByEventId_ReturnsInternalServerError_OnException()
        {
            // Arrange
            int eventId = 1;
            _mockDeelnemerService.Setup(s => s.GetAllCompetitorsInEvent(eventId))
                                 .ThrowsAsync(new Exception("Test exception"));

            // Act
            var ex = await Record.ExceptionAsync(() => _controller.GetDeelnemerListByEventId(eventId));

            // Assert
            Assert.NotNull(ex);
            Assert.IsType<Exception>(ex);
            Assert.Equal("Test exception", ex.Message);
        }

        [Fact]
        public void InvalidateDeelnemerCache_RemovesCacheEntry_AndReturnsOk()
        {
            // Arrange
            int eventId = 5;
            string cacheKey = $"deelnemers_{eventId}";

            var dummyList = new List<DeelnemerDto> { new DeelnemerDto(), new DeelnemerDto() };
            _memoryCache.Set(cacheKey, dummyList);

            // Act
            var result = _controller.InvalidateDeelnemerCache(eventId);

            // Assert
            Assert.False(_memoryCache.TryGetValue(cacheKey, out _));
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task GetResultsByCompetitorEventId_ReturnsOk_WithResults()
        {
            // Arrange
            int eventId = 1;
            int competitorInEventId = 2;

            var expectedResult = new CompetitorScoreDto
            {
                TotalScore = 50, 
                CompetitorInEventId = 1
            };

            _mockResultService
                .Setup(s => s.GetCompetitorResultsByEventId(eventId, competitorInEventId))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetResultsByCompetitorEventId(eventId, competitorInEventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<CompetitorScoreDto>(okResult.Value);
            Assert.Equal(expectedResult.TotalScore, data.TotalScore);
            Assert.Equal(expectedResult.CompetitorInEventId, data.CompetitorInEventId);
        }

        [Fact]
        public async Task GetResultsByCompetitorEventId_ReturnsOk_WithNullResult()
        {
            // Arrange
            int eventId = 1;
            int competitorInEventId = 2;

            _mockResultService
                .Setup(s => s.GetCompetitorResultsByEventId(eventId, competitorInEventId))
                .ReturnsAsync((CompetitorScoreDto?)null); // Service geeft null terug

            // Act
            var result = await _controller.GetResultsByCompetitorEventId(eventId, competitorInEventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value); // Waarde moet null zijn
        }

        [Fact]
        public async Task GetResultsByCompetitorEventId_ThrowsException_Returns500()
        {
            // Arrange
            int eventId = 1;
            int competitorInEventId = 2;

            _mockResultService
                .Setup(s => s.GetCompetitorResultsByEventId(eventId, competitorInEventId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            Func<Task> act = async () => await _controller.GetResultsByCompetitorEventId(eventId, competitorInEventId);

            // Assert
            var exception = await Assert.ThrowsAsync<Exception>(act);
            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public async Task GetListOfCompetitorsPicksForDeelnemer_ReturnsOk_WithCalculatedPoints()
        {
            // Arrange
            int id = 1;
            int eventId = 5;

            var picks = new List<GameCompetitorEventPick>
            {
                new GameCompetitorEventPick { CompetitorsInEvent = new CompetitorsInEvent { Id = 1 } },
                new GameCompetitorEventPick { CompetitorsInEvent = new CompetitorsInEvent { Id = 2 } }
            };

            _mockDeelnemerService.Setup(s => s.GetAllPicks(id)).ReturnsAsync(picks);

            _mockMapper.Setup(m => m.Map<List<ResultDto>>(picks))
                       .Returns((List<GameCompetitorEventPick> src) =>
                           src.ConvertAll(p => new ResultDto { CompetitorInEventId = p.CompetitorsInEvent.Id }));

            _mockResultService.Setup(s => s.GetCompetitorResultsByEventId(eventId, 1))
                              .ReturnsAsync(new CompetitorScoreDto { TotalScore = 10, LaatsteScore = 5 });
            _mockResultService.Setup(s => s.GetCompetitorResultsByEventId(eventId, 2))
                              .ReturnsAsync(new CompetitorScoreDto { TotalScore = 20, LaatsteScore = 15 });

            // Act
            var result = await _controller.GetListOfCompetitorsPicksForDeelnemer(id, eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<List<ResultDto>>(okResult.Value);

            Assert.Equal(2, data.Count);
            Assert.Equal(10, data[0].Points);
            Assert.Equal(5, data[0].LatestPoints);
            Assert.Equal(20, data[1].Points);
            Assert.Equal(15, data[1].LatestPoints);
        }

        [Fact]
        public async Task GetListOfCompetitorsPicksForDeelnemer_ReturnsEmptyList_WhenNoPicks()
        {
            // Arrange
            int id = 1;
            int eventId = 5;

            _mockDeelnemerService.Setup(s => s.GetAllPicks(id)).ReturnsAsync((List<GameCompetitorEventPick>)null);

            // Act
            var result = await _controller.GetListOfCompetitorsPicksForDeelnemer(id, eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<List<ResultDto>>(okResult.Value);
            Assert.Empty(data);
        }

        [Fact]
        public async Task GetListOfCompetitorsPicksForDeelnemer_SetsPointsZero_WhenResultsNull()
        {
            // Arrange
            int id = 1;
            int eventId = 5;

            var picks = new List<GameCompetitorEventPick>
            {
                new GameCompetitorEventPick { CompetitorsInEvent = new CompetitorsInEvent { Id = 1 } }
            };

            _mockDeelnemerService.Setup(s => s.GetAllPicks(id)).ReturnsAsync(picks);

            _mockMapper.Setup(m => m.Map<List<ResultDto>>(picks))
                       .Returns(picks.ConvertAll(p => new ResultDto { CompetitorInEventId = p.CompetitorsInEvent.Id }));

            _mockResultService.Setup(s => s.GetCompetitorResultsByEventId(eventId, 1))
                              .ReturnsAsync((CompetitorScoreDto)null);

            // Act
            var result = await _controller.GetListOfCompetitorsPicksForDeelnemer(id, eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<List<ResultDto>>(okResult.Value);

            Assert.Single(data);
            Assert.Equal(0, data[0].Points);
            Assert.Equal(0, data[0].LatestPoints);
        }

        [Fact]
        public async Task GetListOfCompetitorsPicksForDeelnemer_ThrowsException_ReturnsThrows()
        {
            // Arrange
            int id = 1;
            int eventId = 5;

            _mockDeelnemerService.Setup(s => s.GetAllPicks(id))
                                 .ThrowsAsync(new Exception("Service failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _controller.GetListOfCompetitorsPicksForDeelnemer(id, eventId));
        }

        [Fact]
        public async Task GetDeelnemersMetPicks_ReturnsOk_WithMappedDeelnemersEnPicks()
        {
            // Arrange
            int eventId = 1;

            var eventObj = new Event
            {
                EventId = eventId,
                GameCompetitorEvents = new List<GameCompetitorEvent>
                {
                    new GameCompetitorEvent
                    {
                        Id = 10,
                        UserId = "user1",
                        TeamName = "Team A",
                        User = new ApplicationUser { FirstName = "Remco", LastName = "Evenepoel" }
                    }
                }
            };

            var picks = new List<GameCompetitorEventPick>
            {
                new GameCompetitorEventPick { CompetitorsInEvent = new CompetitorsInEvent { Id = 100 } }
            };

            var mappedPicks = new List<ResultDto>
            {
                new ResultDto { CompetitorInEventId = 100 }
            };

            _mockEventService.Setup(s => s.GetEventById(eventId)).ReturnsAsync(eventObj);
            _mockDeelnemerService.Setup(s => s.GetAllPicks(10)).ReturnsAsync(picks);
            _mockMapper.Setup(m => m.Map<List<ResultDto>>(picks)).Returns(mappedPicks);
            _mockResultService.Setup(s => s.GetCompetitorResultsByEventId(eventId, 100))
                              .ReturnsAsync(new CompetitorScoreDto { TotalScore = 25 });

            // Act
            var result = await _controller.GetDeelnemersMetPicks(eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<List<DeelnemerMetPicksDto>>(okResult.Value);

            Assert.Single(data);
            var deelnemer = data[0];
            Assert.Equal("Remco Evenepoel", deelnemer.DeelnemerNaam);
            Assert.Equal("Team A", deelnemer.PoolNaam);
            Assert.Single(deelnemer.Picks);
            Assert.Equal(25, deelnemer.Picks[0].Points);
        }

        [Fact]
        public async Task GetDeelnemersMetPicks_ReturnsEmptyList_WhenNoEventFound()
        {
            // Arrange
            int eventId = 99;
            _mockEventService.Setup(s => s.GetEventById(eventId)).ReturnsAsync((Event)null);

            // Act
            var result = await _controller.GetDeelnemersMetPicks(eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<List<DeelnemerMetPicksDto>>(okResult.Value);
            Assert.Empty(data);
        }

        [Fact]
        public async Task GetDeelnemersMetPicks_ReturnsOk_WhenNoPicks()
        {
            // Arrange
            int eventId = 1;

            var eventObj = new Event
            {
                EventId = eventId,
                GameCompetitorEvents = new List<GameCompetitorEvent>
                {
                    new GameCompetitorEvent
                    {
                        Id = 22,
                        UserId = "userX",
                        TeamName = "No Picks Team",
                        User = new ApplicationUser { FirstName = "Jan", LastName = "Jansen" }
                    }
                }
            };

            _mockEventService.Setup(s => s.GetEventById(eventId)).ReturnsAsync(eventObj);
            _mockDeelnemerService.Setup(s => s.GetAllPicks(22))
                                 .ReturnsAsync((List<GameCompetitorEventPick>)null);
            _mockMapper.Setup(m => m.Map<List<ResultDto>>(It.IsAny<List<GameCompetitorEventPick>>()))
                       .Returns(new List<ResultDto>());

            // Act
            var result = await _controller.GetDeelnemersMetPicks(eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<List<DeelnemerMetPicksDto>>(okResult.Value);
            Assert.Single(data);
            Assert.Empty(data[0].Picks);
        }

        [Fact]
        public async Task GetDeelnemersMetPuntenVoorEvent_ReturnsOk_WithPunten()
        {
            // Arrange
            int eventId = 10;
            var eventObj = new Event
            {
                EventId = eventId,
                GameCompetitorEvents = new List<GameCompetitorEvent>
                {
                    new GameCompetitorEvent { Id = 1, User = new ApplicationUser { FirstName = "Jan", LastName = "Jansen" } },
                    new GameCompetitorEvent { Id = 2, User = new ApplicationUser { FirstName = "Piet", LastName = "Pietersen" } }
                }
            };

            _mockEventService.Setup(s => s.GetEventById(eventId)).ReturnsAsync(eventObj);

            _mockDeelnemerService.Setup(s => s.GetAllPicks(It.IsAny<int>()))
                .ReturnsAsync((int id) => new List<GameCompetitorEventPick>
                {
                    new GameCompetitorEventPick { CompetitorsInEventId = id } // 1 pick per deelnemer
                });

            _mockResultService.Setup(s => s.GetCompetitorResultsByEventId(eventId, 1))
                .ReturnsAsync(new CompetitorScoreDto { TotalScore = 10 });
            _mockResultService.Setup(s => s.GetCompetitorResultsByEventId(eventId, 2))
                .ReturnsAsync(new CompetitorScoreDto { TotalScore = 20 });

            _mockMapper.Setup(m => m.Map<DeelnemerDto>(It.IsAny<GameCompetitorEvent>()))
                .Returns((GameCompetitorEvent e) => new DeelnemerDto { Id = e.Id, DeelnemerNaam = e.User.FirstName });

            // Act
            var result = await _controller.GetDeelnemersMetPuntenVoorEvent(eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<List<DeelnemerDto>>(okResult.Value);

            Assert.Equal(2, data.Count);
            Assert.Equal(10, data[0].Punten);
            Assert.Equal(20, data[1].Punten);
        }

        [Fact]
        public async Task GetDeelnemersMetPuntenVoorEvent_ReturnsEmptyList_WhenNoEventFound()
        {
            // Arrange
            int eventId = 99;
            _mockEventService.Setup(s => s.GetEventById(eventId)).ReturnsAsync((Event)null);

            // Act
            var result = await _controller.GetDeelnemersMetPuntenVoorEvent(eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<List<DeelnemerDto>>(okResult.Value);
            Assert.Empty(data);
        }

        [Fact]
        public async Task GetDeelnemersMetPuntenVoorEvent_SetsZeroPoints_WhenNoPicks()
        {
            // Arrange
            int eventId = 1;
            var eventObj = new Event
            {
                EventId = eventId,
                GameCompetitorEvents = new List<GameCompetitorEvent>
                {
                    new GameCompetitorEvent { Id = 1, User = new ApplicationUser { FirstName = "Test", LastName = "de Tester" } }
                }
            };

            _mockEventService.Setup(s => s.GetEventById(eventId)).ReturnsAsync(eventObj);
            _mockDeelnemerService.Setup(s => s.GetAllPicks(1)).ReturnsAsync((List<GameCompetitorEventPick>)null);
            _mockMapper.Setup(m => m.Map<DeelnemerDto>(It.IsAny<GameCompetitorEvent>()))
                       .Returns(new DeelnemerDto { Id = 1, DeelnemerNaam = "Test" });

            // Act
            var result = await _controller.GetDeelnemersMetPuntenVoorEvent(eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<List<DeelnemerDto>>(okResult.Value);
            Assert.Single(data);
            Assert.Equal(0, data[0].Punten);
        }

        [Fact]
        public async Task GetPicksForDeelnemer_ReturnsOk_WithMappedIds()
        {
            // Arrange
            int deelnemerId = 42;
            var picks = new List<int> { 1, 2, 3 };

            _mockDeelnemerService
                .Setup(s => s.GetAllPicksAsCompetitorIds(deelnemerId))
                .ReturnsAsync(picks);

            _mockMapper
                .Setup(m => m.Map<List<int>>(picks))
                .Returns(picks);

            // Act
            var result = await _controller.GetPicksForDeelnemer(deelnemerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<List<int>>(okResult.Value);
            Assert.Equal(3, data.Count);
            Assert.Equal(new List<int> { 1, 2, 3 }, data);
        }

        [Fact]
        public async Task GetPicksForDeelnemer_ReturnsEmptyList_WhenNoPicks()
        {
            // Arrange
            int deelnemerId = 10;
            var emptyList = new List<int>();

            _mockDeelnemerService
                .Setup(s => s.GetAllPicksAsCompetitorIds(deelnemerId))
                .ReturnsAsync(emptyList);

            _mockMapper
                .Setup(m => m.Map<List<int>>(emptyList))
                .Returns(emptyList);

            // Act
            var result = await _controller.GetPicksForDeelnemer(deelnemerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<List<int>>(okResult.Value);
            Assert.Empty(data);
        }

        [Fact]
        public async Task GetPicksForDeelnemer_ReturnsEmptyList_WhenServiceReturnsNull()
        {
            // Arrange
            int deelnemerId = 7;
            _mockDeelnemerService
                .Setup(s => s.GetAllPicksAsCompetitorIds(deelnemerId))
                .ReturnsAsync((List<int>)null);

            _mockMapper
                .Setup(m => m.Map<List<int>>(It.IsAny<List<int>>()))
                .Returns(new List<int>()); // mapper mag nooit null teruggeven

            // Act
            var result = await _controller.GetPicksForDeelnemer(deelnemerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<List<int>>(okResult.Value);
            Assert.Empty(data);
        }
    }
}
