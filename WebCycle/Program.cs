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
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
        options.UseInMemoryDatabase("TestDb"));
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

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

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

using (var scope = app.Services.CreateScope())
{ 
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (builder.Environment.IsEnvironment("Test"))
    {
        db.Database.EnsureCreated();
        WebCycle.Services.TestDataSeeder.Seed(db);
        Console.WriteLine("[API] Test data seeded.");
    }
    else
    {
        db.Database.Migrate();
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

// Extra ping endpoint voor AppFixture
if (app.Environment.IsEnvironment("Test"))
{
    app.MapGet("/test/seed-ready", (ApplicationDbContext db) =>
    {
        bool ready = db.Events.Any();
        return ready ? Results.Ok() : Results.StatusCode(503);
    });
}

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