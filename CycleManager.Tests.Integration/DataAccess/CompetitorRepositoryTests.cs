using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleManager.Tests.Integration.DataAccess
{
    public class CompetitorRepositoryTests
    {
        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetAllCompetitors_ReturnsOnlyCompetitorsForGivenYear()
        {
            // Arrange
            using var context = CreateContext();

            var country = new Country { CountryId = 1, CountryNameShort = "BEL" };

            var team2024 = new Team { TeamId = 1, CurrentTeamName = "Soudal Quick-Step" };
            var team2023 = new Team { TeamId = 2, CurrentTeamName = "Jumbo-Visma" };

            var remco = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Remco",
                LastName = "Evenepoel",
                PcsName = "remcoev",
                Country = country
            };

            var wout = new Competitor
            {
                CompetitorId = 2,
                FirstName = "Wout",
                LastName = "van Aert",
                PcsName = "woutva",
                Country = country
            };

            context.Countries.Add(country);
            context.Teams.AddRange(team2024, team2023);

            context.Competitors.AddRange(remco, wout);

            context.CompetitorInTeams.AddRange(
                new CompetitorInTeam
                {
                    Id = 1,
                    CompetitorId = 1,
                    TeamId = 1,
                    Year = 2024,
                    Team = team2024
                },
                new CompetitorInTeam
                {
                    Id = 2,
                    CompetitorId = 2,
                    TeamId = 2,
                    Year = 2023,
                    Team = team2023
                });

            await context.SaveChangesAsync();

            var repo = new CompetitorRepository(context);

            // Act
            var result = await repo.GetAllCompetitors(2024);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result); // alleen Remco
            var remcoResult = result.First();
            Assert.Equal("Remco", remcoResult.FirstName);
            Assert.Equal("BEL", remcoResult.CountryShort);
            Assert.Single(remcoResult.Teams);
            Assert.Equal("Soudal Quick-Step", remcoResult.Teams.First().TeamName);
            Assert.Equal(2024, remcoResult.Teams.First().Year);
        }

        [Fact]
        public async Task GetAllCompetitors_ReturnsEmptyList_WhenNoCompetitorsForYear()
        {
            // Arrange
            using var context = CreateContext();

            var country = new Country { CountryId = 1, CountryNameShort = "NED" };
            var team = new Team { TeamId = 1, CurrentTeamName = "Visma-Lease a Bike" };
            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Wout",
                LastName = "Poels",
                Country = country
            };

            context.AddRange(country, team, competitor);

            // CompetitorInTeam in 2023, niet in 2024
            context.CompetitorInTeams.Add(new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                TeamId = 1,
                Year = 2023,
                Team = team
            });

            await context.SaveChangesAsync();

            var repo = new CompetitorRepository(context);

            // Act
            var result = await repo.GetAllCompetitors(2024);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllCompetitors_IncludesTeamAndCountryData()
        {
            // Arrange
            using var context = CreateContext();

            var country = new Country { CountryId = 1, CountryNameShort = "FRA" };
            var team = new Team { TeamId = 1, CurrentTeamName = "Groupama-FDJ" };
            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Thibaut",
                LastName = "Pinot",
                Country = country
            };

            var cit = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                TeamId = 1,
                Year = 2024,
                Team = team
            };

            context.AddRange(country, team, competitor, cit);
            await context.SaveChangesAsync();

            var repo = new CompetitorRepository(context);

            // Act
            var result = await repo.GetAllCompetitors(2024);

            // Assert
            var pinot = Assert.Single(result);
            Assert.Equal("FRA", pinot.CountryShort);
            Assert.Single(pinot.Teams);
            Assert.Equal("Groupama-FDJ", pinot.Teams.First().TeamName);
        }

        [Fact]
        public async Task GetById_ReturnsCompetitorWithRelatedData()
        {
            // Arrange
            using var context = CreateContext();

            var country = new Country { CountryId = 1, CountryNameShort = "ITA" };
            var team1 = new Team { TeamId = 1, CurrentTeamName = "Team Ineos" };
            var team2 = new Team { TeamId = 2, CurrentTeamName = "Astana Qazaqstan" };

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Filippo",
                LastName = "Ganna",
                Country = country
            };

            var cit1 = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = 1,
                TeamId = 1,
                Year = 2024,
                Team = team1
            };

            var cit2 = new CompetitorInTeam
            {
                Id = 2,
                CompetitorId = 1,
                TeamId = 2,
                Year = 2023,
                Team = team2
            };

            context.AddRange(country, team1, team2, competitor, cit1, cit2);
            await context.SaveChangesAsync();

            var repo = new CompetitorRepository(context);

            // Act
            var result = await repo.GetById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Filippo", result.FirstName);
            Assert.Equal("Ganna", result.LastName);
            Assert.NotNull(result.Country);
            Assert.Equal("ITA", result.Country.CountryNameShort);

            Assert.NotNull(result.CompetitorInTeams);
            Assert.Equal(2, result.CompetitorInTeams.Count);

            var teamIds = result.CompetitorInTeams.Select(cit => cit.TeamId).ToList();
            Assert.Contains(1, teamIds);
            Assert.Contains(2, teamIds);

            // Controleer dat Team-navigatie geladen is
            var teamNames = result.CompetitorInTeams.Select(cit => cit.Team.CurrentTeamName).ToList();
            Assert.Contains("Team Ineos", teamNames);
            Assert.Contains("Astana Qazaqstan", teamNames);
        }

        [Fact]
        public async Task GetByTeamId_ReturnsCorrectCompetitors()
        {
            // Arrange
            using var context = CreateContext();

            var country1 = new Country { CountryId = 1, CountryNameShort = "ITA" };
            var country2 = new Country { CountryId = 2, CountryNameShort = "BEL" };

            var team1 = new Team { TeamId = 1, CurrentTeamName = "Team Ineos" };
            var team2 = new Team { TeamId = 2, CurrentTeamName = "QuickStep" };

            var competitor1 = new Competitor { CompetitorId = 1, FirstName = "Filippo", LastName = "Ganna",  Country = country1 };
            var competitor2 = new Competitor { CompetitorId = 2, FirstName = "Remco", LastName = "Evenepoel", Country = country2 };

            var cit1 = new CompetitorInTeam { Id = 1, CompetitorId = 1, TeamId = 1, Year = 2024, Competitor = competitor1, Team = team1 };
            var cit2 = new CompetitorInTeam { Id = 2, CompetitorId = 2, TeamId = 1, Year = 2024, Competitor = competitor2, Team = team1 };
            var cit3 = new CompetitorInTeam { Id = 3, CompetitorId = 2, TeamId = 2, Year = 2024, Competitor = competitor2, Team = team2 }; // andere team

            context.AddRange(country1, country2, team1, team2, competitor1, competitor2, cit1, cit2, cit3);
            await context.SaveChangesAsync();

            var repo = new CompetitorRepository(context);

            // Act
            var result = await repo.GetByTeamId(1, 2024);

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(2, list.Count);

            var competitorNames = list.Select(c => c.CompetitorName).ToList();
            Assert.Contains("Filippo Ganna", competitorNames);
            Assert.Contains("Remco Evenepoel", competitorNames);

            foreach (var cit in list)
            {
                Assert.Equal(1, cit.TeamId);
                Assert.Equal("Team Ineos", cit.TeamName);
                Assert.Equal(2024, cit.Year);
            }
        }

        [Fact]
        public async Task GetCompetitorsByCountry_ReturnsCorrectCount()
        {
            // Arrange
            using var context = CreateContext();

            var country1 = new Country { CountryId = 1, CountryNameShort = "ITA" };
            var country2 = new Country { CountryId = 2, CountryNameShort = "BEL" };

            var competitor1 = new Competitor { CompetitorId = 1, FirstName = "Filippo", LastName = "Ganna", Country = country1, CountryId = country1.CountryId };
            var competitor2 = new Competitor { CompetitorId = 2, FirstName = "Vincenzo", LastName = "Nibali", Country = country1, CountryId = country1.CountryId };
            var competitor3 = new Competitor { CompetitorId = 3, FirstName = "Remco", LastName = "Evenepoel", Country = country2, CountryId = country2.CountryId };

            context.AddRange(country1, country2, competitor1, competitor2, competitor3);
            await context.SaveChangesAsync();

            var repo = new CompetitorRepository(context);

            // Act
            var countItaly = await repo.GetCompetitorsByCountry(1);
            var countBelgium = await repo.GetCompetitorsByCountry(2);
            var countNonExisting = await repo.GetCompetitorsByCountry(999);

            // Assert
            Assert.Equal(2, countItaly);
            Assert.Equal(1, countBelgium);
            Assert.Equal(0, countNonExisting);
        }

        [Fact]
        public async Task GetAvailableYears_ReturnsDistinctYearsInDescendingOrder()
        {
            // Arrange
            using var context = CreateContext();

            var team1 = new Team { TeamId = 1, CurrentTeamName = "Team A" };
            var team2 = new Team { TeamId = 2, CurrentTeamName = "Team B" };

            var competitor1 = new Competitor { CompetitorId = 1, FirstName = "Alice", LastName = "Smith" };
            var competitor2 = new Competitor { CompetitorId = 2, FirstName = "Bob", LastName = "Johnson" };

            var cit1 = new CompetitorInTeam { Id = 1, Competitor = competitor1, Team = team1, Year = 2022 };
            var cit2 = new CompetitorInTeam { Id = 2, Competitor = competitor1, Team = team1, Year = 2023 };
            var cit3 = new CompetitorInTeam { Id = 3, Competitor = competitor2, Team = team2, Year = 2023 };
            var cit4 = new CompetitorInTeam { Id = 4, Competitor = competitor2, Team = team2, Year = 2021 };

            context.AddRange(team1, team2, competitor1, competitor2, cit1, cit2, cit3, cit4);
            await context.SaveChangesAsync();

            var repo = new CompetitorRepository(context);

            // Act
            var years = await repo.GetAvailableYears();

            // Assert
            var expectedYears = new List<int> { 2023, 2022, 2021 };
            Assert.Equal(expectedYears, years);
        }

        [Fact]
        public async Task GetCompetitorByName_ReturnsCompetitor_WhenExists()
        {
            // Arrange
            using var context = CreateContext();

            var country = new Country { CountryId = 1, CountryNameShort = "NL" };
            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Alice",
                LastName = "Smith",
                Country = country,
                CountryId = country.CountryId
            };

            context.AddRange(country, competitor);
            await context.SaveChangesAsync();

            var repo = new CompetitorRepository(context);

            // Act
            var result = await repo.GetCompetitorByName("Alice", "Smith", country.CountryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Alice", result!.FirstName);
            Assert.Equal("Smith", result.LastName);
            Assert.Equal(country.CountryId, result.CountryId);
        }

        [Fact]
        public async Task GetCompetitorByName_ReturnsNull_WhenNotExists()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new CompetitorRepository(context);

            // Act
            var result = await repo.GetCompetitorByName("Nonexistent", "User", 999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCompetitorsByTerm_ReturnsMatchingCompetitors()
        {
            // Arrange
            using var context = CreateContext();

            var country = new Country { CountryId = 1, CountryNameShort = "NL" };

            var competitor1 = new Competitor { CompetitorId = 1, FirstName = "Alice", LastName = "Smith", CountryId = 1, Country = country };
            var competitor2 = new Competitor { CompetitorId = 2, FirstName = "Bob", LastName = "Johnson", CountryId = 1, Country = country };
            var competitor3 = new Competitor { CompetitorId = 3, FirstName = "Charlie", LastName = "Smith", CountryId = 1, Country = country };

            context.AddRange(country, competitor1, competitor2, competitor3);
            await context.SaveChangesAsync();

            var repo = new CompetitorRepository(context);

            // Act
            var results = repo.GetCompetitorsByTerm("Smith").ToList();

            // Assert
            Assert.Equal(2, results.Count); // Alice Smith & Charlie Smith
            Assert.Contains(results, c => c.FirstName == "Alice" && c.LastName == "Smith");
            Assert.Contains(results, c => c.FirstName == "Charlie" && c.LastName == "Smith");
        }

        [Fact]
        public async Task GetCompetitorsByTerm_ReturnsEmpty_WhenNoMatch()
        {
            // Arrange
            using var context = CreateContext();

            var country = new Country { CountryId = 1, CountryNameShort = "NL" };
            context.Add(country);
            await context.SaveChangesAsync();

            var repo = new CompetitorRepository(context);

            // Act
            var results = repo.GetCompetitorsByTerm("Nonexistent").ToList();

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public async Task GetCompetitorsByTerm_TakesMaximum20Results()
        {
            // Arrange
            using var context = CreateContext();

            var country = new Country { CountryId = 1, CountryNameShort = "NL" };
            context.Add(country);

            // Voeg 25 competitors toe met "Test" in de naam
            for (int i = 1; i <= 25; i++)
            {
                context.Add(new Competitor
                {
                    CompetitorId = i,
                    FirstName = $"Test{i}",
                    LastName = $"User{i}",
                    CountryId = 1,
                    Country = country
                });
            }

            await context.SaveChangesAsync();

            var repo = new CompetitorRepository(context);

            // Act
            var results = repo.GetCompetitorsByTerm("Test").ToList();

            // Assert
            Assert.Equal(20, results.Count); // Check dat er maximaal 20 terugkomt
            Assert.All(results, c => Assert.Contains("Test", c.FirstName));
        }

        [Fact]
        public async Task GetCompetitorsByTerm_OrdersByLastName()
        {
            // Arrange
            using var context = CreateContext();

            var country = new Country { CountryId = 1, CountryNameShort = "NL" };
            context.Add(country);

            // Voeg 5 competitors toe met willekeurige lastnames
            var competitors = new List<Competitor>
            {
                new Competitor { CompetitorId = 1, FirstName = "Alice", LastName = "Zimmer", CountryId = 1, Country = country },
                new Competitor { CompetitorId = 2, FirstName = "Bob", LastName = "Anderson", CountryId = 1, Country = country },
                new Competitor { CompetitorId = 3, FirstName = "Charlie", LastName = "Brown", CountryId = 1, Country = country },
                new Competitor { CompetitorId = 4, FirstName = "David", LastName = "Clark", CountryId = 1, Country = country },
                new Competitor { CompetitorId = 5, FirstName = "Eve", LastName = "Davis", CountryId = 1, Country = country }
            };

            context.AddRange(competitors);
            await context.SaveChangesAsync();

            var repo = new CompetitorRepository(context);

            // Act
            var results = repo.GetCompetitorsByTerm("").ToList(); // lege term = alles

            // Assert
            var expectedOrder = competitors.OrderBy(c => c.LastName).Select(c => c.LastName).ToList();
            var actualOrder = results.Select(c => c.LastName).ToList();

            Assert.Equal(expectedOrder, actualOrder); // Controleer dat de volgorde klopt
        }

        [Fact]
        public async Task UpdateCompetitorWithTeam_UpdatesExistingLink_WhenYearExists()
        {
            using var context = CreateContext();
            var repo = new CompetitorRepository(context);

            SeedData(context);

            // Arrange
            var dto = new CompetitorEditDto
            {
                CompetitorId = 10,
                FirstName = "Remco",
                LastName = "Evenepoel",
                CountryId = 1,
                PcsName = "REvenepoel",
                ScraperName = "evenepoel",
                SelectedTeamId = 2, // wijziging naar Jumbo-Visma
                SelectedYear = 2024
            };

            // Act
            await repo.UpdateCompetitorWithTeam(dto);

            // Assert
            var updated = await context.CompetitorInTeams.FirstOrDefaultAsync(c => c.CompetitorId == 10 && c.Year == 2024);
            Assert.NotNull(updated);
            Assert.Equal(2, updated.TeamId); // team is bijgewerkt
        }

        [Fact]
        public async Task UpdateCompetitorWithTeam_AddsNewLink_WhenYearNotExists()
        {
            using var context = CreateContext();
            var repo = new CompetitorRepository(context);

            SeedData(context);

            // Arrange
            var dto = new CompetitorEditDto
            {
                CompetitorId = 10,
                FirstName = "Remco",
                LastName = "Evenepoel",
                CountryId = 1,
                SelectedTeamId = 1,
                SelectedYear = 2025 // nieuw jaar
            };

            // Act
            await repo.UpdateCompetitorWithTeam(dto);

            // Assert
            var newLink = await context.CompetitorInTeams
                .FirstOrDefaultAsync(c => c.CompetitorId == 10 && c.Year == 2025);

            Assert.NotNull(newLink);
            Assert.Equal(1, newLink.TeamId);
        }

        [Fact]
        public async Task UpdateCompetitorWithTeam_Throws_WhenCompetitorNotFound()
        {
            using var context = CreateContext();
            var repo = new CompetitorRepository(context);

            SeedData(context);

            // Arrange
            var dto = new CompetitorEditDto
            {
                CompetitorId = 999,
                FirstName = "Fake",
                LastName = "Rider",
                SelectedTeamId = 1,
                SelectedYear = 2024
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.UpdateCompetitorWithTeam(dto));
        }

        [Fact]
        public async Task GetByIdWithTeamsAsync_ReturnsCompetitor_WithRelatedTeams()
        {
            // Arrange
            using var context = CreateContext();

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jonas",
                LastName = "Vingegaard",
                CompetitorInTeams = new List<CompetitorInTeam>
                {
                    new CompetitorInTeam { Id = 10, TeamId = 100, Year = 2024 },
                    new CompetitorInTeam { Id = 11, TeamId = 101, Year = 2025 }
                }
            };

            context.Competitors.Add(competitor);
            await context.SaveChangesAsync();

            var repository = new CompetitorRepository(context);

            // Act
            var result = await repository.GetByIdWithTeamsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.CompetitorId);
            Assert.NotNull(result.CompetitorInTeams);
            Assert.Equal(2, result.CompetitorInTeams.Count);

            var teamIds = result.CompetitorInTeams.Select(cit => cit.TeamId).ToList();
            Assert.Contains(100, teamIds);
            Assert.Contains(101, teamIds);
        }

        [Fact]
        public async Task GetByIdWithTeamsAsync_ReturnsNull_WhenCompetitorDoesNotExist()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new CompetitorRepository(context);

            // Act
            var result = await repository.GetByIdWithTeamsAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateCompetitorAsync_UpdatesExistingCompetitorAndTeams()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new CompetitorRepository(context);

            // Voeg eerst een Competitor toe met een gekoppeld team
            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Remco",
                LastName = "Evenepoel",
                CompetitorInTeams = new List<CompetitorInTeam>
                {
                    new CompetitorInTeam { TeamId = 100, Year = 2023, IsNationalChampion = false }
                }
            };

            context.Competitors.Add(competitor);
            await context.SaveChangesAsync();

            // Haal opnieuw op vanuit database (tracked instance)
            var existing = await repository.GetByIdWithTeamsAsync(1);
            Assert.NotNull(existing);
            Assert.Single(existing.CompetitorInTeams);

            // Wijzig de data van bestaande entities
            existing.FirstName = "Wout";
            existing.LastName = "van Aert";
            existing.CompetitorInTeams.First().IsNationalChampion = true;

            // Voeg er een tweede team aan toe
            existing.CompetitorInTeams.Add(new CompetitorInTeam
            {
                TeamId = 101,
                Year = 2024,
                IsNationalChampion = false
            });

            // Act
            await repository.UpdateCompetitorAsync(existing);

            // Assert
            var updated = await repository.GetByIdWithTeamsAsync(1);
            Assert.NotNull(updated);
            Assert.Equal("Wout", updated.FirstName);
            Assert.Equal("van Aert", updated.LastName);
            Assert.Equal(2, updated.CompetitorInTeams.Count);

            var firstTeam = updated.CompetitorInTeams.First();
            Assert.True(firstTeam.IsNationalChampion);
        }

        [Fact]
        public async Task UpdateCompetitorAsync_DoesNothing_WhenCompetitorDoesNotExist()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new CompetitorRepository(context);

            var competitor = new Competitor
            {
                CompetitorId = 999,
                FirstName = "Jonas",
                LastName = "Vingegaard",
                CompetitorInTeams = new List<CompetitorInTeam>
                {
                    new CompetitorInTeam { Id = 20, TeamId = 200, Year = 2025 }
                }
            };

            // Act
            await repository.UpdateCompetitorAsync(competitor);

            // Assert
            var count = await context.Competitors.CountAsync();
            Assert.Equal(0, count); // geen nieuwe toegevoegd
        }


        private void SeedData(ApplicationDbContext context)
        {
            var country = new Country { CountryId = 1, CountryNameShort = "BEL" };
            var teamA = new Team { TeamId = 1, CurrentTeamName = "Soudal Quick-Step" };
            var teamB = new Team { TeamId = 2, CurrentTeamName = "Jumbo-Visma" };

            var competitor = new Competitor
            {
                CompetitorId = 10,
                FirstName = "Remco",
                LastName = "Evenepoel",
                CountryId = 1,
                Country = country,
                CompetitorInTeams = new List<CompetitorInTeam>
                {
                    new CompetitorInTeam { Id = 100, CompetitorId = 10, TeamId = 1, Year = 2024 }
                }
            };

            context.Countries.Add(country);
            context.Teams.AddRange(teamA, teamB);
            context.Competitors.Add(competitor);
            context.SaveChanges();
        }
    }
}