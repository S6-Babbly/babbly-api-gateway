using System.Net.Http.Headers;
using System.Text.Json;
using babbly_api_gateway.Models;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace babbly_api_gateway.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public AuthService(IHttpClientFactory httpClientFactory, ILogger<AuthService> logger, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("AuthService");
            _logger = logger;
            _configuration = configuration;
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            var (isValid, _, _) = await ValidateTokenWithPayloadAsync(token);
            return isValid;
        }

        public async Task<(bool isValid, Dictionary<string, object>? payload, string? error)> ValidateTokenWithPayloadAsync(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return (false, null, "Token is empty");
                }

                // Read JWT token without validation first to get claims
                var jsonToken = _tokenHandler.ReadJwtToken(token);
                
                if (jsonToken == null)
                {
                    return (false, null, "Invalid token format");
                }

                // Check token expiration
                if (jsonToken.ValidTo < DateTime.UtcNow)
                {
                    return (false, null, "Token has expired");
                }

                // Extract claims into payload dictionary
                var payload = new Dictionary<string, object>();
                
                foreach (var claim in jsonToken.Claims)
                {
                    if (payload.ContainsKey(claim.Type))
                    {
                        // Handle multiple values for the same claim type
                        if (payload[claim.Type] is List<string> existingList)
                        {
                            existingList.Add(claim.Value);
                        }
                        else
                        {
                            payload[claim.Type] = new List<string> { payload[claim.Type].ToString()!, claim.Value };
                        }
                    }
                    else
                    {
                        payload[claim.Type] = claim.Value;
                    }
                }

                // Ensure we have required claims
                if (!payload.ContainsKey("sub"))
                {
                    return (false, null, "Token missing subject claim");
                }

                _logger.LogInformation("Token validated successfully for user: {UserId}", payload["sub"]);
                return (true, payload, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return (false, null, $"Token validation error: {ex.Message}");
            }
        }

        public async Task<bool> IsAuthorizedAsync(string token, string userId, string resourcePath, string operation)
        {
            // For now, return true for all authenticated users
            // You can implement more sophisticated authorization logic here
            var (isValid, payload, _) = await ValidateTokenWithPayloadAsync(token);
            
            if (!isValid || payload == null)
            {
                return false;
            }

            // Check if the user in the token matches the requested userId
            if (payload.TryGetValue("sub", out var tokenUserId) && tokenUserId.ToString() == userId)
            {
                return true;
            }

            // Check for admin role
            if (payload.TryGetValue("https://babbly.com/roles", out var roles))
            {
                if (roles is string roleString && roleString.Contains("admin"))
                {
                    return true;
                }
                if (roles is List<string> roleList && roleList.Contains("admin"))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<UserAuthInfo> GetUserInfoFromTokenAsync(string token)
        {
            var (isValid, payload, error) = await ValidateTokenWithPayloadAsync(token);
            
            if (!isValid || payload == null)
            {
                return new UserAuthInfo
                {
                    UserId = string.Empty,
                    IsAuthenticated = false,
                    Roles = new List<string>(),
                    Claims = new Dictionary<string, string>()
                };
            }

            var userInfo = new UserAuthInfo
            {
                UserId = payload.GetValueOrDefault("sub")?.ToString() ?? string.Empty,
                IsAuthenticated = true,
                Roles = new List<string>(),
                Claims = new Dictionary<string, string>()
            };

            // Extract roles
            if (payload.TryGetValue("https://babbly.com/roles", out var roles))
            {
                if (roles is string roleString)
                {
                    userInfo.Roles.Add(roleString);
                }
                else if (roles is List<string> roleList)
                {
                    userInfo.Roles.AddRange(roleList);
                }
            }

            // Extract other claims
            foreach (var kvp in payload)
            {
                if (kvp.Value != null)
                {
                    userInfo.Claims[kvp.Key] = kvp.Value.ToString()!;
                }
            }

            return userInfo;
        }
    }
} 