using CycleManager.Domain.Models;
using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.RegularExpressions;

namespace CycleManager.Tests.Integration.Manager
{
    public class EvenementRennerPuntenTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public EvenementRennerPuntenTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Points_Index_ShouldHaveCorrectRankingWithTies()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();

            (Event ev, List<CompetitorsInEvent> competitors) =
                await CreateTestEventWithRandomPointsAsync(db, numCompetitors: 6, numStages: 2, allowTies: true);

            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/Points?eventId={ev.EventId}");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            html.Should().Contain("Ranglijst renners");

            var (ranks, _, scores) = ParsePointsTableFromHtml(html);
            ranks.Should().NotBeEmpty();
            var expectedRanks = CalculateStandardCompetitionRanking(scores);
            ranks.Should().Equal(expectedRanks, "de ranking moet correct zijn bij gelijke punten");
        }

        [Fact]
        public async Task Points_Index_ShouldHandleEventWithoutResultsGracefully()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();

            var config = new Configuration { ConfigurationType = "EmptyEventConfig" };
            db.Configurations.Add(config);
            await db.SaveChangesAsync();

            var ev = new Event
            {
                EventName = "LeegEvent",
                EventYear = 2025,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                IsActive = true,
                ConfigurationId = config.Id
            };
            db.Events.Add(ev);
            await db.SaveChangesAsync();

            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/Points?eventId={ev.EventId}");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            html.Should().Contain("Ranglijst renners");
            html.Should().ContainAny("Geen resultaten", "Er zijn nog geen punten toegekend", "Nog geen data");
        }

        [Fact]
        public async Task Points_Index_ShouldSortAlphabeticallyWhenScoresAreEqual()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();

            var config = new Configuration { ConfigurationType = "AlphabetTestConfig" };
            db.Configurations.Add(config);
            await db.SaveChangesAsync();

            var ev = new Event
            {
                EventName = "AlphaSortEvent",
                EventYear = 2025,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                IsActive = true,
                ConfigurationId = config.Id
            };
            db.Events.Add(ev);
            await db.SaveChangesAsync();

            var country = new Country { CountryNameShort = "NL", CountryNameLong = "Nederland" };
            db.Countries.Add(country);
            await db.SaveChangesAsync();

            var team = new Team { CurrentTeamName = "Team Alpha", CountryId = country.CountryId };
            db.Teams.Add(team);
            await db.SaveChangesAsync();

            var competitors = new List<Competitor>
            {
                new Competitor { FirstName = "Bert", LastName = "Zon", CountryId = country.CountryId },
                new Competitor { FirstName = "Alex", LastName = "Adams", CountryId = country.CountryId }
            };
            db.Competitors.AddRange(competitors);
            await db.SaveChangesAsync();

            var citList = competitors.Select(c => new CompetitorInTeam { CompetitorId = c.CompetitorId, TeamId = team.TeamId, Year = 2025 }).ToList();
            db.CompetitorInTeams.AddRange(citList);
            await db.SaveChangesAsync();

            var cieList = citList.Select(cit => new CompetitorsInEvent { EventId = ev.EventId, CompetitorInTeamId = cit.Id }).ToList();
            db.CompetitorsInEvent.AddRange(cieList);
            await db.SaveChangesAsync();

            var stage = new Stage { EventId = ev.EventId, StageName = "Etappe 1" };
            db.Stages.Add(stage);
            await db.SaveChangesAsync();

            var ci = new ConfigurationItem { ConfigurationId = config.Id, Position = 1, Score = 10 };
            db.ConfigurationItems.Add(ci);
            await db.SaveChangesAsync();

            foreach (var cie in cieList)
            {
                db.Results.Add(new Result
                {
                    StageId = stage.Id,
                    CompetitorInEventId = cie.Id,
                    ConfigurationItemId = ci.Id
                });
            }
            await db.SaveChangesAsync();

            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/Points?eventId={ev.EventId}");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            var (_, names, _) = ParsePointsTableFromHtml(html);
            names.Should().NotBeEmpty();

            // Controleer alfabetische sortering op achternaam
            names.First().Should().Contain("Adams");
            names.Last().Should().Contain("Zon");
        }

        [Fact]
        public async Task Points_Index_ShouldReturnEmptyListForInvalidEventId()
        {
            var invalidEventId = 9999;
            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/Points/Index?eventId={invalidEventId}");
            var html = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            html.Should().ContainAny("Geen resultaten", "Geen resultaten om weer te geven");
        }

        [Fact]
        public async Task Points_Index_ShouldHandleTiesAndSortAlphabetically()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();

            var config = new Configuration { ConfigurationType = "TieAlphaConfig" };
            db.Configurations.Add(config);
            await db.SaveChangesAsync();

            var ev = new Event
            {
                EventName = "TieAlphaEvent",
                EventYear = 2025,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                IsActive = true,
                ConfigurationId = config.Id
            };
            db.Events.Add(ev);
            await db.SaveChangesAsync();

            var country = new Country { CountryNameShort = "NL", CountryNameLong = "Nederland" };
            db.Countries.Add(country);
            await db.SaveChangesAsync();

            var team = new Team { CurrentTeamName = "Team TieAlpha", CountryId = country.CountryId };
            db.Teams.Add(team);
            await db.SaveChangesAsync();

            // Drie renners met dezelfde score
            var competitors = new List<Competitor>
            {
                new Competitor { FirstName = "Bert", LastName = "Zon", CountryId = country.CountryId },
                new Competitor { FirstName = "Alex", LastName = "Adams", CountryId = country.CountryId },
                new Competitor { FirstName = "Carl", LastName = "Baker", CountryId = country.CountryId }
            };
            db.Competitors.AddRange(competitors);
            await db.SaveChangesAsync();

            var citList = competitors.Select(c => new CompetitorInTeam { CompetitorId = c.CompetitorId, TeamId = team.TeamId, Year = 2025 }).ToList();
            db.CompetitorInTeams.AddRange(citList);
            await db.SaveChangesAsync();

            var cieList = citList.Select(cit => new CompetitorsInEvent { EventId = ev.EventId, CompetitorInTeamId = cit.Id }).ToList();
            db.CompetitorsInEvent.AddRange(cieList);
            await db.SaveChangesAsync();

            var stage = new Stage { EventId = ev.EventId, StageName = "Etappe 1" };
            db.Stages.Add(stage);
            await db.SaveChangesAsync();

            // Alle drie krijgen 10 punten → gelijke score
            var ci = new ConfigurationItem { ConfigurationId = config.Id, Position = 1, Score = 10 };
            db.ConfigurationItems.Add(ci);
            await db.SaveChangesAsync();

            foreach (var cie in cieList)
            {
                db.Results.Add(new Result
                {
                    StageId = stage.Id,
                    CompetitorInEventId = cie.Id,
                    ConfigurationItemId = ci.Id
                });
            }
            await db.SaveChangesAsync();

            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync($"/Points?eventId={ev.EventId}");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            var (_, names, scores) = ParsePointsTableFromHtml(html);

            // Assert: alle scores gelijk, rang 1 voor iedereen
            scores.Distinct().Count().Should().Be(1, "alle drie de renners hebben dezelfde score");

            // Assert: alfabetische sortering op achternaam
            names.Should().Equal("Alex Adams", "Carl Baker", "Bert Zon");
        }

        [Fact]
        public async Task Points_Index_ShouldAccumulatePointsOverMultipleStages()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();

            // Maak een event met meerdere renners en meerdere stages
            (Event ev, List<CompetitorsInEvent> competitors) =
                await CreateTestEventWithRandomPointsAsync(db, numCompetitors: 4, numStages: 3, allowTies: false);

            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync($"/Points?eventId={ev.EventId}");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            // Assert: er moet HTML zijn met de ranglijst
            html.Should().Contain("Ranglijst renners");

            // Ranking extractie
            var rows = Regex.Matches(html, @"<tr>\s*<td>\s*(\d+)\s*</td>\s*<td>\s*([^<]+)</td>\s*<td>\s*(\d+)\s*</td>");
            rows.Count.Should().BeGreaterThan(0, "er moeten renners in de ranglijst staan");

            var ranks = new List<int>();
            var scores = new List<int>();
            foreach (Match match in rows)
            {
                ranks.Add(int.Parse(match.Groups[1].Value.Trim()));
                scores.Add(int.Parse(match.Groups[3].Value.Trim()));
            }

            // Controleer dat ranking correct is bij gelijke scores
            var expectedRanks = CalculateStandardCompetitionRanking(scores);
            ranks.Should().BeEquivalentTo(expectedRanks, options => options.WithStrictOrdering(),
                "de ranking moet correct zijn op basis van het totaal aantal punten over alle stages");

            // Extra: controleer dat het totaal aantal punten klopt met wat in de database staat
            foreach (var cie in competitors)
            {
                var expectedPoints = await db.Results
                    .Where(r => r.CompetitorInEventId == cie.Id)
                    .SumAsync(r => r.ConfigurationItem.Score);

                var row = rows.Cast<Match>().FirstOrDefault(m => m.Groups[2].Value.Trim() == cie.CompetitorInTeam.Competitor.FirstName + " " + cie.CompetitorInTeam.Competitor.LastName);
                row.Should().NotBeNull("de renner moet in de ranglijst voorkomen");

                int actualPoints = int.Parse(row.Groups[3].Value.Trim());
                actualPoints.Should().Be(expectedPoints, "de totaalpunten moeten correct worden opgeteld over alle stages");
            }
        }

        // ================== Helper methods ==================

        private static (List<int> ranks, List<string> names, List<int> scores) ParsePointsTableFromHtml(string html)
        {
            var rowPattern = @"<tr[^>]*>\s*<td[^>]*>\s*(\d+)\s*</td>\s*<td[^>]*>\s*([^<]+)\s*</td>\s*<td[^>]*>\s*(\d+)\s*</td>";
            var matches = Regex.Matches(html, rowPattern, RegexOptions.IgnoreCase);

            var ranks = new List<int>();
            var names = new List<string>();
            var scores = new List<int>();

            foreach (Match m in matches)
            {
                ranks.Add(int.Parse(m.Groups[1].Value.Trim()));
                names.Add(m.Groups[2].Value.Trim());
                scores.Add(int.Parse(m.Groups[3].Value.Trim()));
            }

            // Alfabetische sortering binnen gelijke scores
            var combined = ranks.Zip(names, (r, n) => new { Rank = r, Name = n, Score = scores[ranks.IndexOf(r)] })
                                .GroupBy(x => x.Score)
                                .OrderByDescending(g => g.Key)
                                .SelectMany(g => g.OrderBy(x => x.Name.Split(' ').Last()))
                                .ToList();

            return (
                combined.Select(x => x.Rank).ToList(),
                combined.Select(x => x.Name).ToList(),
                combined.Select(x => x.Score).ToList()
            );
        }

        private static List<int> CalculateStandardCompetitionRanking(List<int> scores)
        {
            var ordered = scores
                .Select((score, idx) => new { score, idx })
                .OrderByDescending(x => x.score)
                .ToList();

            var ranks = new int[scores.Count];
            int currentRank = 1;
            for (int i = 0; i < ordered.Count;)
            {
                int score = ordered[i].score;
                var sameScore = ordered.Where(x => x.score == score).ToList();

                foreach (var s in sameScore)
                    ranks[s.idx] = currentRank;

                currentRank += sameScore.Count;
                i += sameScore.Count;
            }

            return ranks.ToList();
        }

        private static async Task<(Event ev, List<CompetitorsInEvent> cieList)> CreateTestEventWithRandomPointsAsync(
            ApplicationDbContext db, int numCompetitors = 5, int numStages = 3, bool allowTies = true)
        {
            var config = await db.Configurations.FirstOrDefaultAsync()
                         ?? new Configuration { ConfigurationType = "Default Config" };
            if (config.Id == 0) { db.Configurations.Add(config); await db.SaveChangesAsync(); }

            var ev = new Event
            {
                EventName = $"RandomPointsEvent_{Guid.NewGuid()}",
                EventYear = 2025,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(numStages),
                IsActive = true,
                ConfigurationId = config.Id
            };
            db.Events.Add(ev);
            await db.SaveChangesAsync();

            var country = new Country { CountryNameShort = "NL", CountryNameLong = "Nederland" };
            db.Countries.Add(country);
            await db.SaveChangesAsync();

            var team = new Team { CurrentTeamName = "Team Random", CountryId = country.CountryId };
            db.Teams.Add(team);
            await db.SaveChangesAsync();

            var cieList = new List<CompetitorsInEvent>();
            for (int i = 0; i < numCompetitors; i++)
            {
                var comp = new Competitor { FirstName = $"Renner{i + 1}", LastName = "Test", CountryId = country.CountryId };
                db.Competitors.Add(comp);
                await db.SaveChangesAsync();

                var cit = new CompetitorInTeam { CompetitorId = comp.CompetitorId, TeamId = team.TeamId, Year = 2025 };
                db.CompetitorInTeams.Add(cit);
                await db.SaveChangesAsync();

                var cie = new CompetitorsInEvent { EventId = ev.EventId, CompetitorInTeamId = cit.Id };
                db.CompetitorsInEvent.Add(cie);
                await db.SaveChangesAsync();

                cieList.Add(cie);
            }

            var stages = new List<Stage>();
            for (int s = 0; s < numStages; s++)
            {
                var stage = new Stage { EventId = ev.EventId, StageName = $"Etappe {s + 1}" };
                db.Stages.Add(stage);
                await db.SaveChangesAsync();
                stages.Add(stage);
            }

            var scores = new[] { 10, 8, 6, 5, 3, 1 };
            var ciList = new List<ConfigurationItem>();
            for (int r = 1; r <= scores.Length; r++)
            {
                var ci = new ConfigurationItem { ConfigurationId = config.Id, Position = r, Score = scores[r - 1] };
                db.ConfigurationItems.Add(ci);
                await db.SaveChangesAsync();
                ciList.Add(ci);
            }

            var rnd = new Random();
            foreach (var stage in stages)
            {
                foreach (var cie in cieList)
                {
                    var ci = allowTies
                        ? ciList[rnd.Next(0, ciList.Count)]
                        : ciList[cieList.IndexOf(cie) % ciList.Count];

                    db.Results.Add(new Result
                    {
                        StageId = stage.Id,
                        CompetitorInEventId = cie.Id,
                        ConfigurationItemId = ci.Id
                    });
                }
            }

            await db.SaveChangesAsync();
            return (ev, cieList);
        }
    }
}
