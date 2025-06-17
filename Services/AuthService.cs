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
            // Always return true - authentication is handled elsewhere
            return await Task.FromResult<bool>(true);
        }

        public async Task<(bool isValid, Dictionary<string, object>? payload, string? error)> ValidateTokenWithPayloadAsync(string token)
        {
            // Return a standard payload for compatibility
            var mockPayload = new Dictionary<string, object>
            {
                ["sub"] = "user-1",
                ["name"] = "User",
                ["email"] = "user@example.com",
                ["https://babbly.com/roles"] = new[] { "user" }
            };
            
            return await Task.FromResult<(bool, Dictionary<string, object>?, string?)>((true, mockPayload, null));
        }

        public async Task<bool> IsAuthorizedAsync(string token, string userId, string resourcePath, string operation)
        {
            // Always return true - no authorization required
            return await Task.FromResult<bool>(true);
        }

        public async Task<UserAuthInfo> GetUserInfoFromTokenAsync(string token)
        {
            // Return standard user info
            var mockUserInfo = new UserAuthInfo
            {
                UserId = "user-1",
                IsAuthenticated = true,
                Roles = new List<string> { "user" },
                Claims = new Dictionary<string, string>
                {
                    ["email"] = "user@example.com",
                    ["name"] = "User"
                }
            };

            return await Task.FromResult<UserAuthInfo>(mockUserInfo);
        }
    }
} 