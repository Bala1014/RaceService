using RaceService.Application.Domain.Common;
using RaceService.Contracts.Enums;

namespace RaceService.Application.Domain.Entities.Catalog;

public sealed class PointsScheme : AuditableEntity
{
    public Guid SeasonId { get; set; }

    public Season Season { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public SessionType AppliesToSessionType { get; set; }

    public ChampionshipType ChampionshipType { get; set; }

    public decimal FastestLapPoints { get; set; }

    public int? FastestLapMaxPosition { get; set; }

    public decimal PolePositionPoints { get; set; }

    public decimal MinimumDistancePercent { get; set; }

    public decimal PointsMultiplier { get; set; } = 1m;

    public decimal MinimumDriverDistancePercent { get; set; }

    public ICollection<PointsSchemeEntry> Entries { get; set; } = new List<PointsSchemeEntry>();
}
