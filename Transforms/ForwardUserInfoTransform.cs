using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace babbly_api_gateway.Transforms
{
    public class ForwardUserInfoTransform : ITransformProvider
    {
        public void ValidateRoute(TransformRouteValidationContext context)
        {
            // No validation needed
        }

        public void ValidateCluster(TransformClusterValidationContext context)
        {
            // No validation needed
        }

        public void Apply(TransformBuilderContext context)
        {
            context.AddRequestTransform(transformContext =>
            {
                var httpContext = transformContext.HttpContext;
                
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        transformContext.ProxyRequest.Headers.Add("X-User-Id", userId);
                    }

                    var userRoles = httpContext.User.FindAll(claim => claim.Type == "https://babbly.com/roles")
                        .Select(c => c.Value);
                    if (userRoles.Any())
                    {
                        transformContext.ProxyRequest.Headers.Add("X-User-Roles", string.Join(",", userRoles));
                    }
                }

                return ValueTask.CompletedTask;
            });
        }
    }
} 