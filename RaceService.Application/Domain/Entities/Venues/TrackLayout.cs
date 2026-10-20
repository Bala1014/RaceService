using RaceService.Application.Domain.Common;
using RaceService.Application.Domain.Entities.Events;
using RaceService.Contracts.Enums;

namespace RaceService.Application.Domain.Entities.Venues;

public sealed class TrackLayout : AuditableEntity
{
    public Guid TrackId { get; set; }

    public Track Track { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public int LengthMeters { get; set; }

    public int? NumberOfTurns { get; set; }

    public TrackDirection Direction { get; set; }

    public TrackType TrackType { get; set; }

    public int? ElevationChangeMeters { get; set; }

    public int? LongestStraightMeters { get; set; }

    public string? MapImageUrl { get; set; }

    public bool IsCurrent { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<RaceEvent> Events { get; set; } = new List<RaceEvent>();

    public ICollection<LapRecord> LapRecords { get; set; } = new List<LapRecord>();
}
