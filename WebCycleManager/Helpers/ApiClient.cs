namespace WebCycleManager.Helpers
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            var baseUrl = config["ApiSettings:BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException(
                    "APISettings:BaseUrl is niet geconfigureerd.");
            }

            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<HttpResponseMessage> PostToApiAsync(string endpoint)
        {
            var response = await _httpClient.PostAsync(endpoint, null);
            response.EnsureSuccessStatusCode();
            return response;
        }
    }
}
