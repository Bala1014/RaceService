using RaceService.Application.Domain.Common;
using RaceService.Application.Domain.Entities.Catalog;
using RaceService.Application.Domain.Entities.Competitors;
using RaceService.Application.Domain.Entities.Events;

namespace RaceService.Application.Domain.Entities.Standings;

public sealed class TeamStanding : Entity
{
    public Guid SeasonId { get; set; }

    public Season Season { get; set; } = null!;

    public Guid TeamId { get; set; }

    public Team Team { get; set; } = null!;

    public Guid? VehicleClassId { get; set; }

    public VehicleClass? VehicleClass { get; set; }

    public Guid? AfterEventId { get; set; }

    public RaceEvent? AfterEvent { get; set; }

    public int Position { get; set; }

    public decimal Points { get; set; }

    public decimal PointsBeforeDrops { get; set; }

    public int Starts { get; set; }

    public int Wins { get; set; }

    public int Podiums { get; set; }

    public int PolePositions { get; set; }

    public int FastestLaps { get; set; }

    public int? PreviousPosition { get; set; }

    public DateTime RecalculatedAtUtc { get; set; }
}
