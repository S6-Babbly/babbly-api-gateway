using System.Security.Claims;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace babbly_api_gateway.Transforms;

/// <summary>
/// A transform that extracts user claims from the HttpContext and adds them to the outgoing request as headers.
/// </summary>
public class ForwardUserClaimsTransform : ITransformProvider
{
    /// <summary>
    /// Implements the ITransformProvider interface to register the transform with YARP.
    /// </summary>
    public void ValidateRoute(TransformRouteValidationContext context)
    {
    }

    /// <summary>
    /// Implements the ITransformProvider interface to register the transform with YARP.
    /// </summary>
    public void ValidateCluster(TransformClusterValidationContext context)
    {
    }

    /// <summary>
    /// Implements the ITransformProvider interface to register the transform with YARP.
    /// </summary>
    public void Apply(TransformBuilderContext context)
    {
        // Add a request transform that will forward user claims as HTTP headers
        context.AddRequestTransform(transformContext =>
        {
            // Extract the user from the current HTTP context
            ClaimsPrincipal? user = transformContext.HttpContext.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                // Extract the user ID from claims
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             user.FindFirst("sub")?.Value;
                             
                if (!string.IsNullOrEmpty(userId))
                {
                    transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Id", userId);
                }

                // Extract user roles
                var userRoles = user.FindAll(c => c.Type == ClaimTypes.Role || 
                                          c.Type == "https://babbly.com/roles")
                                    .Select(c => c.Value);
                                    
                if (userRoles.Any())
                {
                    transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Roles", 
                        string.Join(",", userRoles));
                }

                // Add email if available
                var email = user.FindFirst(ClaimTypes.Email)?.Value ??
                            user.FindFirst("email")?.Value;
                            
                if (!string.IsNullOrEmpty(email))
                {
                    transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Email", email);
                }

                // Add name if available
                var name = user.FindFirst(ClaimTypes.Name)?.Value ??
                           user.FindFirst("name")?.Value;
                           
                if (!string.IsNullOrEmpty(name))
                {
                    transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Name", name);
                }
            }

            return ValueTask.CompletedTask;
        });
    }
} 