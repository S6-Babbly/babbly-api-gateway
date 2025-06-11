using System.Net.Http.Headers;
using System.Text.Json;
using babbly_api_gateway.Models;
using System.Text;

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
                var (isValid, _, _) = await ValidateTokenWithPayloadAsync(token);
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return false;
            }
        }

        public async Task<(bool isValid, Dictionary<string, object>? payload, string? error)> ValidateTokenWithPayloadAsync(string token)
        {
            try
            {
                var requestBody = new { Token = token };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/validate", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var validationResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    if (validationResponse.TryGetProperty("valid", out var validProperty) && validProperty.GetBoolean())
                    {
                        var payload = new Dictionary<string, object>();
                        
                        if (validationResponse.TryGetProperty("payload", out var payloadProperty))
                        {
                            foreach (var prop in payloadProperty.EnumerateObject())
                            {
                                payload[prop.Name] = prop.Value.GetString() ?? prop.Value.ToString();
                            }
                        }
                        
                        return (true, payload, null);
                    }
                    else
                    {
                        var error = validationResponse.TryGetProperty("error", out var errorProperty) 
                            ? errorProperty.GetString() 
                            : "Token validation failed";
                        return (false, null, error);
                    }
                }
                else
                {
                    return (false, null, $"Auth service error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token with payload");
                return (false, null, $"Validation error: {ex.Message}");
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