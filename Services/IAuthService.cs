namespace babbly_api_gateway.Services
{
    public interface IAuthService
    {
        Task<bool> ValidateTokenAsync(string token);
        Task<(bool isValid, Dictionary<string, object>? payload, string? error)> ValidateTokenWithPayloadAsync(string token);
        Task<bool> IsAuthorizedAsync(string token, string userId, string resourcePath, string operation);
        Task<UserAuthInfo> GetUserInfoFromTokenAsync(string token);
    }

    public class UserAuthInfo
    {
        public string UserId { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public bool IsAuthenticated { get; set; }
        public Dictionary<string, string> Claims { get; set; } = new();
    }
} 