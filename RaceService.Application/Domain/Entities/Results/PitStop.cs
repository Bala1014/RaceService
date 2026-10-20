using RaceService.Application.Domain.Common;
using RaceService.Application.Domain.Entities.Competitors;
using RaceService.Application.Domain.Entities.Events;

namespace RaceService.Application.Domain.Entities.Results;

public sealed class PitStop : Entity
{
    public Guid SessionId { get; set; }

    public Session Session { get; set; } = null!;

    public Guid EventEntryId { get; set; }

    public EventEntry EventEntry { get; set; } = null!;

    public int StopNumber { get; set; }

    public int LapNumber { get; set; }

    public int? StationaryTimeMs { get; set; }

    public int? TotalPitLaneTimeMs { get; set; }

    public string? TyreCompoundFitted { get; set; }

    public bool RefuelledStop { get; set; }

    public bool DriverChanged { get; set; }

    public Guid? DriverInId { get; set; }

    public Driver? DriverIn { get; set; }

    public Guid? DriverOutId { get; set; }

    public Driver? DriverOut { get; set; }

    public bool IsUnderCaution { get; set; }

    public bool IsPenaltyServed { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
