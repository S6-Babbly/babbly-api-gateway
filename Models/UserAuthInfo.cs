namespace babbly_api_gateway.Models
{
    public class UserAuthInfo
    {
        public string UserId { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public string Email { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; }
        public string Username { get; set; } = string.Empty;
    }
} 