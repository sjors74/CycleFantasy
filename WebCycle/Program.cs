using DataAccessEF.UnitOfWork;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container. 
var connectionString = builder.Configuration.GetConnectionString("CycleDb");
builder.Services.AddDbContext<DatabaseContext>(x => x.UseLazyLoadingProxies().UseSqlServer(connectionString));
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddTransient<ITeamRepository, TeamRepository>();
builder.Services.AddTransient<ICompetitorRepository, CompetitorRepository>();
builder.Services.AddTransient<ICompetitorsInEventRepository, CompetitorsInEventRepository>();
builder.Services.AddTransient<IEventRepository, EventRepository>();
builder.Services.AddTransient<IResultsRepository, ResultsRepository>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
