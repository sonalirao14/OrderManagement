using Core.Interfaces;
namespace WebApplication1.Middleware

{
    public class AuthContext
    {
        private readonly RequestDelegate _next;
        //private readonly ILogger<AuthContext> _logger;
        private readonly ILoggingService _logger;

        public AuthContext(RequestDelegate next, ILoggingService logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var userId = context.User.FindFirst("userId")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await _logger.LogInformationAsync("UserId extracted and added to context: {UserId}", userId);
                context.Items["UserId"] = userId;
            }

            await _next(context);
        }
    }
}
