using CycleManager.Domain.Dto;
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
        public string EventName { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string? Slogan { get; set; }
        public List<DeelnemerDto> Deelnemers { get; set; } = new();
        public List<ResultDto> Renners { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ClientSettings:ApiBaseUrl"];

            var eventTask = client.GetAsync($"{apiBaseUrl}/event/{EventId}");
            var deelnemersTask = client.GetAsync($"{apiBaseUrl}/Deelnemer?eventId={EventId}");

            await Task.WhenAll(eventTask, deelnemersTask);

            var eventResponse = eventTask.Result;
            var deelnemerResponse = deelnemersTask.Result;

            if(!eventResponse.IsSuccessStatusCode || !deelnemerResponse.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var json = await eventResponse.Content.ReadAsStringAsync();
            var eventData = JsonSerializer.Deserialize<EventDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var deelnemers = await deelnemerResponse.Content.ReadFromJsonAsync<List<DeelnemerDto>>();

            if (eventData == null)
            {
                return NotFound();
            }

            EventName = eventData.EventName;
            StartDate = eventData.StartDate.ToString("d MMMM yyyy", new System.Globalization.CultureInfo("nl-NL"));
            EndDate = eventData.EndDate.ToString("d MMMM yyyy", new System.Globalization.CultureInfo("nl-NL"));
            Slogan = eventData.Slogan;
            Deelnemers = deelnemers?
                .ToList() ?? new List<DeelnemerDto>();
            ViewData["Title"] = EventName;
            return Page();
        }
    }
}
