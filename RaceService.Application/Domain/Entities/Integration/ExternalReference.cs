using RaceService.Application.Domain.Common;

namespace RaceService.Application.Domain.Entities.Integration;

public sealed class ExternalReference : Entity
{
    public string Provider { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;

    public Guid InternalId { get; set; }

    public string ExternalId { get; set; } = string.Empty;

    public string? PayloadHash { get; set; }

    public DateTime LastSyncedAtUtc { get; set; }

    public string? LastPayloadJson { get; set; }
}
