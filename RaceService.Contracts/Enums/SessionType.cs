namespace RaceService.Contracts.Enums;

/// <summary>
/// What kind of on-track running a session is. Combined with
/// <c>Session.SequenceNumber</c> this covers multi-part formats such as F1's Q1/Q2/Q3
/// and the multiple practice sessions of an endurance weekend.
/// </summary>
public enum SessionType
{
    Unknown = 0,
    Testing = 1,
    Practice = 2,
    /// <summary>Short session immediately before the race, common in MotoGP and endurance racing.</summary>
    WarmUp = 3,
    Qualifying = 4,
    /// <summary>Endurance-style single-lap shootout that sets the grid from a qualifying subset.</summary>
    Hyperpole = 5,
    /// <summary>Qualifying that sets the grid for a sprint rather than the main race.</summary>
    SprintQualifying = 6,
    Sprint = 7,
    Race = 8,
    /// <summary>Rally special stage or hillclimb run, timed individually rather than on a grid.</summary>
    Stage = 9
}
