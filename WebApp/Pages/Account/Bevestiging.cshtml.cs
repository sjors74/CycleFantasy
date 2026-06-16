using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Account
{
    public class BevestigingModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public BevestigingModel(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public string Message { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                Message = "Ongeldige bevestigingslink.";
                return Page();
            }

            var client = _httpClientFactory.CreateClient();
            var apiUrl = _config["ClientSettings:ApiBaseUrl"]; // bijv. https://localhost:5001

            var response = await client.GetAsync($"{apiUrl}/api/account/confirmemail?userId={userId}&token={Uri.EscapeDataString(token)}");

            if (response.IsSuccessStatusCode)
                Message = "Je e-mailadres is bevestigd!";
            else
                Message = "Bevestiging mislukt. Misschien is de link verlopen?";

            return Page();
        }
    }
}
