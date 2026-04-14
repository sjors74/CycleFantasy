using CycleManager.Domain.Dto;
using Domain.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace WebApp.Pages.Account
{
    [Authorize(AuthenticationSchemes = "MyCookieAuth")]
    public class ProfielModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public ProfielModel(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration; 
        }

        [BindProperty(SupportsGet = true)]
        public int DeelnemerId { get; set; }

        [BindProperty]
        public string PoolNaam { get; set; }

        [BindProperty]
        public int CurrentEventId { get; set; }

        public List<EventForUserDto> ActueleEvenementen { get; set; } = [];
        public List<EventForUserDto> ToekomstigeEvenementen { get; set; } = [];
        public List<EventForUserDto> HistorischeEvenementen { get; set; } = [];
        public async Task OnGetAsync()
        {
            if(User.Identity != null && User.Identity.IsAuthenticated)
            {
                var client = _clientFactory.CreateClient();
                var apiBaseUrl = _configuration["ClientSettings:ApiBaseUrl"];
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var response = await client.GetFromJsonAsync<EventViewDto>($"{apiBaseUrl}/api/event/{userId}/user");
                if (response == null)
                {
                    return;
                }

                ActueleEvenementen = response.ActieveEvenementen ?? new();
                ToekomstigeEvenementen = response.ToekomstigeEvenementen ?? new();
                HistorischeEvenementen = response.HistorischeEvenementen ?? new();
            }
        }

        public async Task<IActionResult> OnPostCreatePoolAsync()
        {
            if (string.IsNullOrWhiteSpace(PoolNaam))
            {
                ModelState.AddModelError(string.Empty, "De poolnaam mag niet leeg zijn.");
                return Page(); // Je kunt eventueel de pagina opnieuw renderen
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var dto = new
            {
                Id = DeelnemerId,
                PoolNaam,
                UserId = userId,
                EventId = CurrentEventId
            };

            // API-aanroep om de nieuwe pool te maken
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var client = _clientFactory.CreateClient();
            var apiBaseUrl = _configuration["ClientSettings:ApiBaseUrl"];
            var result = await client.PostAsync($"{apiBaseUrl}/api/event/createpool", content);


            if (result.IsSuccessStatusCode)
            {
                // Optioneel: Feedback bericht bij succes
                return RedirectToPage("/Account/Profiel");
            }
            else
            {
                // Foutmelding bij falen
                ModelState.AddModelError(string.Empty, "Er is iets misgegaan bij het aanmaken van de pool.");
                return Page();
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostRenamePoolAsync([FromBody] RenamePoolDto request)
        {
            if (string.IsNullOrWhiteSpace(request.NieuweNaam))
            {
                return new JsonResult(new { success = false, message = "De poolnaam mag niet leeg zijn." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var dto = new
            {
                PoolId = request.PoolId,
                NieuweNaam = request.NieuweNaam.Trim(),
                UserId = userId
            };

            var content = new StringContent(
                JsonSerializer.Serialize(dto),
                Encoding.UTF8,
                "application/json");

            var client = _clientFactory.CreateClient();
            var apiBaseUrl = _configuration["ClientSettings:ApiBaseUrl"];

            var result = await client.PutAsync(
                $"{apiBaseUrl}/api/event/renamepool",
                content);

            if (result.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true });
            }
            else
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Er is iets misgegaan bij het hernoemen van de pool."
                });
            }
        }


    }
}
