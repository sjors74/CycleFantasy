using CycleManager.Domain.Interfaces;
using CycleManager.Domain.Models;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using CycleManager.Services.Settings;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Interfaces;
using Domain.Mapping;
using Humanizer.Localisation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using WebCycleManager.Config;
using WebCycleManager.Helpers;

var builder = WebApplication.CreateBuilder(args);


// Playwright setup
builder.Services.AddSingleton<IBrowser>(sp =>
{
    var playwright = Playwright.CreateAsync().GetAwaiter().GetResult();
    return playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
    {
        Headless = false,
        Args = new[]
        {
            "--disable-blink-features=AutomationControlled"
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
builder.Services.AddAutoMapper(typeof(DomainToResponseMappingProfile));
builder.Services.AddScoped<ICompetitorRepository, CompetitorRepository>();
builder.Services.AddScoped<ICompetitorInTeamRepository, CompetitorInTeamRepository>();
builder.Services.AddScoped<ICompetitorsInEventRepository, CompetitorsInEventRepository>();
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>(); 
builder.Services.AddScoped<IConfigurationItemRepository, ConfigurationItemRepository>();
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

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
