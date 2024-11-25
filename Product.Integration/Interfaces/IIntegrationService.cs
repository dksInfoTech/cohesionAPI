namespace Product.Integration.Interfaces
{
    public interface IIntegrationService
    {
        Task<HttpResponseMessage> ExecuteGetRequestAsync(string url);
        Task<T> ExecutePostRequestAsync<T>(string url, T body);
    }
}