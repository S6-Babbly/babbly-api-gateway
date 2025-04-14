using Microsoft.AspNetCore.Mvc;

namespace babbly_api_gateway.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Check()
    {
        _logger.LogDebug("Health check called");
        return Ok(new { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            version = GetType().Assembly.GetName().Version?.ToString()
        });
    }
} 