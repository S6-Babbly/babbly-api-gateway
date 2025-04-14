using babbly_api_gateway.Models;
using babbly_api_gateway.Services;

namespace babbly_api_gateway.Aggregators;

public class FeedAggregator
{
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;
    private readonly ILikeService _likeService;

    public FeedAggregator(IPostService postService, ICommentService commentService, ILikeService likeService)
    {
        _postService = postService;
        _commentService = commentService;
        _likeService = likeService;
    }

    public async Task<List<AggregatedPost>> GetFeed(int page = 1, int pageSize = 10, Guid? currentUserId = null)
    {
        var posts = await _postService.GetPosts(page, pageSize);
        return await AggregatePostsData(posts, currentUserId);
    }

    public async Task<AggregatedPost?> GetPostDetails(Guid postId, Guid? currentUserId = null)
    {
        var post = await _postService.GetPostById(postId);
        if (post == null)
            return null;

        var comments = await _commentService.GetCommentsByPostId(postId);
        var likes = await _likeService.GetLikesByPostId(postId);
        bool isLikedByCurrentUser = false;

        if (currentUserId.HasValue)
        {
            isLikedByCurrentUser = await _likeService.HasUserLikedPost(currentUserId.Value, postId);
        }

        return new AggregatedPost
        {
            Post = post,
            Comments = comments,
            Likes = likes,
            LikesCount = likes.Count,
            IsLikedByCurrentUser = isLikedByCurrentUser
        };
    }

    private async Task<List<AggregatedPost>> AggregatePostsData(List<Post> posts, Guid? currentUserId = null)
    {
        var result = new List<AggregatedPost>();

        foreach (var post in posts)
        {
            var comments = await _commentService.GetCommentsByPostId(post.Id, 1, 3); // Just get first 3 comments
            var likes = await _likeService.GetLikesByPostId(post.Id, 1, 5); // Just get first 5 likes
            bool isLikedByCurrentUser = false;

            if (currentUserId.HasValue)
            {
                isLikedByCurrentUser = await _likeService.HasUserLikedPost(currentUserId.Value, post.Id);
            }

            result.Add(new AggregatedPost
            {
                Post = post,
                Comments = comments,
                Likes = likes,
                LikesCount = post.LikesCount, // Trust the count on the post
                IsLikedByCurrentUser = isLikedByCurrentUser
            });
        }

        return result;
    }
} 