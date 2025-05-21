namespace WebCycleManager.Helpers
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(config["ApiSettings:BaseUrl"]);
        }

        public async Task<HttpResponseMessage> PostToApiAsync(string endpoint)
        {
            var response = await _httpClient.PostAsync(endpoint, null);
            response.EnsureSuccessStatusCode();
            return response;
        }
    }
}
