using babbly_api_gateway.Aggregators;
using babbly_api_gateway.Models;
using Microsoft.AspNetCore.Mvc;

namespace babbly_api_gateway.Controllers;

[ApiController]
[Route("api/feed")]
public class FeedController : ControllerBase
{
    private readonly FeedAggregator _feedAggregator;

    public FeedController(FeedAggregator feedAggregator)
    {
        _feedAggregator = feedAggregator;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetFeed(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string? userId = null)
    {
        // Use provided userId or default for demo (can be null for public feed)
        string? currentUserId = userId;

        var feed = await _feedAggregator.GetFeed(page, pageSize, currentUserId);
        
        // Convert to flattened format expected by frontend
        var flattenedPosts = feed.Select(aggregatedPost => new FlattenedPost
        {
            Id = aggregatedPost.Post.Id,
            UserId = aggregatedPost.Post.UserId,
            Content = aggregatedPost.Post.Content,
            MediaUrl = aggregatedPost.Post.MediaUrl,
            CreatedAt = aggregatedPost.Post.CreatedAt,
            TimeAgo = GetTimeAgo(aggregatedPost.Post.CreatedAt),
            Likes = aggregatedPost.LikesCount,
            CommentCount = aggregatedPost.Comments?.Count ?? 0,
            IsLikedByCurrentUser = aggregatedPost.IsLikedByCurrentUser
        }).ToList();
        
        // Return paginated response format expected by frontend
        return Ok(new
        {
            items = flattenedPosts,
            page = page,
            pageSize = pageSize,
            total = flattenedPosts.Count // This is a simplified count, ideally should be total across all pages
        });
    }

    [HttpGet("{postId}")]
    public async Task<ActionResult<FlattenedPost>> GetPostDetails(
        Guid postId,
        [FromQuery] string? userId = null)
    {
        // Use provided userId or default for demo (can be null for public view)
        string? currentUserId = userId;

        var postDetails = await _feedAggregator.GetPostDetails(postId, currentUserId);
        if (postDetails == null)
        {
            return NotFound();
        }

        // Convert to flattened format
        var flattenedPost = new FlattenedPost
        {
            Id = postDetails.Post.Id,
            UserId = postDetails.Post.UserId,
            Content = postDetails.Post.Content,
            MediaUrl = postDetails.Post.MediaUrl,
            CreatedAt = postDetails.Post.CreatedAt,
            TimeAgo = GetTimeAgo(postDetails.Post.CreatedAt),
            Likes = postDetails.LikesCount,
            CommentCount = postDetails.Comments?.Count ?? 0,
            IsLikedByCurrentUser = postDetails.IsLikedByCurrentUser
        };

        return Ok(flattenedPost);
    }

    private static string GetTimeAgo(DateTime createdAt)
    {
        var timeSpan = DateTime.UtcNow - createdAt;

        if (timeSpan.TotalDays >= 365)
            return $"{(int)(timeSpan.TotalDays / 365)}y";
        if (timeSpan.TotalDays >= 30)
            return $"{(int)(timeSpan.TotalDays / 30)}mo";
        if (timeSpan.TotalDays >= 1)
            return $"{(int)timeSpan.TotalDays}d";
        if (timeSpan.TotalHours >= 1)
            return $"{(int)timeSpan.TotalHours}h";
        if (timeSpan.TotalMinutes >= 1)
            return $"{(int)timeSpan.TotalMinutes}m";
        return "now";
    }
} 