using CycleManager.Services;
using CycleManager.Services.Interfaces;
using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using System.Diagnostics;
using WebCycle.Services;

namespace CycleManager.Tests.E2E
{
    public class AppFixture
    {
        private Process? _webAppProcess;
        private Process? _apiProcess;

        public IPlaywright Playwright { get; private set; } = null!;
        public IBrowser Browser { get; private set; } = null!;

        public string WebBaseUrl { get; private set; } = "https://localhost:7089";
        public string ApiBaseUrl { get; private set; } = "https://localhost:44302";

        private bool _initialized = false;
        private bool _isRunning = false;

        public IServiceProvider Services { get; private set; } = null!;

        private readonly string _dbName = $"E2ETestDb_{Guid.NewGuid()}";

        // Start de apps en Playwright
        public async Task InitializeAsync()
        {
            if (_initialized)
                return;

            Console.WriteLine("Initializing AppFixture...");

            // Start API
            Console.WriteLine("Starting API...");
            _apiProcess = StartProcess("WebCycle", "https://localhost:44302", "Test");
            await WaitForApiReadyAsync(ApiBaseUrl);
    
            await WaitForUrl(ApiBaseUrl);

            // Start WebApp
            if (!await IsUrlReachable(WebBaseUrl))
            {
                Console.WriteLine("Starting WebApp...");
                _webAppProcess = StartProcess("WebApp", "https://localhost:7089", "Test");
                await WaitForUrl(WebBaseUrl);
            }
            else
            {
                Console.WriteLine("WebApp already running");
            }

            // Launch Playwright
            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            Browser = await Playwright.Chromium.LaunchAsync(new()
            {
                Headless = false, // zet op true in CI
                Args = new[] { "--ignore-certificate-errors" }
            });

            var hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseInMemoryDatabase(_dbName));

