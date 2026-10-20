namespace RaceService.Application.Common;

/// <summary>Kind of failure, which maps directly onto an HTTP status code at the edge.</summary>
public enum ErrorType
{
    /// <summary>The request was malformed or semantically invalid. 400.</summary>
    Validation = 0,

    /// <summary>The addressed resource does not exist. 404.</summary>
    NotFound = 1,

    /// <summary>The request collides with existing state, such as a duplicate slug. 409.</summary>
    Conflict = 2,

    /// <summary>The caller is authenticated but not permitted. 403.</summary>
    Forbidden = 3,

    /// <summary>The request is well formed but the domain rules reject it. 422.</summary>
    Unprocessable = 4
}

/// <summary>
/// A failure with a stable machine-readable code alongside a human-readable description.
/// </summary>
/// <remarks>
/// The code exists so a front end can branch on <c>Entry.DuplicateCarNumber</c> without parsing
/// English prose, which is what makes error handling survive a wording change.
/// </remarks>
public sealed record Error(string Code, string Description, ErrorType Type)
{
    /// <summary>Absence of an error.</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Validation);

    /// <summary>Creates a 404-shaped error.</summary>
    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);

    /// <summary>Creates a 400-shaped error.</summary>
    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);

    /// <summary>Creates a 409-shaped error.</summary>
    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);

    /// <summary>Creates a 403-shaped error.</summary>
    public static Error Forbidden(string code, string description) => new(code, description, ErrorType.Forbidden);

    /// <summary>Creates a 422-shaped error.</summary>
    public static Error Unprocessable(string code, string description) => new(code, description, ErrorType.Unprocessable);
}
