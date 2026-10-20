using RaceService.Application.Domain.Common;
using RaceService.Application.Domain.Entities.Events;
using RaceService.Application.Domain.Entities.Results;
using RaceService.Contracts.Enums;

namespace RaceService.Application.Domain.Entities.Competitors;

public sealed class EventEntry : AuditableEntity
{
    public Guid RaceEventId { get; set; }

    public RaceEvent RaceEvent { get; set; } = null!;

    public Guid EntryId { get; set; }

    public Entry Entry { get; set; } = null!;

    public string? CarNumberOverride { get; set; }

    public EntryStatus Status { get; set; }

    public string? StatusNote { get; set; }

    public int? StartingGridPosition { get; set; }

    public bool StartsFromPitLane { get; set; }

    public bool IsEligibleForPoints { get; set; } = true;

    public uint Version { get; set; }

    public ICollection<EventEntryDriver> Drivers { get; set; } = new List<EventEntryDriver>();

    public ICollection<SessionResult> SessionResults { get; set; } = new List<SessionResult>();
}
