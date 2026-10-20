using RaceService.Application.Domain.Common;
using RaceService.Application.Domain.Entities.Competitors;
using RaceService.Application.Domain.Entities.Standings;

namespace RaceService.Application.Domain.Entities.Catalog;

public sealed class VehicleClass : AuditableEntity
{
    public Guid? SeriesId { get; set; }

    public Series? Series { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ShortName { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ColorHex { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Entry> Entries { get; set; } = new List<Entry>();

    public ICollection<DriverStanding> DriverStandings { get; set; } = new List<DriverStanding>();

    public ICollection<TeamStanding> TeamStandings { get; set; } = new List<TeamStanding>();
}
