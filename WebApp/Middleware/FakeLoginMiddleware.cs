using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

public class FakeAuthMiddleware
{
    private readonly RequestDelegate _next;

    public FakeAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Alleen bij Test-omgeving
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            var fakeUser = context.Request.Query["fakeuser"].ToString();
            if (!string.IsNullOrEmpty(fakeUser))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, fakeUser),
                    new Claim(ClaimTypes.Email, $"{fakeUser}@example.com"),
                    new Claim(ClaimTypes.Role, "User"),
                    new Claim(ClaimTypes.NameIdentifier, "testuser")
                };

                var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                var principal = new ClaimsPrincipal(identity);

                // zet gebruiker op context
                context.User = principal;

                // maak ook cookie aan zodat authenticatie bij volgende request behouden blijft
                await context.SignInAsync("MyCookieAuth", principal);

                Console.WriteLine($"[FakeAuth] Signed in as {fakeUser}");
            }
        }

        if (context.Request.Path.StartsWithSegments("/logout", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("[FakeAuth] Signing out (via /logout)");
            await context.SignOutAsync("MyCookieAuth");
            context.Response.Redirect("/");
            return;
        }

        await _next(context);
    }
}
