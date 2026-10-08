using CycleManager.Domain.Dto;
using CycleManager.Domain.Enums;
using CycleManager.Domain.Models;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using CycleManager.Services.Settings;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CycleManager.Tests.Unit.Services
{
    public class ScraperServiceTests
    {
        private readonly ApplicationDbContext _db;
        private readonly Mock<IPcsScraper> _pcsScraperMock;
        private readonly Mock<ICyclingFlashScraper> _cyclingFlashScraperMock;
        private readonly Mock<IDelayService> _delayServiceMock;
        private readonly Mock<ILogger<ScraperService>> _loggerMock;
        private readonly ScraperService _service;

        public ScraperServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);

            _pcsScraperMock = new Mock<IPcsScraper>();
            _cyclingFlashScraperMock = new Mock<ICyclingFlashScraper>();
            _delayServiceMock = new Mock<IDelayService>();

            _delayServiceMock
                .Setup(x => x.DelayAsync(
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _loggerMock = new Mock<ILogger<ScraperService>>();

            _service = new ScraperService(
                _db,
                Options.Create(new ScraperSettings()),
                _pcsScraperMock.Object,
                _cyclingFlashScraperMock.Object,
                _delayServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task RunCompetitorsAsync_WhenTeamYearDoesNotExist_ReturnsWithoutScraping()
        {
            // Act
            var act = () => _service.RunCompetitorsAsync(999);

            // Assert
            await act.Should().NotThrowAsync();

            _pcsScraperMock.Verify(
                x => x.ScrapeCompetitorsAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task RunCompetitorsAsync_WhenTeamYearExists_ScrapesAndStoresCompetitors()
        {
            // Arrange
            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear
            };

            var scrapedCompetitor = new ScrapedCompetitor
            {
                RiderName = "Test Rider",
                TeamId = 1,
                Year = 2026
            };

            _db.Teams.Add(team);
            _db.SeasonYears.Add(seasonYear);
            _db.TeamYear.Add(teamYear);
            await _db.SaveChangesAsync();

            _pcsScraperMock
                .Setup(x => x.ScrapeCompetitorsAsync(
                    "https://www.procyclingstats.com/team/test-team-2026/overview/start",
                    1,
                    2026))
                .ReturnsAsync(new List<ScrapedCompetitor>
                {
            scrapedCompetitor
                });

            // Act
            await _service.RunCompetitorsAsync(1);

            // Assert
            var stored = await _db.ScrapedCompetitors.ToListAsync();

            stored.Should().ContainSingle();
            stored[0].RiderName.Should().Be("Test Rider");

            _pcsScraperMock.Verify(
                x => x.ScrapeCompetitorsAsync(
                    "https://www.procyclingstats.com/team/test-team-2026/overview/start",
                    1,
                    2026),
                Times.Once);
        }

        [Fact]
        public async Task RunCompetitorsAsync_WhenScraperReturnsNoCompetitors_DoesNotStoreCompetitors()
        {
            // Arrange
            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear
            };

            _db.Teams.Add(team);
            _db.SeasonYears.Add(seasonYear);
            _db.TeamYear.Add(teamYear);
            await _db.SaveChangesAsync();

            _pcsScraperMock
                .Setup(x => x.ScrapeCompetitorsAsync(
                    "https://www.procyclingstats.com/team/test-team-2026/overview/start",
                    1,
                    2026))
                .ReturnsAsync(new List<ScrapedCompetitor>());

            // Act
            await _service.RunCompetitorsAsync(1);

            // Assert
            var stored = await _db.ScrapedCompetitors.ToListAsync();

            stored.Should().BeEmpty();

            _pcsScraperMock.Verify(
                x => x.ScrapeCompetitorsAsync(
                    "https://www.procyclingstats.com/team/test-team-2026/overview/start",
                    1,
                    2026),
                Times.Once);
        }

        [Fact]
        public async Task RunDropoutsAsync_WhenCompetitorIsInDropoutList_MarksCompetitorAsOutOfCompetition()
        {
            // Arrange
            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Test",
                LastName = "Rider"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 1,
                Event = eventEntity,
                CompetitorInTeamId = 1,
                CompetitorInTeam = competitorInTeam,
                EventNumber = 42,
                OutOfCompetition = false
            };

            _db.Events.Add(eventEntity);
            _db.Competitors.Add(competitor);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.CompetitorsInEvent.Add(competitorInEvent);
            await _db.SaveChangesAsync();

            // Controleer dat de testdata daadwerkelijk door de query gevonden wordt.
            var before = await _db.CompetitorsInEvent
                .Where(x => x.EventId == 1)
                .ToListAsync();

            before.Should().ContainSingle();
            before[0].EventNumber.Should().Be(42);
            before[0].OutOfCompetition.Should().BeFalse();

            _pcsScraperMock
                .Setup(x => x.ScrapeDropoutBibsAsync(
                    "https://www.procyclingstats.com/race/test-event/2026/startlist"))
                .ReturnsAsync(new List<int> { 42 });

            // Act
            await _service.RunDropoutsAsync(1, "test-event", 2026);

            // Assert
            var stored = await _db.CompetitorsInEvent
                .SingleAsync(x => x.Id == 1);

            stored.OutOfCompetition.Should().BeTrue();

            _pcsScraperMock.Verify(
                x => x.ScrapeDropoutBibsAsync(
                    "https://www.procyclingstats.com/race/test-event/2026/startlist"),
                Times.Once);
        }

        [Fact]
        public async Task RunDropoutsAsync_WhenCompetitorIsAlreadyOutOfCompetition_DoesNotChangeIt()
        {
            // Arrange
            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Test",
                LastName = "Rider"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 1,
                Event = eventEntity,
                CompetitorInTeamId = 1,
                CompetitorInTeam = competitorInTeam,
                EventNumber = 42,
                OutOfCompetition = true
            };

            _db.Events.Add(eventEntity);
            _db.Competitors.Add(competitor);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.CompetitorsInEvent.Add(competitorInEvent);

            await _db.SaveChangesAsync();

            _pcsScraperMock
                .Setup(x => x.ScrapeDropoutBibsAsync(
                    "https://www.procyclingstats.com/race/test-event/2026/startlist"))
                .ReturnsAsync(new List<int> { 42 });

            // Act
            await _service.RunDropoutsAsync(1, "test-event", 2026);

            // Assert
            var stored = await _db.CompetitorsInEvent
                .SingleAsync(x => x.Id == 1);

            stored.OutOfCompetition.Should().BeTrue();

            _pcsScraperMock.Verify(
                x => x.ScrapeDropoutBibsAsync(
                    "https://www.procyclingstats.com/race/test-event/2026/startlist"),
                Times.Once);
        }

        [Fact]
        public async Task ImportScrapedCompetitorsAsync_WhenThereAreNoScrapedCompetitors_CompletesWithoutImporting()
        {
            // Arrange
            // Geen ScrapedCompetitors in de database.

            // Act
            var act = () => _service.ImportScrapedCompetitorsAsync();

            // Assert
            await act.Should().NotThrowAsync();

            (await _db.Competitors.ToListAsync())
                .Should().BeEmpty();

            (await _db.CompetitorInTeams.ToListAsync())
                .Should().BeEmpty();

            (await _db.Countries.ToListAsync())
                .Should().BeEmpty();
        }

        [Fact]
        public async Task ImportScrapedCompetitorsAsync_WhenCompetitorIsNew_CreatesCompetitorCountryAndTeamLink()
        {
            // Arrange
            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear
            };

            var scraped = new ScrapedCompetitor
            {
                RiderName = "Jansen Jan",
                CountryShortName = "NED",
                TeamId = 1,
                Year = 2026
            };

            _db.Teams.Add(team);
            _db.SeasonYears.Add(seasonYear);
            _db.TeamYear.Add(teamYear);
            _db.ScrapedCompetitors.Add(scraped);

            await _db.SaveChangesAsync();

            // Act
            await _service.ImportScrapedCompetitorsAsync();

            // Assert
            var competitor = await _db.Competitors
                .Include(x => x.Country)
                .SingleAsync();

            competitor.FirstName.Should().Be("Jan");
            competitor.LastName.Should().Be("Jansen");
            competitor.PcsScraperName.Should().Be("Jansen Jan");
            competitor.Country.Should().NotBeNull();
            competitor.Country!.CountryNameShort.Should().Be("NED");

            var competitorInTeam = await _db.CompetitorInTeams
                .SingleAsync();

            competitorInTeam.CompetitorId.Should().Be(competitor.CompetitorId);
            competitorInTeam.TeamYearId.Should().Be(teamYear.TeamYearId);

            scraped.ProcessedAt.Should().NotBeNull();

            (await _db.Countries.CountAsync())
                .Should().Be(1);
        }

        [Fact]
        public async Task ImportScrapedCompetitorsAsync_WhenCountryAlreadyExists_DoesNotCreateDuplicateCountry()
        {
            // Arrange
            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear
            };

            var country = new Country
            {
                CountryId = 1,
                CountryNameShort = "NED",
                CountryNameLong = "Nederland"
            };

            var scraped = new ScrapedCompetitor
            {
                RiderName = "Jansen Jan",
                CountryShortName = "NED",
                TeamId = 1,
                Year = 2026
            };

            _db.Teams.Add(team);
            _db.SeasonYears.Add(seasonYear);
            _db.TeamYear.Add(teamYear);
            _db.Countries.Add(country);
            _db.ScrapedCompetitors.Add(scraped);

            await _db.SaveChangesAsync();

            // Act
            await _service.ImportScrapedCompetitorsAsync();

            // Assert
            var countries = await _db.Countries.ToListAsync();
            countries.Should().ContainSingle();

            var competitor = await _db.Competitors
                .Include(x => x.Country)
                .SingleAsync();

            competitor.CountryId.Should().Be(country.CountryId);
            competitor.Country.Should().BeSameAs(country);

            scraped.ProcessedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task ImportScrapedCompetitorsAsync_WhenCompetitorExistsByPcsName_UpdatesExistingCompetitor()
        {
            // Arrange
            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear
            };

            var country = new Country
            {
                CountryId = 1,
                CountryNameShort = "NED",
                CountryNameLong = "Nederland"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                PcsScraperName = "Jansen Jan",
                CountryId = 1,
                Country = country
            };

            var scraped = new ScrapedCompetitor
            {
                RiderName = "Jansen Jan",
                CountryShortName = "NED",
                TeamId = 1,
                Year = 2026
            };

            _db.Teams.Add(team);
            _db.SeasonYears.Add(seasonYear);
            _db.TeamYear.Add(teamYear);
            _db.Countries.Add(country);
            _db.Competitors.Add(competitor);
            _db.ScrapedCompetitors.Add(scraped);

            await _db.SaveChangesAsync();

            // Act
            await _service.ImportScrapedCompetitorsAsync();

            // Assert
            var competitors = await _db.Competitors
                .Include(x => x.Country)
                .ToListAsync();

            competitors.Should().ContainSingle();

            var stored = competitors[0];

            stored.CompetitorId.Should().Be(1);
            stored.FirstName.Should().Be("Jan");
            stored.LastName.Should().Be("Jansen");
            stored.PcsScraperName.Should().Be("Jansen Jan");
            stored.Country.Should().BeSameAs(country);

            scraped.ProcessedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task ImportScrapedCompetitorsAsync_WhenCompetitorExistsByName_UpdatesPcsScraperName()
        {
            // Arrange
            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear
            };

            var country = new Country
            {
                CountryId = 1,
                CountryNameShort = "NED",
                CountryNameLong = "Nederland"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                PcsName = "jan-jansen",
                PcsScraperName = null,
                CountryId = 1,
                Country = country
            };

            var scraped = new ScrapedCompetitor
            {
                RiderName = "Jansen Jan",
                CountryShortName = "NED",
                TeamId = 1,
                Year = 2026
            };

            _db.Teams.Add(team);
            _db.SeasonYears.Add(seasonYear);
            _db.TeamYear.Add(teamYear);
            _db.Countries.Add(country);
            _db.Competitors.Add(competitor);
            _db.ScrapedCompetitors.Add(scraped);

            await _db.SaveChangesAsync();

            // Act
            await _service.ImportScrapedCompetitorsAsync();

            // Assert
            var competitors = await _db.Competitors
                .Include(x => x.Country)
                .ToListAsync();

            competitors.Should().ContainSingle();

            var stored = competitors[0];

            stored.CompetitorId.Should().Be(1);
            stored.FirstName.Should().Be("Jan");
            stored.LastName.Should().Be("Jansen");
            stored.PcsScraperName.Should().Be("Jansen Jan");
            stored.Country.Should().BeSameAs(country);

            scraped.ProcessedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task ImportScrapedCompetitorsAsync_WhenCompetitorInTeamAlreadyExists_DoesNotCreateDuplicate()
        {
            // Arrange
            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear
            };

            var country = new Country
            {
                CountryId = 1,
                CountryNameShort = "NED",
                CountryNameLong = "Nederland"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                PcsScraperName = "Jansen Jan",
                CountryId = 1,
                Country = country
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor,
                TeamYearId = 1,
                TeamYear = teamYear
            };

            var scraped = new ScrapedCompetitor
            {
                RiderName = "Jansen Jan",
                CountryShortName = "NED",
                TeamId = 1,
                Year = 2026
            };

            _db.Teams.Add(team);
            _db.SeasonYears.Add(seasonYear);
            _db.TeamYear.Add(teamYear);
            _db.Countries.Add(country);
            _db.Competitors.Add(competitor);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.ScrapedCompetitors.Add(scraped);

            await _db.SaveChangesAsync();

            // Act
            await _service.ImportScrapedCompetitorsAsync();

            // Assert
            var competitorInTeams = await _db.CompetitorInTeams.ToListAsync();

            competitorInTeams.Should().ContainSingle();

            competitorInTeams[0].CompetitorId.Should().Be(1);
            competitorInTeams[0].TeamYearId.Should().Be(1);

            scraped.ProcessedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task ImportScrapedCompetitorsAsync_WhenTeamYearDoesNotExist_ThrowsInvalidOperationException()
        {
            // Arrange
            var scraped = new ScrapedCompetitor
            {
                RiderName = "Jansen Jan",
                CountryShortName = "NED",
                TeamId = 999,
                Year = 2026
            };

            _db.ScrapedCompetitors.Add(scraped);
            await _db.SaveChangesAsync();

            // Act
            var act = () => _service.ImportScrapedCompetitorsAsync();

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Geen TeamYear gevonden voor TeamId=999, Year=2026.");
        }

        [Fact]
        public async Task ImportScrapedCompetitorsAsync_WhenNewCompetitorHasNoCountry_ThrowsInvalidOperationException()
        {
            // Arrange
            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear
            };

            var scraped = new ScrapedCompetitor
            {
                RiderName = "Jansen Jan",
                CountryShortName = null,
                TeamId = 1,
                Year = 2026
            };

            _db.Teams.Add(team);
            _db.SeasonYears.Add(seasonYear);
            _db.TeamYear.Add(teamYear);
            _db.ScrapedCompetitors.Add(scraped);

            await _db.SaveChangesAsync();

            // Act
            var act = () => _service.ImportScrapedCompetitorsAsync();

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Geen land gevonden voor competitor Jan Jansen.");
        }

        [Fact]
        public async Task ImportScrapedCompetitorsAsync_WhenExistingCompetitorHasDifferentCountry_UpdatesCountry()
        {
            // Arrange
            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear
            };

            var oldCountry = new Country
            {
                CountryId = 1,
                CountryNameShort = "BEL",
                CountryNameLong = "België"
            };

            var newCountry = new Country
            {
                CountryId = 2,
                CountryNameShort = "NED",
                CountryNameLong = "Nederland"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                PcsScraperName = null,
                CountryId = 1,
                Country = oldCountry
            };

            var scraped = new ScrapedCompetitor
            {
                RiderName = "Jansen Jan",
                CountryShortName = "NED",
                TeamId = 1,
                Year = 2026
            };

            _db.Teams.Add(team);
            _db.SeasonYears.Add(seasonYear);
            _db.TeamYear.Add(teamYear);
            _db.Countries.AddRange(oldCountry, newCountry);
            _db.Competitors.Add(competitor);
            _db.ScrapedCompetitors.Add(scraped);

            await _db.SaveChangesAsync();

            // Act
            await _service.ImportScrapedCompetitorsAsync();

            // Assert
            var stored = await _db.Competitors
                .Include(x => x.Country)
                .SingleAsync();

            stored.CompetitorId.Should().Be(1);
            stored.CountryId.Should().Be(2);
            stored.Country.CountryNameShort.Should().Be("NED");
            stored.PcsScraperName.Should().Be("Jansen Jan");

            scraped.ProcessedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task ImportScrapedCompetitorsAsync_WhenSameNewCompetitorAppearsTwice_CreatesOnlyOneCompetitor()
        {
            // Arrange
            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear
            };

            var country = new Country
            {
                CountryId = 1,
                CountryNameShort = "NED",
                CountryNameLong = "Nederland"
            };

            var scraped1 = new ScrapedCompetitor
            {
                RiderName = "Jansen Jan",
                CountryShortName = "NED",
                TeamId = 1,
                Year = 2026
            };

            var scraped2 = new ScrapedCompetitor
            {
                RiderName = "Jansen Jan",
                CountryShortName = "NED",
                TeamId = 1,
                Year = 2026
            };

            _db.Teams.Add(team);
            _db.SeasonYears.Add(seasonYear);
            _db.TeamYear.Add(teamYear);
            _db.Countries.Add(country);
            _db.ScrapedCompetitors.AddRange(scraped1, scraped2);

            await _db.SaveChangesAsync();

            // Act
            await _service.ImportScrapedCompetitorsAsync();

            // Assert
            var competitors = await _db.Competitors.ToListAsync();
            competitors.Should().ContainSingle();

            var competitorInTeams = await _db.CompetitorInTeams.ToListAsync();
            competitorInTeams.Should().ContainSingle();

            scraped1.ProcessedAt.Should().NotBeNull();
            scraped2.ProcessedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task RunRatingsScrapeAsync_WhenNoActiveRatingCategory_ReturnsWithoutScraping()
        {
            // Arrange
            _db.SeasonYears.Add(new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026,
                Active = true
            });

            await _db.SaveChangesAsync();

            // Geen actieve RatingCategory.

            // Act
            await _service.RunRatingsScrapeAsync();

            // Assert
            _cyclingFlashScraperMock.Verify(
                x => x.ScrapePageResultAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<DateTime>()),
                Times.Never);

            (await _db.RatingScrapeProgress.ToListAsync())
                .Should().BeEmpty();
        }

        [Fact]
        public async Task RunRatingsScrapeAsync_WhenNoProgressExists_ScrapesFirstPageAndCreatesProgress()
        {
            // Arrange
            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026,
                Active = true
            };

            var category = new RatingCategory
            {
                RatingCategoryId = 1,
                Code = "GC",
                Name = "General Classification",
                IsActive = true,
                MaxPages = 3
            };

            _db.SeasonYears.Add(seasonYear);
            _db.RatingCategories.Add(category);
            await _db.SaveChangesAsync();

            _cyclingFlashScraperMock
                .Setup(x => x.ScrapePageResultAsync(
                    "GC",
                    1,
                    It.IsAny<DateTime>()))
                .ReturnsAsync(new List<ScrapeCompetitorRating>());

            // Act
            await _service.RunRatingsScrapeAsync();

            // Assert
            _cyclingFlashScraperMock.Verify(
                x => x.ScrapePageResultAsync(
                    "GC",
                    1,
                    It.IsAny<DateTime>()),
                Times.Once);

            var progress = await _db.RatingScrapeProgress
                .SingleAsync();

            progress.RatingCategoryId.Should().Be(1);
            progress.LastPage.Should().Be(1);
            progress.LastScrapeDate.Should().NotBe(default);
        }

        [Fact]
        public async Task RunRatingsScrapeAsync_WhenProgressExists_ScrapesNextPage()
        {
            // Arrange
            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026,
                Active = true
            };

            var category = new RatingCategory
            {
                RatingCategoryId = 1,
                Code = "GC",
                Name = "General Classification",
                IsActive = true,
                MaxPages = 5
            };

            var progress = new RatingScrapeProgress
            {
                RatingCategoryId = 1,
                RatingCategory = category,
                LastPage = 2,
                LastScrapeDate = DateTime.UtcNow.AddDays(-1)
            };

            _db.SeasonYears.Add(seasonYear);
            _db.RatingCategories.Add(category);
            _db.RatingScrapeProgress.Add(progress);
            await _db.SaveChangesAsync();

            _cyclingFlashScraperMock
                .Setup(x => x.ScrapePageResultAsync(
                    "GC",
                    3,
                    It.IsAny<DateTime>()))
                .ReturnsAsync(new List<ScrapeCompetitorRating>());

            // Act
            await _service.RunRatingsScrapeAsync();

            // Assert
            _cyclingFlashScraperMock.Verify(
                x => x.ScrapePageResultAsync(
                    "GC",
                    3,
                    It.IsAny<DateTime>()),
                Times.Once);

            var storedProgress = await _db.RatingScrapeProgress
                .SingleAsync();

            storedProgress.LastPage.Should().Be(3);
        }

        [Fact]
        public async Task RunRatingsScrapeAsync_WhenLastPageExceedsMaxPages_StartsAgainAtPageOne()
        {
            // Arrange
            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026,
                Active = true
            };

            var category = new RatingCategory
            {
                RatingCategoryId = 1,
                Code = "GC",
                Name = "General Classification",
                IsActive = true,
                MaxPages = 3
            };

            var progress = new RatingScrapeProgress
            {
                RatingCategoryId = 1,
                RatingCategory = category,
                LastPage = 3,
                LastScrapeDate = DateTime.UtcNow.AddDays(-1)
            };

            _db.SeasonYears.Add(seasonYear);
            _db.RatingCategories.Add(category);
            _db.RatingScrapeProgress.Add(progress);
            await _db.SaveChangesAsync();

            _cyclingFlashScraperMock
                .Setup(x => x.ScrapePageResultAsync(
                    "GC",
                    1,
                    It.IsAny<DateTime>()))
                .ReturnsAsync(new List<ScrapeCompetitorRating>());

            // Act
            await _service.RunRatingsScrapeAsync();

            // Assert
            _cyclingFlashScraperMock.Verify(
                x => x.ScrapePageResultAsync(
                    "GC",
                    1,
                    It.IsAny<DateTime>()),
                Times.Once);

            var storedProgress = await _db.RatingScrapeProgress
                .SingleAsync();

            storedProgress.LastPage.Should().Be(1);
        }

        [Fact]
        public async Task RunRatingCompetitorScrapeAsync_WhenCompetitorHasNoCyclingFlashProfile_Throws()
        {
            // Arrange
            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                CountryId = 1
            };

            _db.Competitors.Add(competitor);

            _db.Countries.Add(new Country
            {
                CountryId = 1,
                CountryNameShort = "NED",
                CountryNameLong = "Nederland"
            });

            _db.SeasonYears.Add(new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026,
                Active = true
            });

            await _db.SaveChangesAsync();

            // Act
            var act = () => _service.RunRatingCompetitorScrapeAsync(1);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Competitor heeft geen CyclingFlash profiel.");

            _cyclingFlashScraperMock.Verify(
                x => x.ScrapeCompetitorRatingsAsync(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>()),
                Times.Never);
        }

        [Fact]
        public async Task RunRatingCompetitorScrapeAsync_WhenProfileExists_ScrapesRatingsAndUpdatesLastScraped()
        {
            // Arrange
            var country = new Country
            {
                CountryId = 1,
                CountryNameShort = "NED",
                CountryNameLong = "Nederland"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                CountryId = 1,
                Country = country,
                CyclingFlashScraperName = "jan-jansen"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026,
                Active = true
            };

            _db.Countries.Add(country);
            _db.Competitors.Add(competitor);
            _db.SeasonYears.Add(seasonYear);

            await _db.SaveChangesAsync();

            _cyclingFlashScraperMock
                .Setup(x => x.ScrapeCompetitorRatingsAsync(
                    "jan-jansen",
                    It.IsAny<DateTime>()))
                .ReturnsAsync(new List<ScrapeCompetitorRating>());

            // Act
            await _service.RunRatingCompetitorScrapeAsync(1);

            // Assert
            _cyclingFlashScraperMock.Verify(
                x => x.ScrapeCompetitorRatingsAsync(
                    "jan-jansen",
                    It.IsAny<DateTime>()),
                Times.Once);

            var stored = await _db.Competitors
                .SingleAsync(x => x.CompetitorId == 1);

            stored.CyclingFlashLastScraped.Should().NotBeNull();

            (await _db.ScrapeCompetitorRatings.ToListAsync())
                .Should().BeEmpty();
        }

        [Fact]
        public async Task SyncStartlistAsync_WhenEventDoesNotExist_ThrowsException()
        {
            // Arrange
            var scrapedEntries = new List<ScrapedStartlistEntry>();

            // Act
            var act = () => _service.SyncStartlistAsync(999, scrapedEntries);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Event 999 niet gevonden");
        }

        [Fact]
        public async Task SyncStartlistAsync_WhenEventHasNoTeams_ThrowsInvalidOperationException()
        {
            // Arrange
            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026
            };

            _db.Events.Add(eventEntity);
            await _db.SaveChangesAsync();

            // Act
            var act = () => _service.SyncStartlistAsync(
                1,
                new List<ScrapedStartlistEntry>());

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Geen teams geselecteerd voor dit evenement.");
        }

        [Fact]
        public async Task SyncStartlistAsync_WhenNewRiderMatchesByPcsAndTeam_AddsCompetitorInEvent()
        {
            // Arrange
            var country = new Country
            {
                CountryId = 1,
                CountryNameShort = "NED",
                CountryNameLong = "Nederland"
            };

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear,
                Name = "Test Team"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                PcsName = "jansen-jan",
                CountryId = 1,
                Country = country
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor,
                TeamYearId = 1,
                TeamYear = teamYear
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026
            };

            var eventTeam = new EventTeam
            {
                EventId = 1,
                TeamId = 1
            };

            _db.Countries.Add(country);
            _db.Teams.Add(team);
            _db.SeasonYears.Add(seasonYear);
            _db.TeamYear.Add(teamYear);
            _db.Competitors.Add(competitor);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.Events.Add(eventEntity);
            _db.EventTeam.Add(eventTeam);

            await _db.SaveChangesAsync();

            var scrapedEntry = new ScrapedStartlistEntry
            {
                RiderName = "Jansen Jan",
                PcsName = "jansen-jan",
                TeamPcsName = "test-team",
                TeamName = "Test Team",
                BibNumber = 42
            };

            // Act
            await _service.SyncStartlistAsync(
                1,
                new List<ScrapedStartlistEntry> { scrapedEntry });

            // Assert
            var stored = await _db.CompetitorsInEvent
                .SingleAsync();

            stored.EventId.Should().Be(1);
            stored.CompetitorInTeamId.Should().Be(1);
            stored.EventNumber.Should().Be(42);
            stored.InSelectie.Should().BeTrue();
            stored.OutOfCompetition.Should().BeFalse();
            stored.RemovedFromStartList.Should().BeFalse();
        }

        [Fact]
        public async Task SyncStartlistAsync_WhenCompetitorInEventAlreadyExists_UpdatesExistingEntry()
        {
            // Arrange
            var country = new Country
            {
                CountryId = 1,
                CountryNameShort = "NED",
                CountryNameLong = "Nederland"
            };

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear,
                Name = "Test Team"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                PcsName = "jansen-jan",
                CountryId = 1,
                Country = country
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor,
                TeamYearId = 1,
                TeamYear = teamYear
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026
            };

            var eventTeam = new EventTeam
            {
                EventId = 1,
                TeamId = 1
            };

            var existingEntry = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 1,
                CompetitorInTeamId = 1,
                CompetitorInTeam = competitorInTeam,
                EventNumber = 10,
                InSelectie = false,
                OutOfCompetition = true,
                RemovedFromStartList = true
            };

            _db.Countries.Add(country);
            _db.Teams.Add(team);
            _db.SeasonYears.Add(seasonYear);
            _db.TeamYear.Add(teamYear);
            _db.Competitors.Add(competitor);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.Events.Add(eventEntity);
            _db.EventTeam.Add(eventTeam);
            _db.CompetitorsInEvent.Add(existingEntry);

            await _db.SaveChangesAsync();

            var scrapedEntry = new ScrapedStartlistEntry
            {
                RiderName = "Jansen Jan",
                PcsName = "jansen-jan",
                TeamPcsName = "test-team",
                TeamName = "Test Team",
                BibNumber = 42
            };

            // Act
            await _service.SyncStartlistAsync(
                1,
                new List<ScrapedStartlistEntry> { scrapedEntry });

            // Assert
            var entries = await _db.CompetitorsInEvent.ToListAsync();

            entries.Should().ContainSingle();

            var stored = entries[0];

            stored.EventNumber.Should().Be(42);
            stored.InSelectie.Should().BeTrue();
            stored.RemovedFromStartList.Should().BeFalse();
        }

        [Fact]
        public async Task SyncStartlistAsync_WhenTeamPcsNameIsMissing_MatchesByPcsName()
        {
            // Arrange
            var country = new Country
            {
                CountryId = 1,
                CountryNameShort = "NED",
                CountryNameLong = "Nederland"
            };

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear,
                Name = "Test Team"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                PcsName = "jansen-jan",
                CountryId = 1,
                Country = country
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor,
                TeamYearId = 1,
                TeamYear = teamYear
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026
            };

            var eventTeam = new EventTeam
            {
                EventId = 1,
                TeamId = 1
            };

            _db.Countries.Add(country);
            _db.Teams.Add(team);
            _db.SeasonYears.Add(seasonYear);
            _db.TeamYear.Add(teamYear);
            _db.Competitors.Add(competitor);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.Events.Add(eventEntity);
            _db.EventTeam.Add(eventTeam);

            await _db.SaveChangesAsync();

            var scrapedEntry = new ScrapedStartlistEntry
            {
                RiderName = "Jansen Jan",
                PcsName = "jansen-jan",
                TeamPcsName = null!,
                TeamName = "Test Team",
                BibNumber = 42
            };

            // Act
            await _service.SyncStartlistAsync(
                1,
                new List<ScrapedStartlistEntry> { scrapedEntry });

            // Assert
            var stored = await _db.CompetitorsInEvent
                .SingleAsync();

            stored.CompetitorInTeamId.Should().Be(1);
            stored.EventNumber.Should().Be(42);
            stored.InSelectie.Should().BeTrue();
        }

        [Fact]
        public async Task SyncStartlistAsync_WhenPcsNameIsMissing_MatchesByNameAndTeam()
        {
            // Arrange
            var country = new Country
            {
                CountryId = 1,
                CountryNameShort = "NED",
                CountryNameLong = "Nederland"
            };

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear,
                Name = "Test Team"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                PcsName = "",
                CountryId = 1,
                Country = country
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor,
                TeamYearId = 1,
                TeamYear = teamYear
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026
            };

            var eventTeam = new EventTeam
            {
                EventId = 1,
                TeamId = 1
            };

            _db.Countries.Add(country);
            _db.Teams.Add(team);
            _db.SeasonYears.Add(seasonYear);
            _db.TeamYear.Add(teamYear);
            _db.Competitors.Add(competitor);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.Events.Add(eventEntity);
            _db.EventTeam.Add(eventTeam);

            await _db.SaveChangesAsync();

            var scrapedEntry = new ScrapedStartlistEntry
            {
                RiderName = "Jansen Jan",
                PcsName = null!,
                TeamPcsName = null!,
                TeamName = "Test Team",
                BibNumber = 42
            };

            // Act
            await _service.SyncStartlistAsync(
                1,
                new List<ScrapedStartlistEntry> { scrapedEntry });

            // Assert
            var stored = await _db.CompetitorsInEvent
                .SingleAsync();

            stored.CompetitorInTeamId.Should().Be(1);
            stored.EventNumber.Should().Be(42);
            stored.InSelectie.Should().BeTrue();
        }

        [Fact]
        public async Task SyncStartlistAsync_WhenPcsNameAndTeamDoNotMatch_MatchesByName()
        {
            // Arrange
            var country = new Country
            {
                CountryId = 1,
                CountryNameShort = "NED",
                CountryNameLong = "Nederland"
            };

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear,
                Name = "Test Team"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                PcsName = "",
                CountryId = 1,
                Country = country
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor,
                TeamYearId = 1,
                TeamYear = teamYear
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026
            };

            var eventTeam = new EventTeam
            {
                EventId = 1,
                TeamId = 1
            };

            _db.Countries.Add(country);
            _db.Teams.Add(team);
            _db.SeasonYears.Add(seasonYear);
            _db.TeamYear.Add(teamYear);
            _db.Competitors.Add(competitor);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.Events.Add(eventEntity);
            _db.EventTeam.Add(eventTeam);

            await _db.SaveChangesAsync();

            var scrapedEntry = new ScrapedStartlistEntry
            {
                RiderName = "Jansen Jan",
                PcsName = null!,
                TeamPcsName = "andere-ploeg",
                TeamName = "Andere Ploeg",
                BibNumber = 42
            };

            // Act
            await _service.SyncStartlistAsync(
                1,
                new List<ScrapedStartlistEntry> { scrapedEntry });

            // Assert
            var stored = await _db.CompetitorsInEvent
                .SingleAsync();

            stored.CompetitorInTeamId.Should().Be(1);
            stored.EventNumber.Should().Be(42);
            stored.InSelectie.Should().BeTrue();
        }

        [Fact]
        public async Task SyncStartlistAsync_WhenRiderCannotBeMatched_SkipsRider()
        {
            // Arrange
            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear,
                Name = "Test Team"
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026
            };

            var eventTeam = new EventTeam
            {
                EventId = 1,
                TeamId = 1
            };

            _db.Teams.Add(team);
            _db.SeasonYears.Add(seasonYear);
            _db.TeamYear.Add(teamYear);
            _db.Events.Add(eventEntity);
            _db.EventTeam.Add(eventTeam);

            await _db.SaveChangesAsync();

            var scrapedEntry = new ScrapedStartlistEntry
            {
                RiderName = "Onbekende Renner",
                PcsName = "unknown-rider",
                TeamPcsName = "unknown-team",
                TeamName = "Unknown Team",
                BibNumber = 42
            };

            // Act
            await _service.SyncStartlistAsync(
                1,
                new List<ScrapedStartlistEntry> { scrapedEntry });

            // Assert
            var stored = await _db.CompetitorsInEvent
                .ToListAsync();

            stored.Should().BeEmpty();
        }

        [Fact]
        public async Task SyncStartlistAsync_WhenCompetitorHasNoPcsName_UpdatesPcsName()
        {
            // Arrange
            var country = new Country
            {
                CountryId = 1,
                CountryNameShort = "NED",
                CountryNameLong = "Nederland"
            };

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear,
                Name = "Test Team"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                PcsName = "",
                CountryId = 1,
                Country = country
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor,
                TeamYearId = 1,
                TeamYear = teamYear
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026
            };

            var eventTeam = new EventTeam
            {
                EventId = 1,
                TeamId = 1
            };

            _db.Countries.Add(country);
            _db.Teams.Add(team);
            _db.SeasonYears.Add(seasonYear);
            _db.TeamYear.Add(teamYear);
            _db.Competitors.Add(competitor);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.Events.Add(eventEntity);
            _db.EventTeam.Add(eventTeam);

            await _db.SaveChangesAsync();

            var scrapedEntry = new ScrapedStartlistEntry
            {
                RiderName = "Jansen Jan",
                PcsName = "jansen-jan",
                TeamPcsName = null!,
                TeamName = "Test Team",
                BibNumber = 42
            };

            // Act
            await _service.SyncStartlistAsync(
                1,
                new List<ScrapedStartlistEntry> { scrapedEntry });

            // Assert
            var storedCompetitor = await _db.Competitors
                .SingleAsync(x => x.CompetitorId == 1);

            storedCompetitor.PcsName.Should().Be("jansen-jan");
        }

        [Fact]
        public async Task RefreshStartlistAsync_WhenEventDoesNotExist_ThrowsException()
        {
            // Act
            Func<Task> act = () => _service.RefreshStartlistAsync(999);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Event 999 niet gevonden");

            _pcsScraperMock.Verify(
                x => x.ScrapeStartlistAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task RefreshStartlistAsync_WhenEventExists_ScrapesAndSyncsStartlist()
        {
            // Arrange
            var country = new Country
            {
                CountryId = 1,
                CountryNameShort = "NED",
                CountryNameLong = "Nederland"
            };

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear,
                Name = "Test Team"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                PcsName = "jansen-jan",
                CountryId = 1,
                Country = country
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor,
                TeamYearId = 1,
                TeamYear = teamYear
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026
            };

            var eventTeam = new EventTeam
            {
                EventId = 1,
                TeamId = 1
            };

            _db.Countries.Add(country);
            _db.Teams.Add(team);
            _db.SeasonYears.Add(seasonYear);
            _db.TeamYear.Add(teamYear);
            _db.Competitors.Add(competitor);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.Events.Add(eventEntity);
            _db.EventTeam.Add(eventTeam);

            await _db.SaveChangesAsync();

            var scrapedEntries = new List<ScrapedStartlistEntry>
            {
                new()
                {
                    RiderName = "Jansen Jan",
                    PcsName = "jansen-jan",
                    TeamPcsName = "test-team",
                    TeamName = "Test Team",
                    BibNumber = 42
                }
            };

            _pcsScraperMock
                .Setup(x => x.ScrapeStartlistAsync(
                    "https://www.procyclingstats.com/race/test-event/2026/startlist"))
                .ReturnsAsync(scrapedEntries);

            // Act
            await _service.RefreshStartlistAsync(1);

            // Assert
            _pcsScraperMock.Verify(
                x => x.ScrapeStartlistAsync(
                    "https://www.procyclingstats.com/race/test-event/2026/startlist"),
                Times.Once);

            var stored = await _db.CompetitorsInEvent
                .SingleAsync();

            stored.CompetitorInTeamId.Should().Be(1);
            stored.EventNumber.Should().Be(42);
            stored.InSelectie.Should().BeTrue();
        }

        [Fact]
        public async Task ProcessRatingsAsync_WhenCompetitorCannotBeMatched_SkipsRating()
        {
            // Arrange
            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026,
                Active = true
            };

            var category = new RatingCategory
            {
                RatingCategoryId = 1,
                Code = "GC"
            };

            var rating = new ScrapeCompetitorRating
            {
                Id = 1,
                BatchId = Guid.NewGuid(),
                CompetitorName = "Onbekende Renner",
                RatingCategoryCode = "GC",
                Rating = 75
            };

            _db.SeasonYears.Add(seasonYear);
            _db.RatingCategories.Add(category);
            _db.ScrapeCompetitorRatings.Add(rating);

            await _db.SaveChangesAsync();

            // Act
            await _service.ProcessRatingsAsync(rating.BatchId, 2026);

            // Assert
            var stored = await _db.ScrapeCompetitorRatings
                .SingleAsync();

            stored.Processed.Should().BeFalse();

            var competitorRatings = await _db.CompetitorRatings
                .ToListAsync();

            competitorRatings.Should().BeEmpty();
        }

        [Fact]
        public async Task ProcessRatingsAsync_WhenCompetitorMatchesByProfileUrl_UpdatesExistingRating()
        {
            // Arrange
            var batchId = Guid.NewGuid();

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026,
                Active = true
            };

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear,
                Year = 2026
            };

            var category = new RatingCategory
            {
                RatingCategoryId = 1,
                Code = "GC"
            };

            var country = new Country
            {
                CountryId = 1,
                CountryNameShort = "NED",
                CountryNameLong = "Nederland"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                CountryId = 1,
                Country = country,
                CyclingFlashScraperName = "jan-jansen"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor,
                TeamYearId = 1,
                TeamYear = teamYear
            };

            var existingRating = new CompetitorRating
            {
                CompetitorId = 1,
                Competitor = competitor,
                RatingCategoryId = 1,
                RatingCategory = category,
                Rating = 50,
                RatingDate = new DateTime(2026, 9, 1)
            };

            var rating = new ScrapeCompetitorRating
            {
                Id = 1,
                BatchId = batchId,
                CompetitorName = "Onbekende Naam",
                ProfileUrl = "jan-jansen",
                RatingCategoryCode = "GC",
                Rating = 85,
                RatingDate = new DateTime(2026, 10, 1)
            };

            _db.SeasonYears.Add(seasonYear);
            _db.Teams.Add(team);
            _db.TeamYear.Add(teamYear);
            _db.RatingCategories.Add(category);
            _db.Countries.Add(country);
            _db.Competitors.Add(competitor);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.CompetitorRatings.Add(existingRating);
            _db.ScrapeCompetitorRatings.Add(rating);

            await _db.SaveChangesAsync();

            // Act
            await _service.ProcessRatingsAsync(batchId, 2026);
