using System.Net.Http.Headers;
using System.Text.Json;
using babbly_api_gateway.Models;

namespace babbly_api_gateway.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IHttpClientFactory httpClientFactory, ILogger<AuthService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("AuthService");
            _logger = logger;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("/api/auth/validate");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return false;
            }
        }

        public async Task<bool> IsAuthorizedAsync(string token, string userId, string resourcePath, string operation)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync($"/api/auth/authorize?userId={userId}&resourcePath={resourcePath}&operation={operation}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking authorization");
                return false;
            }
        }

        public async Task<UserAuthInfo> GetUserInfoFromTokenAsync(string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("/api/auth/userinfo");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var userInfo = JsonSerializer.Deserialize<UserAuthInfo>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return userInfo ?? new UserAuthInfo();
                }

                return new UserAuthInfo();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info from token");
                return new UserAuthInfo();
            }
        }
    }
} 