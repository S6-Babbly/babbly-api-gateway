namespace babbly_api_gateway.Models;

public class UserProfile
{
    public User User { get; set; } = null!;
    public List<Post> Posts { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
    public int PostsCount { get; set; }
    public int CommentsCount { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
} 