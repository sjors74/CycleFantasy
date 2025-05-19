using CycleManager.Domain.Dto;
using Domain.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

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

        public List<EventForUserDto> ActueleEvenementen { get; set; } = new();
        public List<EventForUserDto> ToekomstigeEvenementen { get; set; } = new();
        public async Task OnGetAsync()
        {
            if(User.Identity != null && User.Identity.IsAuthenticated)
            {
                var client = _clientFactory.CreateClient();
                var apiBaseUrl = _configuration["ClientSettings:ApiBaseUrl"];
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var response = await client.GetFromJsonAsync<EventViewDto>($"{apiBaseUrl}/event/{userId}/user");
                if (response == null)
                {
                    return;
                }

                ActueleEvenementen = response.ActieveEvenementen ?? new();
                foreach(var evenement in ActueleEvenementen)
                {
                    var geselecteerdeRennersInEvenement = await client.GetFromJsonAsync<List<ResultDto>>($"{apiBaseUrl}/deelnemer/picks/{evenement.CompetitorInEventId}/event/{evenement.EventId}");
                    if(geselecteerdeRennersInEvenement != null && geselecteerdeRennersInEvenement.Any())
                    {
                        evenement.Renners = geselecteerdeRennersInEvenement;
                    }
                }
                ToekomstigeEvenementen = response.ToekomstigeEvenementen ?? new();
                //TODO: doe hetzelfde trucje als hierboven voor ActueleEvenementen.
            }
        }
    }
}
