using CycleManager.Domain.Interfaces;
using CycleManager.Domain.Models;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using CycleManager.Services.Settings;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Interfaces;
using Domain.Mapping;
using Hangfire;
using Hangfire.Dashboard;
using Humanizer.Localisation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using WebCycleManager.Config;
using WebCycleManager.Helpers;

Environment.SetEnvironmentVariable(
    "PLAYWRIGHT_BROWSERS_PATH",
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ms-playwright")
);


var builder = WebApplication.CreateBuilder(args);


// Playwright setup
builder.Services.AddSingleton<IBrowser>(sp =>
{
    var playwright = Playwright.CreateAsync()
        .GetAwaiter()
        .GetResult();

    string? chromePath = null;

    if (Directory.Exists("/ms-playwright"))
    {
        chromePath = Directory
            .GetFiles("/ms-playwright", "chrome", SearchOption.AllDirectories)
            .FirstOrDefault();
    }

    return playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
    {
        Headless = false,
        ExecutablePath = chromePath, // null lokaal = default Playwright browser
        Args = new[]
        {
            "--no-sandbox",
            "--disable-dev-shm-usage"
        }
    }).GetAwaiter().GetResult();
});

var connectionString = builder.Configuration.GetConnectionString("CycleDb");
builder.Services.AddDbContext<ApplicationDbContext>(x => 
    x.UseLazyLoadingProxies(false)
     .UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<DomainToResponseMappingProfile>();
});
builder.Services.AddScoped<ICompetitorRepository, CompetitorRepository>();
builder.Services.AddScoped<ICompetitorInTeamRepository, CompetitorInTeamRepository>();
builder.Services.AddScoped<ICompetitorsInEventRepository, CompetitorsInEventRepository>();
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>(); 
builder.Services.AddScoped<IConfigurationItemRepository, ConfigurationItemRepository>();
builder.Services.AddScoped<IConfigurationItemSpecialRepository, ConfigurationItemSpecialRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IResultsRepository, ResultsRepository>();
builder.Services.AddScoped<IStageRepository, StageRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IGameCompetitorEventPickRepository, GameCompetitorEventPickRepository>();
builder.Services.AddScoped<IGameCompetitorInEventRepository, GameCompetitorInEventRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<INewsItemRepository, NewsItemRepository>();
//Service
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IStageService, StageService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<ICompetitorService, CompetitorService>();
builder.Services.AddScoped<ICompetitorInEventService, CompetitorInEventService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IGameCompetitorInEventService, GameCompetitorInEventService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IScoreRepository, ScoreRepository>();
builder.Services.AddScoped<IScraperService, ScraperService>();
builder.Services.AddScoped<IPcsScraper, PcsScraper>();
builder.Services.AddScoped<IScoreService, ScoreService>();
builder.Services.AddScoped<IAdminScraperService, AdminScraperService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<IScrapeScheduleService, EventScrapeJobRegistrationService>();
builder.Services.AddScoped<IEventScrapeSchedulerService, EventScrapeSchedulerService>();
builder.Services.AddScoped<IScrapeOrchestratorService, ScrapeOrchestratorService>();
builder.Services.AddScoped<IDropoutOrchestratorService, DropoutOrchestratorService>();

builder.Services.AddControllersWithViews()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(WebCycleManager.Resources.SharedResources));
    });
builder.Services.Configure<ApiSettings>(
builder.Configuration.GetSection("ApiSettings"));
builder.Services.Configure<ScraperSettings>(builder.Configuration.GetSection("ScraperSettings"));
builder.Services.AddLogging();
// Voeg HttpClient toe en gebruik BaseUrl uit config
builder.Services.AddHttpClient<IApiClient, ApiClient>((serviceProvider, client) =>
{
    var apiSettings = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(apiSettings.BaseUrl);
});

builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(
        builder.Configuration.GetConnectionString("CycleDb")));

builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AllowAllDashboardAuthorizationFilter() }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var scheduler = scope.ServiceProvider
        .GetRequiredService<IScrapeScheduleService>();

    await scheduler.RegisterSchedulesAsync();
}

RecurringJob.AddOrUpdate<IScrapeScheduleService>(
    "job-registration",
    x => x.RegisterSchedulesAsync(),
    Cron.Hourly);


app.Run();

app.MapGet("/competitors", async (IPcsScraper scraper, string team, int teamId, int year) =>
{
    // Bouw de URL dynamisch
    var url = $"https://www.procyclingstats.com/team/{team}-{year}";

    try
    {
        var competitors = await scraper.ScrapeCompetitorsAsync(url, teamId, year);
        return Results.Ok(competitors);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Scrapen mislukt: {ex.Message}");
    }
});

// Alleen nodig voor integratietests
public partial class Program { }

public class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}