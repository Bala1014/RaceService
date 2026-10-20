using System.ComponentModel.DataAnnotations;
using RaceService.Contracts.Enums;

namespace RaceService.Contracts.Requests;

/// <summary>
/// Replaces a session's entire classification in one call.
/// </summary>
/// <remarks>
/// Whole-classification replacement rather than row-by-row creation, because a result table is
/// only ever valid as a set: positions must be contiguous and unique, exactly one car holds the
/// fastest lap, and points depend on every other row. Accepting rows one at a time would mean
/// the table is inconsistent between requests and would make the uniqueness constraints fire
/// against a half-written state. Posting the set lets it be validated and swapped atomically.
/// </remarks>
public sealed record ReplaceSessionResultsRequest
{
    /// <summary>Trust level to record against the session once the rows are stored.</summary>
    public ResultsStatus ResultsStatus { get; init; } = ResultsStatus.Provisional;

    /// <summary>
    /// Recompute points from the season's points scheme instead of trusting the values supplied
    /// per row. Preferred, since it cannot disagree with the published regulations.
    /// </summary>
    public bool RecalculatePoints { get; init; } = true;

    [MinLength(1)]
    public IReadOnlyList<SessionResultRequest> Results { get; init; } = [];
}

/// <summary>One classification row.</summary>
public sealed record SessionResultRequest
{
    [Required]
    public Guid EventEntryId { get; init; }

    /// <summary>Null for a car with no classified position, such as a non-starter.</summary>
    [Range(1, 200)]
    public int? Position { get; init; }

    [Range(1, 200)]
    public int? ClassPosition { get; init; }

    [Range(1, 200)]
    public int? StartingPosition { get; init; }

    [Range(0, 10_000)]
    public int LapsCompleted { get; init; }

    [Range(0, 10_000)]
    public int LapsLed { get; init; }

    /// <summary>Total elapsed time in milliseconds.</summary>
    [Range(0, long.MaxValue)]
    public long? TotalTimeMs { get; init; }

    [Range(0, long.MaxValue)]
    public long? GapToLeaderMs { get; init; }

    [Range(0, 1000)]
    public int? GapToLeaderLaps { get; init; }

    [Range(1, int.MaxValue)]
    public int? BestLapTimeMs { get; init; }

    [Range(1, 10_000)]
    public int? BestLapNumber { get; init; }

    [Range(0, 600)]
    public decimal? AverageSpeedKph { get; init; }

    [Range(0, 600)]
    public decimal? TopSpeedKph { get; init; }

    public ResultStatus Status { get; init; } = ResultStatus.Classified;

    [StringLength(200)]
    public string? StatusDetail { get; init; }

    public bool IsClassified { get; init; } = true;

    /// <summary>Ignored when the request asks for points to be recalculated.</summary>
    [Range(0, 1000)]
    public decimal PointsScored { get; init; }

    public bool IsFastestLap { get; init; }

    public bool IsPolePosition { get; init; }

    [Range(0, 200)]
    public int PitStopCount { get; init; }

    [Range(0, int.MaxValue)]
    public int PenaltyTimeMs { get; init; }

    /// <summary>
    /// Per-driver split. Optional for a single-driver car, where the crew is taken from the
    /// entry's lineup; required in endurance racing, where the split decides who scores.
    /// </summary>
    public IReadOnlyList<SessionResultDriverRequest> Drivers { get; init; } = [];
}

/// <summary>One driver's share of a car's result.</summary>
public sealed record SessionResultDriverRequest
{
    [Required]
    public Guid DriverId { get; init; }

    [Range(0, 10_000)]
    public int LapsDriven { get; init; }

    [Range(1, int.MaxValue)]
    public int? BestLapTimeMs { get; init; }

    [Range(0, long.MaxValue)]
    public long? TimeDrivenMs { get; init; }

    public bool SetBestLap { get; init; }
}

/// <summary>Uploads a session's lap-by-lap timing sheet.</summary>
public sealed record ReplaceSessionLapsRequest
{
    /// <summary>Laps for the whole session. Replaces any already stored.</summary>
    public IReadOnlyList<SessionLapRequest> Laps { get; init; } = [];
}

/// <summary>One completed lap.</summary>
public sealed record SessionLapRequest
{
    [Required]
    public Guid EventEntryId { get; init; }

    public Guid? DriverId { get; init; }

    [Range(1, 10_000)]
    public int LapNumber { get; init; }

    [Range(1, int.MaxValue)]
    public int? LapTimeMs { get; init; }

    [Range(1, int.MaxValue)]
    public int? Sector1Ms { get; init; }

    [Range(1, int.MaxValue)]
    public int? Sector2Ms { get; init; }

    [Range(1, int.MaxValue)]
    public int? Sector3Ms { get; init; }

    [Range(1, 200)]
    public int? Position { get; init; }

    [Range(0, 600)]
    public decimal? SpeedTrapKph { get; init; }

    [StringLength(30)]
    public string? TyreCompound { get; init; }

    [Range(0, 1000)]
    public int? TyreAgeLaps { get; init; }

    public bool IsPitInLap { get; init; }
    public bool IsPitOutLap { get; init; }
    public bool IsUnderCaution { get; init; }
    public bool IsDeletedByStewards { get; init; }
}

/// <summary>Records a pit stop.</summary>
public sealed record CreatePitStopRequest
{
    [Required]
    public Guid EventEntryId { get; init; }

    [Range(1, 200)]
    public int StopNumber { get; init; }

    [Range(0, 10_000)]
    public int LapNumber { get; init; }

    [Range(0, int.MaxValue)]
    public int? StationaryTimeMs { get; init; }

    [Range(0, int.MaxValue)]
    public int? TotalPitLaneTimeMs { get; init; }

    [StringLength(30)]
    public string? TyreCompoundFitted { get; init; }

    public bool RefuelledStop { get; init; }
    public bool DriverChanged { get; init; }
    public Guid? DriverInId { get; init; }
    public Guid? DriverOutId { get; init; }
    public bool IsUnderCaution { get; init; }
    public bool IsPenaltyServed { get; init; }

    [StringLength(300)]
    public string? Note { get; init; }
}

/// <summary>Records a stewards' decision.</summary>
public sealed record CreatePenaltyRequest
{
    [Required]
    public Guid EventEntryId { get; init; }

    public Guid? DriverId { get; init; }

    [Required]
    public PenaltyType Type { get; init; }

    [Range(0, 3600)]
    public int? TimePenaltySeconds { get; init; }

    [Range(0, 100)]
    public int? GridPositionDrop { get; init; }

    [Range(0, 20)]
    public int? LicencePoints { get; init; }

    [Range(0, 100_000_000)]
    public decimal? FineAmount { get; init; }

    [StringLength(3, MinimumLength = 3)]
    public string? FineCurrency { get; init; }

    [StringLength(1000)]
    public string? Reason { get; init; }

    [Range(0, 10_000)]
    public int? LapNumber { get; init; }

    public DateTime? IssuedAtUtc { get; init; }

    /// <summary>
    /// Session the penalty is served in, when that is a later one — a qualifying grid drop taken
    /// at the start of the race.
    /// </summary>
    public Guid? AppliedToSessionId { get; init; }
}
