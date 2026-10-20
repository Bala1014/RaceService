using RaceService.Contracts.Enums;

namespace RaceService.Contracts.Responses;

/// <summary>A competitor in list form.</summary>
public sealed record DriverSummaryResponse
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? Abbreviation { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string? CountryCode { get; init; }
    public string? Nationality { get; init; }
    public int? PermanentNumber { get; init; }
    public string? HeadshotUrl { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>A competitor with career totals.</summary>
public sealed record DriverDetailResponse
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? Abbreviation { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string? CountryCode { get; init; }
    public string? Nationality { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public DateOnly? DateOfDeath { get; init; }

    /// <summary>Age in whole years, computed server-side so every client agrees.</summary>
    public int? Age { get; init; }

    public string? PlaceOfBirth { get; init; }
    public int? PermanentNumber { get; init; }
    public string? ImageUrl { get; init; }
    public string? HeadshotUrl { get; init; }
    public string? Biography { get; init; }
    public string? WebsiteUrl { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<DriverCareerStatResponse> CareerStats { get; init; } = [];
}

/// <summary>Pre-aggregated career totals.</summary>
public sealed record DriverCareerStatResponse
{
    public Guid? SeriesId { get; init; }
    public string? SeriesName { get; init; }
    public int Starts { get; init; }
    public int Wins { get; init; }
    public int Podiums { get; init; }
    public int PolePositions { get; init; }
    public int FastestLaps { get; init; }
    public int Retirements { get; init; }
    public int LapsCompleted { get; init; }
    public int LapsLed { get; init; }
    public decimal CareerPoints { get; init; }
    public int ChampionshipsWon { get; init; }
    public int? BestFinishPosition { get; init; }
    public int? FirstSeasonYear { get; init; }
    public int? LastSeasonYear { get; init; }

    /// <summary>
    /// When these totals were last rebuilt from results. Exposed so a client can tell how fresh
    /// they are rather than assuming they are live.
    /// </summary>
    public DateTime RecalculatedAtUtc { get; init; }
}

/// <summary>A team in list form.</summary>
public sealed record TeamSummaryResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ShortName { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? CountryCode { get; init; }
    public string? LogoUrl { get; init; }
    public string? PrimaryColorHex { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>A team with its current entries.</summary>
public sealed record TeamDetailResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? OfficialName { get; init; }
    public string ShortName { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? CountryCode { get; init; }
    public string? BaseCity { get; init; }
    public string? BaseCountry { get; init; }
    public int? FoundedYear { get; init; }
    public string? LogoUrl { get; init; }
    public string? PrimaryColorHex { get; init; }
    public string? WebsiteUrl { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<EntryResponse> Entries { get; init; } = [];
}

/// <summary>A car registered for a season.</summary>
public sealed record EntryResponse
{
    public Guid Id { get; init; }
    public Guid SeasonId { get; init; }
    public int SeasonYear { get; init; }
    public string SeriesName { get; init; } = string.Empty;
    public string CarNumber { get; init; } = string.Empty;
    public Guid TeamId { get; init; }
    public string TeamName { get; init; } = string.Empty;
    public string? TeamLogoUrl { get; init; }
    public string EntrantName { get; init; } = string.Empty;
    public Guid? VehicleClassId { get; init; }
    public string? VehicleClassName { get; init; }
    public string? VehicleClassColorHex { get; init; }
    public string? ChassisManufacturerName { get; init; }
    public string? ChassisModel { get; init; }
    public string? EngineManufacturerName { get; init; }
    public string? EngineModel { get; init; }
    public string? TyreSupplierName { get; init; }
    public EntryStatus Status { get; init; }
    public string? LiveryImageUrl { get; init; }
    public string? PrimaryColorHex { get; init; }
    public bool IsEligibleForPoints { get; init; }
}

/// <summary>A car taking part in one round, with its crew.</summary>
public sealed record EventEntryResponse
{
    public Guid Id { get; init; }
    public Guid RaceEventId { get; init; }
    public Guid EntryId { get; init; }

    /// <summary>Number the car actually carries at this round.</summary>
    public string CarNumber { get; init; } = string.Empty;

    public Guid TeamId { get; init; }
    public string TeamName { get; init; } = string.Empty;
    public string? TeamLogoUrl { get; init; }
    public string EntrantName { get; init; } = string.Empty;
    public Guid? VehicleClassId { get; init; }
    public string? VehicleClassName { get; init; }
    public string? VehicleClassColorHex { get; init; }
    public string? ChassisModel { get; init; }
    public string? EngineModel { get; init; }
    public EntryStatus Status { get; init; }
    public string? StatusNote { get; init; }
    public int? StartingGridPosition { get; init; }
    public bool StartsFromPitLane { get; init; }
    public bool IsEligibleForPoints { get; init; }
    public string? LiveryImageUrl { get; init; }
    public IReadOnlyList<EventEntryDriverResponse> Drivers { get; init; } = [];
}

/// <summary>One member of a car's crew.</summary>
public sealed record EventEntryDriverResponse
{
    public Guid DriverId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? Abbreviation { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string? CountryCode { get; init; }
    public string? HeadshotUrl { get; init; }
    public int LineupPosition { get; init; }
    public DriverRole Role { get; init; }
    public DriverGrade Grade { get; init; }
    public bool IsStartingDriver { get; init; }
}