                    // Je services registreren
                    services.AddScoped<IResultService, ResultService>();
                    services.AddScoped<IResultsRepository, ResultsRepository>();
                    services.AddScoped<IScoreRepository, ScoreRepository>();
                });

            var host = hostBuilder.Build();
            Services = host.Services;

            // Database reset + seed
            await ResetDatabaseAsync();


            _initialized = true;
            _isRunning = true;

            Console.WriteLine("AppFixture ready!");
        }

        // Reset database en seed opnieuw
        private async Task ResetDatabaseAsync()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Console.WriteLine("Resetting database...");
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            Console.WriteLine("Seeding database...");
            await SeedData.EnsureSeedAsync(db);
            Console.WriteLine("Database seeded successfully!");
        }

        //Controleer of alles nog draait, anders restart
        public async Task EnsureRunningAsync()
        {
            if (!_initialized)
                await InitializeAsync();

            if (!await IsUrlReachable(WebBaseUrl))
            {
                Console.WriteLine("WebApp not reachable — restarting...");
                _webAppProcess?.Kill(true);
                _webAppProcess = StartProcess("WebApp", "https://localhost:7089", "Test");
                await WaitForUrl(WebBaseUrl);
            }

            if (!await IsUrlReachable(ApiBaseUrl))
            {
                Console.WriteLine("API not reachable — restarting...");
                _apiProcess?.Kill(true);
                _apiProcess = StartProcess("WebCycle", "https://localhost:44302", "Test");
                await WaitForUrl(ApiBaseUrl);
            }

            _isRunning = true;
        }

        //Correct afsluiten
        public async Task DisposeAsync()
        {
            try
            {
                Console.WriteLine("Cleaning up AppFixture...");

                if (Browser != null)
                    await Browser.CloseAsync();

                Playwright?.Dispose();

                if (_webAppProcess is { HasExited: false })
                {
                    _webAppProcess.Kill(true);
                    Console.WriteLine("WebApp stopped.");
                }

                if (_apiProcess is { HasExited: false })
                {
                    _apiProcess.Kill(true);
                    Console.WriteLine("API stopped.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cleanup error: {ex.Message}");
            }
        }

        //Helper: start proces
        private Process StartProcess(string projectName, string url, string environment)
        {
            string solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\")); // test project → solution root
            string projectFile = projectName switch
            {
                "WebCycle" => Path.Combine(solutionRoot, "WebCycle", "WebCycleApi.csproj"),
                "WebApp" => Path.Combine(solutionRoot, "WebApp", "WebApp.csproj"),
                _ => throw new Exception($"Unknown project name: {projectName}")
            };
            if (!File.Exists(projectFile))
                throw new FileNotFoundException($"Project file not found: {projectFile}");

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{projectFile}\" --launch-profile {environment} --urls={url}",
                WorkingDirectory = Path.GetDirectoryName(projectFile)!,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = environment;

            var process = new Process { StartInfo = startInfo };
            process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Console.WriteLine($"[{projectName}] {e.Data}");
            };
            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Console.WriteLine($"[{projectName}:ERR] {e.Data}");
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return process;
        }

        //Helper: check of URL werkt
        private static async Task<bool> IsUrlReachable(string url)
        {
            using var http = new HttpClient();
            try
            {
                var res = await http.GetAsync(url);
                return res.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        //Helper: wacht tot webapp beschikbaar is
        private static async Task WaitForUrl(string url, int timeoutMs = 60000)
        {
            using var http = new HttpClient();
            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    var res = await http.GetAsync(url);
                    if (res.IsSuccessStatusCode)
                        return;
                }
                catch { /* wachten */ }

                await Task.Delay(1000);
            }

            throw new Exception($"Timeout: app at {url} did not respond within {timeoutMs}ms");
        }

        // Helper: wacht tot seed klaar is (voor API)
        private async Task WaitForApiReadyAsync(string baseUrl, int timeoutMs = 30000)
        {
            //await Task.Delay(5000);
            using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    var response = await http.GetAsync($"/test/seed-ready");
                    var body = await response.Content.ReadAsStringAsync();

                    Console.WriteLine("Status: " + response.StatusCode);
                    Console.WriteLine("Body: " + body);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("API seeded and ready.");
                        return;
                    }
                }
                catch { /* wachten */ }

                await Task.Delay(1000);
            }

            throw new Exception($"Timeout: API at {baseUrl} did not return seeded data in {timeoutMs}ms");
        }

        public async Task ApplyCustomConfigAsync(int eventId, int[] customPoints)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Maak nieuwe configuratie
            var config = new Configuration { ConfigurationType = "E2E Custom Test Config" };
            db.Configurations.Add(config);
            await db.SaveChangesAsync();

            for (int i = 0; i < customPoints.Length; i++)
            {
                db.ConfigurationItems.Add(new ConfigurationItem
                {
                    ConfigurationId = config.Id,
                    Position = i + 1,
                    Score = customPoints[i]
                });
            }
            await db.SaveChangesAsync();

            // 2. Koppel config aan event
            var ev = await db.Events.FirstAsync(e => e.EventId == eventId);
            ev.ConfigurationId = config.Id;

            // 3. Verwijder oude scores
            var pickIds = db.GameCompetitorEventPicks
                .Where(p => p.GameCompetitorEvent.EventId == eventId)
                .Select(p => p.Id)
                .ToList();

            db.DeelnemerPickScores.RemoveRange(
                db.DeelnemerPickScores.Where(dps => pickIds.Contains(dps.GameCompetitorEventPickId))
            );

            db.DeelnemerScores.RemoveRange(
                db.DeelnemerScores.Where(ds => ds.Stage.EventId == eventId)
            );

            await db.SaveChangesAsync();

            // 4. Herbereken opnieuw via applicatieservice
            var resultService = scope.ServiceProvider.GetRequiredService<IResultService>();
            await RecalculateEventScoresAsync(eventId);
        }

        public async Task RecalculateEventScoresAsync(int eventId)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var resultService = scope.ServiceProvider.GetRequiredService<IResultService>();

            await resultService.RecalculateEventScoresAsync(eventId);
            await db.SaveChangesAsync();
        }
    }
}
