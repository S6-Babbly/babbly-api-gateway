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
            // For demo purposes, always return true
            _logger.LogInformation("Demo mode: Token validation bypassed");
            return await Task.FromResult<bool>(true);
        }

        public async Task<(bool isValid, Dictionary<string, object>? payload, string? error)> ValidateTokenWithPayloadAsync(string token)
        {
            // For demo purposes, return a mock payload
            _logger.LogInformation("Demo mode: Token validation bypassed");
            
            var mockPayload = new Dictionary<string, object>
            {
                ["sub"] = "demo-user-1",
                ["name"] = "Demo User",
                ["email"] = "demo@example.com",
                ["https://babbly.com/roles"] = new[] { "user" }
            };
            
            return await Task.FromResult<(bool, Dictionary<string, object>?, string?)>((true, mockPayload, null));
        }

        public async Task<bool> IsAuthorizedAsync(string token, string userId, string resourcePath, string operation)
        {
            // For demo purposes, always return true (no authorization required)
            _logger.LogInformation("Demo mode: Authorization check bypassed for {UserId} on {ResourcePath}", userId, resourcePath);
            return await Task.FromResult<bool>(true);
        }

        public async Task<UserAuthInfo> GetUserInfoFromTokenAsync(string token)
        {
            // For demo purposes, return mock user info
            _logger.LogInformation("Demo mode: Returning mock user info");
            
            var mockUserInfo = new UserAuthInfo
            {
                UserId = "demo-user-1",
                IsAuthenticated = true,
                Roles = new List<string> { "user" },
                Claims = new Dictionary<string, string>
                {
                    ["email"] = "demo@example.com",
                    ["name"] = "Demo User"
                }
            };

            return await Task.FromResult<UserAuthInfo>(mockUserInfo);
        }
    }
} 