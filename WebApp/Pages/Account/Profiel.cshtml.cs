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
                foreach(var evenement in ActueleEvenementen)
                {
                    var geselecteerdeRennersInEvenement = await client.GetFromJsonAsync<List<ResultDto>>($"{apiBaseUrl}/api/deelnemer/picks/{evenement.CompetitorInEventId}/event/{evenement.EventId}");
                    if(geselecteerdeRennersInEvenement != null && geselecteerdeRennersInEvenement.Any())
                    {
                        evenement.Renners = geselecteerdeRennersInEvenement;
                    }
                }
                ToekomstigeEvenementen = response.ToekomstigeEvenementen ?? new();
                foreach (var evenement in ToekomstigeEvenementen)
                {
                    var geselecteerdeRennersInEvenement = await client.GetFromJsonAsync<List<ResultDto>>($"{apiBaseUrl}/api/deelnemer/picks/{evenement.CompetitorInEventId}/event/{evenement.EventId}");
                    if (geselecteerdeRennersInEvenement != null && geselecteerdeRennersInEvenement.Any())
                    {
                        evenement.Renners = geselecteerdeRennersInEvenement;
                    }
                }

                HistorischeEvenementen = response.HistorischeEvenementen ?? new();
                foreach(var evenement in HistorischeEvenementen)
                {
                    foreach (var deelnemer in evenement.Deelnemers)
                    {
                        if (deelnemer.UserId == userId)
                        {
                            var geselecteerdeRennersInEvenement = await client.GetFromJsonAsync<List<ResultDto>>($"{apiBaseUrl}/api/deelnemer/picks/{deelnemer.Id}/event/{evenement.EventId}");
                            if (geselecteerdeRennersInEvenement != null && geselecteerdeRennersInEvenement.Any())
                            {
                                evenement.Renners = geselecteerdeRennersInEvenement;
                            }
                        }
                    }
                }
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
                return RedirectToPage("/Account/Profiel", new { userId });
            }
            else
            {
                // Foutmelding bij falen
                ModelState.AddModelError(string.Empty, "Er is iets misgegaan bij het aanmaken van de pool.");
                return Page();
            }
        }
    }
}
