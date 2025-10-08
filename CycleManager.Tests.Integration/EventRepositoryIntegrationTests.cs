using CycleManager.Domain.Models;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Tests.Integration
{
    public class EventRepositoryIntegrationTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public EventRepositoryIntegrationTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private ApplicationDbContext CreateContext()
        {
            return new ApplicationDbContext(_options);
        }

        [Fact]
        public async Task GetAantalDeelnemers_ReturnsCorrectCount_WhenEventHasDeelnemers()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            context.GameCompetitorsEvent.AddRange(
                new GameCompetitorEvent { Id = 1, EventId = 10 },
                new GameCompetitorEvent { Id = 2, EventId = 10 },
                new GameCompetitorEvent { Id = 3, EventId = 20 } // ander event
            );
            await context.SaveChangesAsync();

            var repo = new EventRepository(context);

            // Act
            var count = await repo.GetAantalDeelnemers(10);

            // Assert
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task GetAantalDeelnemers_ReturnsZero_WhenEventHasNoDeelnemers()
        {
            using var context = CreateContext();
            var repo = new EventRepository(context);

            var count = await repo.GetAantalDeelnemers(99);

            Assert.Equal(0, count);
        }

        [Fact]
        public async Task GetActiveEvents_ReturnsOnlyActiveEvents()
        {
            // Arrange
            using var context = CreateContext();
            context.Events.AddRange(
                new Event { EventId = 1, EventName = "Tour", IsActive = true },
                new Event { EventId = 2, EventName = "Giro", IsActive = false }
            );
            await context.SaveChangesAsync();

            var repo = new EventRepository(context);

            // Act
            var result = await repo.GetActiveEvents();

            // Assert
            Assert.Single(result);
            Assert.Equal("Tour", result.First().EventName);
        }

        [Fact]
        public async Task GetTeamsForEvent_ReturnsTeamsWithCompetitors()
        {
            // Arrange
            using var context = CreateContext();

            var country = new Country { CountryId = 1, CountryNameShort = "BEL" };
            var competitor = new Competitor { CompetitorId = 1, FirstName = "Remco", LastName = "Evenepoel", Country = country };
            var team = new Team { TeamId = 1, CurrentTeamName = "Soudal Quick-Step" };
            var competitorInTeam = new CompetitorInTeam { Id = 1, Team = team, Competitor = competitor };
            var eventEntity = new Event { EventId = 1, EventName = "Tour de France" };
            var eventTeam = new EventTeam { TeamId = 1, Event = eventEntity, EventId = 1, Team = team };

            var cie = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 1,
                CompetitorInTeam = competitorInTeam,
                CompetitorInTeamId = 1,
                EventNumber = 11,
                InSelectie = true
            };

            context.Country.Add(country);
            context.Competitors.Add(competitor);
            context.Teams.Add(team);
            context.CompetitorInTeams.Add(competitorInTeam);
            context.Events.Add(eventEntity);
            context.EventTeam.Add(eventTeam);
            context.CompetitorsInEvent.Add(cie);

            await context.SaveChangesAsync();

            var repo = new EventRepository(context);

            // Act
            var result = await repo.GetTeamsForEvent(1);

            // Assert
            var teamDto = Assert.Single(result);
            Assert.Equal("Soudal Quick-Step", teamDto.Naam);
            Assert.Single(teamDto.Renners);
            Assert.Equal("Remco", teamDto.Renners.First().FirstName);
            Assert.Equal("BEL", teamDto.Renners.First().CountryShort);
        }

        [Fact]
        public async Task GetEventById_ReturnsEventWithIncludes()
        {
            // Arrange
            using var context = CreateContext();

            var team = new Team { TeamId = 1, CurrentTeamName = "Visma–Lease a Bike" };
            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Tour de France",
                EventTeams = new List<EventTeam>
                {
                    new EventTeam { EventId = 1, Team = team, TeamId = 1 }
                },
                Stages = new List<Stage>
                {
                    new Stage { Id = 1, StageName = "Etappe 1", StageOrder = 1 }
                }
            };

            context.Teams.Add(team);
            context.Events.Add(eventEntity);
            await context.SaveChangesAsync();

            var repo = new EventRepository(context);

            // Act
            var result = await repo.GetEventById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Tour de France", result.EventName);
            Assert.Single(result.EventTeams);
            Assert.Single(result.Stages);
            Assert.Equal("Etappe 1", result.Stages.First().StageName);
        }

        [Fact]
        public async Task GetEventById_ReturnsNull_WhenNotFound()
        {
            using var context = CreateContext();
            var repo = new EventRepository(context);

            var result = await repo.GetEventById(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetEventDetailsViewModelById_ReturnsMappedViewModel()
        {
            // Arrange
            using var context = CreateContext();

            var eventEntity = new Event
            {
                EventId = 1,
                EventCode = "TDF2025",
                EventName = "Tour de France 2025",
                Slogan = "De mooiste koers ter wereld",
                StartDate = new DateTime(2025, 7, 1),
                EndDate = new DateTime(2025, 7, 21),
                Stages = new List<Stage>
                {
                    new Stage
                    {
                        Id = 1,
                        StageOrder = 1,
                        StageName = "Etappe 1",
                        StartLocation = "Nice",
                        FinishLocation = "Nice",
                        NoScore = false,
                        Results = new List<Result>
                        {
                            new Result { Id = 1, CompetitorInEventId = 10 },
                            new Result { Id = 2, CompetitorInEventId = 20 },
                        }
                    },
                    new Stage
                    {
                        Id = 2,
                        StageOrder = 2,
                        StageName = "Etappe 2",
                        StartLocation = "Nice",
                        FinishLocation = "Gap",
                        NoScore = true,
                        Results = new List<Result>()
                    }
                }
            };

            context.Events.Add(eventEntity);
            await context.SaveChangesAsync();

            var repo = new EventRepository(context);

            // Act
            var result = await repo.GetEventDetailsViewModelById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.EventId);
            Assert.Equal("TDF2025", result.EventCode);
            Assert.Equal("Tour de France 2025", result.EventName);
            Assert.Equal(2, result.Stages.Count);
            Assert.Equal("Etappe 1", result.Stages.First().StageName);
            Assert.Equal(2, result.Stages.First().AantalPosities);
        }

        [Fact]
        public async Task GetEventDetailsViewModelById_ReturnsNull_WhenEventDoesNotExist()
        {
            using var context = CreateContext();
            var repo = new EventRepository(context);

            var result = await repo.GetEventDetailsViewModelById(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllEvents_ReturnsAllEvents_WithIncludes()
        {
            // Arrange
            using var context = CreateContext();

            var config = new Configuration { Id = 1,  ConfigurationType = "Default config" };
            var event1 = new Event
            {
                EventId = 1,
                EventName = "Paris-Roubaix",
                Configuration = config,
                Stages = new List<Stage>
                {
                    new Stage { Id = 1, StageName = "Sectorenrit", StageOrder = 1 }
                }
            };
            var event2 = new Event
            {
                EventId = 2,
                EventName = "Ronde van Vlaanderen",
                Configuration = config,
                Stages = new List<Stage>
                {
                    new Stage { Id = 2, StageName = "Berg en Kassei", StageOrder = 1 }
                }
            };

            context.Configurations.Add(config);
            context.Events.AddRange(event1, event2);
            await context.SaveChangesAsync();

            var repo = new EventRepository(context);

            // Act
            var result = repo.GetAllEvents().ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, e =>
            {
                Assert.NotNull(e.Configuration);
                Assert.NotEmpty(e.Stages);
            });
            Assert.Contains(result, e => e.EventName == "Paris-Roubaix");
            Assert.Contains(result, e => e.EventName == "Ronde van Vlaanderen");
        }

        [Fact]
        public async Task GetAllEvents_ReturnsEmptyList_WhenNoEventsExist()
        {
            using var context = CreateContext();
            var repo = new EventRepository(context);

            // Act
            var result = repo.GetAllEvents().ToList();

            // Assert
            Assert.Empty(result);
        }
    }
}
