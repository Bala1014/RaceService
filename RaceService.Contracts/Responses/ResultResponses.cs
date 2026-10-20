using RaceService.Contracts.Enums;

namespace RaceService.Contracts.Responses;

/// <summary>One row of a classification.</summary>
public sealed record SessionResultResponse
{
    public Guid Id { get; init; }
    public Guid SessionId { get; init; }
    public Guid EventEntryId { get; init; }

    public int? Position { get; init; }
    public int? ClassPosition { get; init; }
    public int? StartingPosition { get; init; }

    /// <summary>
    /// Places gained or lost against the starting position. Positive is a gain. Computed here
    /// because every client would otherwise derive it slightly differently around nulls.
    /// </summary>
    public int? PositionsGained { get; init; }

    public string CarNumber { get; init; } = string.Empty;
    public Guid TeamId { get; init; }
    public string TeamName { get; init; } = string.Empty;
    public string EntrantName { get; init; } = string.Empty;
    public string? VehicleClassName { get; init; }
    public string? VehicleClassColorHex { get; init; }
    public string? ChassisModel { get; init; }

    public int LapsCompleted { get; init; }
    public int LapsLed { get; init; }

    public long? TotalTimeMs { get; init; }

    /// <summary>Total race time formatted as h:mm:ss.fff.</summary>
    public string? TotalTimeDisplay { get; init; }

    public long? GapToLeaderMs { get; init; }
    public int? GapToLeaderLaps { get; init; }

    /// <summary>Gap rendered the way a timing screen shows it: "+12.480" or "+2 laps".</summary>
    public string? GapDisplay { get; init; }

    public int? BestLapTimeMs { get; init; }

    /// <summary>Best lap formatted as m:ss.fff.</summary>
    public string? BestLapDisplay { get; init; }

    public int? BestLapNumber { get; init; }
    public decimal? AverageSpeedKph { get; init; }
    public decimal? TopSpeedKph { get; init; }

    public ResultStatus Status { get; init; }
    public string? StatusDetail { get; init; }
    public bool IsClassified { get; init; }
    public decimal PointsScored { get; init; }
    public bool IsFastestLap { get; init; }
    public bool IsPolePosition { get; init; }
    public int PitStopCount { get; init; }
    public int PenaltyTimeMs { get; init; }

    /// <summary>Per-driver breakdown. One entry for a single-driver car, several for endurance.</summary>
    public IReadOnlyList<SessionResultDriverResponse> Drivers { get; init; } = [];
}

