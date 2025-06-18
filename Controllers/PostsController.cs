using babbly_api_gateway.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace babbly_api_gateway.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly ILogger<PostsController> _logger;

    public PostsController(IPostService postService, ILogger<PostsController> logger)
    {
        _postService = postService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new post (authentication required)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        try
        {
            // Get authenticated user ID from JWT token headers
            var userId = Request.Headers["X-User-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { error = "Authentication required. User ID not found in token." });
            }

            // Create the post data (PostService will forward user headers)
            var postData = new
            {
                Content = request.Content,
                MediaUrl = request.MediaUrl
            };

            var createdPost = await _postService.CreatePost(postData, HttpContext);
            
            return Ok(createdPost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating post");
            return StatusCode(500, new { error = "Failed to create post" });
        }
    }
}

public class CreatePostRequest
{
    public string Content { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
} 