using RaceService.Application.Domain.Common;

namespace RaceService.Application.Domain.Entities.Catalog;

public sealed class Manufacturer : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? CountryCode { get; set; }

    public string? LogoUrl { get; set; }

    public bool SuppliesChassis { get; set; }

    public bool SuppliesEngines { get; set; }

    public bool SuppliesTyres { get; set; }

    public bool IsActive { get; set; } = true;
}
