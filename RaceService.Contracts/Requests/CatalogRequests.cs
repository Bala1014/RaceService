using System.ComponentModel.DataAnnotations;
using RaceService.Contracts.Enums;

namespace RaceService.Contracts.Requests;

/// <summary>Creates a championship.</summary>
public sealed record CreateSeriesRequest
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Required, StringLength(30)]
    public string ShortName { get; init; } = string.Empty;

    /// <summary>Optional. Generated from the name when omitted.</summary>
    [StringLength(120)]
    public string? Slug { get; init; }

    public Discipline Discipline { get; init; } = Discipline.Unknown;

    [StringLength(100)]
    public string? SanctioningBody { get; init; }

    [StringLength(4000)]
    public string? Description { get; init; }

    [Url, StringLength(1000)]
    public string? LogoUrl { get; init; }

    [RegularExpression("^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$")]
    public string? PrimaryColorHex { get; init; }

    [Url, StringLength(1000)]
    public string? WebsiteUrl { get; init; }

    [Range(1894, 2200)]
    public int? InauguralYear { get; init; }

    public int DisplayOrder { get; init; }
}

/// <summary>Updates a championship. Every field is replaced.</summary>
public sealed record UpdateSeriesRequest
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Required, StringLength(30)]
    public string ShortName { get; init; } = string.Empty;

    public Discipline Discipline { get; init; }

    [StringLength(100)]
    public string? SanctioningBody { get; init; }

    [StringLength(4000)]
    public string? Description { get; init; }

    [Url, StringLength(1000)]
    public string? LogoUrl { get; init; }

    [RegularExpression("^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$")]
    public string? PrimaryColorHex { get; init; }

    [Url, StringLength(1000)]
    public string? WebsiteUrl { get; init; }

    [Range(1894, 2200)]
    public int? InauguralYear { get; init; }

    public int DisplayOrder { get; init; }

    public bool IsActive { get; init; } = true;
}

/// <summary>Creates a season under a series.</summary>
public sealed record CreateSeasonRequest
{
    [Required]
    public Guid SeriesId { get; init; }

    [Range(1894, 2200)]
    public int Year { get; init; }

    [Required, StringLength(250, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(120)]
    public string? Slug { get; init; }

    public SeasonStatus Status { get; init; } = SeasonStatus.Announced;

    public DateOnly? StartDate { get; init; }

    public DateOnly? EndDate { get; init; }

    [StringLength(4000)]
    public string? Description { get; init; }

    [Url, StringLength(1000)]
    public string? BannerImageUrl { get; init; }

    [Range(1, 100)]
    public int? PlannedRoundCount { get; init; }

    [Range(0, 20)]
    public int DroppedScoresCount { get; init; }
}

/// <summary>Updates a season.</summary>
public sealed record UpdateSeasonRequest
{
    [Required, StringLength(250, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    public SeasonStatus Status { get; init; }

    public DateOnly? StartDate { get; init; }

    public DateOnly? EndDate { get; init; }

    [StringLength(4000)]
    public string? Description { get; init; }

    [Url, StringLength(1000)]
    public string? BannerImageUrl { get; init; }

    [Range(1, 100)]
    public int? PlannedRoundCount { get; init; }

    [Range(0, 20)]
    public int DroppedScoresCount { get; init; }
}
