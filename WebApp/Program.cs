using Domain.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();
if (builder.Environment.IsEnvironment("Test"))
{
    var apiUrl = builder.Configuration["ClientSettings:ApiBaseUrl"];
    if (string.IsNullOrEmpty(apiUrl) || apiUrl.Contains("7089"))
    {
        builder.Configuration["ClientSettings:ApiBaseUrl"] = "https://localhost:44302";
        Console.WriteLine("Overriding ApiBaseUrl for Test environment to https://localhost:44302");
    }
}

// Add services to the container.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None; // of None bij meerdere domeinen
        options.Cookie.HttpOnly = true;
        options.Cookie.Name = "MyAppAuth"; // optioneel: unieke naam om verwarring te voorkomen
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
if (app.Environment.IsEnvironment("Test"))
{
    app.UseMiddleware<FakeAuthMiddleware>();
}
app.UseAuthentication();

app.UseAuthorization();
app.MapRazorPages();

app.Run();

Console.WriteLine($"ENVIRONMENT: {builder.Environment.EnvironmentName}");

if (app.Environment.IsEnvironment("Test"))
{
    app.UseExceptionHandler("/Error"); // voegt try/catch om de pipeline
    app.UseMiddleware<FakeAuthMiddleware>();
}