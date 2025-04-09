using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace babbly_api_gateway.Controllers
{
    [ApiController]
    public class MockController : ControllerBase
    {
        private readonly ILogger<MockController> _logger;
        private readonly IConfiguration _configuration;
        private readonly bool _useMockServices;

        public MockController(ILogger<MockController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _useMockServices = _configuration.GetValue<bool>("MOCK_SERVICES") || 
                               Environment.GetEnvironmentVariable("MOCK_SERVICES")?.ToLower() == "true";
        }

        private bool ShouldMock()
        {
            // Only mock requests when MOCK_SERVICES is enabled
            return _useMockServices;
        }

        // Handle all Posts endpoints
        [Route("api/posts")]
        [Route("api/posts/{**everything}")]
        [HttpGet, HttpPost, HttpPut, HttpDelete, HttpPatch]
        public IActionResult MockPosts()
        {
            if (!ShouldMock()) return NotFound();
            
            _logger.LogInformation("Mocked Post Service Request");
            return Ok(new
            {
                success = true,
                message = "This is a mock response from the Post service",
                data = new[]
                {
                    new { id = 1, title = "Mock Post 1", content = "This is a mock post", userId = 1 },
                    new { id = 2, title = "Mock Post 2", content = "This is another mock post", userId = 2 }
                }
            });
        }

        // Handle all Users endpoints
        [Route("api/users")]
        [Route("api/users/{**everything}")]
        [HttpGet, HttpPost, HttpPut, HttpDelete, HttpPatch]
        public IActionResult MockUsers()
        {
            if (!ShouldMock()) return NotFound();
            
            _logger.LogInformation("Mocked User Service Request");
            return Ok(new
            {
                success = true,
                message = "This is a mock response from the User service",
                data = new[]
                {
                    new { id = 1, username = "mockuser1", email = "user1@example.com" },
                    new { id = 2, username = "mockuser2", email = "user2@example.com" }
                }
            });
        }

        // Handle all Comments endpoints
        [Route("api/comments")]
        [Route("api/comments/{**everything}")]
        [HttpGet, HttpPost, HttpPut, HttpDelete, HttpPatch]
        public IActionResult MockComments()
        {
            if (!ShouldMock()) return NotFound();
            
            _logger.LogInformation("Mocked Comment Service Request");
            return Ok(new
            {
                success = true,
                message = "This is a mock response from the Comment service",
                data = new[]
                {
                    new { id = 1, content = "Mock comment 1", postId = 1, userId = 1 },
                    new { id = 2, content = "Mock comment 2", postId = 1, userId = 2 }
                }
            });
        }

        // Handle all Likes endpoints
        [Route("api/likes")]
        [Route("api/likes/{**everything}")]
        [HttpGet, HttpPost, HttpPut, HttpDelete, HttpPatch]
        public IActionResult MockLikes()
        {
            if (!ShouldMock()) return NotFound();
            
            _logger.LogInformation("Mocked Like Service Request");
            return Ok(new
            {
                success = true,
                message = "This is a mock response from the Like service",
                data = new[]
                {
                    new { id = 1, postId = 1, userId = 1 },
                    new { id = 2, postId = 1, userId = 2 }
                }
            });
        }

        // Mock health endpoints for each service
        [Route("api/health/posts")]
        [HttpGet]
        public IActionResult MockPostsHealth()
        {
            if (!ShouldMock()) return NotFound();
            
            _logger.LogInformation("Mocked Post Service Health Request");
            return Ok(new
            {
                status = "Healthy (Mock)",
                service = "Posts",
                timestamp = DateTime.UtcNow
            });
        }

        [Route("api/health/users")]
        [HttpGet]
        public IActionResult MockUsersHealth()
        {
            if (!ShouldMock()) return NotFound();
            
            _logger.LogInformation("Mocked User Service Health Request");
            return Ok(new
            {
                status = "Healthy (Mock)",
                service = "Users",
                timestamp = DateTime.UtcNow
            });
        }

        [Route("api/health/comments")]
        [HttpGet]
        public IActionResult MockCommentsHealth()
        {
            if (!ShouldMock()) return NotFound();
            
            _logger.LogInformation("Mocked Comment Service Health Request");
            return Ok(new
            {
                status = "Healthy (Mock)",
                service = "Comments",
                timestamp = DateTime.UtcNow
            });
        }

        [Route("api/health/likes")]
        [HttpGet]
        public IActionResult MockLikesHealth()
        {
            if (!ShouldMock()) return NotFound();
            
            _logger.LogInformation("Mocked Like Service Health Request");
            return Ok(new
            {
                status = "Healthy (Mock)",
                service = "Likes",
                timestamp = DateTime.UtcNow
            });
        }
    }
} 