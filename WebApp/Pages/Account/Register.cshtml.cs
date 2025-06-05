using CycleManager.Domain.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public RegisterModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public RegisterDto? Input { get; set; }

        public string? Message { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ClientSettings:ApiBaseUrl"];

            var response = await client.PostAsJsonAsync($"{apiBaseUrl}/api/account", Input);

            if (response.IsSuccessStatusCode)
            {
                Message = "Registratie gelukt! Check je mail om te bevestigen.";
                ModelState.Clear();
                return Page();
            }
            else
            {
                var errors = await response.Content.ReadFromJsonAsync<IEnumerable<string>>();
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return Page();
            }

        }
    }
}
