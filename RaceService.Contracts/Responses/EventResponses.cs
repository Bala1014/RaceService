using RaceService.Contracts.Common;
using RaceService.Contracts.Enums;

namespace RaceService.Contracts.Responses;

/// <summary>A race weekend in list and calendar form.</summary>
public sealed record RaceEventSummaryResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ShortName { get; init; }
    public string Slug { get; init; } = string.Empty;
    public int? RoundNumber { get; init; }
    public EventStatus Status { get; init; }

    /// <summary>Start of the weekend, in UTC, at the venue, and in the caller's zone.</summary>
    public LocalizedTime StartTime { get; init; } = new();

    public LocalizedTime EndTime { get; init; } = new();

    public Guid SeasonId { get; init; }
    public int SeasonYear { get; init; }
    public Guid SeriesId { get; init; }
    public string SeriesName { get; init; } = string.Empty;
    public string SeriesShortName { get; init; } = string.Empty;
    public string SeriesSlug { get; init; } = string.Empty;
    public string? SeriesColorHex { get; init; }

    public Guid TrackId { get; init; }
    public string TrackName { get; init; } = string.Empty;
    public string TrackSlug { get; init; } = string.Empty;
    public string TrackCountry { get; init; } = string.Empty;
    public string? TrackCountryCode { get; init; }

    public string? PosterImageUrl { get; init; }
    public bool HasSprint { get; init; }

    /// <summary>The main race of the weekend, which is what a calendar tile counts down to.</summary>
    public SessionSummaryResponse? FeatureSession { get; init; }

    public int SessionCount { get; init; }
    public int EntryCount { get; init; }
    public bool HasResults { get; init; }
}

/// <summary>A race weekend with its full schedule.</summary>
public sealed record RaceEventDetailResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? OfficialName { get; init; }
    public string? ShortName { get; init; }
    public string Slug { get; init; } = string.Empty;
    public int? RoundNumber { get; init; }
    public EventStatus Status { get; init; }
    public LocalizedTime StartTime { get; init; } = new();
    public LocalizedTime EndTime { get; init; } = new();
    public string TimeZoneId { get; init; } = "UTC";

    public SeasonSummaryResponse Season { get; init; } = new();
    public TrackLayoutResponse TrackLayout { get; init; } = new();
    public TrackSummaryResponse Track { get; init; } = new();

    public string? ShortDescription { get; init; }
    public string? Description { get; init; }
    public string? PosterImageUrl { get; init; }
    public string? BannerImageUrl { get; init; }

    public int? ScheduledDistanceMeters { get; init; }
    public int? ScheduledDurationMinutes { get; init; }
    public bool HasSprint { get; init; }
    public bool IsNonChampionship { get; init; }
    public bool IsPublished { get; init; }

    /// <summary>The same event a year earlier, for a "last time out" panel.</summary>
    public Guid? PreviousEditionId { get; init; }

    public IReadOnlyList<SessionSummaryResponse> Sessions { get; init; } = [];
    public int EntryCount { get; init; }
}

/// <summary>A session in list form.</summary>
public sealed record SessionSummaryResponse
{
    public Guid Id { get; init; }
    public Guid RaceEventId { get; init; }
    public SessionType Type { get; init; }
    public int SequenceNumber { get; init; }
    public string Name { get; init; } = string.Empty;
    public int WeekendOrder { get; init; }
    public LocalizedTime ScheduledStart { get; init; } = new();
    public DateTime? ScheduledEndUtc { get; init; }
    public SessionStatus Status { get; init; }
    public ResultsStatus ResultsStatus { get; init; }
    public int? ScheduledDurationMinutes { get; init; }
    public int? ScheduledLaps { get; init; }
    public bool IsPointsScoring { get; init; }
    public bool HasResults { get; init; }
}

/// <summary>A session with everything recorded about it after the flag.</summary>
public sealed record SessionDetailResponse
{
    public Guid Id { get; init; }
    public Guid RaceEventId { get; init; }
    public string RaceEventName { get; init; } = string.Empty;
    public string RaceEventSlug { get; init; } = string.Empty;
    public SessionType Type { get; init; }
    public int SequenceNumber { get; init; }
    public string Name { get; init; } = string.Empty;
    public LocalizedTime ScheduledStart { get; init; } = new();
    public DateTime? ScheduledEndUtc { get; init; }
    public DateTime? ActualStartUtc { get; init; }
    public DateTime? ActualEndUtc { get; init; }
    public SessionStatus Status { get; init; }
    public string? StatusNote { get; init; }
    public ResultsStatus ResultsStatus { get; init; }
    public DateTime? ResultsPublishedAtUtc { get; init; }
    public int? ScheduledDurationMinutes { get; init; }
    public int? ScheduledLaps { get; init; }
    public int? CompletedLaps { get; init; }
    public bool IsPointsScoring { get; init; }

    public string? WeatherSummary { get; init; }
    public decimal? AirTemperatureC { get; init; }
    public decimal? TrackTemperatureC { get; init; }
    public decimal? HumidityPercent { get; init; }
    public int? RedFlagCount { get; init; }
    public int? SafetyCarCount { get; init; }

    public IReadOnlyList<SessionResultResponse> Results { get; init; } = [];
}
