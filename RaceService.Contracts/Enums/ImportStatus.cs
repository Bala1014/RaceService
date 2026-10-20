namespace RaceService.Contracts.Enums;

/// <summary>Outcome of a single run of an external data import.</summary>
public enum ImportStatus
{
    Unknown = 0,
    Running = 1,
    Succeeded = 2,
    PartiallySucceeded = 3,
    Failed = 4,
    /// <summary>Provider reported no change since the last run, so nothing was written.</summary>
    Skipped = 5
}
