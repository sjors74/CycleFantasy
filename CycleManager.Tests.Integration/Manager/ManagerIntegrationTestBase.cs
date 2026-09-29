using CycleManager.Domain.Models;
using Domain.Context;
using Domain.Models;

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
    }
}
