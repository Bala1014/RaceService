using RaceService.Application.Domain.Common;

namespace RaceService.Application.Domain.Entities.Catalog;

public sealed class PointsSchemeEntry : Entity
{
    public Guid PointsSchemeId { get; set; }

    public PointsScheme PointsScheme { get; set; } = null!;

    public int Position { get; set; }

    public decimal Points { get; set; }
}
