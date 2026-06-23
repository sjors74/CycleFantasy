using CycleManager.Domain.Interfaces;
using CycleManager.Domain.Models;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using CycleManager.Services.Settings;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Playwright;
using System.Text;
using WebCycle.Services;


var builder = WebApplication.CreateBuilder(args);

// -------------------
// Configuration
// -------------------
var connectionString = builder.Configuration.GetConnectionString("CycleDb");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7089", "https://webapp20250508172648.azurewebsites.net")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));

// -------------------
// Database
// -------------------

if (builder.Environment.IsEnvironment("Test"))
{
    // In-memory DB voor tests
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("TestDb", TestDb.Root));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseLazyLoadingProxies(false)
               .UseSqlServer(connectionString, sqlOptions =>
               {
                   sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                   sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
               }));
}


//Playwright setup
builder.Services.AddSingleton<IBrowser>(sp =>
{
    var playwright = Playwright.CreateAsync()
        .GetAwaiter()
        .GetResult();

    return playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
    {
        Headless = true
    }).GetAwaiter().GetResult();
});

// -------------------
// Identity & DI
// -------------------
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Repositories & Services
builder.Services.AddTransient<ITeamRepository, TeamRepository>();
builder.Services.AddTransient<ICompetitorRepository, CompetitorRepository>();
builder.Services.AddTransient<ICompetitorsInEventRepository, CompetitorsInEventRepository>();
builder.Services.AddScoped<ICompetitorInTeamRepository, CompetitorInTeamRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddTransient<IEventRepository, EventRepository>();
builder.Services.AddTransient<IResultsRepository, ResultsRepository>();
builder.Services.AddTransient<IScoreRepository, ScoreRepository>();
builder.Services.AddTransient<IStageRepository, StageRepository>();
builder.Services.AddTransient<IGameCompetitorInEventRepository, GameCompetitorInEventRepository>();
builder.Services.AddScoped<IGameCompetitorInEventService, GameCompetitorInEventService>();
builder.Services.AddTransient<IGameCompetitorEventPickRepository, GameCompetitorEventPickRepository>();
builder.Services.AddTransient<IResultService, ResultService>();
builder.Services.AddTransient<IEventService, EventService>();
builder.Services.AddScoped<ICompetitorService, CompetitorService>();
builder.Services.AddTransient<ICompetitorInEventService, CompetitorInEventService>();
builder.Services.AddTransient<ITeamService, TeamService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddTransient<INewsItemRepository, NewsItemRepository>();
builder.Services.AddTransient<INewsService, NewsService>();
builder.Services.AddScoped<IEventDashboardService, EventDashboardService>();
builder.Services.AddScoped<IScrapeScheduleService, EventScrapeJobRegistrationService>();
builder.Services.AddScoped<IPcsScraper, PcsScraper>();
builder.Services.AddScoped<IScraperService, ScraperService>();

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
});

// JWT Auth
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var config = builder.Configuration;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = config["Jwt:Issuer"],
        ValidAudience = config["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]))
    };
});

// -------------------
// Build app
// -------------------
var app = builder.Build();

// -------------------
// Seed & Ensure DB
// -------------------
if (app.Environment.IsEnvironment("Test"))
{
    Console.WriteLine("Test environment detected — running SeedData...");
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        //db.Database.Migrate(); // (optioneel als SQL Server)
        await SeedData.EnsureSeedAsync(db);
    }
}
else
{
    Console.WriteLine("Not Test environment — skipping SeedData");
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (builder.Environment.IsEnvironment("Test"))
        {
            //var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            //await TestDataSeeder.SeedAsync(db, env);
        }
        else
        {
            db.Database.Migrate();
        }
    }
}
// -------------------
// Middleware
// -------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "API draait!");


// -------------------
// Start the app
// -------------------

if (!builder.Environment.IsEnvironment("Test"))
{
    app.Run(); // normale run voor dev/prod
}
else
{
    Console.WriteLine("Running in Test mode");
    await app.RunAsync(); // start de host
    Console.WriteLine("[API] Test environment: keeping host alive...");

}

public static class TestDb
{
    public static readonly InMemoryDatabaseRoot Root = new();
}