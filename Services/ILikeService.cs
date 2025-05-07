using babbly_api_gateway.Models;

namespace babbly_api_gateway.Services;

public interface ILikeService
{
    Task<List<Like>> GetLikesByPostId(Guid postId);
    Task<List<Like>> GetLikesByPostId(Guid postId, int page, int pageSize);
    Task<List<Like>> GetLikesByUserId(Guid userId);
    Task<bool> HasUserLikedPost(Guid userId, Guid postId);
} 