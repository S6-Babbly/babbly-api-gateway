using babbly_api_gateway.Services;
using System.Security.Claims;

namespace babbly_api_gateway.Middleware;

/// <summary>
/// Middleware for validating JWT tokens using the Auth Service
/// </summary>
public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenValidationMiddleware> _logger;
    private readonly IServiceProvider _serviceProvider;

    public TokenValidationMiddleware(
        RequestDelegate next, 
        ILogger<TokenValidationMiddleware> logger,
        IServiceProvider serviceProvider)
    {
        _next = next;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only validate tokens for API routes that aren't auth endpoints
        var path = context.Request.Path;
        if (path.StartsWithSegments("/api") && 
            !path.StartsWithSegments("/api/auth/login") &&
            !path.StartsWithSegments("/api/auth/callback") &&
            !path.StartsWithSegments("/api/auth/logout") &&
            !path.StartsWithSegments("/api/feed") && // Allow feed to be accessed without auth for now
            !path.StartsWithSegments("/api/health"))
        {
            string? token = null;
            
            // Extract token from Authorization header
            string authorization = context.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authorization.Substring("Bearer ".Length).Trim();
            }

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    // Use the auth service to validate the token
                    using var scope = _serviceProvider.CreateScope();
                    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                    
                    var (isValid, payload, error) = await authService.ValidateTokenWithPayloadAsync(token);
                    
                    if (!isValid || payload == null)
                    {
                        _logger.LogWarning("Token validation failed: {Error}", error);
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsJsonAsync(new { error = error ?? "Invalid token" });
                        return;
                    }

                    // Extract user ID and add to context
                    if (payload.TryGetValue("sub", out var sub) && sub != null)
                    {
                        context.Items["CurrentUserId"] = sub.ToString();
                        
                        // Add user info to headers for downstream services
                        context.Request.Headers.Append("X-User-Id", sub.ToString());
                    }

                    // Add roles to headers if available
                    if (payload.TryGetValue("https://babbly.com/roles", out var roles) && roles != null)
                    {
                        context.Request.Headers.Append("X-User-Roles", roles.ToString());
                    }

                    _logger.LogInformation("Token validated for user {UserId}", sub);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Token validation failed");
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { error = "Token validation error" });
                    return;
                }
            }
            else
            {
                // No token provided for protected route
                _logger.LogWarning("No token provided for protected route: {Path}", path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Authentication required" });
                return;
            }
        }

        await _next(context);
    }
} 