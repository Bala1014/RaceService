using RaceService.Application.Domain.Common;
using RaceService.Application.Domain.Entities.Catalog;
using RaceService.Contracts.Enums;

namespace RaceService.Application.Domain.Entities.Competitors;

public sealed class Entry : AuditableEntity, ISoftDeletable
{
    public Guid SeasonId { get; set; }

    public Season Season { get; set; } = null!;

    public Guid TeamId { get; set; }

    public Team Team { get; set; } = null!;

    public string CarNumber { get; set; } = string.Empty;

    public Guid? VehicleClassId { get; set; }

    public VehicleClass? VehicleClass { get; set; }

    public string EntrantName { get; set; } = string.Empty;

    public Guid? ChassisManufacturerId { get; set; }

    public Manufacturer? ChassisManufacturer { get; set; }

    public string? ChassisModel { get; set; }

    public Guid? EngineManufacturerId { get; set; }

    public Manufacturer? EngineManufacturer { get; set; }

    public string? EngineModel { get; set; }

    public Guid? TyreSupplierId { get; set; }

    public Manufacturer? TyreSupplier { get; set; }

    public EntryStatus Status { get; set; }

    public string? LiveryImageUrl { get; set; }

    public string? PrimaryColorHex { get; set; }

    public bool IsEligibleForPoints { get; set; } = true;

    public Guid? SubmittedByUserId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public Guid? DeletedByUserId { get; set; }

    public uint Version { get; set; }

    public ICollection<EventEntry> EventEntries { get; set; } = new List<EventEntry>();
}
