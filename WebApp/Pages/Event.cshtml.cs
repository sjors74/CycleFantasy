using Domain.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace WebApp.Pages
{
    public class EventModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public EventModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string? Slogan { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ClientSettings:ApiBaseUrl"];

            var response = await client.GetAsync($"{apiBaseUrl}/event/{EventId}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var json = await response.Content.ReadAsStringAsync();
            var eventData = JsonSerializer.Deserialize<EventDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (eventData == null)
            {
                return NotFound();
            }

            EventName = eventData.EventName;
            StartDate = eventData.StartDate.ToString("d MMMM yyyy", new System.Globalization.CultureInfo("nl-NL"));
            EndDate = eventData.EndDate.ToString("d MMMM yyyy", new System.Globalization.CultureInfo("nl-NL"));
            Slogan = eventData.Slogan;

            ViewData["Title"] = EventName;
            return Page();
        }
    }
}
