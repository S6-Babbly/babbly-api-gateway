using babbly_api_gateway.Models;

namespace babbly_api_gateway.Services;

public interface IUserService
{
    Task<User?> GetUserById(string id);
    Task<User?> GetUserByUsername(string username);
    Task<List<User>> GetUsers();
    Task<List<User>> GetFollowers(string userId);
    Task<List<User>> GetFollowing(string userId);
} 