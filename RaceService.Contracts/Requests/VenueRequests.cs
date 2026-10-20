using System.ComponentModel.DataAnnotations;
using RaceService.Contracts.Enums;

namespace RaceService.Contracts.Requests;

/// <summary>Creates a venue, optionally with its first layout.</summary>
public sealed record CreateTrackRequest
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(250)]
    public string? OfficialName { get; init; }

    [StringLength(150)]
    public string? Slug { get; init; }

    [StringLength(120)]
    public string? City { get; init; }

    [Required, StringLength(100)]
    public string Country { get; init; } = string.Empty;

    [StringLength(3, MinimumLength = 3)]
    public string? CountryCode { get; init; }

    /// <summary>IANA identifier such as "Europe/Monaco". Validated against the tz database.</summary>
    [Required, StringLength(80)]
    public string TimeZoneId { get; init; } = "UTC";

    [Range(-90, 90)]
    public double? Latitude { get; init; }

    [Range(-180, 180)]
    public double? Longitude { get; init; }

    [Range(1894, 2200)]
    public int? OpenedYear { get; init; }

    [StringLength(4000)]
    public string? Description { get; init; }

    [Url, StringLength(1000)]
    public string? ImageUrl { get; init; }

    [Url, StringLength(1000)]
    public string? WebsiteUrl { get; init; }

    public CreateTrackLayoutRequest? InitialLayout { get; init; }
}

/// <summary>Updates a venue.</summary>
public sealed record UpdateTrackRequest
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(250)]
    public string? OfficialName { get; init; }

    [StringLength(120)]
    public string? City { get; init; }

    [Required, StringLength(100)]
    public string Country { get; init; } = string.Empty;

    [StringLength(3, MinimumLength = 3)]
    public string? CountryCode { get; init; }

    [Required, StringLength(80)]
    public string TimeZoneId { get; init; } = "UTC";

    [Range(-90, 90)]
    public double? Latitude { get; init; }

    [Range(-180, 180)]
    public double? Longitude { get; init; }

    [Range(1894, 2200)]
    public int? OpenedYear { get; init; }

    [StringLength(4000)]
    public string? Description { get; init; }

    [Url, StringLength(1000)]
    public string? ImageUrl { get; init; }

    [Url, StringLength(1000)]
    public string? WebsiteUrl { get; init; }

    public bool IsActive { get; init; } = true;
}

/// <summary>Creates a layout under a venue.</summary>
public sealed record CreateTrackLayoutRequest
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(150)]
    public string? Slug { get; init; }

    /// <summary>Lap distance in whole metres.</summary>
    [Range(100, 100_000)]
    public int LengthMeters { get; init; }

    [Range(1, 300)]
    public int? NumberOfTurns { get; init; }

    public TrackDirection Direction { get; init; } = TrackDirection.Clockwise;

    public TrackType TrackType { get; init; } = TrackType.PermanentCircuit;

    [Range(0, 2000)]
    public int? ElevationChangeMeters { get; init; }

    [Range(0, 20_000)]
    public int? LongestStraightMeters { get; init; }

    [Url, StringLength(1000)]
    public string? MapImageUrl { get; init; }

    /// <summary>Marks this as the configuration in current championship use, demoting any other.</summary>
    public bool IsCurrent { get; init; } = true;
}
