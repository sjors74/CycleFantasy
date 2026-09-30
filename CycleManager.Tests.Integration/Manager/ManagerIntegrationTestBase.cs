using CycleManager.Domain.Models;
using Domain.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Tests.Integration.Manager
{
    public abstract class ManagerIntegrationTestBase
    {
        protected readonly CustomWebApplicationFactory _factory;
        protected readonly HttpClient _client;

        protected ManagerIntegrationTestBase()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        protected async Task<TeamYear> CreateTestTeamWithYearAsync(
            ApplicationDbContext db,
            int year,
            string teamName = "Test Team",
            string countryName = "Nederland")
        {
            var country = new Country
            {
                CountryNameLong = countryName,
                CountryNameShort = "NL"
            };

            db.Countries.Add(country);
            await db.SaveChangesAsync();

            var seasonYear = new SeasonYear
            {
                Year = year,
                Active = true
            };

            db.SeasonYears.Add(seasonYear);
            await db.SaveChangesAsync();

            var team = new Team
            {
                CurrentTeamName = teamName,
                CountryId = country.CountryId
            };

            db.Teams.Add(team);
            await db.SaveChangesAsync();

            var teamYear = new TeamYear
            {
                TeamId = team.TeamId,
                Team = team,
                SeasonYearId = seasonYear.SeasonYearId,
                SeasonYear = seasonYear,
                Year = year,
                Name = team.CurrentTeamName
            };

            db.TeamYear.Add(teamYear);
            await db.SaveChangesAsync();

            return teamYear;
        }

        protected async Task<Team> CreateTestTeamWithThreeYearsAsync(ApplicationDbContext db)
        {
            var teamYear2025 = await CreateTestTeamWithYearAsync(db, 2025);
            var team = teamYear2025.Team;

            foreach (var year in new[] { 2026, 2027 })
            {
                var seasonYear = new SeasonYear
                {
                    Year = year,
                    Active = true
                };

                db.SeasonYears.Add(seasonYear);
                await db.SaveChangesAsync();

                db.TeamYear.Add(new TeamYear
                {
                    TeamId = team.TeamId,
                    Team = team,
                    SeasonYearId = seasonYear.SeasonYearId,
                    SeasonYear = seasonYear,
                    Year = year,
                    Name = $"Team{year}"
                });

                await db.SaveChangesAsync();
            }

            return team;
        }

        protected async Task<Competitor> CreateTestCompetitorAsync(ApplicationDbContext db, string firstName = "Test", string lastName = "Competitor")
        {
            var country = await db.Countries.FirstOrDefaultAsync();

            if (country == null)
            {
                country = new Country
                {
                    CountryNameLong = "Nederland",
                    CountryNameShort = "NL"
                };

                db.Countries.Add(country);
                await db.SaveChangesAsync();
            }

            var competitor = new Competitor
            {
                FirstName = firstName,
                LastName = lastName,
                Country = country,
                CountryId = country.CountryId
            };

            db.Competitors.Add(competitor);
            await db.SaveChangesAsync();

            return competitor;
        }

        protected async Task<Competitor> CreateTestCompetitorWithTeamAsync(ApplicationDbContext db, int year, string firstName = "Test", string lastName = "Competitor")
        {
            var country = await db.Countries.FirstOrDefaultAsync();

            if (country == null)
            {
                country = new Country
                {
                    CountryNameLong = "Nederland",
                    CountryNameShort = "NL"
                };

                db.Countries.Add(country);
                await db.SaveChangesAsync();
            }

            var team = new Team
            {
                CurrentTeamName = "Test Team",
                CountryId = country.CountryId
            };

            db.Teams.Add(team);
            await db.SaveChangesAsync();

            var seasonYear = new SeasonYear
            {
                Year = year,
                Active = true
            };

            db.SeasonYears.Add(seasonYear);
            await db.SaveChangesAsync();

            var teamYear = new TeamYear
            {
                TeamId = team.TeamId,
                Team = team,
                Year = year,
                SeasonYearId = seasonYear.SeasonYearId,
                SeasonYear = seasonYear,
                Name = team.CurrentTeamName
            };

            db.TeamYear.Add(teamYear);
            await db.SaveChangesAsync();

            var competitor = new Competitor
            {
                FirstName = firstName,
                LastName = lastName,
                CountryId = country.CountryId
            };

            db.Competitors.Add(competitor);
            await db.SaveChangesAsync();

            var competitorInTeam = new CompetitorInTeam
            {
                CompetitorId = competitor.CompetitorId,
                Competitor = competitor,
                TeamYearId = teamYear.TeamYearId,
                TeamYear = teamYear
            };

            db.CompetitorInTeams.Add(competitorInTeam);
            await db.SaveChangesAsync();

            return competitor;
        }

        protected async Task<SeasonYear> CreateActiveSeasonYearAsync(ApplicationDbContext db, int year = 2026)
        {
            var seasonYear = await db.SeasonYears
                .FirstOrDefaultAsync(x => x.Active);

            if (seasonYear != null)
                return seasonYear;

            seasonYear = new SeasonYear
            {
                Year = year,
                Active = true
            };

            db.SeasonYears.Add(seasonYear);
            await db.SaveChangesAsync();

            return seasonYear;
        }

        protected static async Task<(Event ev, List<CompetitorsInEvent> cieList)> CreateTestEventWithRandomPointsAsync(
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

            var seasonYear = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2025,
                Active = true
            };

            db.SeasonYears.Add(seasonYear);
            await db.SaveChangesAsync();

            var teamYear = new TeamYear
            {
                TeamId = team.TeamId,
                Year = 2025,
                SeasonYearId = seasonYear.SeasonYearId,
                Name = "Team Random"
            };

            db.TeamYear.Add(teamYear);
            await db.SaveChangesAsync();


            var cieList = new List<CompetitorsInEvent>();
            for (int i = 0; i < numCompetitors; i++)
            {
                var comp = new Competitor { FirstName = $"Renner{i + 1}", LastName = "Test", CountryId = country.CountryId };
                db.Competitors.Add(comp);
                await db.SaveChangesAsync();

                var cit = new CompetitorInTeam { CompetitorId = comp.CompetitorId, TeamYear = teamYear, TeamYearId = teamYear.TeamYearId };
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
