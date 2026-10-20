using System.Security.Claims;
using RaceService.Application.Abstractions;

namespace RaceService.Api.Authorization;

/// <summary>
/// Reads the caller's identity from the JWT on the current request.
/// </summary>
/// <remarks>
/// Identity is issued by a separate UserService. This service never validates a password or
/// stores a credential; it only verifies the token's signature and reads the subject and roles
/// out of it. That is what keeps the two services independently deployable.
/// </remarks>
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    /// <summary>Initialises the accessor.</summary>
    public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    /// <inheritdoc />
    public Guid? UserId
    {
        get
        {
            var value = _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? _accessor.HttpContext?.User.FindFirstValue("sub");

            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    /// <inheritdoc />
    public bool IsAuthenticated => _accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    /// <inheritdoc />
    public bool IsInRole(string role) => _accessor.HttpContext?.User.IsInRole(role) ?? false;
}
