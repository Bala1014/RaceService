using RaceService.Contracts.Enums;

namespace RaceService.Contracts.Responses;

/// <summary>A championship in list form.</summary>
public sealed record SeriesSummaryResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ShortName { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public Discipline Discipline { get; init; }
    public string? SanctioningBody { get; init; }
    public string? LogoUrl { get; init; }
    public string? PrimaryColorHex { get; init; }
    public bool IsActive { get; init; }

    /// <summary>Year of the most recent season, so a client can deep-link without a second call.</summary>
    public int? LatestSeasonYear { get; init; }
}

/// <summary>A championship with its seasons and classes.</summary>
public sealed record SeriesDetailResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ShortName { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public Discipline Discipline { get; init; }
    public string? SanctioningBody { get; init; }
    public string? Description { get; init; }
    public string? LogoUrl { get; init; }
    public string? PrimaryColorHex { get; init; }
    public string? WebsiteUrl { get; init; }
    public int? InauguralYear { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<SeasonSummaryResponse> Seasons { get; init; } = [];
    public IReadOnlyList<VehicleClassResponse> VehicleClasses { get; init; } = [];
}

/// <summary>A season in list form.</summary>
public sealed record SeasonSummaryResponse
{
    public Guid Id { get; init; }
    public Guid SeriesId { get; init; }
    public string SeriesName { get; init; } = string.Empty;
    public string SeriesSlug { get; init; } = string.Empty;
    public int Year { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public SeasonStatus Status { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public int RoundCount { get; init; }
    public int? PlannedRoundCount { get; init; }
}

/// <summary>A season with its calendar.</summary>
public sealed record SeasonDetailResponse
{
    public Guid Id { get; init; }
    public Guid SeriesId { get; init; }
    public string SeriesName { get; init; } = string.Empty;
    public string SeriesSlug { get; init; } = string.Empty;
    public int Year { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public SeasonStatus Status { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? Description { get; init; }
    public string? BannerImageUrl { get; init; }
    public int? PlannedRoundCount { get; init; }
    public int DroppedScoresCount { get; init; }
    public int EntryCount { get; init; }
    public IReadOnlyList<RaceEventSummaryResponse> Calendar { get; init; } = [];
    public IReadOnlyList<VehicleClassResponse> VehicleClasses { get; init; } = [];
}

/// <summary>A class within a series.</summary>
public sealed record VehicleClassResponse
{
    public Guid Id { get; init; }
    public Guid? SeriesId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ShortName { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ColorHex { get; init; }
    public int DisplayOrder { get; init; }
}

/// <summary>A marque.</summary>
public sealed record ManufacturerResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? CountryCode { get; init; }
    public string? LogoUrl { get; init; }
    public bool SuppliesChassis { get; init; }
    public bool SuppliesEngines { get; init; }
    public bool SuppliesTyres { get; init; }
}

/// <summary>One row of a points table.</summary>
public sealed record PointsSchemeResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public SessionType AppliesToSessionType { get; init; }
    public ChampionshipType ChampionshipType { get; init; }
    public decimal FastestLapPoints { get; init; }
    public int? FastestLapMaxPosition { get; init; }
    public decimal PolePositionPoints { get; init; }
    public decimal MinimumDistancePercent { get; init; }
    public decimal MinimumDriverDistancePercent { get; init; }
    public IReadOnlyDictionary<int, decimal> Positions { get; init; } = new Dictionary<int, decimal>();
}
