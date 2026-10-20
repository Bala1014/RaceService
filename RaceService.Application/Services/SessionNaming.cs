using RaceService.Contracts.Enums;

namespace RaceService.Application.Services;

/// <summary>
/// Produces the display name for a session when an editor has not supplied one.
/// </summary>
/// <remarks>
/// Centralised so that "Practice 2" is spelled the same way whether the session was created
/// through the API, the seed file or an import — a detail that is invisible until three sources
/// disagree and the schedule sorts oddly.
/// </remarks>
public static class SessionNaming
{
    /// <summary>Derives a name from a session's type and sequence.</summary>
    public static string Derive(SessionType type, int sequenceNumber) => type switch
    {
        SessionType.Practice => $"Practice {sequenceNumber}",
        SessionType.Qualifying => sequenceNumber > 1 ? $"Qualifying {sequenceNumber}" : "Qualifying",
        SessionType.SprintQualifying => sequenceNumber > 1 ? $"Sprint Qualifying {sequenceNumber}" : "Sprint Qualifying",
        SessionType.Hyperpole => "Hyperpole",
        SessionType.Sprint => sequenceNumber > 1 ? $"Sprint {sequenceNumber}" : "Sprint",
        SessionType.Race => sequenceNumber > 1 ? $"Race {sequenceNumber}" : "Race",
        SessionType.WarmUp => "Warm Up",
        SessionType.Testing => $"Test {sequenceNumber}",
        SessionType.Stage => $"Stage {sequenceNumber}",
        _ => $"Session {sequenceNumber}"
    };
}
