using babbly_api_gateway.Models;

namespace babbly_api_gateway.Mocks;

public class MockCommentService
{
    private static List<Comment> _comments = new List<Comment>();

    static MockCommentService()
    {
        // Initialize with some fake data
        _comments = new List<Comment>
        {
            new Comment
            {
                Id = Guid.Parse("c1111111-1111-1111-1111-111111111111"),
                PostId = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Content = "Great work! What technologies did you use?",
                CreatedAt = DateTime.Now.AddDays(-2).AddHours(1),
                Username = "janedoe",
                UserProfileImageUrl = "https://randomuser.me/api/portraits/women/1.jpg"
            },
            new Comment
            {
                Id = Guid.Parse("c2222222-2222-2222-2222-222222222222"),
                PostId = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
                UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Content = "Looks awesome. I'd love to collaborate on something like this.",
                CreatedAt = DateTime.Now.AddDays(-2).AddHours(3),
                Username = "bobsmith",
                UserProfileImageUrl = "https://randomuser.me/api/portraits/men/2.jpg"
            },
            new Comment
            {
                Id = Guid.Parse("c3333333-3333-3333-3333-333333333333"),
                PostId = Guid.Parse("a2222222-2222-2222-2222-222222222222"),
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Content = "The color scheme is perfect! Very modern.",
                CreatedAt = DateTime.Now.AddDays(-1).AddHours(2),
                Username = "johndoe",
                UserProfileImageUrl = "https://randomuser.me/api/portraits/men/1.jpg"
            },
            new Comment
            {
                Id = Guid.Parse("c4444444-4444-4444-4444-444444444444"),
                PostId = Guid.Parse("a3333333-3333-3333-3333-333333333333"),
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Content = "Stunning photo! What camera did you use?",
                CreatedAt = DateTime.Now.AddHours(-4),
                Username = "janedoe",
                UserProfileImageUrl = "https://randomuser.me/api/portraits/women/1.jpg"
            },
            new Comment
            {
                Id = Guid.Parse("c5555555-5555-5555-5555-555555555555"),
                PostId = Guid.Parse("a4444444-4444-4444-4444-444444444444"),
                UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Content = "I've been learning Rust too! Check out the Rust Book, it's really helpful.",
                CreatedAt = DateTime.Now.AddHours(-10),
                Username = "bobsmith",
                UserProfileImageUrl = "https://randomuser.me/api/portraits/men/2.jpg"
            }
        };
    }

    public async Task<List<Comment>> GetCommentsByPostId(Guid postId, int page = 1, int pageSize = 20)
    {
        // Simulate async operation
        await Task.Delay(75);
        return _comments
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public async Task<List<Comment>> GetCommentsByUserId(Guid userId, int page = 1, int pageSize = 20)
    {
        await Task.Delay(75);
        return _comments
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public async Task<Comment?> GetCommentById(Guid id)
    {
        await Task.Delay(50);
        return _comments.FirstOrDefault(c => c.Id == id);
    }
} 