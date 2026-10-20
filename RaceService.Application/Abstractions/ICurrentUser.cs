namespace RaceService.Application.Abstractions;

/// <summary>
/// Who is making the current request. Implemented in the API layer from the JWT, so the
/// application and persistence layers can stamp audit columns without referencing HTTP.
/// </summary>
public interface ICurrentUser
{
    /// <summary>Subject id from the token, or null for an anonymous request or a background job.</summary>
    Guid? UserId { get; }

    bool IsAuthenticated { get; }

    bool IsInRole(string role);
}
