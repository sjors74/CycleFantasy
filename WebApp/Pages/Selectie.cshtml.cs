using CycleManager.Domain.Dto;
using Domain.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace WebApp.Pages
{
    [IgnoreAntiforgeryToken]
    public class SelectieModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SelectieModel> _logger;
        
        public List<TeamDto> AllTeams { get; set; } = [];
        public List<TeamDto> VisibleTeams { get; set; } = [];

        [BindProperty(SupportsGet = true)]
        public int EvenementId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int DeelnemerId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; }

        [BindProperty]
        public string SelectedRidersJson { get; set; } = "";

        public List<int> SelectedRiders { get; set; } = new();

        private const string SessionKey = "SelectedRiders";

        private const int PageSize = 3;

        public SelectieModel(HttpClient httpClient, IConfiguration configuration, ILogger<SelectieModel> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public void OnGet()
        {

            LoadTeams();

            SelectedRiders = GetSavedRiders();

            SelectedRidersJson = JsonSerializer.Serialize(SelectedRiders);

            VisibleTeams = AllTeams.Skip(PageIndex * PageSize).Take(PageSize).ToList();
        }

        public void OnPost(string action)
        {
            LoadTeams();

            SelectedRiders =
    JsonSerializer.Deserialize<List<int>>(SelectedRidersJson)
    ?? new();

            if (action == "next") PageIndex++;
            if (action == "previous") PageIndex--;

            ModelState.Remove(nameof(PageIndex));

            if (PageIndex < 0) PageIndex = 0;

            var maxPage = (int)Math.Ceiling((double)AllTeams.Count / PageSize) - 1;


            if (PageIndex > maxPage)
                PageIndex = maxPage;

            VisibleTeams = AllTeams
                .Skip(PageIndex * PageSize)
                .Take(PageSize)
                .ToList();

            SelectedRidersJson =
    JsonSerializer.Serialize(SelectedRiders);

        }

        public async Task<IActionResult> OnPostBevestigenAsync()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var selectedIds = JsonSerializer.Deserialize<List<int>>(SelectedRidersJson) ?? new List<int>();
            var dto = new
            {
                UserId = userId,
                RennerIds = selectedIds, 
                EventId = EvenementId,
                DeelnemerId
            };

            var content = new StringContent(
                JsonSerializer.Serialize(dto),
                Encoding.UTF8,
                "application/json");

            var apiBaseUrl = _configuration["ClientSettings:ApiBaseUrl"];

            var response = await _httpClient.PostAsync(
                $"{apiBaseUrl}/api/event/selectie",
                content);

            if (response.IsSuccessStatusCode)
                return RedirectToPage("/Account/Profiel", new { userId });

            ModelState.AddModelError("", "Opslaan mislukt.");

            //LoadTeams();

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

        public async Task<IActionResult> OnPostLaadMeerRennersAsync(int teamId, int eventId, List<int> alreadyLoadedIds)
        {
            var competitors = new List<CompetitorDto>();
            var apiBaseUrl = _configuration["ClientSettings:ApiBaseUrl"];
            try
            {
                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/event/team/{teamId}/teams-with-more-renners");
                if(!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API call failed. StatusCode: {StatusCode}, Content: {Content}",
                         response.StatusCode, content);

                    return StatusCode((int)response.StatusCode, "Fout bij ophalen van renners.");
                }
                
                var json = await response.Content.ReadAsStringAsync();
                competitors = JsonSerializer.Deserialize<List<CompetitorDto>>(json, new JsonSerializerOptions
                {
                        PropertyNameCaseInsensitive = true
                }) ?? new List<CompetitorDto>();
            }
            catch(HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request naar API mislukt voor teamId {TeamId}", teamId);
                return StatusCode(503, "Kan de API niet bereiken.");
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Deserialisatie van CompetitorDto mislukt.");
                return StatusCode(500, "Fout bij verwerken van renners data.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Onverwachte fout bij ophalen van renners voor teamId {TeamId}", teamId);
                return StatusCode(500, "Onverwachte fout opgetreden.");
            }

            var filteredCompetitors = competitors
                .Where(dto => !alreadyLoadedIds.Contains(dto.CompetitorInTeamId))
                .ToList();
           
            return new JsonResult(filteredCompetitors);
        }
    }
}