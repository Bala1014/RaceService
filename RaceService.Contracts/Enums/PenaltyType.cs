namespace RaceService.Contracts.Enums;

/// <summary>Sanction handed down by the stewards.</summary>
public enum PenaltyType
{
    Unknown = 0,
    Reprimand = 1,
    Warning = 2,
    /// <summary>Fixed time added to the car's total after the flag.</summary>
    TimePenalty = 3,
    /// <summary>Car must enter the pit lane and wait before rejoining.</summary>
    StopAndGo = 4,
    DriveThrough = 5,
    GridPenalty = 6,
    /// <summary>Sent to the back of the grid regardless of qualifying position.</summary>
    PitLaneStart = 7,
    Disqualification = 8,
    Exclusion = 9,
    Fine = 10,
    /// <summary>Points added to the driver's licence, tracked toward a race ban.</summary>
    LicencePoints = 11
}
