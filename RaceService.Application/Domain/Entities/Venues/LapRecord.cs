using RaceService.Application.Domain.Common;
using RaceService.Application.Domain.Entities.Catalog;
using RaceService.Application.Domain.Entities.Competitors;
using RaceService.Application.Domain.Entities.Events;
using RaceService.Contracts.Enums;

namespace RaceService.Application.Domain.Entities.Venues;

public sealed class LapRecord : AuditableEntity
{
    public Guid TrackLayoutId { get; set; }

    public TrackLayout TrackLayout { get; set; } = null!;

    public Guid? SeriesId { get; set; }

    public Series? Series { get; set; }

    public Guid? VehicleClassId { get; set; }

    public VehicleClass? VehicleClass { get; set; }

    public RecordType RecordType { get; set; }

    public int LapTimeMs { get; set; }

    public Guid? DriverId { get; set; }

    public Driver? Driver { get; set; }

    public Guid? TeamId { get; set; }

    public Team? Team { get; set; }

    public Guid? SessionId { get; set; }

    public Session? Session { get; set; }

    public DateOnly SetOn { get; set; }

    public string? VehicleDescription { get; set; }

    public bool IsCurrent { get; set; }
}
