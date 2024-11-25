using Microsoft.Extensions.Logging;
using Product.Bal;
using Product.Integration.Interfaces;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Product.Integration
{
    public class IntegrationService : IIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly IIntegrationAuthService _authService;
        private readonly ILogger<IntegrationService> _logger;

        public IntegrationService(IHttpClientFactory httpClientFactory, IIntegrationAuthService authService, ILogger<IntegrationService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultClient");
            //_httpClient.DefaultRequestHeaders.Add("User-Agent", "Argiro-App");
            _authService = authService;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> ExecuteGetRequestAsync(string url)
        {
            //using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
            var token = await _authService.GetToken();

            //var request = new HttpRequestMessage(HttpMethod.Get, "https://api.bloomberg.com/eap/catalogs/54357/requests/u2G2fiiRXSb1/fieldList/?pageSize=500");
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());
            request.Headers.Add("api-version", "2"); // Add any other headers
            //request.Headers.Add("Accept", "*/*"); // Add any other headers
            //request.Headers.Add("Accept-Encoding", "gzip,deflate,br"); // Add any other headers
            //request.Headers.Add("Connection", "keep-alive"); // Add any other headers

            var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Error in request: {Content}", content.ToString());
                _logger.LogError("Error in response: {Response}", response.ToString());
            }

            response.EnsureSuccessStatusCode();

            return response;
        }

        public async Task<T> ExecutePostRequestAsync<T>(string url, T body)
        {
            var token = await _authService.GetToken();

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("Some-Additional-Header", "HeaderValue"); // Add any other headers

            var jsonBody = JsonSerializer.Serialize(body);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content);
        }
    }
}