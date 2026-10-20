namespace RaceService.Contracts.Enums;

/// <summary>
/// Lifecycle of a race weekend. Distinct from <see cref="SessionStatus"/>: an event is
/// <c>InProgress</c> for the whole weekend while individual sessions come and go.
/// </summary>
public enum EventStatus
{
    Unknown = 0,
    /// <summary>Being edited by an admin, never visible to the public.</summary>
    Draft = 1,
    Scheduled = 2,
    InProgress = 3,
    Completed = 4,
    Postponed = 5,
    Cancelled = 6,
    /// <summary>Historic event kept for archives, excluded from default listings.</summary>
    Archived = 7
}
