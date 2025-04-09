using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace babbly_api_gateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HealthController> _logger;
        private readonly bool _useMockServices;

        public HealthController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<HealthController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _useMockServices = _configuration.GetValue<bool>("MOCK_SERVICES") || 
                               Environment.GetEnvironmentVariable("MOCK_SERVICES")?.ToLower() == "true";
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "API Gateway Healthy", timestamp = DateTime.UtcNow });
        }

        [HttpGet("services")]
        public async Task<IActionResult> GetServicesHealthAsync()
        {
            var services = new Dictionary<string, string>
            {
                { "posts", "/api/health/posts" },
                { "users", "/api/health/users" },
                { "comments", "/api/health/comments" },
                { "likes", "/api/health/likes" }
            };

            var result = new Dictionary<string, object>
            {
                { "gateway", new { status = "Healthy", timestamp = DateTime.UtcNow } }
            };

            // If we're in mock mode, return mock healthy status for all services
            if (_useMockServices)
            {
                _logger.LogInformation("Using mock services health status");
                foreach (var service in services)
                {
                    result.Add(service.Key, new { status = "Healthy (Mock)", timestamp = DateTime.UtcNow });
                }
                return Ok(result);
            }

            // Otherwise, check the actual service health
            var baseUrl = _configuration["GatewayOptions:BaseUrl"] ?? "http://localhost:5010";
            var client = _httpClientFactory.CreateClient();
            
            foreach (var service in services)
            {
                try
                {
                    var response = await client.GetAsync($"{baseUrl}{service.Value}");
                    if (response.IsSuccessStatusCode)
                    {
                        result.Add(service.Key, new { status = "Healthy", timestamp = DateTime.UtcNow });
                    }
                    else
                    {
                        result.Add(service.Key, new { status = "Unhealthy", statusCode = response.StatusCode, timestamp = DateTime.UtcNow });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking health for {Service}", service.Key);
                    result.Add(service.Key, new { status = "Unhealthy", error = ex.Message, timestamp = DateTime.UtcNow });
                }
            }

            return Ok(result);
        }
    }
} 