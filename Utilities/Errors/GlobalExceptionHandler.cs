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
        var status = exception is UnauthorizedAccessException
            ? StatusCodes.Status401Unauthorized
            : StatusCodes.Status500InternalServerError;

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception processing {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = status;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = status == StatusCodes.Status401Unauthorized
                ? "Unauthorized"
                : "Internal server error",
            Detail = status == StatusCodes.Status401Unauthorized
                ? exception.Message
                : "An unexpected error occurred.",
            Instance = httpContext.Request.Path
        };

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
