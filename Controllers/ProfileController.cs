using babbly_api_gateway.Aggregators;
using babbly_api_gateway.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace babbly_api_gateway.Controllers;

[ApiController]
[Route("api/profiles")]
public class ProfileController : ControllerBase
{
    private readonly ProfileAggregator _profileAggregator;

    public ProfileController(ProfileAggregator profileAggregator)
    {
        _profileAggregator = profileAggregator;
    }

    [HttpGet("id/{userId}")]
    public async Task<ActionResult<UserProfile>> GetProfileById(
        Guid userId, 
        [FromQuery] int postsPage = 1, 
        [FromQuery] int postsPageSize = 10)
    {
        var profile = await _profileAggregator.GetUserProfileById(userId, postsPage, postsPageSize);
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpGet("username/{username}")]
    public async Task<ActionResult<UserProfile>> GetProfileByUsername(
        string username, 
        [FromQuery] int postsPage = 1, 
        [FromQuery] int postsPageSize = 10)
    {
        var profile = await _profileAggregator.GetUserProfileByUsername(username, postsPage, postsPageSize);
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserProfile>> GetMyProfile(
        [FromQuery] int postsPage = 1, 
        [FromQuery] int postsPageSize = 10)
    {
        if (!HttpContext.Items.TryGetValue("CurrentUserId", out var userIdObj) || userIdObj is not string userId)
        {
            return Unauthorized();
        }

        if (!Guid.TryParse(userId, out var currentUserId))
        {
            return BadRequest("Invalid user ID format");
        }

        var profile = await _profileAggregator.GetUserProfileById(currentUserId, postsPage, postsPageSize);
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }
} 