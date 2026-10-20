using System.ComponentModel.DataAnnotations;
using RaceService.Contracts.Enums;

namespace RaceService.Contracts.Requests;

/// <summary>Creates a race weekend, optionally with its whole schedule in one call.</summary>
public sealed record CreateRaceEventRequest
{
    [Required]
    public Guid SeasonId { get; init; }

    [Required]
    public Guid TrackLayoutId { get; init; }

    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(300)]
    public string? OfficialName { get; init; }

    [StringLength(60)]
    public string? ShortName { get; init; }

    [StringLength(150)]
    public string? Slug { get; init; }

    [Range(1, 100)]
    public int? RoundNumber { get; init; }

    /// <summary>Defaults to the venue's zone when omitted.</summary>
    [StringLength(80)]
    public string? TimeZoneId { get; init; }

    public EventStatus Status { get; init; } = EventStatus.Draft;

    [StringLength(500)]
    public string? ShortDescription { get; init; }

    [StringLength(8000)]
    public string? Description { get; init; }

    [Url, StringLength(1000)]
    public string? PosterImageUrl { get; init; }

    [Url, StringLength(1000)]
    public string? BannerImageUrl { get; init; }

    [Range(1000, 10_000_000)]
    public int? ScheduledDistanceMeters { get; init; }

    [Range(1, 10_000)]
    public int? ScheduledDurationMinutes { get; init; }

    public bool HasSprint { get; init; }

    public bool IsNonChampionship { get; init; }

    public bool IsPublished { get; init; }

    /// <summary>
    /// The weekend's sessions. Supplying them here is strongly preferred: the event's own start
    /// and end are derived from the sessions, so an event created without any has no meaningful
    /// place on a calendar.
    /// </summary>
    public IReadOnlyList<CreateSessionRequest> Sessions { get; init; } = [];
}

/// <summary>Updates a race weekend's metadata. Sessions are managed through their own endpoints.</summary>
public sealed record UpdateRaceEventRequest
{
    [Required]
    public Guid TrackLayoutId { get; init; }

    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(300)]
    public string? OfficialName { get; init; }

    [StringLength(60)]
    public string? ShortName { get; init; }

    [Range(1, 100)]
    public int? RoundNumber { get; init; }

    [StringLength(80)]
    public string? TimeZoneId { get; init; }

    [StringLength(500)]
    public string? ShortDescription { get; init; }

    [StringLength(8000)]
    public string? Description { get; init; }

    [Url, StringLength(1000)]
    public string? PosterImageUrl { get; init; }

    [Url, StringLength(1000)]
    public string? BannerImageUrl { get; init; }

    [Range(1000, 10_000_000)]
    public int? ScheduledDistanceMeters { get; init; }

    [Range(1, 10_000)]
    public int? ScheduledDurationMinutes { get; init; }

    public bool HasSprint { get; init; }

    public bool IsNonChampionship { get; init; }

    public bool IsPublished { get; init; }
}

/// <summary>Moves an event to a new lifecycle state.</summary>
public sealed record UpdateEventStatusRequest
{
    [Required]
    public EventStatus Status { get; init; }

    /// <summary>Why the status changed. Required when cancelling or postponing.</summary>
    [StringLength(500)]
    public string? Reason { get; init; }
}

/// <summary>Creates a session within a weekend.</summary>
public sealed record CreateSessionRequest
{
    [Required]
    public SessionType Type { get; init; }

    /// <summary>Distinguishes FP1 from FP2, or Q1 from Q3. Defaults to 1.</summary>
    [Range(1, 20)]
    public int SequenceNumber { get; init; } = 1;

    /// <summary>Optional. Derived from type and sequence when omitted, e.g. "Practice 2".</summary>
    [StringLength(120)]
    public string? Name { get; init; }

    /// <summary>Scheduled start. Must be supplied in UTC.</summary>
    [Required]
    public DateTime ScheduledStartUtc { get; init; }

    public DateTime? ScheduledEndUtc { get; init; }

    [Range(1, 3000)]
    public int? ScheduledDurationMinutes { get; init; }

    [Range(1, 2000)]
    public int? ScheduledLaps { get; init; }

    /// <summary>Defaults to true for race and sprint sessions when not specified.</summary>
    public bool? IsPointsScoring { get; init; }
}

/// <summary>Updates a session's schedule.</summary>
public sealed record UpdateSessionRequest
{
    [StringLength(120)]
    public string? Name { get; init; }

    [Required]
    public DateTime ScheduledStartUtc { get; init; }

    public DateTime? ScheduledEndUtc { get; init; }

    [Range(1, 3000)]
    public int? ScheduledDurationMinutes { get; init; }

    [Range(1, 2000)]
    public int? ScheduledLaps { get; init; }

    public bool IsPointsScoring { get; init; }
}

/// <summary>Records what actually happened to a session.</summary>
public sealed record UpdateSessionStatusRequest
{
    [Required]
    public SessionStatus Status { get; init; }

    public DateTime? ActualStartUtc { get; init; }

    public DateTime? ActualEndUtc { get; init; }

    [Range(0, 2000)]
    public int? CompletedLaps { get; init; }

    [StringLength(300)]
    public string? WeatherSummary { get; init; }

    [Range(-50, 70)]
    public decimal? AirTemperatureC { get; init; }

    [Range(-50, 120)]
    public decimal? TrackTemperatureC { get; init; }

    [Range(0, 100)]
    public decimal? HumidityPercent { get; init; }

    [Range(0, 50)]
    public int? RedFlagCount { get; init; }

    [Range(0, 50)]
    public int? SafetyCarCount { get; init; }

    [StringLength(1000)]
    public string? StatusNote { get; init; }
}
