using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace UserManagementAPI.Middleware;

// Requires a valid "Authorization: Bearer <token>" header on every request before reaching the endpoint.
public class TokenAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<TokenAuthenticationMiddleware> logger)
{
    // The OpenAPI document itself is left public so API clients/tools can discover the contract.
    private static readonly string[] AnonymousPathPrefixes = ["/openapi"];

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (AnonymousPathPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
        }

        var expectedToken = configuration["Authentication:ApiToken"];
        if (string.IsNullOrEmpty(expectedToken))
        {
            logger.LogError("Authentication:ApiToken is not configured; rejecting request to {Path}.", path);
            await WriteUnauthorizedAsync(context, "API authentication is not configured.");
            return;
        }

        var authHeader = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Missing or malformed Authorization header for {Method} {Path}.", context.Request.Method, path);
            await WriteUnauthorizedAsync(context, "A valid bearer token is required.");
            return;
        }

        var providedToken = authHeader["Bearer ".Length..].Trim();
        if (!TokensMatch(providedToken, expectedToken))
        {
            logger.LogWarning("Invalid bearer token for {Method} {Path}.", context.Request.Method, path);
            await WriteUnauthorizedAsync(context, "A valid bearer token is required.");
            return;
        }

        await next(context);
    }

    // Constant-time comparison avoids leaking token contents via response-time side channels.
    private static bool TokensMatch(string provided, string expected)
    {
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);

        return providedBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
    }

    private static async Task WriteUnauthorizedAsync(HttpContext context, string message)
    {
        context.Response.Clear();
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
    }
}
