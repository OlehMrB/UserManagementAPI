namespace UserManagementAPI.Middleware;

// Logs the HTTP method, path, and resulting status code for every request/response pair.
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path;

        logger.LogInformation("Incoming request: {Method} {Path}", method, path);

        await next(context);

        logger.LogInformation("Outgoing response: {Method} {Path} responded {StatusCode}", method, path, context.Response.StatusCode);
    }
}
