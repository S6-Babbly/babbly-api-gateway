using babbly_api_gateway.Aggregators;
using babbly_api_gateway.Models;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<ActionResult<List<AggregatedPost>>> GetFeed(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        // Extract current user ID if authenticated
        Guid? currentUserId = null;
        if (HttpContext.Items.TryGetValue("CurrentUserId", out var userIdObj) && userIdObj is string userId)
        {
            if (Guid.TryParse(userId, out var parsedUserId))
            {
                currentUserId = parsedUserId;
            }
        }

        var feed = await _feedAggregator.GetFeed(page, pageSize, currentUserId);
        return Ok(feed);
    }

    [HttpGet("{postId}")]
    public async Task<ActionResult<AggregatedPost>> GetPostDetails(Guid postId)
    {
        // Extract current user ID if authenticated
        Guid? currentUserId = null;
        if (HttpContext.Items.TryGetValue("CurrentUserId", out var userIdObj) && userIdObj is string userId)
        {
            if (Guid.TryParse(userId, out var parsedUserId))
            {
                currentUserId = parsedUserId;
            }
        }

        var postDetails = await _feedAggregator.GetPostDetails(postId, currentUserId);
        if (postDetails == null)
        {
            return NotFound();
        }

        return Ok(postDetails);
    }
} 