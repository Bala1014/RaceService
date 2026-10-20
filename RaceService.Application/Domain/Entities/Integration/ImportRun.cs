using RaceService.Application.Domain.Common;
using RaceService.Contracts.Enums;

namespace RaceService.Application.Domain.Entities.Integration;

public sealed class ImportRun : Entity
{
    public string Provider { get; set; } = string.Empty;

    public string Resource { get; set; } = string.Empty;

    public ImportStatus Status { get; set; }

    public DateTime StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public int RecordsRead { get; set; }

    public int RecordsCreated { get; set; }

    public int RecordsUpdated { get; set; }

    public int RecordsSkipped { get; set; }

    public int RecordsFailed { get; set; }

    public string? ErrorMessage { get; set; }

    public string? CorrelationId { get; set; }

    public Guid? TriggeredByUserId { get; set; }
}
