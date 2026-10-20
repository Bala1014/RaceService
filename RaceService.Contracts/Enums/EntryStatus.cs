namespace RaceService.Contracts.Enums;

/// <summary>Registration state of a car, at either season or event level.</summary>
public enum EntryStatus
{
    Unknown = 0,
    Pending = 1,
    Confirmed = 2,
    /// <summary>Accepted only if another entry drops out.</summary>
    Reserve = 3,
    Rejected = 4,
    Withdrawn = 5,
    /// <summary>Excluded by the organiser after having been confirmed.</summary>
    Excluded = 6
}
