using IsraelAirlines.core.Exceptions;
using System.Net;
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

            context.Response.ContentType = "application/json";

            context.Response.StatusCode = exception switch
            {
                NotFoundException => (int)HttpStatusCode.NotFound,
                ConflictException => (int)HttpStatusCode.Conflict,
                AuthenticationException => (int)HttpStatusCode.Unauthorized,
                ArgumentException => (int)HttpStatusCode.BadRequest,

                _ => (int)HttpStatusCode.InternalServerError
            };

            var message = context.Response.StatusCode ==
                          (int)HttpStatusCode.InternalServerError
                ? "An unexpected error occurred."
                : exception.Message;

            var response = new
            {
                statusCode = context.Response.StatusCode,
                message
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}