using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using CycleManager.Tests.Integration.Helpers;
using Domain.Context;
using Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName;

    public CustomWebApplicationFactory()
    {
        _dbName = Guid.NewGuid().ToString();
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Verwijder bestaande DbContext registratie
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(ApplicationDbContext));

            // Voeg in-memory DbContext toe
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // verwijder echte ScraperService
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IScraperService));
            if (descriptor != null)
                services.Remove(descriptor);

            // voeg fake service toe
            services.AddTransient<IScraperService, FakeScraperService>();

            // Build service provider en seed database
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            db.ChangeTracker.Clear();

            SeedData(db);
            Console.WriteLine(db.Teams.Count());
        });
    }

    private void SeedData(ApplicationDbContext db)
    {
        Console.WriteLine($"Seeding InMemory DB: {db.Database.ProviderName}");
        // 1) Countries
        var countryBE = new Country { CountryNameShort = "be", CountryNameLong = "België" };
        var countryNL = new Country { CountryNameShort = "nl", CountryNameLong = "Nederland" };
        db.Countries.AddRange(countryBE, countryNL);
        db.SaveChanges();

        // 2) Team (zonder zelf IDs in te vullen — laat EF ze toewijzen)
        var team = new Team
        {
            TeamId = 1,
            CurrentTeamName = "OriginalTeam",
            PcsName = "PCS_Original",
            CountryId = countryBE.CountryId, // of gebruik navigation property team.Country = countryBE;
            TeamYears = new List<TeamYear>
        {
            new TeamYear { Year = 2025, Name = "Team2025" },
            new TeamYear { Year = 2026, Name = "Team2026" },
            new TeamYear { Year = 2027, Name = "Team2027" }
        }
        };
        db.Teams.Add(team);
        db.SaveChanges(); // belangrijk: team krijgt nu een TeamId

        // 3) Competitors
        var competitor1 = new Competitor { FirstName = "Rider", LastName = "One",  CountryId = countryBE.CountryId };
        var competitor2 = new Competitor { FirstName = "Rider", LastName = "Two",  CountryId = countryNL.CountryId };
        db.Competitors.AddRange(competitor1, competitor2);
        db.SaveChanges(); // competitors krijgen PKs

        // 4) CompetitorInTeam: koppel correct aan team en competitor
        // - let op: stel zowel FK-velden als navigations in (voor zekerheid)
        var cit1 = new CompetitorInTeam {  CompetitorId = competitor1.CompetitorId, TeamId = team.TeamId, Year = 2025, IsNationalChampion = false };
        var cit2 = new CompetitorInTeam {  CompetitorId = competitor2.CompetitorId, TeamId = team.TeamId, Year = 2025, IsNationalChampion = false
        };

        // Voeg toe aan DbSet én ook aan navigatiecollecties zodat in-memory virtueel geladen wordt
        db.CompetitorInTeams.AddRange(cit1, cit2);

        // Zorg dat team navigation reflecteert dat er items zijn
        team.CompetitorInTeams = team.CompetitorInTeams ?? new List<CompetitorInTeam>();
        team.CompetitorInTeams.Add(cit1);
        team.CompetitorInTeams.Add(cit2);

        // En koppel competitors -> competitorInTeams navigations (handig voor tests)
        competitor1.CompetitorInTeams = competitor1.CompetitorInTeams ?? new List<CompetitorInTeam>();
        competitor1.CompetitorInTeams.Add(cit1);
        competitor2.CompetitorInTeams = competitor2.CompetitorInTeams ?? new List<CompetitorInTeam>();
        competitor2.CompetitorInTeams.Add(cit2);

        db.SaveChanges();
        Console.WriteLine($"Seeded teams: {db.Teams.Count()}");
    }

    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }
}
