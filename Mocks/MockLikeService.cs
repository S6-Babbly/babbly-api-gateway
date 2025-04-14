using babbly_api_gateway.Models;

namespace babbly_api_gateway.Mocks;

public class MockLikeService
{
    private static List<Like> _likes = new List<Like>();

    static MockLikeService()
    {
        // Initialize with some fake data
        _likes = new List<Like>
        {
            new Like
            {
                Id = Guid.Parse("l1111111-1111-1111-1111-111111111111"),
                PostId = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                CreatedAt = DateTime.Now.AddDays(-2).AddMinutes(30),
                Username = "janedoe",
                UserProfileImageUrl = "https://randomuser.me/api/portraits/women/1.jpg"
            },
            new Like
            {
                Id = Guid.Parse("l2222222-2222-2222-2222-222222222222"),
                PostId = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
                UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                CreatedAt = DateTime.Now.AddDays(-2).AddHours(1),
                Username = "bobsmith",
                UserProfileImageUrl = "https://randomuser.me/api/portraits/men/2.jpg"
            },
            new Like
            {
                Id = Guid.Parse("l3333333-3333-3333-3333-333333333333"),
                PostId = Guid.Parse("a2222222-2222-2222-2222-222222222222"),
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CreatedAt = DateTime.Now.AddDays(-1).AddMinutes(45),
                Username = "johndoe",
                UserProfileImageUrl = "https://randomuser.me/api/portraits/men/1.jpg"
            },
            new Like
            {
                Id = Guid.Parse("l4444444-4444-4444-4444-444444444444"),
                PostId = Guid.Parse("a3333333-3333-3333-3333-333333333333"),
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CreatedAt = DateTime.Now.AddHours(-4).AddMinutes(15),
                Username = "johndoe",
                UserProfileImageUrl = "https://randomuser.me/api/portraits/men/1.jpg"
            },
            new Like
            {
                Id = Guid.Parse("l5555555-5555-5555-5555-555555555555"),
                PostId = Guid.Parse("a3333333-3333-3333-3333-333333333333"),
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                CreatedAt = DateTime.Now.AddHours(-4).AddMinutes(30),
                Username = "janedoe",
                UserProfileImageUrl = "https://randomuser.me/api/portraits/women/1.jpg"
            }
        };
    }

    public async Task<List<Like>> GetLikesByPostId(Guid postId, int page = 1, int pageSize = 50)
    {
        // Simulate async operation
        await Task.Delay(60);
        return _likes
            .Where(l => l.PostId == postId)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public async Task<List<Like>> GetLikesByUserId(Guid userId, int page = 1, int pageSize = 50)
    {
        await Task.Delay(60);
        return _likes
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public async Task<bool> HasUserLikedPost(Guid userId, Guid postId)
    {
        await Task.Delay(30);
        return _likes.Any(l => l.UserId == userId && l.PostId == postId);
    }
} 