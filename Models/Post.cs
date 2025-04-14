namespace babbly_api_gateway.Models;

public class Post
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UserProfileImageUrl { get; set; } = string.Empty;
} 