using RaceService.Application.Domain.Common;
using RaceService.Application.Domain.Entities.Competitors;
using RaceService.Contracts.Enums;

namespace RaceService.Application.Domain.Entities.Catalog;

public sealed class Series : AuditableEntity, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;

    public string ShortName { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public Discipline Discipline { get; set; }

    public string? SanctioningBody { get; set; }

    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    public string? PrimaryColorHex { get; set; }

    public string? WebsiteUrl { get; set; }

    public int? InauguralYear { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public Guid? DeletedByUserId { get; set; }

    public ICollection<Season> Seasons { get; set; } = new List<Season>();

    public ICollection<VehicleClass> VehicleClasses { get; set; } = new List<VehicleClass>();

    public ICollection<DriverCareerStat> DriverCareerStats { get; set; } = new List<DriverCareerStat>();
}
