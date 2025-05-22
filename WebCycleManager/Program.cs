using CycleManager.Domain.Interfaces;
using CycleManager.Domain.Models;
using CycleManager.Services;
using CycleManager.Services.Interfaces;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebCycleManager.Config;
using WebCycleManager.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("CycleDb");
builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseLazyLoadingProxies(false).UseSqlServer(connectionString));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddTransient<ICompetitorRepository, CompetitorRepository>();
builder.Services.AddTransient<ICompetitorsInEventRepository, CompetitorsInEventRepository>();
builder.Services.AddTransient<IConfigurationRepository, ConfigurationRepository>(); 
builder.Services.AddTransient<IConfigurationItemRepository, ConfigurationItemRepository>();
builder.Services.AddTransient<ICountryRepository, CountryRepository>();
builder.Services.AddTransient<IEventRepository, EventRepository>();
builder.Services.AddTransient<IResultsRepository, ResultsRepository>();
builder.Services.AddTransient<IStageRepository, StageRepository>();
builder.Services.AddTransient<ITeamRepository, TeamRepository>();
builder.Services.AddTransient<IGameCompetitorEventPickRepository, GameCompetitorEventPickRepository>();
builder.Services.AddTransient<IGameCompetitorInEventRepository, GameCompetitorInEventRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
//Service
builder.Services.AddTransient<IEventService, EventService>();
builder.Services.AddTransient<IStageService, StageService>();
builder.Services.AddTransient<IConfigurationService, ConfigurationService>();
builder.Services.AddTransient<IResultService, ResultService>();
builder.Services.AddTransient<ICountryService, CountryService>();
builder.Services.AddTransient<ICompetitorService, CompetitorService>();
builder.Services.AddTransient<ICompetitorInEventService, CompetitorInEventService>();
builder.Services.AddTransient<ITeamService, TeamService>();
builder.Services.AddTransient<IGameCompetitorInEventService, GameCompetitorInEventService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddControllersWithViews();
builder.Services.Configure<ApiSettings>(
builder.Configuration.GetSection("ApiSettings"));

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
