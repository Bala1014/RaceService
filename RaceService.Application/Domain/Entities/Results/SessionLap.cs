using RaceService.Application.Domain.Common;
using RaceService.Application.Domain.Entities.Competitors;
using RaceService.Application.Domain.Entities.Events;

namespace RaceService.Application.Domain.Entities.Results;

public sealed class SessionLap : Entity
{
    public Guid SessionId { get; set; }

    public Session Session { get; set; } = null!;

    public Guid EventEntryId { get; set; }

    public EventEntry EventEntry { get; set; } = null!;

    public Guid? DriverId { get; set; }

    public Driver? Driver { get; set; }

    public int LapNumber { get; set; }

    public int? LapTimeMs { get; set; }

    public int? Sector1Ms { get; set; }

    public int? Sector2Ms { get; set; }

    public int? Sector3Ms { get; set; }

    public int? Position { get; set; }

    public decimal? SpeedTrapKph { get; set; }

    public string? TyreCompound { get; set; }

    public int? TyreAgeLaps { get; set; }

    public bool IsPitInLap { get; set; }

    public bool IsPitOutLap { get; set; }

    public bool IsPersonalBest { get; set; }

    public bool IsSessionBest { get; set; }

    public bool IsUnderCaution { get; set; }

    public bool IsDeletedByStewards { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
