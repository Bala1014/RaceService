namespace RaceService.Contracts.Enums;

/// <summary>Live lifecycle of a single session.</summary>
public enum SessionStatus
{
    Unknown = 0,
    Scheduled = 1,
    Delayed = 2,
    Running = 3,
    /// <summary>Stopped under a red flag; may resume, so this is not a terminal state.</summary>
    Suspended = 4,
    Finished = 5,
    Cancelled = 6,
    Abandoned = 7
}
