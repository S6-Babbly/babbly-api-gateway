using babbly_api_gateway.Models;

namespace babbly_api_gateway.Services;

public interface IPostService
{
    Task<List<Post>> GetPosts(int page = 1, int pageSize = 10);
    Task<Post?> GetPostById(Guid id);
    Task<List<Post>> GetPostsByUserId(Guid userId, int page = 1, int pageSize = 10);
}