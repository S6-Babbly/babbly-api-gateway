using babbly_api_gateway.Models;

namespace babbly_api_gateway.Mocks;

public class MockPostService
{
    private static List<Post> _posts = new List<Post>();

    static MockPostService()
    {
        // Initialize with some fake data
        _posts = new List<Post>
        {
            new Post
            {
                Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Content = "Just finished building a new feature for our app! #coding #productivity",
                MediaUrl = "https://source.unsplash.com/random/800x600/?coding",
                CreatedAt = DateTime.Now.AddDays(-2),
                LikesCount = 24,
                CommentsCount = 5,
                Username = "johndoe",
                UserProfileImageUrl = "https://randomuser.me/api/portraits/men/1.jpg"
            },
            new Post
            {
                Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"),
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Content = "Working on a new UI design. Feedback welcome! #design #ux",
                MediaUrl = "https://source.unsplash.com/random/800x600/?design",
                CreatedAt = DateTime.Now.AddDays(-1),
                LikesCount = 42,
                CommentsCount = 8,
                Username = "janedoe",
                UserProfileImageUrl = "https://randomuser.me/api/portraits/women/1.jpg"
            },
            new Post
            {
                Id = Guid.Parse("a3333333-3333-3333-3333-333333333333"),
                UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Content = "Captured this amazing sunset today. #photography #nature",
                MediaUrl = "https://source.unsplash.com/random/800x600/?sunset",
                CreatedAt = DateTime.Now.AddHours(-5),
                LikesCount = 76,
                CommentsCount = 12,
                Username = "bobsmith",
                UserProfileImageUrl = "https://randomuser.me/api/portraits/men/2.jpg"
            },
            new Post
            {
                Id = Guid.Parse("a4444444-4444-4444-4444-444444444444"),
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Content = "Learning a new programming language today. Any tips for mastering Rust? #coding #rust",
                MediaUrl = "",
                CreatedAt = DateTime.Now.AddHours(-12),
                LikesCount = 18,
                CommentsCount = 7,
                Username = "johndoe",
                UserProfileImageUrl = "https://randomuser.me/api/portraits/men/1.jpg"
            },
            new Post
            {
                Id = Guid.Parse("a5555555-5555-5555-5555-555555555555"),
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Content = "Just had an amazing user testing session. So many insights! #ux #research",
                MediaUrl = "",
                CreatedAt = DateTime.Now.AddDays(-3),
                LikesCount = 31,
                CommentsCount = 4,
                Username = "janedoe",
                UserProfileImageUrl = "https://randomuser.me/api/portraits/women/1.jpg"
            }
        };
    }

    public async Task<List<Post>> GetPosts(int page = 1, int pageSize = 10)
    {
        // Simulate async operation
        await Task.Delay(100);
        return _posts
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public async Task<Post?> GetPostById(Guid id)
    {
        await Task.Delay(50);
        return _posts.FirstOrDefault(p => p.Id == id);
    }

    public async Task<List<Post>> GetPostsByUserId(Guid userId, int page = 1, int pageSize = 10)
    {
        await Task.Delay(75);
        return _posts
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }
} 