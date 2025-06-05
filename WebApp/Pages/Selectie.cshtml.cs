using CycleManager.Domain.Dto;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace WebApp.Pages
{
    public class SelectieModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public List<TeamDto> AllTeams { get; set; } = [];
        public List<TeamDto> VisibleTeams { get; set; } = [];

        [BindProperty(SupportsGet = true)]
        public int EvenementId { get; set; }

        [BindProperty]
        public string SelectedRidersJson { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public int DeelnemerId { get; set; }

        public List<int> SelectedRiders { get; set; } = [];

        [BindProperty]
        public int StartIndex { get; set; }

        private const string SessionKey = "SelectedRiders";

        public SelectieModel(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public void OnGet()
        {
            var savedIds = GetSavedRiders();
            SaveSelectedRidersToSession(savedIds);
            LoadTeams(); // Doe een fetch naar [api/event/id/teams-with-renners")]
            StartIndex = 0;
            VisibleTeams = AllTeams.Take(3).ToList();
            SelectedRiders = GetSelectedRidersFromSession();
        }

        public void OnPost(string action)
        {
            LoadTeams();
            VisibleTeams = AllTeams.Skip(StartIndex).Take(3).ToList();

            if (!string.IsNullOrEmpty(SelectedRidersJson))
            {
                SelectedRiders = JsonSerializer.Deserialize<List<int>>(SelectedRidersJson) ?? new List<int>();
            }

            var currentSelection = GetSelectedRidersFromSession();

            foreach (var id in SelectedRiders)
            {
                if (!currentSelection.Contains(id))
                    currentSelection.Add(id);
            }

            var visibleRiderIds = VisibleTeams.SelectMany(t => t.Renners.Select(r => r.CompetitorId)).ToList();
            currentSelection.RemoveAll(id => visibleRiderIds.Contains(id) && !SelectedRiders.Contains(id));

            SaveSelectedRidersToSession(currentSelection);

            if (action == "next")
                StartIndex += 3;
            else if (action == "previous")
                StartIndex -= 3;

            if (StartIndex < 0) StartIndex = 0;

            int pageSize = 3;
            int maxPage = (AllTeams.Count + pageSize - 1) / pageSize - 1; // aantal pagina's - 1
            int maxStartIndex = maxPage * pageSize;

            if (StartIndex > maxStartIndex)
                StartIndex = maxStartIndex;

            VisibleTeams = AllTeams.Skip(StartIndex).Take(3).ToList();

            SelectedRiders = GetSelectedRidersFromSession();
        }

        public async Task<IActionResult> OnPostBevestigenAsync()
        {
            if (!string.IsNullOrEmpty(SelectedRidersJson))
            {
                var selectedIds = JsonSerializer.Deserialize<List<int>>(SelectedRidersJson) ?? [];
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var dto = new
                {
                    UserId = userId,
                    RennerIds = selectedIds,
                    EventId = EvenementId,
                    DeelnemerId
                };

                var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                var apiBaseUrl = _configuration["ClientSettings:ApiBaseUrl"];
                var response = await _httpClient.PostAsync($"{apiBaseUrl}/api/event/selectie", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Account/Profiel", new { userId = dto.UserId });
                }

                ModelState.AddModelError(string.Empty, "Opslaan mislukt.");
            }

            // Herladen indien fout
            LoadTeams();
            VisibleTeams = AllTeams.Skip(StartIndex).Take(3).ToList();
            SelectedRiders = GetSelectedRidersFromSession();
            return Page();
        }


        private void LoadTeams()
        {
            var apiBaseUrl = _configuration["ClientSettings:ApiBaseUrl"];
            var response = _httpClient.GetAsync($"{apiBaseUrl}/api/event/{EvenementId}/teams-with-renners").Result;

            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                AllTeams = JsonSerializer.Deserialize<List<TeamDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<TeamDto>();
            }
            else
            {
                AllTeams = new List<TeamDto>(); // fallback
            }
        }
        private List<int> GetSelectedRidersFromSession()
        {
            var json = HttpContext.Session.GetString(SessionKey);
            return string.IsNullOrEmpty(json)
                ? new List<int>()
                : JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }

        private void SaveSelectedRidersToSession(List<int> riderIds)
        {
            var json = JsonSerializer.Serialize(riderIds);
            HttpContext.Session.SetString(SessionKey, json);
        }

        private List<int> GetSavedRiders()
        {
            var savedIds = new List<int>();
            var apiBaseUrl = _configuration["ClientSettings:ApiBaseUrl"];
            var response = _httpClient.GetAsync($"{apiBaseUrl}/api/Deelnemer/Picks/{DeelnemerId}").Result;

            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                savedIds = JsonSerializer.Deserialize<List<int>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<int>();


            }
            return savedIds;
        }
    }
}