using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace WebApplication1.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtMiddleware> _logger;

        public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            _logger.LogInformation("Authorization Header Received: {AuthHeader}", authHeader ?? "null");

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Replace("Bearer ", "");

                try
                {
                    var jwtHandler = context.RequestServices.GetRequiredService<JwtSecurityTokenHandlerWrapper>();
                    var claimsPrincipal = jwtHandler.ValidateJwtToken(token);

                    context.User = claimsPrincipal;
                    _logger.LogInformation("JWT token validated and HttpContext.User set");
                }
                catch (SecurityTokenValidationException ex)
                {
                    _logger.LogError(ex, "Token validation failed");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: Invalid or missing token.");
                    return;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unexpected error during JWT validation");
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync("Internal Server Error");
                    return;
                }
            }
            else
            {
                _logger.LogWarning("Missing or invalid Authorization header");
            }

            await _next(context);
        }
    }
}
