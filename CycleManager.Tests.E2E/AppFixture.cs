using Microsoft.Playwright;
using System.Diagnostics;
using System.Net.Sockets;

namespace CycleManager.Tests.E2E;

public class AppFixture
{
    private Process? _webAppProcess;
    private Process? _apiProcess;

    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public string WebBaseUrl { get; private set; } = "https://localhost:7089";
    public string ApiBaseUrl { get; private set; } = "https://localhost:44302";

    private bool _initialized = false;

    public async Task InitializeAsync()
    {
        if(_initialized) 
            return;

        Console.WriteLine("Initializing AppFixture...");

        // Start Api als die nog niet draait
        if (!await IsUrlReachable(ApiBaseUrl))
        {
            Console.WriteLine("Starting API...");
            _apiProcess = StartProcess("WebCycle", "https://localhost:44302", "Test");

            Console.WriteLine("Checking if API port 44302 is open...");
            if (!await IsPortOpen("localhost", 44302))
            {
                throw new Exception("API port 44302 is not open. Check launchSettings.json or HTTPS bindings!");
            }
            Console.WriteLine("API port 44302 is open.");

            Console.WriteLine("Waiting for API to be ready...");
            await WaitForUrl(ApiBaseUrl);
            await WaitForApiReadyAsync("https://localhost:44302");
            Console.WriteLine("API ready!");

            // Extra buffer voor init (EF, DI, etc.)
            await Task.Delay(1000);
        }
        else
        {
            Console.WriteLine("API already running");
        }

        // Start WebApp als die nog niet draait
        if (!await IsUrlReachable(WebBaseUrl))
        {
            Console.WriteLine("Starting WebApp...");
            _webAppProcess = StartProcess("WebApp", "https://localhost:7089", "Test");

            Console.WriteLine("Waiting for WebApp to be ready...");
            await WaitForUrl(WebBaseUrl);
            Console.WriteLine("WebApp ready!");
        }
        else
        {
            Console.WriteLine("WebApp already running");
        }

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new()
        {
            Headless = false,
            Args = new[] { "--ignore-certificate-errors" }
        });

        _initialized = true;
        Console.WriteLine("Appfixture ready!");
    }

    public async Task EnsureRunningAsync()
    {
        if (!await IsUrlReachable(WebBaseUrl))
        {
            Console.WriteLine("WebApp not responding, restarting...");
            _webAppProcess?.Kill(true);
            _webAppProcess = StartProcess("WebApp", "https://localhost:7089", "Test");
            await WaitForUrl(WebBaseUrl);
        }

        if (!await IsUrlReachable(ApiBaseUrl))
        {
            Console.WriteLine("API not responding, restarting...");
            _apiProcess?.Kill(true);
            _apiProcess = StartProcess("WebCycle", "https://localhost:44302", "Test");

            await WaitForUrl(ApiBaseUrl);
        }
    }

    public async Task DisposeAsync()
    {
        try 
        {
            if (Browser != null)
                await Browser.CloseAsync();

            Playwright?.Dispose();

            if (_webAppProcess is { HasExited: false })
            {
                _webAppProcess.Kill(true);
                Console.WriteLine("Webapp stopped.");
            }

            if (_apiProcess is { HasExited: false })
            {
                _apiProcess.Kill(true);
                Console.WriteLine("Api stopped.");
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Cleanup error: {ex.Message}");
        }
    }

    private Process StartProcess(string projectName, string url, string environment)
    {
        // Vind de root van de solution (relatief vanaf het testproject)
        string solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\"));

        // Kies het juiste projectpad
        string projectFile = projectName switch
        {
            "WebCycle" => Path.Combine(solutionRoot, "WebCycle", "WebCycleApi.csproj"),
            "WebApp" => Path.Combine(solutionRoot, "WebApp", "WebApp.csproj"),
            _ => throw new Exception($"Unknown project name: {projectName}")
        };

        if (!File.Exists(projectFile))
            throw new FileNotFoundException($"Project file not found: {projectFile}");

        string projectDir = Path.GetDirectoryName(projectFile)!;

        // 🔨 Build project indien nodig
        Console.WriteLine($"[AppFixture] Ensuring project is built...");
        var buildProc = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{projectFile}\"",
            WorkingDirectory = projectDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        });
        buildProc!.WaitForExit();

        // 🚀 Start de app via het juiste launch-profile
        Console.WriteLine($"[AppFixture] Starting project: {projectName}");
        Console.WriteLine($"[AppFixture] URL: {url}");
        Console.WriteLine($"[AppFixture] Environment: {environment}");
        Console.WriteLine();

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            // ⬇️ Belangrijk: gebruik het launch-profile 'Test' (zorgt voor juiste DB & config)
            Arguments = $"run --project \"{projectFile}\" --launch-profile {environment} --urls={url}",
            WorkingDirectory = projectDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Zet ook expliciet de environment-variable
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

        process.EnableRaisingEvents = true;
        process.Exited += (s, e) =>
        {
            Console.WriteLine($"[AppFixture] {projectName} exited unexpectedly (code {process.ExitCode})");
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return process;
    }


    //Helper: Poll of URL bereikbaar is
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

            await Task.Delay(500);
        }

        throw new Exception($"Timeout: app at {url} did not respond within {timeoutMs}ms");
    }

    private async Task WaitForApiReadyAsync(string baseUrl, int timeoutMs = 30000)
    {
        await Task.Delay(5000);

        using var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        var sw = Stopwatch.StartNew();

        while (sw.ElapsedMilliseconds  < timeoutMs)
        {
            try
            {
                var response = await httpClient.GetAsync($"/test/seed-ready");
                if (response.IsSuccessStatusCode)
                {
                        Console.WriteLine("[AppFixture] API seeded and ready.");
                        return;
                }
            }
            catch
            {
            //    Console.WriteLine($"[AppFixture] Waiting... ({ex.Message})");
            }


            await Task.Delay(1000);
        }

        throw new Exception($"Timeout: API at {baseUrl} did not return seeded data within {timeoutMs}ms");
    }

    private static async Task<bool> IsPortOpen(string host, int port, int timeoutMs = 5000)
    {
        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(host, port);
                var completedTask = await Task.WhenAny(connectTask, Task.Delay(500));
                if (completedTask == connectTask && client.Connected)
                    return true;
            }
            catch
            {
                // even wachten
            }
            await Task.Delay(200);
        }
        return false;
    }

}
