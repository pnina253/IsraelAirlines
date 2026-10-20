using IsraelAirlines.core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;
using System.Text.Json;

namespace IsraelAirlines.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(
             exception,
            "An unhandled exception occurred. CorrelationId: {CorrelationId}",
             context.TraceIdentifier);

            if (context.Response.HasStarted)
            {
                throw;
            }

            var statusCode = exception switch
            {
                BadRequestException => StatusCodes.Status400BadRequest,
                AuthenticationException => StatusCodes.Status401Unauthorized,
                ForbiddenException => StatusCodes.Status403Forbidden,
                NotFoundException => StatusCodes.Status404NotFound,
                ConflictException => StatusCodes.Status409Conflict,
                DbUpdateConcurrencyException => StatusCodes.Status409Conflict,

                _ => StatusCodes.Status500InternalServerError
            };

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = ReasonPhrases.GetReasonPhrase(statusCode),
                Detail = statusCode == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred."
                    : exception.Message,
                Instance = context.Request.Path
            };
            problem.Extensions["correlationId"] = context.TraceIdentifier;

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(
                problem, (JsonSerializerOptions?)null, "application/problem+json");
        }
    }
}