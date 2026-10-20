namespace RaceService.Api.Authorization;

/// <summary>
/// Authorization policy and role names.
/// </summary>
/// <remarks>
/// Constants rather than string literals scattered through attributes, so a renamed role is a
/// compile error instead of an endpoint that silently stops being protected.
/// </remarks>
public static class Policies
{
    /// <summary>Full control over the racing catalogue: series, seasons, events, results.</summary>
    public const string Administrator = "race.administrator";

    /// <summary>May edit content but not delete or archive it.</summary>
    public const string Editor = "race.editor";

    /// <summary>May submit and manage entries.</summary>
    public const string Entrant = "race.entrant";
}

/// <summary>Role names as they appear in the JWT issued by the UserService.</summary>
public static class Roles
{
    public const string Administrator = "Administrator";
    public const string Editor = "Editor";
    public const string Entrant = "Entrant";
}
