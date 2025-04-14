using babbly_api_gateway.Models;

namespace babbly_api_gateway.Services;

public interface ILikeService
{
    Task<List<Like>> GetLikesByPostId(Guid postId, int page = 1, int pageSize = 50);
    Task<List<Like>> GetLikesByUserId(Guid userId, int page = 1, int pageSize = 50);
    Task<bool> HasUserLikedPost(Guid userId, Guid postId);
} 