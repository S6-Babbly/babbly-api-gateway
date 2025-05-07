using babbly_api_gateway.Models;

namespace babbly_api_gateway.Services;

public interface IPostService
{
    Task<Post?> GetPostById(Guid id);
    Task<List<Post>> GetPosts(int page = 1, int pageSize = 10);
    Task<List<Post>> GetPostsByUserId(Guid userId);
    Task<List<Post>> GetPostsByUserId(Guid userId, int page, int pageSize);
    Task<List<Post>> GetFeed(Guid userId, int page = 1, int pageSize = 10);
}