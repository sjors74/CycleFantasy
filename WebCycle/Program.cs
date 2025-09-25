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

// Add services to the container. 
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

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseLazyLoadingProxies(false)
        .UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
            sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        }));
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddTransient<ITeamRepository, TeamRepository>();
builder.Services.AddTransient<ICompetitorRepository, CompetitorRepository>();
builder.Services.AddTransient<ICompetitorsInEventRepository, CompetitorsInEventRepository>();
builder.Services.AddScoped<ICompetitorInTeamRepository,CompetitorInTeamRepository>();
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
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
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

var app = builder.Build();

using(var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
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

app.Run();
