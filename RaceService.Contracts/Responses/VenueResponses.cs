using RaceService.Contracts.Enums;

namespace RaceService.Contracts.Responses;

/// <summary>A venue in list form.</summary>
public sealed record TrackSummaryResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? City { get; init; }
    public string Country { get; init; } = string.Empty;
    public string? CountryCode { get; init; }
    public string TimeZoneId { get; init; } = "UTC";
    public string? ImageUrl { get; init; }

    /// <summary>Length of the configuration currently in championship use, in metres.</summary>
    public int? CurrentLayoutLengthMeters { get; init; }
}

/// <summary>A venue with its layouts and records.</summary>
public sealed record TrackDetailResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? OfficialName { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string? City { get; init; }
    public string Country { get; init; } = string.Empty;
    public string? CountryCode { get; init; }
    public string TimeZoneId { get; init; } = "UTC";
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public int? OpenedYear { get; init; }
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public string? WebsiteUrl { get; init; }
    public IReadOnlyList<TrackLayoutResponse> Layouts { get; init; } = [];
}

/// <summary>One configuration of a venue.</summary>
public sealed record TrackLayoutResponse
{
    public Guid Id { get; init; }
    public Guid TrackId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public int LengthMeters { get; init; }

    /// <summary>Lap distance in kilometres, precomputed so clients need not agree on rounding.</summary>
    public decimal LengthKm { get; init; }

    public int? NumberOfTurns { get; init; }
    public TrackDirection Direction { get; init; }
    public TrackType TrackType { get; init; }
    public int? ElevationChangeMeters { get; init; }
    public int? LongestStraightMeters { get; init; }
    public string? MapImageUrl { get; init; }
    public bool IsCurrent { get; init; }
    public IReadOnlyList<LapRecordResponse> LapRecords { get; init; } = [];
}

/// <summary>A benchmark lap.</summary>
public sealed record LapRecordResponse
{
    public Guid Id { get; init; }
    public RecordType RecordType { get; init; }
    public int LapTimeMs { get; init; }

    /// <summary>Lap time formatted as m:ss.fff, e.g. "1:27.456".</summary>
    public string LapTimeDisplay { get; init; } = string.Empty;

    public Guid? SeriesId { get; init; }
    public string? SeriesName { get; init; }
    public string? VehicleClassName { get; init; }
    public Guid? DriverId { get; init; }
    public string? DriverName { get; init; }
    public string? TeamName { get; init; }
    public string? VehicleDescription { get; init; }
    public DateOnly SetOn { get; init; }
    public bool IsCurrent { get; init; }
}
