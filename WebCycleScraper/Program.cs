using CycleManager.Services;
using CycleManager.Services.Settings;
using Domain.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false);
            })
            .ConfigureServices((context, services) =>
            {
                // ⬇️ Add your EF DbContext from DataAccessEF
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("CycleDb")));

                // ⬇️ Your scraper logic class
                services.AddScoped<ScraperService>();
                services.AddScoped<PcsScraper>();
                services.Configure<ScraperSettings>(context.Configuration.GetSection("ScraperSettings"));
                services.AddLogging();

            })
            .Build();

        // Run scraper logic
        using var scope = host.Services.CreateScope();
        var config = host.Services.GetRequiredService<IConfiguration>();
        var settings = config.GetSection("ScraperSettings").Get<ScraperSettings>();
        var scraper = scope.ServiceProvider.GetRequiredService<ScraperService>();
        await scraper.RunAsync(settings.EventId, settings.EventName, settings.Year, settings.Stage); // run your scraping logic
    }
}
