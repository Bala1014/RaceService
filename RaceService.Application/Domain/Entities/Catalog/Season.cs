using RaceService.Application.Domain.Common;
using RaceService.Application.Domain.Entities.Competitors;
using RaceService.Application.Domain.Entities.Events;
using RaceService.Application.Domain.Entities.Standings;
using RaceService.Contracts.Enums;

namespace RaceService.Application.Domain.Entities.Catalog;

public sealed class Season : AuditableEntity, ISoftDeletable
{
    public Guid SeriesId { get; set; }

    public Series Series { get; set; } = null!;

    public int Year { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public SeasonStatus Status { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Description { get; set; }

    public string? BannerImageUrl { get; set; }

    public int? PlannedRoundCount { get; set; }

    public int DroppedScoresCount { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public Guid? DeletedByUserId { get; set; }

    public ICollection<RaceEvent> Events { get; set; } = new List<RaceEvent>();

    public ICollection<Entry> Entries { get; set; } = new List<Entry>();

    public ICollection<PointsScheme> PointsSchemes { get; set; } = new List<PointsScheme>();

    public ICollection<DriverStanding> DriverStandings { get; set; } = new List<DriverStanding>();

    public ICollection<TeamStanding> TeamStandings { get; set; } = new List<TeamStanding>();
}
