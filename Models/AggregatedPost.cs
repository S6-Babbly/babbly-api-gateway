namespace babbly_api_gateway.Models;

public class AggregatedPost
{
    public Post Post { get; set; } = null!;
    public List<Comment> Comments { get; set; } = new();
    public List<Like> Likes { get; set; } = new();
    public int LikesCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
} 