using System.Diagnostics;
using System.Text.Json;
using System.Web;
using WebCycleApp.Models;

namespace WebCycleApp.Services
{
    public class RestService : IRestService
    {
        HttpClient _client;
        JsonSerializerOptions _serializerOptions;
        IHttpsClientHandlerService _httpsClientHandlerService;

        public List<Event> Events { get; private set; }
        public List<Competitor> Competitors { get; private set; }

        public RestService(IHttpsClientHandlerService service)
        {
#if DEBUG
            _httpsClientHandlerService = service;
            HttpMessageHandler handler = _httpsClientHandlerService.GetPlatformMessageHandler();
            if (handler != null)
                _client = new HttpClient(handler);
            else
                _client = new HttpClient();
#else
            _client = new HttpClient();
#endif
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        public async Task<Event> GetEventByEventId(int id)
        {
            Events = new List<Event>();
            Uri uri = new Uri($"{Constants.EventUrl}/{id}");

            try
            {
                HttpResponseMessage response = await _client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    Events = JsonSerializer.Deserialize<List<Event>>(content, _serializerOptions);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(@"\tERROR {0}", ex.Message);
            }
            return Events.First();
        }

        public async Task<List<Competitor>> GetRandomCompetitorListByEventId(int id, int number)
        {
            Competitors = new List<Competitor>();
            Uri uri = new Uri($"{Constants.CompetitorUrl}/{id}/{number}");

            try
            {
                HttpResponseMessage response = await _client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    Competitors = JsonSerializer.Deserialize<List<Competitor>>(content, _serializerOptions);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"\tERROR {0}", ex.Message);
            }
            return Competitors;
        }
    }
}
