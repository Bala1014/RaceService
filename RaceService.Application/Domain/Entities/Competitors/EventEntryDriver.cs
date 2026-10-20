using RaceService.Contracts.Enums;

namespace RaceService.Application.Domain.Entities.Competitors;

public sealed class EventEntryDriver
{
    public Guid EventEntryId { get; set; }

    public EventEntry EventEntry { get; set; } = null!;

    public Guid DriverId { get; set; }

    public Driver Driver { get; set; } = null!;

    public int LineupPosition { get; set; } = 1;

    public DriverRole Role { get; set; }

    public DriverGrade Grade { get; set; }

    public bool IsStartingDriver { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
