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
        // alleen fake-authenticatie toepassen als er een ?fakeuser= query aanwezig is
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            var fakeUser = context.Request.Query["fakeuser"].ToString();
            if (!string.IsNullOrEmpty(fakeUser))
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, fakeUser),
                new Claim(ClaimTypes.Email, $"{fakeUser}@example.com"),
                new Claim(ClaimTypes.Role, "User")
            };

                var identity = new ClaimsIdentity(claims, "FakeAuth");
                var principal = new ClaimsPrincipal(identity);
                context.User = principal;

                Console.WriteLine($"[FakeAuth] Signed in as {fakeUser}");
            }
            else
            {
                Console.WriteLine($"[FakeAuth] No fakeuser provided -> request remains anonymous");
            }
        }

        await _next(context);
    }
}
