using Product.Integration.Constants;
using Product.Integration.Interfaces;
using Product.Integration.Models;
using System.Text.Json;

namespace Product.Integration
{
    public class BloombergAuthService : IIntegrationAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IntegrationConfig _integrationConfig;
        private string _accessToken;
        private DateTime _tokenExpiry;
        private readonly object _lock = new object();

        public BloombergAuthService(HttpClient httpClient, IntegrationConfig integrationConfig)
        {
            _httpClient = httpClient;
            _integrationConfig = integrationConfig;
        }

        public async Task<string> GetToken()
        {
            // Check if token is already available and valid
            lock (_lock)
            {
                if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiry > DateTime.UtcNow)
                {
                    return _accessToken;
                }
            }

            // Authentication request
            var authRequest = new HttpRequestMessage(HttpMethod.Post, _integrationConfig.BloombergConfig.AuthApiEndpoint);

            // Add necessary headers or body parameters here
            authRequest.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>(IntegrationConstants.ClientId, _integrationConfig.BloombergConfig.ClientId),
                new KeyValuePair<string, string>(IntegrationConstants.ClientSecret, _integrationConfig.BloombergConfig.ClientSecret),
                new KeyValuePair<string, string>(IntegrationConstants.GrantType, _integrationConfig.BloombergConfig.GrantType)
            });

            var response = await _httpClient.SendAsync(authRequest);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JwtTokenResponse>(content);
            //var tokenResponse = JsonSerializer.Deserialize<JwtTokenResponse>(content, new JsonSerializerOptions
            //{
            //    PropertyNameCaseInsensitive = true
            //});

            lock (_lock)
            {
                _accessToken = tokenResponse.AccessToken;
                _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // Subtract 60 seconds for safety margin
            }

            return _accessToken;
        }
    }
}