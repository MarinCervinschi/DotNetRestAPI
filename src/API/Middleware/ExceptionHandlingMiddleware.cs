using System.Net;
using System.Text.Json;

namespace src.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            KeyNotFoundException => new { message = "Resource not found", statusCode = 404 },
            UnauthorizedAccessException => new { message = "Unauthorized access", statusCode = 401 },
            ArgumentException => new { message = exception.Message, statusCode = 400 },
            _ => new { message = "Internal server error", statusCode = 500 }
        };

        context.Response.StatusCode = response.statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
