using RaceService.Application.Domain.Common;
using RaceService.Application.Domain.Entities.Results;

namespace RaceService.Application.Domain.Entities.Competitors;

public sealed class Driver : AuditableEntity, ISoftDeletable
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? Abbreviation { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string? CountryCode { get; set; }

    public string? Nationality { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public DateOnly? DateOfDeath { get; set; }

    public string? PlaceOfBirth { get; set; }

    public int? PermanentNumber { get; set; }

    public string? ImageUrl { get; set; }

    public string? HeadshotUrl { get; set; }

    public string? Biography { get; set; }

    public string? WebsiteUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public Guid? DeletedByUserId { get; set; }

    public ICollection<DriverCareerStat> CareerStats { get; set; } = new List<DriverCareerStat>();

    public ICollection<EventEntryDriver> EventEntries { get; set; } = new List<EventEntryDriver>();

    public ICollection<SessionResultDriver> SessionResults { get; set; } = new List<SessionResultDriver>();
}
