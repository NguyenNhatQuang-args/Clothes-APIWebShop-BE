namespace Backend_Clothes_API.Middleware
{
    public class JwtTokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtTokenValidationMiddleware> _logger;

        public JwtTokenValidationMiddleware(RequestDelegate next, ILogger<JwtTokenValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if route requires authentication
            var endpoint = context.GetEndpoint();
            var requiresAuth = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>() != null;

            if (requiresAuth)
            {
                var token = ExtractToken(context);

                if (string.IsNullOrEmpty(token))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "Unauthorized",
                        errors = new[] { "Missing or invalid authorization token" }
                    });
                    return;
                }
            }

            await _next(context);
        }

        private static string? ExtractToken(HttpContext context)
        {
            // Try to get from Authorization header first
            var authHeader = context.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }

            // Try to get from cookies
            if (context.Request.Cookies.TryGetValue("AccessToken", out var token))
            {
                return token;
            }

            return null;
        }
    }
}
