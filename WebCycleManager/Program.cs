using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("CycleDb");
builder.Services.AddDbContext<DatabaseContext>(x => x.UseLazyLoadingProxies().UseSqlServer(connectionString));
builder.Services.AddTransient<IEventRepository, EventRepository>();
builder.Services.AddTransient<ICompetitorRepository, CompetitorRepository>();
builder.Services.AddTransient<ICompetitorsInEventRepository, CompetitorsInEventRepository>();
builder.Services.AddTransient<IConfigurationRepository, ConfigurationRepository>(); 
builder.Services.AddTransient<ICountryRepository, CountryRepository>();
builder.Services.AddTransient<IResultsRepository, ResultsRepository>();
builder.Services.AddTransient<IStageRepository, StageRepository>();
builder.Services.AddTransient<ITeamRepository, TeamRepository>();
builder.Services.AddControllersWithViews();

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
