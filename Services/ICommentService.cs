using babbly_api_gateway.Models;

namespace babbly_api_gateway.Services;

public interface ICommentService
{
    Task<Comment?> GetCommentById(Guid id);
    Task<List<Comment>> GetCommentsByPostId(Guid postId);
    Task<List<Comment>> GetCommentsByPostId(Guid postId, int page, int pageSize);
    Task<List<Comment>> GetCommentsByUserId(string userId);
    Task<List<Comment>> GetCommentsByUserId(string userId, int page, int pageSize);
}