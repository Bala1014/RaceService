namespace RaceService.Contracts.Enums;

/// <summary>Lifecycle of a championship season.</summary>
public enum SeasonStatus
{
    Unknown = 0,
    /// <summary>Calendar published but no round has run yet.</summary>
    Announced = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}
