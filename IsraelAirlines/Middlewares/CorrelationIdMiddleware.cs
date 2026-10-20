using System.Text.RegularExpressions;

namespace IsraelAirlines.API.Middlewares;

// Gives every request an id that appears in every log line it writes and in the response.
public partial class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // A caller may pass its own id to trace a request across systems; anything else is replaced.
    [GeneratedRegex("^[A-Za-z0-9-]{1,64}$")]
    private static partial Regex SafeId();

    public async Task InvokeAsync(HttpContext context)
    {
        var incoming = context.Request.Headers[HeaderName].ToString();
        var correlationId = SafeId().IsMatch(incoming)
            ? incoming
            : Guid.NewGuid().ToString();

        // NLog's ${aspnet-traceidentifier} reads this value.
        context.TraceIdentifier = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (_logger.BeginScope(
            new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            }))
        {
            await _next(context);
        }
    }
}
