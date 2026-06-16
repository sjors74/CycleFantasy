namespace WebCycleManager.Helpers
{
    public interface IApiClient
    {
        Task<HttpResponseMessage> PostToApiAsync(string endpoint);
    }
}
