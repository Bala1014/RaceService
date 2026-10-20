using RaceService.Application.Domain.Common;
using RaceService.Application.Domain.Entities.Competitors;
using RaceService.Application.Domain.Entities.Events;
using RaceService.Contracts.Enums;

namespace RaceService.Application.Domain.Entities.Results;

public sealed class SessionResult : AuditableEntity
{
    public Guid SessionId { get; set; }

    public Session Session { get; set; } = null!;

    public Guid EventEntryId { get; set; }

    public EventEntry EventEntry { get; set; } = null!;

    public int? Position { get; set; }

    public int? ClassPosition { get; set; }

    public int? StartingPosition { get; set; }

    public int LapsCompleted { get; set; }

    public int LapsLed { get; set; }

    public long? TotalTimeMs { get; set; }

    public long? GapToLeaderMs { get; set; }

    public int? GapToLeaderLaps { get; set; }

    public long? GapToAheadMs { get; set; }

    public int? BestLapTimeMs { get; set; }

    public int? BestLapNumber { get; set; }

    public decimal? AverageSpeedKph { get; set; }

    public decimal? TopSpeedKph { get; set; }

    public ResultStatus Status { get; set; }

    public string? StatusDetail { get; set; }

    public bool IsClassified { get; set; }

    public decimal PointsScored { get; set; }

    public bool IsFastestLap { get; set; }

    public bool IsPolePosition { get; set; }

    public int PitStopCount { get; set; }

    public int PenaltyTimeMs { get; set; }

    public uint Version { get; set; }

    public ICollection<SessionResultDriver> Drivers { get; set; } = new List<SessionResultDriver>();
}
