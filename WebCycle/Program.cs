using DataAccessEF.UnitOfWork;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using CycleManager.Domain.Interfaces;
using CycleManager.Services.Interfaces;
using CycleManager.Services;

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

builder.Services.AddDbContext<DatabaseContext>(x => x.UseLazyLoadingProxies().UseSqlServer(connectionString));
//builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddTransient<ITeamRepository, TeamRepository>();
builder.Services.AddTransient<ICompetitorRepository, CompetitorRepository>();
builder.Services.AddTransient<ICompetitorsInEventRepository, CompetitorsInEventRepository>();
builder.Services.AddTransient<IEventRepository, EventRepository>();
builder.Services.AddTransient<IResultsRepository, ResultsRepository>();
builder.Services.AddTransient<IStageRepository, StageRepository>();
builder.Services.AddTransient<IGameCompetitorInEventRepository, GameCompetitorInEventRepository>();
builder.Services.AddScoped<IGameCompetitorInEventService, GameCompetitorInEventService>();
builder.Services.AddTransient<IGameCompetitorEventPickRepository, GameCompetitorEventPickRepository>();
builder.Services.AddTransient<IResultService, ResultService>();
builder.Services.AddTransient<IEventService, EventService>();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
var app = builder.Build();

using(var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
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
