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
    /// Create a new post (no authentication required for demo)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        try
        {
            // Use a default user ID for demo purposes
            var userId = request.UserId ?? "demo-user-1";

            // Create the post with the specified or default user ID
            var postData = new
            {
                UserId = userId,
                Content = request.Content,
                MediaUrl = request.MediaUrl ?? ""
            };

            var createdPost = await _postService.CreatePost(postData);
            
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
    public string? UserId { get; set; } // Optional user ID for demo
} 