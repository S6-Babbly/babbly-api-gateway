using babbly_api_gateway.Models;

namespace babbly_api_gateway.Services;

public interface ICommentService
{
    Task<List<Comment>> GetCommentsByPostId(Guid postId, int page = 1, int pageSize = 20);
    Task<List<Comment>> GetCommentsByUserId(Guid userId, int page = 1, int pageSize = 20);
    Task<Comment?> GetCommentById(Guid id);
}