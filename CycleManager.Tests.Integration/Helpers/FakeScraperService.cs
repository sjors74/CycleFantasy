using CycleManager.Domain.Dto;
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
                var country = await _db.Countries
                    .FirstOrDefaultAsync(c => c.CountryNameShort == sc.CountryShortName)
                    ?? throw new InvalidOperationException(
                        $"Country '{sc.CountryShortName}' niet gevonden.");

                var teamYear = await _db.TeamYear
                    .FirstOrDefaultAsync(ty =>
                        ty.TeamId == sc.TeamId &&
                        ty.Year == sc.Year)
                    ?? throw new InvalidOperationException(
                        $"TeamYear voor team {sc.TeamId}, jaar {sc.Year} niet gevonden.");

                var competitor = new Competitor
                {
                    FirstName = sc.RiderName.Split(' ')[0],
                    LastName = sc.RiderName.Split(' ')[1],
                    PcsScraperName = sc.RiderName,
                    Country = country
                };

                _db.Competitors.Add(competitor);

                _db.CompetitorInTeams.Add(new CompetitorInTeam
                {
                    Competitor = competitor, TeamYearId = teamYear.TeamYearId
                });

                sc.ProcessedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        public Task RefreshStartlistAsync(int eventId)
        {
            throw new NotImplementedException();
        }

        public Task RunAsync(int eventId, string eventName, int year, int stageNumber)
        {
            throw new NotImplementedException();
        }

        public async Task RunCompetitorsAsync(int teamYearId)
        {
            var teamYear = await _db.TeamYear
                .FirstOrDefaultAsync(ty => ty.TeamYearId == teamYearId);

            if (teamYear == null)
                throw new InvalidOperationException(
                    $"TeamYear '{teamYearId}' niet gevonden.");

            var competitors = new[]
            {
                new ScrapedCompetitor
                {
                    TeamId = teamYear.TeamId,
                    Year = teamYear.Year,
                    RiderName = $"Rider One_{teamYear.Year}",
                    ImportedAt = DateTime.UtcNow,
                    CountryShortName = "be",
                    ProcessedAt = null
                },
                new ScrapedCompetitor
                {
                    TeamId = teamYear.TeamId,
                    Year = teamYear.Year,
                    RiderName = $"Rider Two_{teamYear.Year}",
                    ImportedAt = DateTime.UtcNow,
                    CountryShortName = "be",
                    ProcessedAt = null
                }
            };

            _db.ScrapedCompetitors.AddRange(competitors);
            await _db.SaveChangesAsync();
        }

        public Task RunDropoutsAsync(int eventId, string eventName, int year)
        {
            throw new NotImplementedException();
        }

        public Task RunRatingCompetitorScrapeAsync(int competitorId)
        {
            throw new NotImplementedException();
        }

        public Task RunRatingsScrapeAsync()
        {
            throw new NotImplementedException();
        }

        public Task SyncStartlistAsync(int eventId, List<ScrapedStartlistEntry> scrapedEntries)
        {
            throw new NotImplementedException();
        }
    }
}
