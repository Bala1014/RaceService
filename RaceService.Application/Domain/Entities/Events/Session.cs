using RaceService.Application.Domain.Common;
using RaceService.Application.Domain.Entities.Results;
using RaceService.Contracts.Enums;

namespace RaceService.Application.Domain.Entities.Events;

public sealed class Session : AuditableEntity
{
    public Guid RaceEventId { get; set; }

    public RaceEvent RaceEvent { get; set; } = null!;

    public SessionType Type { get; set; }

    public int SequenceNumber { get; set; } = 1;

    public string Name { get; set; } = string.Empty;

    public int WeekendOrder { get; set; }

    public DateTime ScheduledStartUtc { get; set; }

    public DateTime? ScheduledEndUtc { get; set; }

    public DateTime? ActualStartUtc { get; set; }

    public DateTime? ActualEndUtc { get; set; }

    public SessionStatus Status { get; set; }

    public int? ScheduledDurationMinutes { get; set; }

    public int? ScheduledLaps { get; set; }

    public int? CompletedLaps { get; set; }

    public bool IsPointsScoring { get; set; }

    public ResultsStatus ResultsStatus { get; set; }

    public DateTime? ResultsPublishedAtUtc { get; set; }

    public string? WeatherSummary { get; set; }

    public decimal? AirTemperatureC { get; set; }

    public decimal? TrackTemperatureC { get; set; }

    public decimal? HumidityPercent { get; set; }

    public int? RedFlagCount { get; set; }

    public int? SafetyCarCount { get; set; }

    public string? StatusNote { get; set; }

    public uint Version { get; set; }

    public ICollection<SessionResult> Results { get; set; } = new List<SessionResult>();

    public ICollection<SessionLap> Laps { get; set; } = new List<SessionLap>();

    public ICollection<PitStop> PitStops { get; set; } = new List<PitStop>();

    public ICollection<Penalty> Penalties { get; set; } = new List<Penalty>();
}
