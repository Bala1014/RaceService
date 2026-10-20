using RaceService.Application.Domain.Common;
using RaceService.Application.Domain.Entities.Competitors;
using RaceService.Application.Domain.Entities.Events;
using RaceService.Contracts.Enums;

namespace RaceService.Application.Domain.Entities.Results;

public sealed class Penalty : AuditableEntity
{
    public Guid SessionId { get; set; }

    public Session Session { get; set; } = null!;

    public Guid EventEntryId { get; set; }

    public EventEntry EventEntry { get; set; } = null!;

    public Guid? DriverId { get; set; }

    public Driver? Driver { get; set; }

    public PenaltyType Type { get; set; }

    public int? TimePenaltySeconds { get; set; }

    public int? GridPositionDrop { get; set; }

    public int? LicencePoints { get; set; }

    public decimal? FineAmount { get; set; }

    public string? FineCurrency { get; set; }

    public string? Reason { get; set; }

    public int? LapNumber { get; set; }

    public DateTime? IssuedAtUtc { get; set; }

    public Guid? AppliedToSessionId { get; set; }

    public Session? AppliedToSession { get; set; }

    public bool IsServed { get; set; }

    public bool IsRescinded { get; set; }
}
