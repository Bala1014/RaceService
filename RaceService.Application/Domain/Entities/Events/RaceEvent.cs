using RaceService.Application.Domain.Common;
using RaceService.Application.Domain.Entities.Catalog;
using RaceService.Application.Domain.Entities.Competitors;
using RaceService.Application.Domain.Entities.Venues;
using RaceService.Contracts.Enums;

namespace RaceService.Application.Domain.Entities.Events;

public sealed class RaceEvent : AuditableEntity, ISoftDeletable
{
    public Guid SeasonId { get; set; }

    public Season Season { get; set; } = null!;

    public Guid TrackLayoutId { get; set; }

    public TrackLayout TrackLayout { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string? OfficialName { get; set; }

    public string? ShortName { get; set; }

    public string Slug { get; set; } = string.Empty;

    public int? RoundNumber { get; set; }

    public DateTime StartTimeUtc { get; set; }

    public DateTime EndTimeUtc { get; set; }

    public string TimeZoneId { get; set; } = "UTC";

    public EventStatus Status { get; set; }

    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    public string? PosterImageUrl { get; set; }

    public string? BannerImageUrl { get; set; }

    public int? ScheduledDistanceMeters { get; set; }

    public int? ScheduledDurationMinutes { get; set; }

    public bool HasSprint { get; set; }

    public bool IsNonChampionship { get; set; }

    public Guid? PreviousEditionId { get; set; }

    public RaceEvent? PreviousEdition { get; set; }

    public bool IsPublished { get; set; }

    public DateTime? PublishedAtUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public Guid? DeletedByUserId { get; set; }

    public uint Version { get; set; }

    public ICollection<Session> Sessions { get; set; } = new List<Session>();

    public ICollection<EventEntry> Entries { get; set; } = new List<EventEntry>();
}
