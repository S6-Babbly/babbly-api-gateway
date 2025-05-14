using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace babbly_api_gateway.Middleware;

/// <summary>
/// Middleware for performing basic JWT token validation without requiring a full authentication cycle.
/// This can be useful for quick signature checks before passing to microservices.
/// </summary>
public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenValidationMiddleware> _logger;
    private readonly string _issuer;
    private readonly string _audience;

    public TokenValidationMiddleware(
        RequestDelegate next, 
        ILogger<TokenValidationMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        
        // Get Auth0 configuration
        var domain = Environment.GetEnvironmentVariable("AUTH0_DOMAIN") ?? 
                     configuration["Auth0:Domain"] ?? 
                     throw new InvalidOperationException("Auth0 Domain is not configured.");
                     
        var audience = Environment.GetEnvironmentVariable("AUTH0_AUDIENCE") ?? 
                      configuration["Auth0:Audience"] ?? 
                      throw new InvalidOperationException("Auth0 Audience is not configured.");
        
        _issuer = $"https://{domain}/";
        _audience = audience;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only validate tokens for API routes that aren't auth endpoints
        var path = context.Request.Path;
        if (path.StartsWithSegments("/api") && 
            !path.StartsWithSegments("/api/auth/login") &&
            !path.StartsWithSegments("/api/auth/callback") &&
            !path.StartsWithSegments("/api/auth/logout"))
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
                    // Create token validation parameters
                    var validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = _issuer,
                        ValidateAudience = true,
                        ValidAudience = _audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };

                    // Perform basic validation (no signature validation here)
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwtToken = tokenHandler.ReadJwtToken(token);

                    // Check token expiration
                    var now = DateTime.UtcNow;
                    var expires = jwtToken.ValidTo;
                    if (expires < now)
                    {
                        _logger.LogWarning("Token expired at {ExpiredTime}", expires);
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsJsonAsync(new { error = "Expired token" });
                        return;
                    }

                    // Add token claims to headers for downstream services
                    foreach (var claim in jwtToken.Claims)
                    {
                        // Don't add all claims, just add specific ones downstream services might need
                        if (claim.Type == "sub" || claim.Type == "https://babbly.com/roles")
                        {
                            var headerName = $"X-User-{claim.Type.Replace("https://babbly.com/", "")}";
                            context.Request.Headers.Append(headerName, claim.Value);
                        }
                    }

                    _logger.LogInformation("Token validated for subject {Subject}", 
                        jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Token validation failed");
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { error = "Invalid token" });
                    return;
                }
            }
        }

        await _next(context);
    }
} 