/// <summary>One driver's contribution to a car's result.</summary>
public sealed record SessionResultDriverResponse
{
    public Guid DriverId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? Abbreviation { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string? CountryCode { get; init; }
    public int LapsDriven { get; init; }
    public int? BestLapTimeMs { get; init; }
    public string? BestLapDisplay { get; init; }
    public long? TimeDrivenMs { get; init; }
    public decimal PointsScored { get; init; }
    public bool IsEligibleForPoints { get; init; }
    public bool SetBestLap { get; init; }
}

/// <summary>One completed lap.</summary>
public sealed record SessionLapResponse
{
    public Guid Id { get; init; }
    public Guid EventEntryId { get; init; }
    public string CarNumber { get; init; } = string.Empty;
    public Guid? DriverId { get; init; }
    public string? DriverName { get; init; }
    public int LapNumber { get; init; }
    public int? LapTimeMs { get; init; }
    public string? LapTimeDisplay { get; init; }
    public int? Sector1Ms { get; init; }
    public int? Sector2Ms { get; init; }
    public int? Sector3Ms { get; init; }
    public int? Position { get; init; }
    public decimal? SpeedTrapKph { get; init; }
    public string? TyreCompound { get; init; }
    public int? TyreAgeLaps { get; init; }
    public bool IsPitInLap { get; init; }
    public bool IsPitOutLap { get; init; }
    public bool IsPersonalBest { get; init; }
    public bool IsSessionBest { get; init; }
    public bool IsUnderCaution { get; init; }
    public bool IsDeletedByStewards { get; init; }
}

/// <summary>A visit to the pits.</summary>
public sealed record PitStopResponse
{
    public Guid Id { get; init; }
    public Guid EventEntryId { get; init; }
    public string CarNumber { get; init; } = string.Empty;
    public int StopNumber { get; init; }
    public int LapNumber { get; init; }
    public int? StationaryTimeMs { get; init; }
    public string? StationaryTimeDisplay { get; init; }
    public int? TotalPitLaneTimeMs { get; init; }
    public string? TyreCompoundFitted { get; init; }
    public bool RefuelledStop { get; init; }
    public bool DriverChanged { get; init; }
    public string? DriverInName { get; init; }
    public string? DriverOutName { get; init; }
    public bool IsUnderCaution { get; init; }
    public bool IsPenaltyServed { get; init; }
    public string? Note { get; init; }
}

/// <summary>A stewards' decision.</summary>
public sealed record PenaltyResponse
{
    public Guid Id { get; init; }
    public Guid SessionId { get; init; }
    public Guid EventEntryId { get; init; }
    public string CarNumber { get; init; } = string.Empty;
    public Guid? DriverId { get; init; }
    public string? DriverName { get; init; }
    public PenaltyType Type { get; init; }
    public int? TimePenaltySeconds { get; init; }
    public int? GridPositionDrop { get; init; }
    public int? LicencePoints { get; init; }
    public decimal? FineAmount { get; init; }
    public string? FineCurrency { get; init; }
    public string? Reason { get; init; }
    public int? LapNumber { get; init; }
    public DateTime? IssuedAtUtc { get; init; }
    public Guid? AppliedToSessionId { get; init; }
    public bool IsServed { get; init; }
    public bool IsRescinded { get; init; }
}

/// <summary>A driver's place in a championship table.</summary>
public sealed record DriverStandingResponse
{
    public int Position { get; init; }
    public int? PreviousPosition { get; init; }

    /// <summary>Movement since the previous round. Positive means gained places.</summary>
    public int? PositionChange { get; init; }

    public Guid DriverId { get; init; }
    public string DriverName { get; init; } = string.Empty;
    public string DriverSlug { get; init; } = string.Empty;
    public string? Abbreviation { get; init; }
    public string? CountryCode { get; init; }
    public string? HeadshotUrl { get; init; }

    /// <summary>Teams the driver has scored with this season; more than one after a mid-season move.</summary>
    public IReadOnlyList<string> TeamNames { get; init; } = [];

    public string? VehicleClassName { get; init; }
    public decimal Points { get; init; }
    public decimal PointsBeforeDrops { get; init; }
    public int Starts { get; init; }
    public int Wins { get; init; }
    public int Podiums { get; init; }
    public int PolePositions { get; init; }
    public int FastestLaps { get; init; }
    public int Retirements { get; init; }
    public DateTime RecalculatedAtUtc { get; init; }
}

/// <summary>A team's place in a championship table.</summary>
public sealed record TeamStandingResponse
{
    public int Position { get; init; }
    public int? PreviousPosition { get; init; }
    public int? PositionChange { get; init; }
    public Guid TeamId { get; init; }
    public string TeamName { get; init; } = string.Empty;
    public string TeamSlug { get; init; } = string.Empty;
    public string? CountryCode { get; init; }
    public string? LogoUrl { get; init; }
    public string? PrimaryColorHex { get; init; }
    public string? VehicleClassName { get; init; }
    public decimal Points { get; init; }
    public decimal PointsBeforeDrops { get; init; }
    public int Starts { get; init; }
    public int Wins { get; init; }
    public int Podiums { get; init; }
    public int PolePositions { get; init; }
    public int FastestLaps { get; init; }
    public DateTime RecalculatedAtUtc { get; init; }
}
