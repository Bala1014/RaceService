using System.ComponentModel.DataAnnotations;
using RaceService.Contracts.Enums;

namespace RaceService.Contracts.Requests;

/// <summary>Creates a competitor.</summary>
public sealed record CreateDriverRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string FirstName { get; init; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 1)]
    public string LastName { get; init; } = string.Empty;

    /// <summary>Overrides the "first last" default, for mononyms and suffixes.</summary>
    [StringLength(200)]
    public string? FullName { get; init; }

    [StringLength(5)]
    public string? Abbreviation { get; init; }

    [StringLength(150)]
    public string? Slug { get; init; }

    [StringLength(3, MinimumLength = 3)]
    public string? CountryCode { get; init; }

    [StringLength(100)]
    public string? Nationality { get; init; }

    public DateOnly? DateOfBirth { get; init; }

    [StringLength(150)]
    public string? PlaceOfBirth { get; init; }

    [Range(0, 999)]
    public int? PermanentNumber { get; init; }

    [Url, StringLength(1000)]
    public string? ImageUrl { get; init; }

    [Url, StringLength(1000)]
    public string? HeadshotUrl { get; init; }

    [StringLength(8000)]
    public string? Biography { get; init; }

    [Url, StringLength(1000)]
    public string? WebsiteUrl { get; init; }
}

/// <summary>Updates a competitor.</summary>
public sealed record UpdateDriverRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string FirstName { get; init; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 1)]
    public string LastName { get; init; } = string.Empty;

    [StringLength(200)]
    public string? FullName { get; init; }

    [StringLength(5)]
    public string? Abbreviation { get; init; }

    [StringLength(3, MinimumLength = 3)]
    public string? CountryCode { get; init; }

    [StringLength(100)]
    public string? Nationality { get; init; }

    public DateOnly? DateOfBirth { get; init; }

    public DateOnly? DateOfDeath { get; init; }

    [StringLength(150)]
    public string? PlaceOfBirth { get; init; }

    [Range(0, 999)]
    public int? PermanentNumber { get; init; }

    [Url, StringLength(1000)]
    public string? ImageUrl { get; init; }

    [Url, StringLength(1000)]
    public string? HeadshotUrl { get; init; }

    [StringLength(8000)]
    public string? Biography { get; init; }

    [Url, StringLength(1000)]
    public string? WebsiteUrl { get; init; }

    public bool IsActive { get; init; } = true;
}

/// <summary>Creates a team.</summary>
public sealed record CreateTeamRequest
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(300)]
    public string? OfficialName { get; init; }

    [Required, StringLength(50)]
    public string ShortName { get; init; } = string.Empty;

    [StringLength(150)]
    public string? Slug { get; init; }

    [StringLength(3, MinimumLength = 3)]
    public string? CountryCode { get; init; }

    [StringLength(120)]
    public string? BaseCity { get; init; }

    [StringLength(100)]
    public string? BaseCountry { get; init; }

    [Range(1894, 2200)]
    public int? FoundedYear { get; init; }

    [Url, StringLength(1000)]
    public string? LogoUrl { get; init; }

    [RegularExpression("^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$")]
    public string? PrimaryColorHex { get; init; }

    [Url, StringLength(1000)]
    public string? WebsiteUrl { get; init; }

    [StringLength(4000)]
    public string? Description { get; init; }
}

/// <summary>Updates a team.</summary>
public sealed record UpdateTeamRequest
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(300)]
    public string? OfficialName { get; init; }

    [Required, StringLength(50)]
    public string ShortName { get; init; } = string.Empty;

    [StringLength(3, MinimumLength = 3)]
    public string? CountryCode { get; init; }

    [StringLength(120)]
    public string? BaseCity { get; init; }

    [StringLength(100)]
    public string? BaseCountry { get; init; }

    [Range(1894, 2200)]
    public int? FoundedYear { get; init; }

    [Url, StringLength(1000)]
    public string? LogoUrl { get; init; }

    [RegularExpression("^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$")]
    public string? PrimaryColorHex { get; init; }

    [Url, StringLength(1000)]
    public string? WebsiteUrl { get; init; }

    [StringLength(4000)]
    public string? Description { get; init; }

    public bool IsActive { get; init; } = true;
}

