namespace server.Common.Settings
{
    public class InterceptorHttpLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<InterceptorHttpLoggingMiddleware> _logger;

        public InterceptorHttpLoggingMiddleware(RequestDelegate next, ILogger<InterceptorHttpLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.ToString();
            // Bỏ qua log cho Hangfire
            if (path.StartsWith("/tms/hangfire"))
            {
                await _next(context);
                return;
            }

            var method = context.Request.Method;
            _logger.LogInformation("Request: {Method} {Path}", method, path);

            var originalBody = context.Response.Body;
            using var newBody = new MemoryStream();
            context.Response.Body = newBody;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception for request {Method} {Path}", method, path);

                // Trả về lỗi cho client (có thể custom JSON response)
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("An unexpected error occurred.");
            }

            newBody.Seek(0, SeekOrigin.Begin);
            var statusCode = context.Response.StatusCode;
            _logger.LogInformation("Response: {StatusCode}", statusCode);

            newBody.Seek(0, SeekOrigin.Begin);
            await newBody.CopyToAsync(originalBody);
        }
    }
}

