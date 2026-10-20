using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;

namespace IsraelAirlines.API.Middlewares;

// Every error the API returns has the same JSON shape, including the correlation id.
public static class ProblemDetailsWriter
{
    public static Task WriteAsync(HttpContext context, int statusCode, string? detail)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = detail,
            Instance = context.Request.Path
        };
        problem.Extensions["correlationId"] = context.TraceIdentifier;

        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(
            problem, (JsonSerializerOptions?)null, "application/problem+json");
    }
}
