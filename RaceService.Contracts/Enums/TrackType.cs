namespace RaceService.Contracts.Enums;

/// <summary>Physical character of a circuit layout.</summary>
public enum TrackType
{
    Unknown = 0,
    PermanentCircuit = 1,
    StreetCircuit = 2,
    /// <summary>Permanent circuit that closes public roads for part of the lap, such as Le Mans.</summary>
    Hybrid = 3,
    Oval = 4,
    Roval = 5,
    Rallycross = 6,
    Dirt = 7
}
