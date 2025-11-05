using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using Domain.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Tests.Integration.Helpers
{
    public class FakeScraperService : IScraperService
    {
        private readonly ApplicationDbContext _db;

        public FakeScraperService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task ImportScrapedCompetitorsAsync()
        {
            var scraped = await _db.ScrapedCompetitors
                .Where(sc => sc.ProcessedAt == null)
                .ToListAsync();

            foreach (var sc in scraped)
            {
                var competitor = new Competitor
                {
                    FirstName = sc.RiderName.Split(' ')[0],
                    LastName = sc.RiderName.Split(' ')[1],
                    ScraperName = sc.RiderName,
                    Country = await _db.Countries.FirstOrDefaultAsync(c => c.CountryNameShort == sc.CountryShortName)
                };

                _db.Competitors.Add(competitor);

                _db.CompetitorInTeams.Add(new CompetitorInTeam
                {
                    Competitor = competitor,
                    TeamId = sc.TeamId,
                    Year = sc.Year
                });

                sc.ProcessedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        public Task RunAsync(int eventId, string eventName, int year, int stageNumber)
        {
            throw new NotImplementedException();
        }

        public Task RunCompetitorsAsync(int teamId, int year)
        {
            if (year <= DateTime.Now.Year + 3)
            {
                var competitors = new[]
                {
                    new ScrapedCompetitor
                    {
                        TeamId = teamId,
                        Year = year,
                        RiderName = $"Rider One_{year}",
                        ImportedAt = DateTime.UtcNow,
                        CountryShortName = "be",
                        ProcessedAt = null
                    },
                    new ScrapedCompetitor
                    {
                        TeamId = teamId,
                        Year = year,
                        RiderName = $"Rider Two_{year}",
                        ImportedAt = DateTime.UtcNow,
                        CountryShortName = "be",
                        ProcessedAt = null
                    }
                };

                _db.ScrapedCompetitors.AddRange(competitors);
                _db.SaveChanges();
            }
            return Task.CompletedTask;
        }

        public Task RunDropoutsAsync(int eventId, string eventName, int year)
        {
            throw new NotImplementedException();
        }
    }
}
