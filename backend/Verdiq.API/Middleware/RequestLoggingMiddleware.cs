using System.Diagnostics;
using System.Security.Claims;

namespace Verdiq.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;
        var queryString = context.Request.QueryString;

        await _next(context);

        stopwatch.Stop();

        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anon";
        var statusCode = context.Response.StatusCode;

        if (statusCode >= 500)
        {
            _logger.LogError(
                "HTTP {Method} {Path}{QueryString} responded {StatusCode} in {ElapsedMs}ms [User: {UserId}]",
                method, path, queryString, statusCode, stopwatch.ElapsedMilliseconds, userId);
        }
        else if (statusCode >= 400)
        {
            _logger.LogWarning(
                "HTTP {Method} {Path}{QueryString} responded {StatusCode} in {ElapsedMs}ms [User: {UserId}]",
                method, path, queryString, statusCode, stopwatch.ElapsedMilliseconds, userId);
        }
        else
        {
            _logger.LogInformation(
                "HTTP {Method} {Path}{QueryString} responded {StatusCode} in {ElapsedMs}ms [User: {UserId}]",
                method, path, queryString, statusCode, stopwatch.ElapsedMilliseconds, userId);
        }
    }
}
