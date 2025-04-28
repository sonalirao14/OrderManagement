using Core.Exceptions;
using Core.Interfaces;
using System.Net;
//using Newtonsoft.Json;
using System.Text.Json;
namespace WebApplication1.Middleware
{
    public class ExceptionHandling
    {
        private readonly RequestDelegate _next;
        private readonly ILoggingService _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionHandling(RequestDelegate next, ILoggingService logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); 
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            string message = "An unexpected error occurred.";
            string? details = null;

            switch (ex)
            {
                case NotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    message = "Resource not found.";
                    break;

                case ValidationException validationEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = "Validation failed.";
                    details = validationEx.Message;
                    break;

                case InsufficientStockException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = "Insufficient stock for one or more products.";
                    details = ex.Message;
                    break;

                case UnauthorizedException:
                    statusCode = HttpStatusCode.Unauthorized;
                    message = "Unauthorized access.";
                    break;

                default:
                    // For unhandled exceptions, include details only in Development
                    details = _env.IsDevelopment() ? ex.ToString() : null;
                    break;
            }

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                await _logger.LogErrorAsync(ex, "Unhandled error: {Message}, Path: {Path}, TraceId: {TraceId}",
                    message, context.Request.Path, context.TraceIdentifier);
            }
            else
            {
                await _logger.LogWarningAsync("Handled error: {Message}, Path: {Path}, TraceId: {TraceId}",
                    message, context.Request.Path, context.TraceIdentifier); // Removed 'ex'
            }
           
            var response = new
            {
                status = (int)statusCode,
                message,
                details
            };

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

    }


}
