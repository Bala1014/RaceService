using RaceService.Application.Domain.Common;
using RaceService.Application.Domain.Entities.Catalog;

namespace RaceService.Application.Domain.Entities.Competitors;

public sealed class DriverCareerStat : Entity
{
    public Guid DriverId { get; set; }

    public Driver Driver { get; set; } = null!;

    public Guid? SeriesId { get; set; }

    public Series? Series { get; set; }

    public int Starts { get; set; }

    public int Wins { get; set; }

    public int Podiums { get; set; }

    public int PolePositions { get; set; }

    public int FastestLaps { get; set; }

    public int Retirements { get; set; }

    public int LapsCompleted { get; set; }

    public int LapsLed { get; set; }

    public decimal CareerPoints { get; set; }

    public int ChampionshipsWon { get; set; }

    public int? BestFinishPosition { get; set; }

    public int? FirstSeasonYear { get; set; }

    public int? LastSeasonYear { get; set; }

    public DateTime RecalculatedAtUtc { get; set; }
}
