namespace src.API.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var requestId = Guid.NewGuid().ToString()[..8];

        logger.LogInformation("[{RequestId}] {Method} {Path} - Start",
            requestId, context.Request.Method, context.Request.Path);

        await next(context);

        var duration = DateTime.UtcNow - startTime;
        logger.LogInformation("[{RequestId}] {Method} {Path} - {StatusCode} in {Duration}ms",
            requestId, context.Request.Method, context.Request.Path,
            context.Response.StatusCode, duration.TotalMilliseconds);
    }
}
