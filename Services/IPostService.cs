using babbly_api_gateway.Models;

namespace babbly_api_gateway.Services;

public interface IPostService
{
    Task<Post?> GetPostById(Guid id);
    Task<List<Post>> GetPosts(int page = 1, int pageSize = 10);
    Task<List<Post>> GetPostsByUserId(string userId);
    Task<List<Post>> GetPostsByUserId(string userId, int page, int pageSize);
    Task<List<Post>> GetFeed(string userId, int page = 1, int pageSize = 10);
    Task<Post> CreatePost(object postData);
}