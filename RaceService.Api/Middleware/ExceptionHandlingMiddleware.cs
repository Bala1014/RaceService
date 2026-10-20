using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RaceService.Api.Middleware;

/// <summary>
/// Turns an unhandled exception into an RFC 9457 problem document.
/// </summary>
/// <remarks>
/// The detail is deliberately generic outside development: an exception message can carry a
/// connection string, a SQL fragment or a file path, none of which belong in a public response.
/// The trace id is returned instead so a report can be tied to the log entry that has the real
/// story.
/// </remarks>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    /// <summary>Initialises the middleware.</summary>
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>Invokes the middleware.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client hung up. Not an error, and not worth a stack trace in the logs.
            _logger.LogInformation("Request {Path} was cancelled by the client.", context.Request.Path);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // Someone else saved the same row between our read and our write. That is a caller
            // problem, not a server fault, so it is a 409 telling them to re-read and retry.
            _logger.LogWarning(exception, "Concurrency conflict on {Method} {Path}.", context.Request.Method, context.Request.Path);

            await WriteAsync(
                context,
                StatusCodes.Status409Conflict,
                "Concurrency.Conflict",
                "This record was modified by someone else. Reload it and apply your changes again.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception for {Method} {Path}.", context.Request.Method, context.Request.Path);

            await WriteAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Server.Error",
                _environment.IsDevelopment() ? exception.ToString() : "An unexpected error occurred.");
        }
    }

    private static async Task WriteAsync(HttpContext context, int statusCode, string code, string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = code,
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem);
    }
}
