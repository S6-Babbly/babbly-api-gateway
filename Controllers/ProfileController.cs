using babbly_api_gateway.Aggregators;
using babbly_api_gateway.Models;
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
        string userId, 
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
    public async Task<ActionResult<UserProfile>> GetMyProfile(
        [FromQuery] int postsPage = 1, 
        [FromQuery] int postsPageSize = 10,
        [FromQuery] string? userId = null)
    {
        // Use provided userId or default for demo
        var demoUserId = userId ?? "demo-user-1";

        var profile = await _profileAggregator.GetUserProfileById(demoUserId, postsPage, postsPageSize);
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }
} 