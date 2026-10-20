using RaceService.Application.Domain.Entities.Competitors;

namespace RaceService.Application.Domain.Entities.Results;

public sealed class SessionResultDriver
{
    public Guid SessionResultId { get; set; }

    public SessionResult SessionResult { get; set; } = null!;

    public Guid DriverId { get; set; }

    public Driver Driver { get; set; } = null!;

    public int LapsDriven { get; set; }

    public int? BestLapTimeMs { get; set; }

    public long? TimeDrivenMs { get; set; }

    public decimal PointsScored { get; set; }

    public bool IsEligibleForPoints { get; set; }

    public bool SetBestLap { get; set; }
}
