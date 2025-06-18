using babbly_api_gateway.Aggregators;
using babbly_api_gateway.Models;
using babbly_api_gateway.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace babbly_api_gateway.Controllers;

[ApiController]
[Route("api/profiles")]
public class ProfileController : ControllerBase
{
    private readonly ProfileAggregator _profileAggregator;
    private readonly IUserService _userService;
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;
    private readonly ILikeService _likeService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        ProfileAggregator profileAggregator,
        IUserService userService,
        IPostService postService,
        ICommentService commentService,
        ILikeService likeService,
        IHttpClientFactory httpClientFactory,
        ILogger<ProfileController> logger)
    {
        _profileAggregator = profileAggregator;
        _userService = userService;
        _postService = postService;
        _commentService = commentService;
        _likeService = likeService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
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

    /// <summary>
    /// Get current user's profile
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<User>> GetMyProfile()
    {
        try
        {
            var userId = HttpContext.Items["CurrentUserId"]?.ToString();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var client = _httpClientFactory.CreateClient("UserService");
            
            // Forward the user ID header to the user service
            client.DefaultRequestHeaders.Add("X-User-Id", userId);
            
            var response = await client.GetAsync("/api/users/me");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<User>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return Ok(user);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound("User profile not found");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error getting user profile: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return StatusCode(500, "Error retrieving user profile");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetMyProfile");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update current user's profile
    /// </summary>
    [HttpPut("me")]
    public async Task<ActionResult<User>> UpdateMyProfile([FromBody] UpdateUserProfileRequest updateRequest)
    {
        try
        {
            var userId = HttpContext.Items["CurrentUserId"]?.ToString();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var client = _httpClientFactory.CreateClient("UserService");
            
            // Forward the user ID header to the user service
            client.DefaultRequestHeaders.Add("X-User-Id", userId);
            
            var json = JsonSerializer.Serialize(updateRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await client.PutAsync("/api/users/profile", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<User>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return Ok(user);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return BadRequest(errorContent);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound("User not found");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error updating user profile: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return StatusCode(500, "Error updating user profile");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateMyProfile");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete current user account and all associated data
    /// </summary>
    [HttpDelete("me")]
    public async Task<ActionResult> DeleteMyAccount()
    {
        try
        {
            var userId = HttpContext.Items["CurrentUserId"]?.ToString();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            _logger.LogInformation("Starting account deletion process for user {UserId}", userId);

            // Step 1: Get user data first to get the internal user ID
            var user = await _userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Step 2: Delete all likes by this user
            try
            {
                var likeClient = _httpClientFactory.CreateClient("LikeService");
                await likeClient.DeleteAsync($"/api/likes/user/{userId}");
                _logger.LogInformation("Deleted likes for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deleting likes for user {UserId}", userId);
            }

            // Step 3: Delete all comments by this user
            try
            {
                var commentClient = _httpClientFactory.CreateClient("CommentService");
                await commentClient.DeleteAsync($"/api/comments/user/{userId}");
                _logger.LogInformation("Deleted comments for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deleting comments for user {UserId}", userId);
            }

            // Step 4: Delete all posts by this user
            try
            {
                var postClient = _httpClientFactory.CreateClient("PostService");
                await postClient.DeleteAsync($"/api/posts/user/{userId}");
                _logger.LogInformation("Deleted posts for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deleting posts for user {UserId}", userId);
            }

            // Step 5: Finally delete the user profile
            var userClient = _httpClientFactory.CreateClient("UserService");
            userClient.DefaultRequestHeaders.Add("X-User-Id", userId);
            
            var userResponse = await userClient.DeleteAsync($"/api/users/{user.Id}");
            
            if (userResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully deleted user account {UserId}", userId);
                return Ok(new { message = "Account deleted successfully" });
            }
            else
            {
                var errorContent = await userResponse.Content.ReadAsStringAsync();
                _logger.LogError("Error deleting user: {StatusCode} - {Content}", userResponse.StatusCode, errorContent);
                return StatusCode(500, "Error deleting user account");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteMyAccount for user {UserId}", HttpContext.Items["CurrentUserId"]);
            return StatusCode(500, "Internal server error during account deletion");
        }
    }
}

/// <summary>
/// Request model for updating user profile
/// </summary>
public class UpdateUserProfileRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
} 