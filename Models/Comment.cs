namespace babbly_api_gateway.Models;

public class Comment
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UserProfileImageUrl { get; set; } = string.Empty;
} 