namespace RaceService.Contracts.Enums;

/// <summary>
/// Why a driver is attached to an event entry. Order within the crew is carried by
/// <c>EventEntryDriver.LineupPosition</c>, not by this enum.
/// </summary>
public enum DriverRole
{
    Unknown = 0,
    /// <summary>Nominated to drive the car in the race.</summary>
    Race = 1,
    /// <summary>Named on the entry as cover, only drives if a race driver is unavailable.</summary>
    Reserve = 2,
    /// <summary>Permitted in practice sessions only.</summary>
    Test = 3
}
