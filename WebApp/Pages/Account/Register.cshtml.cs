using CycleManager.Domain.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

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
                string content = await response.Content.ReadAsStringAsync();

                try
                {
                    var errors = JsonSerializer.Deserialize<IEnumerable<string>>(content);
                    if (errors != null)
                    {
                        foreach (var error in errors)
                        {
                            ModelState.AddModelError(string.Empty, error);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Er is een fout opgetreden tijdens registratie.");
                    }
                }
                catch (JsonException)
                {
                    ModelState.AddModelError(string.Empty, $"Er is een fout opgetreden: {content}");
                }

                return Page();
            }

        }
    }
}
