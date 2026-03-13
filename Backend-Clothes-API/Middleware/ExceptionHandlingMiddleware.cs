using Backend_Clothes_API.Models.Responses;
using System.Net;
using System.Text.Json;

namespace Backend_Clothes_API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<object>();
            var statusCode = HttpStatusCode.InternalServerError;

            switch (exception)
            {
                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    response = ApiResponse<object>.ErrorResponse("Unauthorized", exception.Message);
                    break;

                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    response = ApiResponse<object>.ErrorResponse("Not Found", exception.Message);
                    break;

                case ArgumentException:
                    statusCode = HttpStatusCode.BadRequest;
                    response = ApiResponse<object>.ErrorResponse("Bad Request", exception.Message);
                    break;

                case InvalidOperationException:
                    statusCode = HttpStatusCode.BadRequest;
                    response = ApiResponse<object>.ErrorResponse("Invalid Operation", exception.Message);
                    break;

                default:
                    response = ApiResponse<object>.ErrorResponse(
                        "Internal Server Error",
                        "An unexpected error occurred. Please try again later.");
                    break;
            }

            context.Response.StatusCode = (int)statusCode;
            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
