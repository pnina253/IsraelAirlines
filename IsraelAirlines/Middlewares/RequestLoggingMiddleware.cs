using System.Diagnostics;

namespace IsraelAirlines.API.Middlewares;

// One log line per incoming request and one when it finishes. Only the method and path are
// written: query strings and bodies can hold emails, passwords or tokens.
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation(
            "Request started: {Method} {Path}",
            context.Request.Method, context.Request.Path);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);

            _logger.LogInformation(
                "Request finished: {Method} {Path} -> {StatusCode} in {ElapsedMs} ms",
                context.Request.Method, context.Request.Path,
                context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
        catch
        {
            // GlobalExceptionMiddleware logs the exception itself and writes the response.
            _logger.LogInformation(
                "Request aborted by an exception: {Method} {Path} after {ElapsedMs} ms",
                context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
