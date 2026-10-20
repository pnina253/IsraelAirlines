using IsraelAirlines.core.Exceptions;
using System.Security.Authentication;

namespace IsraelAirlines.API.Middlewares;

// First in the pipeline, so an exception from any later step becomes a JSON error.
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
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client went away; there is nobody to answer.
            _logger.LogInformation(
                "Request {Method} {Path} was cancelled by the client",
                context.Request.Method, context.Request.Path);
        }
        catch (Exception exception)
        {
            var statusCode = exception switch
            {
                BadRequestException => StatusCodes.Status400BadRequest,
                AuthenticationException => StatusCodes.Status401Unauthorized,
                ForbiddenException => StatusCodes.Status403Forbidden,
                NotFoundException => StatusCodes.Status404NotFound,
                ConflictException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                // A real failure: full exception with stack trace.
                _logger.LogError(
                    exception,
                    "Unhandled exception for {Method} {Path}. CorrelationId: {CorrelationId}",
                    context.Request.Method, context.Request.Path, context.TraceIdentifier);
            }
            else
            {
                // An expected, handled business outcome (validation, not found, conflict).
                _logger.LogWarning(
                    "Request {Method} {Path} rejected with {StatusCode}: {Reason}. CorrelationId: {CorrelationId}",
                    context.Request.Method, context.Request.Path, statusCode,
                    exception.Message, context.TraceIdentifier);
            }

            if (context.Response.HasStarted)
            {
                throw;
            }

            await ProblemDetailsWriter.WriteAsync(
                context,
                statusCode,
                statusCode == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred."
                    : exception.Message);
        }
    }
}
