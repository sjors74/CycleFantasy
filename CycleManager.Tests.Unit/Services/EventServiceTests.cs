using CycleManager.Domain.Dto;
using CycleManager.Domain.Interfaces;
using CycleManager.Domain.Models;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CycleManager.Tests.Unit.Services
{
    public class EventServiceTests
    {
        private readonly Mock<IEventRepository> _eventRepositoryMock;
        private readonly Mock<IStageRepository> _stageRepositoryMock;
        private readonly Mock<IResultsRepository> _resultRepositoryMock;
        private readonly Mock<IGameCompetitorInEventRepository> _deelnemersRepositoryMock;
        private readonly Mock<IGameCompetitorEventPickRepository> _picksRepositoryMock;
        private readonly Mock<ICompetitorInEventService> _competitorServiceMock;
        private readonly Mock<IResultService> _resultServiceMock;
        private readonly Mock<IScrapeScheduleService> _scrapeScheduleServiceMock;

        private readonly EventService _service;

        public EventServiceTests()
        {
            _eventRepositoryMock = new Mock<IEventRepository>();
            _stageRepositoryMock = new Mock<IStageRepository>();
            _resultRepositoryMock = new Mock<IResultsRepository>();
            _deelnemersRepositoryMock = new Mock<IGameCompetitorInEventRepository>();
            _picksRepositoryMock = new Mock<IGameCompetitorEventPickRepository>();
            _competitorServiceMock = new Mock<ICompetitorInEventService>();
            _resultServiceMock = new Mock<IResultService>();
            _scrapeScheduleServiceMock = new Mock<IScrapeScheduleService>();

            _service = new EventService(
                _eventRepositoryMock.Object,
                _stageRepositoryMock.Object,
                _resultRepositoryMock.Object,
                _deelnemersRepositoryMock.Object,
                _picksRepositoryMock.Object,
                _competitorServiceMock.Object,
                _resultServiceMock.Object,
                _scrapeScheduleServiceMock.Object);
        }

        [Fact]
        public async Task Create_AddsEvent_SavesAndRegistersSchedules()
        {
            // Arrange
            var entity = new Event
            {
                EventId = 1,
                EventName = "Tour de France",
                EventYear = 2026
            };

            // Act
            await _service.Create(entity);

            // Assert
            _eventRepositoryMock.Verify(
                x => x.Add(entity),
                Times.Once);

            _eventRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);

            _scrapeScheduleServiceMock.Verify(
                x => x.RegisterSchedulesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task Delete_RemovesEvent_SavesAndRegistersSchedules()
        {
            // Arrange
            var entity = new Event
            {
                EventId = 1,
                EventName = "Tour de France",
                EventYear = 2026
            };

            // Act
            await _service.Delete(entity);

            // Assert
            _eventRepositoryMock.Verify(
                x => x.Remove(entity),
                Times.Once);

            _eventRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);

            _scrapeScheduleServiceMock.Verify(
                x => x.RegisterSchedulesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetAllEvents_ReturnsEventsOrderedByYearDescendingAndStartDateAscending()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var db = new ApplicationDbContext(options);

            db.Events.AddRange(
                new Event
                {
                    EventId = 1,
                    EventName = "Event 2025",
                    EventYear = 2025,
                    StartDate = new DateTime(2025, 7, 10)
                },
                new Event
                {
                    EventId = 2,
                    EventName = "Event 2026 Later",
                    EventYear = 2026,
                    StartDate = new DateTime(2026, 7, 10)
                },
                new Event
                {
                    EventId = 3,
                    EventName = "Event 2026 Earlier",
                    EventYear = 2026,
                    StartDate = new DateTime(2026, 7, 1)
                });

            await db.SaveChangesAsync();

            _eventRepositoryMock
                .Setup(x => x.GetAllEvents())
                .Returns(db.Events);

            // Act
            var result = (await _service.GetAllEvents()).ToList();

            // Assert
            result.Should().HaveCount(3);

            result[0].EventId.Should().Be(3);
            result[1].EventId.Should().Be(2);
            result[2].EventId.Should().Be(1);
        }

        [Fact]
        public async Task GetEventById_ReturnsRepositoryResult()
        {
            // Arrange
            var entity = new Event
            {
                EventId = 1,
                EventName = "Tour de France",
                EventYear = 2026
            };

            _eventRepositoryMock
                .Setup(x => x.GetEventById(1))
                .ReturnsAsync(entity);

            // Act
            var result = await _service.GetEventById(1);

            // Assert
            result.Should().BeSameAs(entity);

            _eventRepositoryMock.Verify(
                x => x.GetEventById(1),
                Times.Once);
        }

        [Fact]
        public async Task Update_UpdatesEventAndSaves()
        {
            // Arrange
            var entity = new Event
            {
                EventId = 1,
                EventName = "Tour de France",
                EventYear = 2026
            };

            // Act
            await _service.Update(entity);

            // Assert
            _eventRepositoryMock.Verify(
                x => x.Update(entity),
                Times.Once);

            _eventRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetAllStagesForEvent_ReturnsNumberOfStages()
        {
            // Arrange
            var stages = new List<Stage>
            {
                new Stage { Id = 1, StageName = "1" },
                new Stage { Id = 2, StageName = "2" },
                new Stage { Id = 3, StageName = "3" }
            };

            _stageRepositoryMock
                .Setup(x => x.GetByEventId(10))
                .ReturnsAsync(stages);

            // Act
            var result = await _service.GetAllStagesForEvent(10);

            // Assert
            result.Should().Be(3);

            _stageRepositoryMock.Verify(
                x => x.GetByEventId(10),
                Times.Once);
        }

        [Fact]
        public async Task GetStagesWithResultsForEvent_ReturnsStagesWithResultStatus()
        {
            // Arrange
            var stages = new List<Stage>
            {
                new Stage
                {
                    Id = 1,
                    StageName = "1"
                },
                new Stage
                {
                    Id = 2,
                    StageName = "2"
                }
            };

            _stageRepositoryMock
                .Setup(x => x.GetByEventId(10))
                .ReturnsAsync(stages);

            _resultRepositoryMock
                .Setup(x => x.GetResultsByStageId(1))
                .ReturnsAsync(0);

            _resultRepositoryMock
                .Setup(x => x.GetResultsByStageId(2))
                .ReturnsAsync(25);

            // Act
            var result = (await _service.GetStagesWithResultsForEvent(10)).ToList();

            // Assert
            result.Should().HaveCount(2);

            result[0].StageNumber.Should().Be("1");
            result[0].HasResult.Should().BeFalse();

            result[1].StageNumber.Should().Be("2");
            result[1].HasResult.Should().BeTrue();

            _stageRepositoryMock.Verify(
                x => x.GetByEventId(10),
                Times.Once);

            _resultRepositoryMock.Verify(
                x => x.GetResultsByStageId(1),
                Times.Once);

            _resultRepositoryMock.Verify(
                x => x.GetResultsByStageId(2),
                Times.Once);
        }

        [Fact]
        public async Task GetEventsByUserId_ReturnsMappedEventWithParticipantAndCompetitor()
        {
            // Arrange
            var country = new Country
            {
                CountryId = 1,
                CountryNameLong = "Nederland",
                CountryNameShort = "NED"
            };

            var ratingCategory = new RatingCategory
            {
                RatingCategoryId = 1,
                Name = "GC",
                Code = "GC",
                Color = "gold"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Mathieu",
                LastName = "van der Poel",
                PcsName = "MvdP",
                CountryId = country.CountryId,
                Country = country
            };

            var competitorRating = new CompetitorRating
            {
                CompetitorRatingId = 1,
                CompetitorId = competitor.CompetitorId,
                Competitor = competitor,
                RatingCategoryId = ratingCategory.RatingCategoryId,
                RatingCategory = ratingCategory,
                Rating = 8
            };

            competitor.Ratings.Add(competitorRating);

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                Name = "Alpecin-Deceuninck"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = competitor.CompetitorId,
                Competitor = competitor,
                TeamYearId = teamYear.TeamYearId,
                TeamYear = teamYear,
                IsNationalChampion = true
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 10,
                CompetitorInTeamId = competitorInTeam.Id,
                CompetitorInTeam = competitorInTeam,
                EventId = 100,
                EventNumber = 11
            };

            var user = new ApplicationUser
            {
                Id = "user-1",
                FirstName = "Sjors",
                LastName = "Stevens"
            };

            var eventEntity = new Event
            {
                EventId = 100,
                EventName = "Tour de France",
                EventYear = 2026,
                StartDate = new DateTime(2026, 7, 4),
                EndDate = new DateTime(2026, 7, 26),
                Slogan = "Grand Départ",
                CountryCode = "FR",
                CanSubscribe = true
            };

            var gameCompetitorEvent = new GameCompetitorEvent
            {
                Id = 20,
                TeamName = "Mijn Team",
                UserId = user.Id,
                User = user,
                EventId = 100,
                Event = eventEntity
            };

            eventEntity.GameCompetitorEvents.Add(gameCompetitorEvent);

            var pick = new GameCompetitorEventPick
            {
                Id = 30,
                GameCompetitorEventId = gameCompetitorEvent.Id,
                CompetitorsInEventId = competitorInEvent.Id,
                GameCompetitorEvent = gameCompetitorEvent,
                CompetitorsInEvent = competitorInEvent
            };

            gameCompetitorEvent.Renners.Add(pick);

            var totalScore = new DeelnemerScore
            {
                GameCompetitorEventId = 20,
                TotalScore = 125
            };

            var stageScore = new DeelnemerStageScore
            {
                GameCompetitorEventId = 20,
                StageId = 5,
                Score = 42
            };

            var competitorScore = new CompetitorScoreDto
            {
                CompetitorInEventId = 10,
                NormalScore = 75,
                SpecialScore = 0,
            };

            _deelnemersRepositoryMock
                .Setup(x => x.GetEventsByUserId("user-1"))
                .ReturnsAsync(new List<Event>
                {
                    eventEntity
                });

            _resultServiceMock
                .Setup(x => x.GetTotalScoresByEventIdAsync(100))
                .ReturnsAsync(new List<DeelnemerScore> { totalScore });

            _resultServiceMock
                .Setup(x => x.GetScoresByEventIdAsync(100))
                .ReturnsAsync(new List<DeelnemerStageScore> { stageScore });

            _resultServiceMock
                .Setup(x => x.GetCompetitorResultsForEvent(100))
                .ReturnsAsync(new List<CompetitorScoreDto> { competitorScore });

            // Act
            var result = (await _service.GetEventsByUserId("user-1")).ToList();

            // Assert
            result.Should().HaveCount(1);

            var eventResult = result[0];

            eventResult.EventId.Should().Be(100);
            eventResult.EventName.Should().Be("Tour de France");
            eventResult.StartDate.Should().Be(new DateTime(2026, 7, 4));
            eventResult.EndDate.Should().Be(new DateTime(2026, 7, 26));
            eventResult.Slogan.Should().Be("Grand Départ");
            eventResult.CountryCode.Should().Be("FR");
            eventResult.ColorName.Should().Be("FR");
            eventResult.CanSubscribe.Should().BeTrue();
            eventResult.UserId.Should().Be("user-1");

            eventResult.Deelnemers.Should().HaveCount(1);

            var deelnemerResult = eventResult.Deelnemers.First();

            deelnemerResult.Id.Should().Be(20);
            deelnemerResult.PoolNaam.Should().Be("Mijn Team");
            deelnemerResult.DeelnemerNaam.Should().Be("Sjors Stevens");
            deelnemerResult.Punten.Should().Be(125);
            deelnemerResult.LaatsteScore.Should().Be(42);

            deelnemerResult.Renners.Should().HaveCount(1);

            var competitorResult = deelnemerResult.Renners.First();

            competitorResult.FirstName.Should().Be("Mathieu");
            competitorResult.LastName.Should().Be("van der Poel");
            competitorResult.CountryShort.Should().Be("NED");
            competitorResult.EventNumber.Should().Be("11");
            competitorResult.PcsName.Should().Be("MvdP");
            competitorResult.Punten.Should().Be(75);
            competitorResult.CurrentTeamName.Should().Be("Alpecin-Deceuninck");
            competitorResult.IsNationalChampion.Should().BeTrue();
            competitorResult.CompetitorInTeamId.Should().Be(1);

            competitorResult.Ratings.Should().HaveCount(1);
            competitorResult.Ratings[0].RatingCategoryId.Should().Be(1);
            competitorResult.Ratings[0].Code.Should().Be("GC");
            competitorResult.Ratings[0].CategoryName.Should().Be("GC");
            competitorResult.Ratings[0].Color.Should().Be("gold");
            competitorResult.Ratings[0].Rating.Should().Be(8);
        }

        [Fact]
        public async Task GetEventsByUserId_WhenEventHasNoParticipants_ReturnsEventWithoutParticipants()
        {
            // Arrange
            var eventEntity = new Event
            {
                EventId = 100,
                EventName = "Tour de France",
                EventYear = 2026,
                StartDate = new DateTime(2026, 7, 4),
                EndDate = new DateTime(2026, 7, 26),
                Slogan = "Grand Départ",
                CountryCode = "FR",
                CanSubscribe = true
            };

            _deelnemersRepositoryMock
                .Setup(x => x.GetEventsByUserId("user-1"))
                .ReturnsAsync(new List<Event> { eventEntity });

            _resultServiceMock
                .Setup(x => x.GetTotalScoresByEventIdAsync(100))
                .ReturnsAsync(new List<DeelnemerScore>());

            _resultServiceMock
                .Setup(x => x.GetScoresByEventIdAsync(100))
                .ReturnsAsync(new List<DeelnemerStageScore>());

            _resultServiceMock
                .Setup(x => x.GetCompetitorResultsForEvent(100))
                .ReturnsAsync(new List<CompetitorScoreDto>());

            // Act
            var result = (await _service.GetEventsByUserId("user-1")).ToList();

            // Assert
            result.Should().HaveCount(1);

            result[0].EventId.Should().Be(100);
            result[0].EventName.Should().Be("Tour de France");
            result[0].Deelnemers.Should().BeEmpty();
        }

        [Fact]
        public async Task GetEventsByUserId_WhenCompetitorHasNoScore_ReturnsZeroPoints()
        {
            // Arrange
            var country = new Country
            {
                CountryId = 1,
                CountryNameShort = "NED"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Mathieu",
                LastName = "van der Poel",
                PcsName = "MvdP",
                CountryId = 1,
                Country = country
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                Name = "Alpecin-Deceuninck"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor,
                TeamYearId = 1,
                TeamYear = teamYear
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 10,
                CompetitorInTeamId = 1,
                CompetitorInTeam = competitorInTeam,
                EventId = 100,
                EventNumber = 11
            };

            var user = new ApplicationUser
            {
                Id = "user-1",
                FirstName = "Sjors",
                LastName = "Stevens"
            };

            var eventEntity = new Event
            {
                EventId = 100,
                EventName = "Tour de France",
                EventYear = 2026,
                StartDate = new DateTime(2026, 7, 4),
                EndDate = new DateTime(2026, 7, 26),
                CountryCode = "FR",
                CanSubscribe = true
            };

            var gameCompetitorEvent = new GameCompetitorEvent
            {
                Id = 20,
                TeamName = "Mijn Team",
                UserId = user.Id,
                User = user,
                EventId = 100,
                Event = eventEntity
            };

            var pick = new GameCompetitorEventPick
            {
                Id = 30,
                GameCompetitorEventId = 20,
                CompetitorsInEventId = 10,
                GameCompetitorEvent = gameCompetitorEvent,
                CompetitorsInEvent = competitorInEvent
            };

            gameCompetitorEvent.Renners.Add(pick);
            eventEntity.GameCompetitorEvents.Add(gameCompetitorEvent);

            _deelnemersRepositoryMock
                .Setup(x => x.GetEventsByUserId("user-1"))
                .ReturnsAsync(new List<Event> { eventEntity });

            _resultServiceMock
                .Setup(x => x.GetTotalScoresByEventIdAsync(100))
                .ReturnsAsync(new List<DeelnemerScore>());

            _resultServiceMock
                .Setup(x => x.GetScoresByEventIdAsync(100))
                .ReturnsAsync(new List<DeelnemerStageScore>());

            // Geen score voor CompetitorsInEventId 10
            _resultServiceMock
                .Setup(x => x.GetCompetitorResultsForEvent(100))
                .ReturnsAsync(new List<CompetitorScoreDto>());

            // Act
            var result = (await _service.GetEventsByUserId("user-1")).Single();

            // Assert
            var deelnemer = result.Deelnemers.Single();
            var renner = deelnemer.Renners.Single();

            renner.Punten.Should().Be(0);
        }

        [Fact]
        public async Task GetEventsByUserId_WhenParticipantHasNoTotalScore_ReturnsZeroPoints()
        {
            // Arrange
            var eventEntity = new Event
            {
                EventId = 100,
                EventName = "Tour de France",
                EventYear = 2026,
                StartDate = new DateTime(2026, 7, 4),
                EndDate = new DateTime(2026, 7, 26),
                CountryCode = "FR",
                CanSubscribe = true
            };

            var user = new ApplicationUser
            {
                Id = "user-1",
                FirstName = "Sjors",
                LastName = "Stevens"
            };

            var gameCompetitorEvent = new GameCompetitorEvent
            {
                Id = 20,
                TeamName = "Mijn Team",
                UserId = user.Id,
                User = user,
                EventId = 100,
                Event = eventEntity
            };

            eventEntity.GameCompetitorEvents.Add(gameCompetitorEvent);

            _deelnemersRepositoryMock
                .Setup(x => x.GetEventsByUserId("user-1"))
                .ReturnsAsync(new List<Event> { eventEntity });

            // Geen totaalscore voor deelnemer 20
            _resultServiceMock
                .Setup(x => x.GetTotalScoresByEventIdAsync(100))
                .ReturnsAsync(new List<DeelnemerScore>());

            _resultServiceMock
                .Setup(x => x.GetScoresByEventIdAsync(100))
                .ReturnsAsync(new List<DeelnemerStageScore>());

            _resultServiceMock
                .Setup(x => x.GetCompetitorResultsForEvent(100))
                .ReturnsAsync(new List<CompetitorScoreDto>());

            // Act
            var result = (await _service.GetEventsByUserId("user-1")).Single();

            // Assert
            var deelnemer = result.Deelnemers.Single();

            deelnemer.Punten.Should().Be(0);
        }

        [Fact]
        public async Task GetEventsByUserId_WhenParticipantHasNoStageScore_ReturnsZeroLastStageScore()
        {
            // Arrange
            var eventEntity = new Event
            {
                EventId = 100,
                EventName = "Tour de France",
                EventYear = 2026,
                StartDate = new DateTime(2026, 7, 4),
                EndDate = new DateTime(2026, 7, 26),
                CountryCode = "FR",
                CanSubscribe = true
            };

            var user = new ApplicationUser
            {
                Id = "user-1",
                FirstName = "Sjors",
                LastName = "Stevens"
            };

            var gameCompetitorEvent = new GameCompetitorEvent
            {
                Id = 20,
                TeamName = "Mijn Team",
                UserId = user.Id,
                User = user,
                EventId = 100,
                Event = eventEntity
            };

            eventEntity.GameCompetitorEvents.Add(gameCompetitorEvent);

            _deelnemersRepositoryMock
                .Setup(x => x.GetEventsByUserId("user-1"))
                .ReturnsAsync(new List<Event> { eventEntity });

            _resultServiceMock
                .Setup(x => x.GetTotalScoresByEventIdAsync(100))
                .ReturnsAsync(new List<DeelnemerScore>
                {
            new DeelnemerScore
            {
                GameCompetitorEventId = 20,
                TotalScore = 100
            }
                });

            // Geen etappescore voor deelnemer 20
            _resultServiceMock
                .Setup(x => x.GetScoresByEventIdAsync(100))
                .ReturnsAsync(new List<DeelnemerStageScore>());

            _resultServiceMock
                .Setup(x => x.GetCompetitorResultsForEvent(100))
                .ReturnsAsync(new List<CompetitorScoreDto>());

            // Act
            var result = (await _service.GetEventsByUserId("user-1")).Single();

            // Assert
            var deelnemer = result.Deelnemers.Single();

            deelnemer.Punten.Should().Be(100);
            deelnemer.LaatsteScore.Should().Be(0);
        }

        [Fact]
        public async Task GetEventsByUserId_WhenMultipleStageScoresExist_UsesLatestStageScore()
        {
            // Arrange
            var eventEntity = new Event
            {
                EventId = 100,
                EventName = "Tour de France",
                EventYear = 2026,
                StartDate = new DateTime(2026, 7, 4),
                EndDate = new DateTime(2026, 7, 26),
                CountryCode = "FR",
                CanSubscribe = true
            };

            var user = new ApplicationUser
            {
                Id = "user-1",
                FirstName = "Sjors",
                LastName = "Stevens"
            };

            var gameCompetitorEvent = new GameCompetitorEvent
            {
                Id = 20,
                TeamName = "Mijn Team",
                UserId = user.Id,
                User = user,
                EventId = 100,
                Event = eventEntity
            };

            eventEntity.GameCompetitorEvents.Add(gameCompetitorEvent);

            _deelnemersRepositoryMock
                .Setup(x => x.GetEventsByUserId("user-1"))
                .ReturnsAsync(new List<Event> { eventEntity });

            _resultServiceMock
                .Setup(x => x.GetTotalScoresByEventIdAsync(100))
                .ReturnsAsync(new List<DeelnemerScore>
                {
                new DeelnemerScore
                {
                    GameCompetitorEventId = 20,
                    TotalScore = 150
                }
                });

            _resultServiceMock
                .Setup(x => x.GetScoresByEventIdAsync(100))
                .ReturnsAsync(new List<DeelnemerStageScore>
                {
                    new DeelnemerStageScore
                    {
                        GameCompetitorEventId = 20,
                        StageId = 3,
                        Score = 25
                    },
                    new DeelnemerStageScore
                    {
                        GameCompetitorEventId = 20,
                        StageId = 5,
                        Score = 40
                    },
                    new DeelnemerStageScore
                    {
                        GameCompetitorEventId = 20,
                        StageId = 4,
                        Score = 30
                    }
                });

            _resultServiceMock
                .Setup(x => x.GetCompetitorResultsForEvent(100))
                .ReturnsAsync(new List<CompetitorScoreDto>());

            // Act
            var result = (await _service.GetEventsByUserId("user-1")).Single();

            // Assert
            var deelnemer = result.Deelnemers.Single();

            deelnemer.Punten.Should().Be(150);
            deelnemer.LaatsteScore.Should().Be(40);
        }

        [Fact]
        public async Task SaveSelectie_CreatesPicksForSelectedCompetitors()
        {
            // Arrange
            var selectie = new SelectieDto
            {
                DeelnemerId = 20,
                EventId = 100,
                RennerIds = new List<int> { 10, 11 }
            };

            var deelnemer = new GameCompetitorEvent
            {
                Id = 20,
                EventId = 100
            };

            var eventEntity = new Event
            {
                EventId = 100,
                CanSubscribe = true
            };

            var competitorInEvent1 = new CompetitorsInEvent
            {
                Id = 101,
                EventId = 100,
                CompetitorInTeamId = 10
            };

            var competitorInEvent2 = new CompetitorsInEvent
            {
                Id = 102,
                EventId = 100,
                CompetitorInTeamId = 11
            };

            _deelnemersRepositoryMock
                .Setup(x => x.GetById(20))
                .ReturnsAsync(deelnemer);

            _eventRepositoryMock
                .Setup(x => x.GetEventById(100))
                .ReturnsAsync(eventEntity);

            _competitorServiceMock
                .Setup(x => x.FindOrCreate(100, 10))
                .ReturnsAsync(competitorInEvent1);

            _competitorServiceMock
                .Setup(x => x.FindOrCreate(100, 11))
                .ReturnsAsync(competitorInEvent2);

            // Act
            await _service.SaveSelectie(selectie);

            // Assert
            _competitorServiceMock.Verify(
                x => x.FindOrCreate(100, 10),
                Times.Once);

            _competitorServiceMock.Verify(
                x => x.FindOrCreate(100, 11),
                Times.Once);

            _picksRepositoryMock.Verify(
                x => x.CreateGamePicksAsync(
                    20,
                    It.Is<List<GameCompetitorEventPick>>(picks =>
                        picks.Count == 2 &&
                        picks.Any(p =>
                            p.GameCompetitorEventId == 20 &&
                            p.CompetitorsInEventId == 101) &&
                        picks.Any(p =>
                            p.GameCompetitorEventId == 20 &&
                            p.CompetitorsInEventId == 102))),
                Times.Once);
        }

        [Fact]
        public async Task SaveSelectie_WhenPoolDoesNotExist_ThrowsInvalidOperationException()
        {
            // Arrange
            var selectie = new SelectieDto
            {
                DeelnemerId = 20,
                EventId = 100,
                RennerIds = new List<int> { 10 }
            };

            _deelnemersRepositoryMock
                .Setup(x => x.GetById(20))
                .ReturnsAsync((GameCompetitorEvent?)null);

            // Act
            var act = () => _service.SaveSelectie(selectie);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Pool niet gevonden.");

            _eventRepositoryMock.Verify(
                x => x.GetEventById(It.IsAny<int>()),
                Times.Never);

            _competitorServiceMock.Verify(
                x => x.FindOrCreate(It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);

            _picksRepositoryMock.Verify(
                x => x.CreateGamePicksAsync(
                    It.IsAny<int>(),
                    It.IsAny<List<GameCompetitorEventPick>>()),
                Times.Never);
        }

        [Fact]
        public async Task SaveSelectie_WhenEventDoesNotExist_ThrowsInvalidOperationException()
        {
            // Arrange
            var selectie = new SelectieDto
            {
                DeelnemerId = 20,
                EventId = 100,
                RennerIds = new List<int> { 10 }
            };

            var deelnemer = new GameCompetitorEvent
            {
                Id = 20,
                EventId = 100
            };

            _deelnemersRepositoryMock
                .Setup(x => x.GetById(20))
                .ReturnsAsync(deelnemer);

            _eventRepositoryMock
                .Setup(x => x.GetEventById(100))
                .ReturnsAsync((Event?)null);

            // Act
            var act = () => _service.SaveSelectie(selectie);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Evenement niet gevonden.");

            _competitorServiceMock.Verify(
                x => x.FindOrCreate(It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);

            _picksRepositoryMock.Verify(
                x => x.CreateGamePicksAsync(
                    It.IsAny<int>(),
                    It.IsAny<List<GameCompetitorEventPick>>()),
                Times.Never);
        }

        [Fact]
        public async Task SaveSelectie_WhenSubscriptionIsClosed_ThrowsInvalidOperationException()
        {
            // Arrange
            var selectie = new SelectieDto
            {
                DeelnemerId = 20,
                EventId = 100,
                RennerIds = new List<int> { 10 }
            };

            var deelnemer = new GameCompetitorEvent
            {
                Id = 20,
                EventId = 100
            };

            var eventEntity = new Event
            {
                EventId = 100,
                CanSubscribe = false
            };

            _deelnemersRepositoryMock
                .Setup(x => x.GetById(20))
                .ReturnsAsync(deelnemer);

            _eventRepositoryMock
                .Setup(x => x.GetEventById(100))
                .ReturnsAsync(eventEntity);

            // Act
            var act = () => _service.SaveSelectie(selectie);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Inschrijven voor dit evenement is gesloten.");

            _competitorServiceMock.Verify(
                x => x.FindOrCreate(It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);

            _picksRepositoryMock.Verify(
                x => x.CreateGamePicksAsync(
                    It.IsAny<int>(),
                    It.IsAny<List<GameCompetitorEventPick>>()),
                Times.Never);
        }

        [Fact]
        public async Task CreatePoolAsync_CreatesPoolAndReturnsUpdatedDto()
        {
            // Arrange
            var deelnemerDto = new DeelnemerDto
            {
                PoolNaam = "  Mijn Team  ",
                UserId = "user-1",
                EventId = 100
            };

            var createdEvent = new GameCompetitorEvent
            {
                Id = 25,
                EventId = 100,
                UserId = "user-1",
                TeamName = "Mijn Team"
            };

            _deelnemersRepositoryMock
                .Setup(x => x.CreateGameCompetitorEventAsync(
                    It.Is<DeelnemerCreateDto>(dto =>
                        dto.TeamName == "Mijn Team" &&
                        dto.UserId == "user-1" &&
                        dto.EventId == 100)))
                .ReturnsAsync(createdEvent);

            // Act
            var result = await _service.CreatePoolAsync(deelnemerDto);

            // Assert
            result.Should().BeSameAs(deelnemerDto);
            result.Id.Should().Be(25);

            _deelnemersRepositoryMock.Verify(
                x => x.CreateGameCompetitorEventAsync(
                    It.Is<DeelnemerCreateDto>(dto =>
                        dto.TeamName == "Mijn Team" &&
                        dto.UserId == "user-1" &&
                        dto.EventId == 100)),
                Times.Once);
        }

        [Fact]
        public async Task CreatePoolAsync_WhenUserIdIsMissing_ThrowsInvalidOperationException()
        {
            // Arrange
            var deelnemerDto = new DeelnemerDto
            {
                PoolNaam = "Mijn Team",
                UserId = " ",
                EventId = 100
            };

            // Act
            var act = () => _service.CreatePoolAsync(deelnemerDto);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Er is geen gebruiker gekoppeld aan deze pool.");

            _deelnemersRepositoryMock.Verify(
                x => x.CreateGameCompetitorEventAsync(It.IsAny<DeelnemerCreateDto>()),
                Times.Never);
        }

        [Fact]
        public async Task CreatePoolAsync_WhenPoolCreationReturnsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var deelnemerDto = new DeelnemerDto
            {
                PoolNaam = "Mijn Team",
                UserId = "user-1",
                EventId = 100
            };

            _deelnemersRepositoryMock
                .Setup(x => x.CreateGameCompetitorEventAsync(
                    It.IsAny<DeelnemerCreateDto>()))
                .ReturnsAsync((GameCompetitorEvent?)null);

            // Act
            var act = () => _service.CreatePoolAsync(deelnemerDto);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Het aanmaken van de pool is mislukt.");
        }

        //[Fact]
        //public async Task CreatePoolAsync_WhenUniqueConstraintViolationOccurs_ThrowsInvalidOperationException()
        //{
        //    // Arrange
        //    var deelnemerDto = new DeelnemerDto
        //    {
        //        PoolNaam = "Mijn Team",
        //        UserId = "user-1",
        //        EventId = 100
        //    };

        //    var sqlErrorCollection = (SqlErrorCollection)Activator.CreateInstance(
        //        typeof(SqlErrorCollection),
        //        System.Reflection.BindingFlags.Instance |
        //        System.Reflection.BindingFlags.NonPublic,
        //        null,
        //        null,
        //        null)!;

        //    var sqlError = (SqlError)Activator.CreateInstance(
        //        typeof(SqlError),
        //        System.Reflection.BindingFlags.Instance |
        //        System.Reflection.BindingFlags.NonPublic,
        //        null,
        //        new object[]
        //        {
        //            2601,
        //            (byte)14,
        //            (byte)1,
        //            "server",
        //            "Violation of UNIQUE KEY constraint.",
        //            "procedure",
        //            1
        //        },
        //        null)!;

        //    typeof(SqlErrorCollection)
        //        .GetMethod(
        //            "Add",
        //            System.Reflection.BindingFlags.Instance |
        //            System.Reflection.BindingFlags.NonPublic)!
        //        .Invoke(sqlErrorCollection, new object[] { sqlError });

        //    var sqlException = (SqlException)typeof(SqlException)
        //        .GetMethod(
        //            "CreateException",
        //            System.Reflection.BindingFlags.Static |
        //            System.Reflection.BindingFlags.NonPublic,
        //            null,
        //            new[] { typeof(SqlErrorCollection), typeof(string) },
        //            null)!
        //        .Invoke(null, new object[] { sqlErrorCollection, "16.0" })!;

        //    var dbUpdateException = new DbUpdateException(
        //        "Unique constraint violation",
        //        sqlException);

        //    _deelnemersRepositoryMock
        //        .Setup(x => x.CreateGameCompetitorEventAsync(
        //            It.IsAny<DeelnemerCreateDto>()))
        //        .ThrowsAsync(dbUpdateException);

        //    // Act
        //    var act = () => _service.CreatePoolAsync(deelnemerDto);

        //    // Assert
        //    await act.Should()
        //        .ThrowAsync<InvalidOperationException>()
        //        .WithMessage("Je hebt al een pool met deze naam.");
        //}
    }
}