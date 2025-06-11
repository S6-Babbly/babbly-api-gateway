namespace babbly_api_gateway.Models;

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Auth0Id { get; set; } = string.Empty;
    public List<string> Followers { get; set; } = new();
    public List<string> Following { get; set; } = new();
} 