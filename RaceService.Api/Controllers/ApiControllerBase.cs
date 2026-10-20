using Microsoft.AspNetCore.Mvc;
using RaceService.Api.Authorization;
using RaceService.Application.Common;

namespace RaceService.Api.Controllers;

/// <summary>
/// Shared behaviour for every controller: turning a <see cref="Result"/> into an HTTP response.
/// </summary>
/// <remarks>
/// One translation point means the status code for a given failure is decided once. Doing it per
/// action is how an API ends up returning 500 for a missing record on one endpoint and 404 on
/// the next.
/// </remarks>
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>True when the caller holds the administrator role.</summary>
    protected bool IsAdministrator => User.IsInRole(Roles.Administrator);

    /// <summary>Subject id from the token, or null for an anonymous caller.</summary>
    protected Guid? CurrentUserId
    {
        get
        {
            var value = User.FindFirst("sub")?.Value
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    /// <summary>Returns 200 with the value, or the failure's status code.</summary>
    protected IActionResult FromResult<T>(Result<T> result)
        => result.IsSuccess ? Ok(result.Value) : Problem(result.Error);

    /// <summary>Returns 204, or the failure's status code.</summary>
    protected IActionResult FromResult(Result result)
        => result.IsSuccess ? NoContent() : Problem(result.Error);

    /// <summary>Returns 201 pointing at <paramref name="routeName"/>, or the failure's status code.</summary>
    protected IActionResult CreatedFromResult<T>(Result<T> result, string routeName, Func<T, object> routeValues)
        => result.IsSuccess
            ? CreatedAtRoute(routeName, routeValues(result.Value), result.Value)
            : Problem(result.Error);

    /// <summary>Renders an error as an RFC 9457 problem document.</summary>
    protected IActionResult Problem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Unprocessable => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Code,
            Detail = error.Description,
            Type = $"https://racingvacing.com/errors/{error.Code}",
            Instance = HttpContext.Request.Path
        };

        // The stable code is repeated in an extension so clients can branch on it without
        // parsing the title, which is presentation and may be localised later.
        problem.Extensions["code"] = error.Code;
        problem.Extensions["traceId"] = HttpContext.TraceIdentifier;

        return new ObjectResult(problem) { StatusCode = statusCode };
    }
}
