namespace babbly_api_gateway.Middleware;

public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds the token validation middleware to the HTTP pipeline.
    /// </summary>
    public static IApplicationBuilder UseTokenValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TokenValidationMiddleware>();
    }
} 