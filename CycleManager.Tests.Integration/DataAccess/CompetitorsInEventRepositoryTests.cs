using CycleManager.Domain.Models;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Tests.Integration.DataAccess
{
    public class CompetitorsInEventRepositoryTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private CompetitorsInEventRepository GetRepository(ApplicationDbContext context)
        {
            return new CompetitorsInEventRepository(context);
        }

        [Fact]
        public async Task GetById_ReturnsCompetitorInEvent_WhenExists()
        {
            using var context = GetInMemoryContext();

            // Arrange
            var competitor = new Competitor { CompetitorId = 1, FirstName = "John", LastName = "Doe", CountryId = 1 };
            var country = new Country { CountryId = 1, CountryNameShort = "NL" };
            context.Country.Add(country);
            context.Competitors.Add(competitor);

            var team = new Team { TeamId = 1, CurrentTeamName = "TeamA" };
            context.Teams.Add(team);

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                TeamId = 1,
                Year = 2025
            };
            context.CompetitorInTeams.Add(competitorInTeam);

            var cie = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 1,
                CompetitorInTeamId = 1,
                CompetitorInTeam = competitorInTeam
            };
            context.CompetitorsInEvent.Add(cie);

            await context.SaveChangesAsync();

            var repo = GetRepository(context);

            // Act
            var result = await repo.GetById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.NotNull(result.CompetitorInTeam);
            Assert.Equal("John", result.CompetitorInTeam.Competitor.FirstName);
        }

        [Fact]
        public async Task GetCompetitors_ReturnsAllForEvent()
        {
            using var context = GetInMemoryContext();

            // Arrange
            var country = new Country { CountryId = 1, CountryNameShort = "NL" };
            context.Country.Add(country);

            var competitor1 = new Competitor { CompetitorId = 1, FirstName = "John", LastName = "Doe", CountryId = 1, Country = country };
            var competitor2 = new Competitor { CompetitorId = 2, FirstName = "Jane", LastName = "Smith", CountryId = 1, Country = country };
            context.Competitors.AddRange(competitor1, competitor2);

            var team = new Team { TeamId = 1, CurrentTeamName = "TeamA" };
            context.Teams.Add(team);

            var cit1 = new CompetitorInTeam { Id = 1, CompetitorId = 1, TeamId = 1, Competitor = competitor1, Team = team, Year = 2025 };
            var cit2 = new CompetitorInTeam { Id = 2, CompetitorId = 2, TeamId = 1, Competitor = competitor2, Team = team, Year = 2025 };
            context.CompetitorInTeams.AddRange(cit1, cit2);

            var cie1 = new CompetitorsInEvent { Id = 1, EventId = 100, CompetitorInTeamId = 1, CompetitorInTeam = cit1 };
            var cie2 = new CompetitorsInEvent { Id = 2, EventId = 100, CompetitorInTeamId = 2, CompetitorInTeam = cit2 };
            context.CompetitorsInEvent.AddRange(cie1, cie2);

            await context.SaveChangesAsync();
            var repo = GetRepository(context);

            // Act
            var result = (await repo.GetCompetitors(100)).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.Id == 1);
            Assert.Contains(result, x => x.Id == 2);
        }

        [Fact]
        public void GetRandomElements_ReturnsCorrectNumberOfElements()
        {
            // Arrange
            var list = Enumerable.Range(1, 10).ToList();

            // Act
            var randomList = CompetitorsInEventRepository.GetRandomElements(list, 3);

            // Assert
            Assert.Equal(3, randomList.Count);
            Assert.All(randomList, item => Assert.Contains(item, list));
        }

        [Fact]
        public async Task GetCompetitorsInEventByIds_ReturnsCorrectEntry()
        {
            using var context = GetInMemoryContext();

            var country = new Country { CountryId = 1, CountryNameShort = "NL" };
            context.Country.Add(country);

            var competitor = new Competitor { CompetitorId = 1, FirstName = "John", LastName = "Doe", CountryId = 1, Country = country };
            context.Competitors.Add(competitor);

            var team = new Team { TeamId = 1, CurrentTeamName = "TeamA" };
            context.Teams.Add(team);

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                TeamId = 1,
                Competitor = competitor,
                Team = team,
                Year = 2025
            };
            context.CompetitorInTeams.Add(competitorInTeam);

            var cie = new CompetitorsInEvent
            {
                Id = 1,
                EventId = 50,
                CompetitorInTeamId = 1,
                CompetitorInTeam = competitorInTeam
            };
            context.CompetitorsInEvent.Add(cie);

            await context.SaveChangesAsync();
            var repo = GetRepository(context);

            // Act
            var result = await repo.GetCompetitorsInEventByIds(50, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(50, result.EventId);
            Assert.Equal(1, result.CompetitorInTeamId);
        }

        [Fact]
        public async Task GetRandomNumberofCompetitors_ReturnsDeterministicSubset()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "RandomTestDB")
                .Options;

            using var context = new ApplicationDbContext(options);

            var eventId = 100;

            // Voeg testdata toe
            var competitors = Enumerable.Range(1, 5).Select(i =>
                new Competitor { CompetitorId = i, FirstName = $"C{i}", LastName = $"L{i}", CountryId = 1 }
            ).ToList();

            var team = new Team { TeamId = 1, CurrentTeamName = "TeamA" };
            context.Teams.Add(team);

            foreach (var c in competitors)
            {
                context.Competitors.Add(c);
                var cit = new CompetitorInTeam { Id = c.CompetitorId, CompetitorId = c.CompetitorId, TeamId = 1, Team = team, Competitor = c, Year = 2025 };
                context.CompetitorInTeams.Add(cit);
                context.CompetitorsInEvent.Add(new CompetitorsInEvent { Id = c.CompetitorId, EventId = eventId, CompetitorInTeamId = cit.Id, CompetitorInTeam = cit });
            }

            await context.SaveChangesAsync();

            // Gebruik een deterministische "randomizer" (bijv. reverse order)
            Func<IEnumerable<CompetitorsInEvent>, IEnumerable<CompetitorsInEvent>> deterministicRandomizer =
                list => list.OrderByDescending(c => c.Id);

            var repo = new CompetitorsInEventRepository(context, deterministicRandomizer);

            // Act
            var result = (await repo.GetRandomNumberofCompetitors(eventId, 3)).ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(new[] { 5, 4, 3 }, result.Select(c => c.Id));
        }

    }
}
