using RaceService.Application.Domain.Common;

namespace RaceService.Application.Domain.Entities.Venues;

public sealed class Track : AuditableEntity, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;

    public string? OfficialName { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string? City { get; set; }

    public string Country { get; set; } = string.Empty;

    public string? CountryCode { get; set; }

    public string TimeZoneId { get; set; } = "UTC";

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public int? OpenedYear { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public string? WebsiteUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public Guid? DeletedByUserId { get; set; }

    public ICollection<TrackLayout> Layouts { get; set; } = new List<TrackLayout>();
}
