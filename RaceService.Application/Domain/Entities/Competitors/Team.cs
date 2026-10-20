using RaceService.Application.Domain.Common;
using RaceService.Application.Domain.Entities.Standings;

namespace RaceService.Application.Domain.Entities.Competitors;

public sealed class Team : AuditableEntity, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;

    public string? OfficialName { get; set; }

    public string ShortName { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? CountryCode { get; set; }

    public string? BaseCity { get; set; }

    public string? BaseCountry { get; set; }

    public int? FoundedYear { get; set; }

    public string? LogoUrl { get; set; }

    public string? PrimaryColorHex { get; set; }

    public string? WebsiteUrl { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public Guid? DeletedByUserId { get; set; }

    public ICollection<Entry> Entries { get; set; } = new List<Entry>();

    public ICollection<TeamStanding> Standings { get; set; } = new List<TeamStanding>();
}
