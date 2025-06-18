using babbly_api_gateway.Models;
using babbly_api_gateway.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace babbly_api_gateway.Aggregators;

public class FeedAggregator
{
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;
    private readonly ILikeService _likeService;
    private readonly IUserService _userService;

    public FeedAggregator(IPostService postService, ICommentService commentService, ILikeService likeService, IUserService userService)
    {
        _postService = postService;
        _commentService = commentService;
        _likeService = likeService;
        _userService = userService;
    }

    public async Task<List<AggregatedPost>> GetFeed(int page = 1, int pageSize = 10, string? currentUserId = null)
    {
        var posts = await _postService.GetPosts(page, pageSize);
        return await AggregatePostsData(posts, currentUserId);
    }

    public async Task<AggregatedPost?> GetPostDetails(Guid postId, string? currentUserId = null)
    {
        var post = await _postService.GetPostById(postId);
        if (post == null)
            return null;

        var user = await _userService.GetUserById(post.UserId);
        var comments = await _commentService.GetCommentsByPostId(postId);
        var likes = await _likeService.GetLikesByPostId(postId);
        bool isLikedByCurrentUser = false;

        if (!string.IsNullOrEmpty(currentUserId))
        {
            isLikedByCurrentUser = await _likeService.HasUserLikedPost(currentUserId, postId);
        }

        return new AggregatedPost
        {
            Post = post,
            User = user,
            Comments = comments,
            Likes = likes,
            LikesCount = likes.Count,
            IsLikedByCurrentUser = isLikedByCurrentUser
        };
    }

    private async Task<List<AggregatedPost>> AggregatePostsData(List<Post> posts, string? currentUserId = null)
    {
        var result = new List<AggregatedPost>();

        foreach (var post in posts)
        {
            Console.WriteLine($"🔄 FeedAggregator: Processing post {post.Id} by user {post.UserId}");
            
            var user = await _userService.GetUserById(post.UserId);
            Console.WriteLine($"👤 FeedAggregator: User data for {post.UserId}: {(user != null ? $"Found - {user.Username}" : "NOT FOUND")}");
            
            var comments = await _commentService.GetCommentsByPostId(post.Id, 1, 3); // Just get first 3 comments
            var likes = await _likeService.GetLikesByPostId(post.Id, 1, 5); // Just get first 5 likes
            bool isLikedByCurrentUser = false;

            if (!string.IsNullOrEmpty(currentUserId))
            {
                isLikedByCurrentUser = await _likeService.HasUserLikedPost(currentUserId, post.Id);
            }

            var aggregatedPost = new AggregatedPost
            {
                Post = post,
                User = user,
                Comments = comments,
                Likes = likes,
                LikesCount = post.LikesCount, // Trust the count on the post
                IsLikedByCurrentUser = isLikedByCurrentUser
            };
            
            Console.WriteLine($"📦 FeedAggregator: Aggregated post has user: {(aggregatedPost.User != null ? "YES" : "NO")}");
            result.Add(aggregatedPost);
        }

        return result;
    }
} 