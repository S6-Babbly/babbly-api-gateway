using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace babbly_api_gateway.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public HealthController(
        ILogger<HealthController> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
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

    [HttpGet("services")]
    public async Task<IActionResult> CheckServices()
    {
        var results = new Dictionary<string, object>();
        
        // Gateway health
        results.Add("gateway", new { 
            status = "healthy", 
            timestamp = DateTime.UtcNow 
        });
        
        // Check individual services
        var serviceChecks = new List<Task>();
        
        serviceChecks.Add(CheckServiceHealth("UserService", "users", results));
        serviceChecks.Add(CheckServiceHealth("PostService", "posts", results));
        serviceChecks.Add(CheckServiceHealth("CommentService", "comments", results));
        serviceChecks.Add(CheckServiceHealth("LikeService", "likes", results));
        
        await Task.WhenAll(serviceChecks);
        
        return Ok(results);
    }

    private async Task CheckServiceHealth(string clientName, string serviceName, Dictionary<string, object> results)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(clientName);
            var response = await client.GetAsync("api/health");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var healthData = JsonSerializer.Deserialize<JsonDocument>(content);
                results.Add(serviceName, healthData);
            }
            else
            {
                results.Add(serviceName, new { status = "unhealthy", statusCode = response.StatusCode });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking {ServiceName} health", serviceName);
            results.Add(serviceName, new { status = "error", message = ex.Message });
        }
    }
} 