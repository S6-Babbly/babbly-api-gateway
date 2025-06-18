using System;

namespace babbly_api_gateway.Models;

public class Post
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UserProfileImageUrl { get; set; } = string.Empty;
}

// Frontend-compatible flattened post format
public class FlattenedPost
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
    public int Likes { get; set; } // Like count
    public int CommentCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    
    // User information for display
    public User? User { get; set; }
} 