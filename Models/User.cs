namespace babbly_api_gateway.Models;

public class User
{
    public int Id { get; set; }
    public string Auth0Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public UserExtraData? ExtraData { get; set; }
}

public class UserExtraData
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? DisplayName { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Bio { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 