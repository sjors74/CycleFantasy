using CycleManager.Services;
using CycleManager.Services.Interfaces;
using CycleManager.Services.Settings;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.Configure<ScraperSettings>(
            context.Configuration.GetSection("ScraperSettings"));
        services.AddScoped<ScraperService>();
        services.AddScoped<PcsScraper>();
        services.AddTransient<IStageService, StageService>();
        services.AddTransient<IStageRepository, StageRepository>();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(Environment.GetEnvironmentVariable("CycleDbConnectionString"))
        );
    })
    .Build();

host.Run();
