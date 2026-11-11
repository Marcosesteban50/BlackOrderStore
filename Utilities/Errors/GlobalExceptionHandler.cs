using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Black_order.Utilities.Errors;

/// <summary>
/// Converts unhandled exceptions into RFC 7807 ProblemDetails responses.
/// Keeps API errors consistent instead of leaking stack traces or empty 500s.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "Authentication is required."),
            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "Not found",
                exception.Message),
            InvalidOperationException => (
                StatusCodes.Status400BadRequest,
                "Invalid operation",
                exception.Message),
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Invalid request",
                exception.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal server error",
                "An unexpected error occurred.")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception processing {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(
                "Request {Method} {Path} failed with {Status}: {Message}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                status,
                exception.Message);
        }

        httpContext.Response.StatusCode = status;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
