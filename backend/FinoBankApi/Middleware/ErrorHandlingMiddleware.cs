using System.Net;
using System.Text.Json;

namespace FinoBankApi.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ErrorHandlingMiddleware(
            RequestDelegate next, 
            ILogger<ErrorHandlingMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
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

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An error occurred while processing the request.");

            context.Response.ContentType = "application/json";

            var response = context.Response;
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = "An unexpected error occurred.";

            switch (exception)
            {
                case KeyNotFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    message = exception.Message;
                    break;
                
                case UnauthorizedAccessException:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    message = "Unauthorized access.";
                    break;

                case Exception e when e.GetType() == typeof(Exception):
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = exception.Message;
                    break;
                
                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    message = _env.IsDevelopment()
                        ? exception.Message 
                        : "Internal Server Error";
                    break;
            }

            response.StatusCode = statusCode;

            var errorResponse = new
            {
                status = statusCode,
                message = message,
                timestamp = DateTime.UtcNow
            };

            var result = JsonSerializer.Serialize(errorResponse);
            return response.WriteAsync(result);
        }
    }
}