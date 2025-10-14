using CycleManager.Domain.Models;
using Domain.Context;
using Domain.Models;

namespace WebCycle.Services
{
    public static class TestDataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db, IWebHostEnvironment env)
        {
            // Alleen in Test-omgeving
            if (!env.IsEnvironment("Test"))
                return;

            Console.WriteLine("[Seeder] Resetting and seeding test database...");

            // Volledige reset van de database (werkt ook met InMemory en SQLite)
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            // Landen
            var nl = new Country { CountryId = 1, CountryNameLong = "Nederland", CountryNameShort = "nl" };
            var be = new Country { CountryId = 2, CountryNameLong = "België", CountryNameShort = "be" };
            await db.Countries.AddRangeAsync(nl, be);

            // Deelnemers en teams
            var rider1 = new Competitor { FirstName = "Jan", LastName = "Fietser", CountryId = 1 };
            var rider2 = new Competitor { FirstName = "Piet", LastName = "Peloton", CountryId = 2 };
            var teamYear25 = new TeamYear { TeamId = 1, TeamYearId = 1, Name = "TstTeam", Year = 2025 };
            var cit1 = new CompetitorInTeam { Competitor = rider1, TeamYear = teamYear25 };
            var cit2 = new CompetitorInTeam { Competitor = rider2, TeamYear = teamYear25 };

            // Evenementen
            var event1 = new Event
            {
                EventId = 1,
                EventName = "Tour de Test",
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(15),
                IsActive = true,
                CountryCode = "nl",
                Slogan = "test test test",
                Stages = new List<Stage>
                {
                    new Stage
                    {
                        Id = 3,
                        StageDate = DateTime.UtcNow.AddDays(1),
                        StageName = "Proloog",
                        StageOrder = 1,
                        StartLocation = "Startville",
                        FinishLocation = "Finishburg"
                    }
                }
            };

            var event2 = new Event
            {
                EventId = 2,
                EventName = "Cycle Classic",
                StartDate = DateTime.Now.AddDays(3),
                EndDate = DateTime.Now.AddDays(10),
                IsActive = true,
                CountryCode = "it",
                Slogan = "viva italia",
                Stages = new List<Stage>
                {
                    new Stage
                    {
                        Id = 1,
                        StageDate = DateTime.UtcNow.AddDays(1),
                        StageName = "Proloog",
                        StageOrder = 1,
                        StartLocation = "Startville",
                        FinishLocation = "Finishburg"
                    },
                    new Stage
                    {
                        Id = 2,
                        StageDate = DateTime.UtcNow.AddDays(2),
                        StageName = "1",
                        StageOrder = 2,
                        StartLocation = "Finishburg",
                        FinishLocation = "SomewhereElst"
                    }
                }
            };

            await db.Events.AddRangeAsync(event1, event2);

            // Competitors koppelen aan event
            var cie1 = new CompetitorsInEvent { Event = event1, CompetitorInTeam = cit1 };
            var cie2 = new CompetitorsInEvent { Event = event1, CompetitorInTeam = cit2 };

            // Configuratie en resultaten
            var config = new Configuration { Id = 1, ConfigurationType = "langEvent" };
            var configItem1 = new ConfigurationItem { Id = 1, Configuration = config, Position = 1, Score = 50 };
            var configItem2 = new ConfigurationItem { Id = 2, Configuration = config, Position = 2, Score = 30 };

            var result = new Result
            {
                Id = 1,
                StageId = 3,
                ConfigurationItem = configItem1,
                CompetitorInEvent = cie1
            };

            await db.AddRangeAsync(rider1, rider2, teamYear25, cit1, cit2, cie1, cie2, config, configItem1, configItem2, result);

            await db.SaveChangesAsync();

            Console.WriteLine("[Seeder] Test database ready!");
        }
    }
}
