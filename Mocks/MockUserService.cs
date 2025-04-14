using babbly_api_gateway.Models;

namespace babbly_api_gateway.Mocks;

public class MockUserService
{
    private static List<User> _users = new List<User>();

    static MockUserService()
    {
        // Initialize with some fake data
        _users = new List<User>
        {
            new User
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Username = "johndoe",
                Email = "john@example.com",
                DisplayName = "John Doe",
                ProfileImageUrl = "https://randomuser.me/api/portraits/men/1.jpg",
                Bio = "Software developer and coffee lover",
                CreatedAt = DateTime.Now.AddYears(-2),
                Auth0Id = "auth0|123456789",
                Followers = new List<string> { "janedoe", "bobsmith" },
                Following = new List<string> { "janedoe" }
            },
            new User
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Username = "janedoe",
                Email = "jane@example.com",
                DisplayName = "Jane Doe",
                ProfileImageUrl = "https://randomuser.me/api/portraits/women/1.jpg",
                Bio = "UX Designer | Travel enthusiast",
                CreatedAt = DateTime.Now.AddYears(-1),
                Auth0Id = "auth0|987654321",
                Followers = new List<string> { "johndoe" },
                Following = new List<string> { "johndoe", "bobsmith" }
            },
            new User
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Username = "bobsmith",
                Email = "bob@example.com",
                DisplayName = "Bob Smith",
                ProfileImageUrl = "https://randomuser.me/api/portraits/men/2.jpg",
                Bio = "Photographer and technology geek",
                CreatedAt = DateTime.Now.AddMonths(-8),
                Auth0Id = "auth0|456123789",
                Followers = new List<string> { "janedoe" },
                Following = new List<string> { "johndoe" }
            }
        };
    }

    public async Task<User?> GetUserById(Guid id)
    {
        // Simulate async operation
        await Task.Delay(50);
        return _users.FirstOrDefault(u => u.Id == id);
    }

    public async Task<User?> GetUserByUsername(string username)
    {
        await Task.Delay(50);
        return _users.FirstOrDefault(u => u.Username == username);
    }

    public async Task<List<User>> GetUsers()
    {
        await Task.Delay(50);
        return _users;
    }

    public async Task<List<User>> GetFollowers(Guid userId)
    {
        await Task.Delay(50);
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return new List<User>();

        return _users.Where(u => user.Followers.Contains(u.Username)).ToList();
    }

    public async Task<List<User>> GetFollowing(Guid userId)
    {
        await Task.Delay(50);
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return new List<User>();

        return _users.Where(u => user.Following.Contains(u.Username)).ToList();
    }
} 