/// <summary>Registers a car for a season.</summary>
public sealed record CreateEntryRequest
{
    [Required]
    public Guid TeamId { get; init; }

    /// <summary>A string so "007" and "63A" survive intact.</summary>
    [Required, StringLength(10, MinimumLength = 1)]
    public string CarNumber { get; init; } = string.Empty;

    [Required, StringLength(250, MinimumLength = 2)]
    public string EntrantName { get; init; } = string.Empty;

    public Guid? VehicleClassId { get; init; }

    public Guid? ChassisManufacturerId { get; init; }

    [StringLength(120)]
    public string? ChassisModel { get; init; }

    public Guid? EngineManufacturerId { get; init; }

    [StringLength(120)]
    public string? EngineModel { get; init; }

    public Guid? TyreSupplierId { get; init; }

    public EntryStatus Status { get; init; } = EntryStatus.Pending;

    [Url, StringLength(1000)]
    public string? LiveryImageUrl { get; init; }

    [RegularExpression("^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$")]
    public string? PrimaryColorHex { get; init; }

    public bool IsEligibleForPoints { get; init; } = true;
}

/// <summary>Updates a season entry.</summary>
public sealed record UpdateEntryRequest
{
    [Required]
    public Guid TeamId { get; init; }

    [Required, StringLength(10, MinimumLength = 1)]
    public string CarNumber { get; init; } = string.Empty;

    [Required, StringLength(250, MinimumLength = 2)]
    public string EntrantName { get; init; } = string.Empty;

    public Guid? VehicleClassId { get; init; }
    public Guid? ChassisManufacturerId { get; init; }

    [StringLength(120)]
    public string? ChassisModel { get; init; }

    public Guid? EngineManufacturerId { get; init; }

    [StringLength(120)]
    public string? EngineModel { get; init; }

    public Guid? TyreSupplierId { get; init; }

    [Url, StringLength(1000)]
    public string? LiveryImageUrl { get; init; }

    [RegularExpression("^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$")]
    public string? PrimaryColorHex { get; init; }

    public bool IsEligibleForPoints { get; init; } = true;
}

/// <summary>Changes an entry's registration state.</summary>
public sealed record UpdateEntryStatusRequest
{
    [Required]
    public EntryStatus Status { get; init; }

    [StringLength(500)]
    public string? Reason { get; init; }
}

/// <summary>Adds a season entry to one round, with its crew for that round.</summary>
public sealed record CreateEventEntryRequest
{
    [Required]
    public Guid EntryId { get; init; }

    /// <summary>Number carried at this round, when it differs from the season registration.</summary>
    [StringLength(10)]
    public string? CarNumberOverride { get; init; }

    public EntryStatus Status { get; init; } = EntryStatus.Confirmed;

    [Range(1, 100)]
    public int? StartingGridPosition { get; init; }

    public bool StartsFromPitLane { get; init; }

    public bool IsEligibleForPoints { get; init; } = true;

    /// <summary>The crew. At least one driver is required.</summary>
    [MinLength(1)]
    public IReadOnlyList<EventEntryDriverRequest> Drivers { get; init; } = [];
}

/// <summary>One member of a crew.</summary>
public sealed record EventEntryDriverRequest
{
    [Required]
    public Guid DriverId { get; init; }

    /// <summary>Order within the crew, 1-based. Must be unique inside the entry.</summary>
    [Range(1, 20)]
    public int LineupPosition { get; init; } = 1;

    public DriverRole Role { get; init; } = DriverRole.Race;

    public DriverGrade Grade { get; init; } = DriverGrade.NotCategorised;

    /// <summary>Exactly one driver in the crew may be the starter.</summary>
    public bool IsStartingDriver { get; init; }
}

/// <summary>Replaces a car's crew for one round.</summary>
public sealed record UpdateEventEntryLineupRequest
{
    [MinLength(1)]
    public IReadOnlyList<EventEntryDriverRequest> Drivers { get; init; } = [];
}