;
            // Assert
            var storedScrapeRating = await _db.ScrapeCompetitorRatings
                .SingleAsync();

            storedScrapeRating.Processed.Should().BeTrue();

            var storedRating = await _db.CompetitorRatings
                .SingleAsync();

            storedRating.Rating.Should().Be(85);
            storedRating.RatingDate.Should().Be(new DateTime(2026, 10, 1));
        }

        [Fact]
        public async Task ProcessRatingsAsync_WhenCompetitorRatingDoesNotExist_CreatesRating()
        {
            // Arrange
            var batchId = Guid.NewGuid();

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026,
                Active = true
            };

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear,
                Year = 2026
            };

            var category = new RatingCategory
            {
                RatingCategoryId = 1,
                Code = "GC"
            };

            var country = new Country
            {
                CountryId = 1,
                CountryNameShort = "NED",
                CountryNameLong = "Nederland"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                CountryId = 1,
                Country = country,
                CyclingFlashScraperName = "jan-jansen"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor,
                TeamYearId = 1,
                TeamYear = teamYear
            };

            var rating = new ScrapeCompetitorRating
            {
                Id = 1,
                BatchId = batchId,
                CompetitorName = "Onbekende Naam",
                ProfileUrl = "jan-jansen",
                RatingCategoryCode = "GC",
                Rating = 85,
                RatingDate = new DateTime(2026, 10, 1)
            };

            _db.SeasonYears.Add(seasonYear);
            _db.Teams.Add(team);
            _db.TeamYear.Add(teamYear);
            _db.RatingCategories.Add(category);
            _db.Countries.Add(country);
            _db.Competitors.Add(competitor);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.ScrapeCompetitorRatings.Add(rating);

            await _db.SaveChangesAsync();

            // Act
            await _service.ProcessRatingsAsync(batchId, 2026);

            // Assert
            var storedScrapeRating = await _db.ScrapeCompetitorRatings
                .SingleAsync();

            storedScrapeRating.Processed.Should().BeTrue();

            var storedRating = await _db.CompetitorRatings
                .SingleAsync();

            storedRating.CompetitorId.Should().Be(1);
            storedRating.RatingCategoryId.Should().Be(1);
            storedRating.Rating.Should().Be(85);
            storedRating.RatingDate.Should().Be(new DateTime(2026, 10, 1));
        }

        [Fact]
        public async Task ProcessRatingsAsync_WhenCompetitorMatchesByName_ProcessesRating()
        {
            // Arrange
            var batchId = Guid.NewGuid();

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026,
                Active = true
            };

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear,
                Year = 2026
            };

            var category = new RatingCategory
            {
                RatingCategoryId = 1,
                Code = "GC"
            };

            var country = new Country
            {
                CountryId = 1,
                CountryNameShort = "NED",
                CountryNameLong = "Nederland"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                CountryId = 1,
                Country = country,
                CyclingFlashScraperName = null
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor,
                TeamYearId = 1,
                TeamYear = teamYear
            };

            var rating = new ScrapeCompetitorRating
            {
                Id = 1,
                BatchId = batchId,
                CompetitorName = "Jan Jansen",
                ProfileUrl = null,
                RatingCategoryCode = "GC",
                Rating = 85,
                RatingDate = new DateTime(2026, 10, 1)
            };

            _db.SeasonYears.Add(seasonYear);
            _db.Teams.Add(team);
            _db.TeamYear.Add(teamYear);
            _db.RatingCategories.Add(category);
            _db.Countries.Add(country);
            _db.Competitors.Add(competitor);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.ScrapeCompetitorRatings.Add(rating);

            await _db.SaveChangesAsync();

            // Act
            await _service.ProcessRatingsAsync(batchId, 2026);

            // Assert
            var storedScrapeRating = await _db.ScrapeCompetitorRatings
                .SingleAsync();

            storedScrapeRating.Processed.Should().BeTrue();

            var storedRating = await _db.CompetitorRatings
                .SingleAsync();

            storedRating.CompetitorId.Should().Be(1);
            storedRating.Rating.Should().Be(85);
            storedRating.RatingDate.Should().Be(new DateTime(2026, 10, 1));
        }

        [Fact]
        public async Task ProcessRatingsAsync_WhenFullNameDoesNotMatch_UsesShortNameMatch()
        {
            // Arrange
            var batchId = Guid.NewGuid();

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026,
                Active = true
            };

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = 1,
                Team = team,
                SeasonYearId = 1,
                SeasonYear = seasonYear,
                Year = 2026
            };

            var category = new RatingCategory
            {
                RatingCategoryId = 1,
                Code = "GC"
            };

            var country = new Country
            {
                CountryId = 1,
                CountryNameShort = "NED",
                CountryNameLong = "Nederland"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                CountryId = 1,
                Country = country
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                Competitor = competitor,
                TeamYearId = 1,
                TeamYear = teamYear
            };

            var rating = new ScrapeCompetitorRating
            {
                Id = 1,
                BatchId = batchId,
                CompetitorName = "Jan Jansen Extra",
                ProfileUrl = null,
                RatingCategoryCode = "GC",
                Rating = 85,
                RatingDate = new DateTime(2026, 10, 1)
            };

            _db.SeasonYears.Add(seasonYear);
            _db.Teams.Add(team);
            _db.TeamYear.Add(teamYear);
            _db.RatingCategories.Add(category);
            _db.Countries.Add(country);
            _db.Competitors.Add(competitor);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.ScrapeCompetitorRatings.Add(rating);

            await _db.SaveChangesAsync();

            // Act
            await _service.ProcessRatingsAsync(batchId, 2026);

            // Assert
            var storedScrapeRating = await _db.ScrapeCompetitorRatings
                .SingleAsync();

            storedScrapeRating.Processed.Should().BeTrue();

            var storedRating = await _db.CompetitorRatings
                .SingleAsync();

            storedRating.CompetitorId.Should().Be(1);
            storedRating.Rating.Should().Be(85);
            storedRating.RatingDate.Should().Be(new DateTime(2026, 10, 1));
        }

        [Fact]
        public async Task ProcessRatingsAsync_WhenRatingCategoryIsUnknown_SkipsRating()
        {
            var batchId = Guid.NewGuid();

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = team.TeamId,
                Team = team,
                SeasonYearId = seasonYear.SeasonYearId,
                SeasonYear = seasonYear,
                Year = 2026
            };

            var country = new Country
            {
                CountryId = 1,
                CountryNameShort = "NED",
                CountryNameLong = "Nederland"
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                CountryId = country.CountryId,
                Country = country
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = competitor.CompetitorId,
                Competitor = competitor,
                TeamYearId = teamYear.TeamYearId,
                TeamYear = teamYear
            };

            var rating = new ScrapeCompetitorRating
            {
                BatchId = batchId,
                CompetitorName = "Jan Jansen",
                RatingCategoryCode = "UNKNOWN",
                Rating = 85,
                RatingDate = new DateTime(2026, 10, 1),
                Processed = false
            };

            _db.SeasonYears.Add(seasonYear);
            _db.Teams.Add(team);
            _db.TeamYear.Add(teamYear);
            _db.Countries.Add(country);
            _db.Competitors.Add(competitor);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.ScrapeCompetitorRatings.Add(rating);

            await _db.SaveChangesAsync();

            await _service.ProcessRatingsAsync(batchId, 2026);

            var storedRating = await _db.ScrapeCompetitorRatings
                .SingleAsync();

            storedRating.Processed.Should().BeFalse();

            var competitorRatings = await _db.CompetitorRatings
                .ToListAsync();

            competitorRatings.Should().BeEmpty();
        }

        [Fact]
        public async Task RunAsync_WhenStageDoesNotExist_ThrowsException()
        {
            await FluentActions
                .Invoking(() => _service.RunAsync(
                    eventId: 1,
                    eventName: "test-event",
                    stageNumber: 1,
                    year: 2026))
                .Should()
                .ThrowAsync<Exception>()
                .WithMessage("Stage niet gevonden");
        }

        [Fact]
        public async Task RunAsync_WhenEventHasNoConfiguration_ThrowsInvalidOperationException()
        {
            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventYear = 2026,
                Configuration = null
            };

            var stage = new Stage
            {
                Id = 1,
                StageName = "1",
                EventId = eventEntity.EventId,
                Event = eventEntity
            };

            _db.Events.Add(eventEntity);
            _db.Stages.Add(stage);

            await _db.SaveChangesAsync();

            await FluentActions
                .Invoking(() => _service.RunAsync(
                    eventId: 1,
                    eventName: "test-event",
                    stageNumber: 1,
                    year: 2026))
                .Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Event 1 heeft geen configuratie.");
        }

        [Fact]
        public async Task RunAsync_WhenStageExists_RemovesOldResultsAndStoresNewResults()
        {
            var configuration = new Configuration
            {
                Id = 1,
                ConfigurationItems = new List<ConfigurationItem>
                {
                    new ConfigurationItem
                    {
                        Id = 1,
                        Position = 1
                    }
                }
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                StageName = "1",
                EventId = eventEntity.EventId,
                Event = eventEntity
            };

            var oldResult = new ScrapedStageResult
            {
                EventId = 1,
                StageId = 1,
                BibNumber = 99,
                RiderName = "Old Rider",
                TeamName = "Old Team",
                Position = 10
            };

            _db.Events.Add(eventEntity);
            _db.Stages.Add(stage);
            _db.ScrapedStageResults.Add(oldResult);

            await _db.SaveChangesAsync();

            var newResult = new ScrapedStageResult
            {
                BibNumber = 12,
                RiderName = "Jan Jansen",
                TeamName = "Test Team",
                Position = 1
            };

            _pcsScraperMock
                .Setup(x => x.ScrapeStageResultsAsync(
                    It.IsAny<string>(),
                    1,
                    1))
                .ReturnsAsync(new List<ScrapedStageResult>
                {
            newResult
                });

            _pcsScraperMock
                .Setup(x => x.ScrapeClassificationWinnerWithRetryAsync(
                    It.IsAny<string>(),
                    stage.Id,
                    It.IsAny<QuestionType>()))
                .ReturnsAsync((ScrapedStageSpecialResult?)null);

            await _service.RunAsync(
                eventId: 1,
                eventName: "test-event",
                stageNumber: 1,
                year: 2026);

            var results = await _db.ScrapedStageResults
                .Where(x => x.EventId == 1 && x.StageId == 1)
                .ToListAsync();

            results.Should().ContainSingle();
            results[0].BibNumber.Should().Be(12);
            results[0].RiderName.Should().Be("Jan Jansen");
            results[0].Position.Should().Be(1);
        }

        [Fact]
        public async Task RunAsync_WhenSpecialResultIsScraped_StoresSpecialResult()
        {
            var configuration = new Configuration
            {
                Id = 1,
                ConfigurationItems = new List<ConfigurationItem>
                {
                    new ConfigurationItem
                    {
                        Id = 1,
                        Position = 1
                    }
                },
                Specials = new List<ConfigurationItemSpecial>
                {
                    new ConfigurationItemSpecial
                    {
                        Id = 10,
                        Question = QuestionType.GC
                    }
                }
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                StageName = "1",
                EventId = eventEntity.EventId,
                Event = eventEntity
            };

            _db.Events.Add(eventEntity);
            _db.Stages.Add(stage);

            await _db.SaveChangesAsync();

            _pcsScraperMock
                .Setup(x => x.ScrapeStageResultsAsync(
                    It.IsAny<string>(),
                    1,
                    1))
                .ReturnsAsync(new List<ScrapedStageResult>());

            _pcsScraperMock
                .Setup(x => x.ScrapeClassificationWinnerWithRetryAsync(
                    It.IsAny<string>(),
                    stage.Id,
                    QuestionType.GC))
                .ReturnsAsync(new ScrapedStageSpecialResult
                {
                    BibNumber = 12,
                    QuestionType = QuestionType.GC
                });

            await _service.RunAsync(
                eventId: 1,
                eventName: "test-event",
                stageNumber: 1,
                year: 2026);

            var specialResults = await _db.ScrapedSpecialResults
                .Where(x => x.StageId == stage.Id)
                .ToListAsync();

            specialResults.Should().ContainSingle();
            specialResults[0].BibNumber.Should().Be(12);
            specialResults[0].QuestionType.Should().Be(QuestionType.GC);
        }

        [Fact]
        public async Task RunAsync_WhenExistingSpecialResultsExist_ReplacesThem()
        {
            var configuration = new Configuration
            {
                Id = 1,
                ConfigurationItems = new List<ConfigurationItem>
                {
                    new ConfigurationItem
                    {
                        Id = 1,
                        Position = 1
                    }
                },
                Specials = new List<ConfigurationItemSpecial>
                {
                    new ConfigurationItemSpecial
                    {
                        Id = 10,
                        Question = QuestionType.GC
                    }
                }
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                StageName = "1",
                EventId = eventEntity.EventId,
                Event = eventEntity
            };

            var oldSpecial = new ScrapedStageSpecialResult
            {
                BibNumber = 99,
                QuestionType = QuestionType.GC
            };

            _db.Events.Add(eventEntity);
            _db.Stages.Add(stage);
            await _db.SaveChangesAsync();

            _db.ScrapedSpecialResults.Add(new Domain.Models.ScrapedSpecialResult
            {
                StageId = stage.Id,
                BibNumber = 99,
                QuestionType = QuestionType.GC,
                ImportedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();

            _pcsScraperMock
                .Setup(x => x.ScrapeStageResultsAsync(
                    It.IsAny<string>(),
                    1,
                    1))
                .ReturnsAsync(new List<ScrapedStageResult>());

            _pcsScraperMock
                .Setup(x => x.ScrapeClassificationWinnerWithRetryAsync(
                    It.IsAny<string>(),
                    stage.Id,
                    QuestionType.GC))
                .ReturnsAsync(new ScrapedStageSpecialResult
                {
                    BibNumber = 12,
                    QuestionType = QuestionType.GC
                });

            await _service.RunAsync(
                1,
                "test-event",
                1,
                2026);

            var results = await _db.ScrapedSpecialResults
                .Where(x => x.StageId == stage.Id)
                .ToListAsync();

            results.Should().ContainSingle();
            results[0].BibNumber.Should().Be(12);
        }

        [Fact]
        public async Task RunAsync_WhenScrapedResultHasNoMatchingCompetitor_SkipsResult()
        {
            var configuration = new Configuration
            {
                Id = 1,
                ConfigurationItems = new List<ConfigurationItem>
                {
                    new ConfigurationItem
                    {
                        Id = 1,
                        Position = 1
                    }
                }
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                StageName = "1",
                EventId = eventEntity.EventId,
                Event = eventEntity
            };

            _db.Events.Add(eventEntity);
            _db.Stages.Add(stage);
            await _db.SaveChangesAsync();

            _pcsScraperMock
                .Setup(x => x.ScrapeStageResultsAsync(
                    It.IsAny<string>(),
                    1,
                    1))
                .ReturnsAsync(new List<ScrapedStageResult>
                {
                    new ScrapedStageResult
                    {
                        BibNumber = 999,
                        RiderName = "Unknown Rider",
                        TeamName = "Unknown Team",
                        Position = 1
                    }
                });

            await _service.RunAsync(
                1,
                "test-event",
                1,
                2026);

            var scrapedResult = await _db.ScrapedStageResults
                .SingleAsync();

            scrapedResult.MatchedCompetitorInEventId.Should().BeNull();

            (await _db.Results.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task RunAsync_WhenScrapedResultHasInvalidBib_SkipsResult()
        {
            var configuration = new Configuration
            {
                Id = 1,
                ConfigurationItems = new List<ConfigurationItem>
                {
                    new ConfigurationItem
                    {
                        Id = 1,
                        Position = 1
                    }
                }
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                StageName = "1",
                EventId = eventEntity.EventId,
                Event = eventEntity
            };

            _db.Events.Add(eventEntity);
            _db.Stages.Add(stage);
            await _db.SaveChangesAsync();

            _pcsScraperMock
                .Setup(x => x.ScrapeStageResultsAsync(
                    It.IsAny<string>(),
                    1,
                    1))
                .ReturnsAsync(new List<ScrapedStageResult>
                {
                    new ScrapedStageResult
                    {
                        BibNumber = 0,
                        RiderName = "Unknown Rider",
                        TeamName = "Unknown Team",
                        Position = 1
                    }
                });

            await _service.RunAsync(
                1,
                "test-event",
                1,
                2026);

            var scrapedResult = await _db.ScrapedStageResults
                .SingleAsync();

            scrapedResult.MatchedCompetitorInEventId.Should().BeNull();

            (await _db.Results.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task RunAsync_WhenResultAlreadyExists_UpdatesConfigurationItem()
        {
            var configurationItem = new ConfigurationItem
            {
                Id = 10,
                Position = 1
            };

            var configuration = new Configuration
            {
                Id = 1,
                ConfigurationItems = new List<ConfigurationItem>
                {
                    configurationItem
                }
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                StageName = "1",
                EventId = 1,
                Event = eventEntity
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen"
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                Year = 2026
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = competitor.CompetitorId,
                Competitor = competitor,
                TeamYearId = teamYear.TeamYearId,
                TeamYear = teamYear
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 1,
                CompetitorInTeamId = competitorInTeam.Id,
                CompetitorInTeam = competitorInTeam,
                EventNumber = 12
            };

            var oldConfigurationItem = new ConfigurationItem
            {
                Id = 20,
                Position = 2
            };

            var existingResult = new Result
            {
                Id = 1,
                StageId = stage.Id,
                CompetitorInEventId = competitorInEvent.Id,
                CompetitorInEvent = competitorInEvent,
                ConfigurationItemId = oldConfigurationItem.Id,
                ConfigurationItem = oldConfigurationItem
            };

            _db.Events.Add(eventEntity);
            _db.Stages.Add(stage);
            _db.Competitors.Add(competitor);
            _db.TeamYear.Add(teamYear);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.CompetitorsInEvent.Add(competitorInEvent);
            _db.ConfigurationItems.Add(oldConfigurationItem);
            _db.Results.Add(existingResult);

            await _db.SaveChangesAsync();

            _pcsScraperMock
                .Setup(x => x.ScrapeStageResultsAsync(
                    It.IsAny<string>(),
                    1,
                    1))
                .ReturnsAsync(new List<ScrapedStageResult>
                {
                    new ScrapedStageResult
                    {
                        BibNumber = 12,
                        RiderName = "Jan Jansen",
                        TeamName = "Test Team",
                        Position = 1
                    }
                });

            await _service.RunAsync(
                1,
                "test-event",
                1,
                2026);

            var result = await _db.Results
                .SingleAsync();

            result.ConfigurationItemId.Should().Be(configurationItem.Id);
        }

        [Fact]
        public async Task RunAsync_WhenExistingResultHasNoConfigurationItem_RemovesResult()
        {
            var configurationItem = new ConfigurationItem
            {
                Id = 10,
                Position = 1
            };

            var configuration = new Configuration
            {
                Id = 1,
                ConfigurationItems = new List<ConfigurationItem>
                {
                    configurationItem
                }
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                StageName = "1",
                EventId = 1,
                Event = eventEntity
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen"
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                Year = 2026
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = competitor.CompetitorId,
                Competitor = competitor,
                TeamYearId = teamYear.TeamYearId,
                TeamYear = teamYear
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 1,
                CompetitorInTeamId = competitorInTeam.Id,
                CompetitorInTeam = competitorInTeam,
                EventNumber = 12
            };

            var existingResult = new Result
            {
                Id = 1,
                StageId = stage.Id,
                CompetitorInEventId = competitorInEvent.Id,
                CompetitorInEvent = competitorInEvent,
                ConfigurationItemId = 999
            };

            _db.Events.Add(eventEntity);
            _db.Stages.Add(stage);
            _db.Competitors.Add(competitor);
            _db.TeamYear.Add(teamYear);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.CompetitorsInEvent.Add(competitorInEvent);
            _db.Results.Add(existingResult);

            await _db.SaveChangesAsync();

            _pcsScraperMock
                .Setup(x => x.ScrapeStageResultsAsync(
                    It.IsAny<string>(),
                    1,
                    1))
                .ReturnsAsync(new List<ScrapedStageResult>
                {
                    new ScrapedStageResult
                    {
                        BibNumber = 12,
                        RiderName = "Jan Jansen",
                        TeamName = "Test Team",
                        Position = 99
                    }
                });

            await _service.RunAsync(
                1,
                "test-event",
                1,
                2026);

            var results = await _db.Results
                .ToListAsync();

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task RunAsync_WhenNoResultExists_CreatesResult()
        {
            var configurationItem = new ConfigurationItem
            {
                Id = 10,
                Position = 1
            };

            var configuration = new Configuration
            {
                Id = 1,
                ConfigurationItems = new List<ConfigurationItem>
                {
                    configurationItem
                }
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                StageName = "1",
                EventId = 1,
                Event = eventEntity
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen"
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                Year = 2026
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = competitor.CompetitorId,
                Competitor = competitor,
                TeamYearId = teamYear.TeamYearId,
                TeamYear = teamYear
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 1,
                CompetitorInTeamId = competitorInTeam.Id,
                CompetitorInTeam = competitorInTeam,
                EventNumber = 12
            };

            _db.Events.Add(eventEntity);
            _db.Stages.Add(stage);
            _db.Competitors.Add(competitor);
            _db.TeamYear.Add(teamYear);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.CompetitorsInEvent.Add(competitorInEvent);

            await _db.SaveChangesAsync();

            _pcsScraperMock
                .Setup(x => x.ScrapeStageResultsAsync(
                    It.IsAny<string>(),
                    1,
                    1))
                .ReturnsAsync(new List<ScrapedStageResult>
                {
                    new ScrapedStageResult
                    {
                        BibNumber = 12,
                        RiderName = "Jan Jansen",
                        TeamName = "Test Team",
                        Position = 1
                    }
                });

            await _service.RunAsync(
                1,
                "test-event",
                1,
                2026);

            var results = await _db.Results
                .ToListAsync();

            results.Should().ContainSingle();

            results[0].StageId.Should().Be(stage.Id);
            results[0].CompetitorInEventId.Should().Be(competitorInEvent.Id);
            results[0].ConfigurationItemId.Should().Be(configurationItem.Id);
        }

        [Fact]
        public async Task RunAsync_WhenNoResultAndNoConfigurationItem_DoesNotCreateResult()
        {
            var configuration = new Configuration
            {
                Id = 1,
                ConfigurationItems = new List<ConfigurationItem>()
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                StageName = "1",
                EventId = 1,
                Event = eventEntity
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen"
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                Year = 2026
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = competitor.CompetitorId,
                Competitor = competitor,
                TeamYearId = teamYear.TeamYearId,
                TeamYear = teamYear
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 1,
                CompetitorInTeamId = competitorInTeam.Id,
                CompetitorInTeam = competitorInTeam,
                EventNumber = 12
            };

            _db.Events.Add(eventEntity);
            _db.Stages.Add(stage);
            _db.Competitors.Add(competitor);
            _db.TeamYear.Add(teamYear);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.CompetitorsInEvent.Add(competitorInEvent);

            await _db.SaveChangesAsync();

            _pcsScraperMock
                .Setup(x => x.ScrapeStageResultsAsync(
                    It.IsAny<string>(),
                    0,
                    1))
                .ReturnsAsync(new List<ScrapedStageResult>
                {
                    new ScrapedStageResult
                    {
                        BibNumber = 12,
                        RiderName = "Jan Jansen",
                        TeamName = "Test Team",
                        Position = 99
                    }
                });

            await _service.RunAsync(
                1,
                "test-event",
                1,
                2026);

            (await _db.Results.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task RunAsync_WhenSpecialHasNoMatchingCompetitor_DoesNotCreateSpecialResult()
        {
            var configuration = new Configuration
            {
                Id = 1,
                ConfigurationItems = new List<ConfigurationItem>
                {
                    new ConfigurationItem
                    {
                        Id = 1,
                        Position = 1
                    }
                },
                Specials = new List<ConfigurationItemSpecial>
                {
                    new ConfigurationItemSpecial
                    {
                        Id = 10,
                        Question = QuestionType.GC
                    }
                }
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                StageName = "1",
                EventId = 1,
                Event = eventEntity
            };

            _db.Events.Add(eventEntity);
            _db.Stages.Add(stage);
            await _db.SaveChangesAsync();

            _pcsScraperMock
                .Setup(x => x.ScrapeStageResultsAsync(
                    It.IsAny<string>(),
                    1,
                    1))
                .ReturnsAsync(new List<ScrapedStageResult>());

            _pcsScraperMock
                .Setup(x => x.ScrapeClassificationWinnerWithRetryAsync(
                    It.IsAny<string>(),
                    stage.Id,
                    QuestionType.GC))
                .ReturnsAsync(new ScrapedStageSpecialResult
                {
                    BibNumber = 999,
                    QuestionType = QuestionType.GC
                });

            await _service.RunAsync(
                1,
                "test-event",
                1,
                2026);

            var specialResults = await _db.SpecialResults
                .ToListAsync();

            specialResults.Should().BeEmpty();
        }

        [Fact]
        public async Task RunAsync_WhenSpecialHasNoConfiguration_DoesNotCreateSpecialResult()
        {
            var configuration = new Configuration
            {
                Id = 1,
                ConfigurationItems = new List<ConfigurationItem>
                {
                    new ConfigurationItem
                    {
                        Id = 1,
                        Position = 1
                    }
                },
                Specials = new List<ConfigurationItemSpecial>
                {
                    new ConfigurationItemSpecial
                    {
                        Id = 10,
                        Question = QuestionType.GC
                    }
                }
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                StageName = "1",
                EventId = eventEntity.EventId,
                Event = eventEntity
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                PcsName = "jan-jansen"
            };

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = team.TeamId,
                Team = team,
                Year = 2026
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = competitor.CompetitorId,
                Competitor = competitor,
                TeamYearId = teamYear.TeamYearId,
                TeamYear = teamYear
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 1,
                CompetitorInTeamId = competitorInTeam.Id,
                CompetitorInTeam = competitorInTeam,
                EventNumber = 12
            };

            _db.Events.Add(eventEntity);
            _db.Stages.Add(stage);
            _db.Competitors.Add(competitor);
            _db.Teams.Add(team);
            _db.TeamYear.Add(teamYear);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.CompetitorsInEvent.Add(competitorInEvent);

            await _db.SaveChangesAsync();

            _pcsScraperMock
                .Setup(x => x.ScrapeStageResultsAsync(
                    It.IsAny<string>(),
                    1,
                    1))
                .ReturnsAsync(new List<ScrapedStageResult>());

            _pcsScraperMock
                .Setup(x => x.ScrapeClassificationWinnerWithRetryAsync(
                    It.IsAny<string>(),
                    stage.Id,
                    QuestionType.GC))
                .ReturnsAsync(new ScrapedStageSpecialResult
                {
                    BibNumber = 12,
                    QuestionType = QuestionType.KOM
                });

            await _service.RunAsync(
                1,
                "test-event",
                1,
                2026);

            var specialResults = await _db.SpecialResults
                .ToListAsync();

            specialResults.Should().BeEmpty();
        }

        [Fact]
        public async Task RunAsync_WhenSpecialResultAlreadyExists_DoesNotCreateDuplicate()
        {
            var configurationSpecial = new ConfigurationItemSpecial
            {
                Id = 10,
                Question = QuestionType.GC
            };

            var configuration = new Configuration
            {
                Id = 1,
                ConfigurationItems = new List<ConfigurationItem>
                {
                    new ConfigurationItem
                    {
                        Id = 1,
                        Position = 1
                    }
                },
                Specials = new List<ConfigurationItemSpecial>
                {
                    configurationSpecial
                }
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                StageName = "1",
                EventId = 1,
                Event = eventEntity
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                PcsName = "jan-jansen"
            };

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = team.TeamId,
                Team = team,
                Year = 2026
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = competitor.CompetitorId,
                Competitor = competitor,
                TeamYearId = teamYear.TeamYearId,
                TeamYear = teamYear
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 1,
                CompetitorInTeamId = competitorInTeam.Id,
                CompetitorInTeam = competitorInTeam,
                EventNumber = 12
            };

            var existingSpecialResult = new SpecialResult
            {
                StageId = stage.Id,
                CompetitorInEventId = competitorInEvent.Id,
                SpecialId = configurationSpecial.Id
            };

            _db.Events.Add(eventEntity);
            _db.Stages.Add(stage);
            _db.Competitors.Add(competitor);
            _db.Teams.Add(team);
            _db.TeamYear.Add(teamYear);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.CompetitorsInEvent.Add(competitorInEvent);
            _db.SpecialResults.Add(existingSpecialResult);

            await _db.SaveChangesAsync();

            _pcsScraperMock
                .Setup(x => x.ScrapeStageResultsAsync(
                    It.IsAny<string>(),
                    1,
                    1))
                .ReturnsAsync(new List<ScrapedStageResult>());

            _pcsScraperMock
                .Setup(x => x.ScrapeClassificationWinnerWithRetryAsync(
                    It.IsAny<string>(),
                    stage.Id,
                    QuestionType.GC))
                .ReturnsAsync(new ScrapedStageSpecialResult
                {
                    BibNumber = 12,
                    QuestionType = QuestionType.GC
                });

            await _service.RunAsync(
                1,
                "test-event",
                1,
                2026);

            var results = await _db.SpecialResults
                .Where(x => x.StageId == stage.Id)
                .ToListAsync();

            results.Should().ContainSingle();
            results[0].CompetitorInEventId.Should().Be(competitorInEvent.Id);
            results[0].SpecialId.Should().Be(configurationSpecial.Id);

            _delayServiceMock.Verify(
                x => x.DelayAsync(
                    TimeSpan.FromSeconds(20),
                    It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        public async Task RunAsync_WhenSpecialResultDoesNotExist_CreatesSpecialResult()
        {
            var configurationSpecial = new ConfigurationItemSpecial
            {
                Id = 10,
                Question = QuestionType.GC
            };

            var configuration = new Configuration
            {
                Id = 1,
                ConfigurationItems = new List<ConfigurationItem>
                {
                    new ConfigurationItem
                    {
                        Id = 1,
                        Position = 1
                    }
                },
                Specials = new List<ConfigurationItemSpecial>
                {
                    configurationSpecial
                }
            };

            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventCode = "test-event",
                EventYear = 2026,
                Configuration = configuration
            };

            var stage = new Stage
            {
                Id = 1,
                StageName = "1",
                EventId = 1,
                Event = eventEntity
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen",
                PcsName = "jan-jansen"
            };

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team",
                PcsName = "test-team"
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = team.TeamId,
                Team = team,
                Year = 2026
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = competitor.CompetitorId,
                Competitor = competitor,
                TeamYearId = teamYear.TeamYearId,
                TeamYear = teamYear
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 1,
                CompetitorInTeamId = competitorInTeam.Id,
                CompetitorInTeam = competitorInTeam,
                EventNumber = 12
            };

            _db.Events.Add(eventEntity);
            _db.Stages.Add(stage);
            _db.Competitors.Add(competitor);
            _db.Teams.Add(team);
            _db.TeamYear.Add(teamYear);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.CompetitorsInEvent.Add(competitorInEvent);

            await _db.SaveChangesAsync();

            _pcsScraperMock
                .Setup(x => x.ScrapeStageResultsAsync(
                    It.IsAny<string>(),
                    1,
                    1))
                .ReturnsAsync(new List<ScrapedStageResult>());

            _pcsScraperMock
                .Setup(x => x.ScrapeClassificationWinnerWithRetryAsync(
                    It.IsAny<string>(),
                    stage.Id,
                    QuestionType.GC))
                .ReturnsAsync(new ScrapedStageSpecialResult
                {
                    BibNumber = 12,
                    QuestionType = QuestionType.GC
                });

            await _service.RunAsync(
                1,
                "test-event",
                1,
                2026);

            var results = await _db.SpecialResults
                .Where(x => x.StageId == stage.Id)
                .ToListAsync();

            results.Should().ContainSingle();
            results[0].CompetitorInEventId.Should().Be(competitorInEvent.Id);
            results[0].SpecialId.Should().Be(configurationSpecial.Id);
        }

        [Fact]
        public async Task RunDropoutsAsync_WhenNoCompetitorsDropOut_DoesNotChangeCompetitors()
        {
            var eventEntity = new Event
            {
                EventId = 1,
                EventName = "Test Event",
                EventYear = 2026
            };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jan",
                LastName = "Jansen"
            };

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "Test Team"
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                TeamId = team.TeamId,
                Team = team,
                Year = 2026
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = competitor.CompetitorId,
                Competitor = competitor,
                TeamYearId = teamYear.TeamYearId,
                TeamYear = teamYear
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = 1,
                EventId = eventEntity.EventId,
                CompetitorInTeamId = competitorInTeam.Id,
                CompetitorInTeam = competitorInTeam,
                EventNumber = 12,
                OutOfCompetition = false
            };

            _db.Events.Add(eventEntity);
            _db.Teams.Add(team);
            _db.TeamYear.Add(teamYear);
            _db.Competitors.Add(competitor);
            _db.CompetitorInTeams.Add(competitorInTeam);
            _db.CompetitorsInEvent.Add(competitorInEvent);

            await _db.SaveChangesAsync();

            _pcsScraperMock
                .Setup(x => x.ScrapeDropoutBibsAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<int> { 99, 100 });

            await _service.RunDropoutsAsync(
                eventId: 1,
                eventName: "test-event",
                year: 2026);

            var result = await _db.CompetitorsInEvent
                .SingleAsync();

            result.OutOfCompetition.Should().BeFalse();
        }
    }
}