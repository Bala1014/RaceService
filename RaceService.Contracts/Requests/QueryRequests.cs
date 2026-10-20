using RaceService.Contracts.Common;
using RaceService.Contracts.Enums;

namespace RaceService.Contracts.Requests;

/// <summary>Filters the race event list.</summary>
public sealed record RaceEventQuery : TimeZoneAwareRequest
{
    public Guid? SeriesId { get; init; }

    /// <summary>Series slug, as an alternative to the id for public URLs.</summary>
    public string? SeriesSlug { get; init; }

    public Guid? SeasonId { get; init; }

    public int? Year { get; init; }

    public Guid? TrackId { get; init; }

    public EventStatus? Status { get; init; }

    /// <summary>Inclusive lower bound on the event's start, in UTC.</summary>
    public DateTime? FromUtc { get; init; }

    /// <summary>Exclusive upper bound on the event's start, in UTC.</summary>
    public DateTime? ToUtc { get; init; }

    public string? Country { get; init; }

    /// <summary>Free-text match against event and venue names.</summary>
    public string? Search { get; init; }

    /// <summary>
    /// Include unpublished drafts. Honoured only for callers holding the administrator role;
    /// ignored otherwise, so a public caller cannot page through unreleased calendars.
    /// </summary>
    public bool IncludeUnpublished { get; init; }
}

/// <summary>Filters the calendar view.</summary>
public sealed record CalendarQuery : TimeZoneAwareRequest
{
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
    public Guid? SeriesId { get; init; }
    public string? SeriesSlug { get; init; }
    public int? Year { get; init; }

    /// <summary>Restricts to a single month, used by the month-grid view.</summary>
    public int? Month { get; init; }
}

/// <summary>Filters the driver list.</summary>
public sealed record DriverQuery : PagedRequest
{
    public string? Search { get; init; }
    public string? CountryCode { get; init; }
    public bool? IsActive { get; init; }

    /// <summary>Restricts to drivers entered in this season.</summary>
    public Guid? SeasonId { get; init; }

    public Guid? TeamId { get; init; }
}

/// <summary>Filters the team list.</summary>
public sealed record TeamQuery : PagedRequest
{
    public string? Search { get; init; }
    public string? CountryCode { get; init; }
    public bool? IsActive { get; init; }
    public Guid? SeasonId { get; init; }
}

/// <summary>Filters the venue list.</summary>
public sealed record TrackQuery : PagedRequest
{
    public string? Search { get; init; }
    public string? Country { get; init; }
    public string? CountryCode { get; init; }
    public bool? IsActive { get; init; }
}

/// <summary>Filters the series list.</summary>
public sealed record SeriesQuery : PagedRequest
{
    public string? Search { get; init; }
    public Discipline? Discipline { get; init; }
    public bool? IsActive { get; init; }
}

/// <summary>Filters a season's entry list.</summary>
public sealed record EntryQuery : PagedRequest
{
    public Guid? TeamId { get; init; }
    public Guid? VehicleClassId { get; init; }
    public EntryStatus? Status { get; init; }
    public string? Search { get; init; }
}

/// <summary>Selects a standings table.</summary>
public sealed record StandingsQuery
{
    /// <summary>Restricts to one class in a multi-class championship.</summary>
    public Guid? VehicleClassId { get; init; }

    /// <summary>
    /// Returns the table as it stood after this round instead of the current one, which is what
    /// a title-race replay needs.
    /// </summary>
    public Guid? AfterEventId { get; init; }
}
