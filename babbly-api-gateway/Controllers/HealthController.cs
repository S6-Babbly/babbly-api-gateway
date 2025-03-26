using Microsoft.AspNetCore.Mvc;

namespace babbly_api_gateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "API Gateway Healthy", timestamp = DateTime.UtcNow });
        }
    }
} 