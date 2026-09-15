using CycleManager.Domain.Dto;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace WebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public LoginModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public LoginDto Input { get; set; } = new();

        public string Message { get; set; } = string.Empty;

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ClientSettings:ApiBaseUrl"];

            var response = await client.PostAsJsonAsync($"{apiBaseUrl}/api/account/login", Input);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

                if (result == null)
                {
                    ModelState.AddModelError(string.Empty, "Ongeldige reactie van de server.");
                    return Page();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, Input.Email),
                    new Claim(ClaimTypes.NameIdentifier, result.UserId)
                };

                var identity  = new ClaimsIdentity(claims, "MyCookieAuth");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("MyCookieAuth", principal);

                return RedirectToPage("/Account/Profiel");
            }

            ModelState.AddModelError(string.Empty, "Ongeldige login.");
            return Page();
        }

        public void OnGet()
        {
        }
    }
}
