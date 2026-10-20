using Microsoft.EntityFrameworkCore;
using RaceService.Application.Abstractions;
using RaceService.Application.Common;
using RaceService.Application.Database;
using RaceService.Application.Domain.Entities.Events;
using RaceService.Application.Mapping;
using RaceService.Contracts.Common;
using RaceService.Contracts.Enums;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Application.Services;

/// <inheritdoc />
public sealed class RaceEventService : IRaceEventService
{
    private readonly ApplicationDbContext _context;
    private readonly ISlugService _slugs;
    private readonly ITimeZoneService _timeZones;
    private readonly IDateTimeProvider _clock;

    /// <summary>Initialises the service.</summary>
    public RaceEventService(
        ApplicationDbContext context,
        ISlugService slugs,
        ITimeZoneService timeZones,
        IDateTimeProvider clock)
    {
        _context = context;
        _slugs = slugs;
        _timeZones = timeZones;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task<PagedResult<RaceEventSummaryResponse>> SearchAsync(
        RaceEventQuery query,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var events = BuildQuery(query, isAdmin);

        var total = await events.LongCountAsync(cancellationToken);

        // Ordering is applied before paging and matches ix_race_event_published_start, so the
        // database can walk the index rather than sorting the whole result set.
        var ordered = query.SortDescending
            ? events.OrderByDescending(raceEvent => raceEvent.StartTimeUtc)
            : events.OrderBy(raceEvent => raceEvent.StartTimeUtc);

        var page = await ordered
            .Skip(query.Skip)
            .Take(query.PageSize)
            .Select(SummaryProjection())
            .ToListAsync(cancellationToken);

        var items = page.Select(row => Localize(row, query.TimeZone)).ToList();

        return PagedResult<RaceEventSummaryResponse>.Create(items, query.Page, query.PageSize, total);
    }

    /// <inheritdoc />
    public async Task<Result<RaceEventDetailResponse>> GetByIdAsync(
        Guid id,
        string? viewerTimeZone,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var raceEvent = await DetailQuery(isAdmin)
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        return raceEvent is null
            ? Result.Failure<RaceEventDetailResponse>(Error.NotFound("RaceEvent.NotFound", $"No race event with id '{id}'."))
            : Result.Success(await ToDetailAsync(raceEvent, viewerTimeZone, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<Result<RaceEventDetailResponse>> GetBySlugAsync(
        string seriesSlug,
        int year,
        string eventSlug,
        string? viewerTimeZone,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var raceEvent = await DetailQuery(isAdmin)
            .FirstOrDefaultAsync(
                candidate => candidate.Slug == eventSlug
                    && candidate.Season.Year == year
                    && candidate.Season.Series.Slug == seriesSlug,
                cancellationToken);

        return raceEvent is null
            ? Result.Failure<RaceEventDetailResponse>(
                Error.NotFound("RaceEvent.NotFound", $"No race event '{eventSlug}' in {seriesSlug} {year}."))
            : Result.Success(await ToDetailAsync(raceEvent, viewerTimeZone, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RaceEventSummaryResponse>> GetUpcomingAsync(
        int count,
        Guid? seriesId,
        string? viewerTimeZone,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;

        var events = _context.RaceEvents
            .Where(raceEvent => raceEvent.IsPublished && raceEvent.EndTimeUtc >= now)
            .Where(raceEvent => raceEvent.Status == EventStatus.Scheduled || raceEvent.Status == EventStatus.InProgress);

        if (seriesId is not null)
        {
            events = events.Where(raceEvent => raceEvent.Season.SeriesId == seriesId);
        }

        var page = await events
            .OrderBy(raceEvent => raceEvent.StartTimeUtc)
            .Take(Math.Clamp(count, 1, 50))
            .Select(SummaryProjection())
            .ToListAsync(cancellationToken);

        return [.. page.Select(row => Localize(row, viewerTimeZone))];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RaceEventSummaryResponse>> GetCalendarAsync(
        CalendarQuery query,
        CancellationToken cancellationToken = default)
    {
        var (from, to) = ResolveCalendarWindow(query);

        var events = _context.RaceEvents
            .Where(raceEvent => raceEvent.IsPublished)
            .Where(raceEvent => raceEvent.StartTimeUtc < to && raceEvent.EndTimeUtc >= from);

        if (query.SeriesId is not null)
        {
            events = events.Where(raceEvent => raceEvent.Season.SeriesId == query.SeriesId);
        }

        if (!string.IsNullOrWhiteSpace(query.SeriesSlug))
        {
            events = events.Where(raceEvent => raceEvent.Season.Series.Slug == query.SeriesSlug);
        }

        // The calendar is a bounded window, so it is returned whole rather than paged: a client
        // rendering a month grid needs every event in the month or the grid has holes.
        var page = await events
            .OrderBy(raceEvent => raceEvent.StartTimeUtc)
            .Take(500)
            .Select(SummaryProjection())
            .ToListAsync(cancellationToken);

        return [.. page.Select(row => Localize(row, query.TimeZone))];
    }

    /// <inheritdoc />
    public async Task<Result<RaceEventDetailResponse>> CreateAsync(
        CreateRaceEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var season = await _context.Seasons
            .Include(candidate => candidate.Series)
            .FirstOrDefaultAsync(candidate => candidate.Id == request.SeasonId, cancellationToken);

        if (season is null)
        {
            return Result.Failure<RaceEventDetailResponse>(
                Error.NotFound("Season.NotFound", $"No season with id '{request.SeasonId}'."));
        }

        var layout = await _context.TrackLayouts
            .Include(candidate => candidate.Track)
            .FirstOrDefaultAsync(candidate => candidate.Id == request.TrackLayoutId, cancellationToken);

        if (layout is null)
        {
            return Result.Failure<RaceEventDetailResponse>(
                Error.NotFound("TrackLayout.NotFound", $"No track layout with id '{request.TrackLayoutId}'."));
        }

        var timeZoneId = request.TimeZoneId ?? layout.Track.TimeZoneId;

        if (!_timeZones.IsValid(timeZoneId))
        {
            return Result.Failure<RaceEventDetailResponse>(
                Error.Validation("RaceEvent.InvalidTimeZone", $"'{timeZoneId}' is not a known IANA time zone."));
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? await _slugs.GenerateUniqueAsync(
                request.Name,
                candidate => _context.RaceEvents.AnyAsync(
                    existing => existing.SeasonId == season.Id && existing.Slug == candidate,
                    cancellationToken))
            : _slugs.Generate(request.Slug);

        if (await _context.RaceEvents.AnyAsync(
                existing => existing.SeasonId == season.Id && existing.Slug == slug,
                cancellationToken))
        {
            return Result.Failure<RaceEventDetailResponse>(
                Error.Conflict("RaceEvent.DuplicateSlug", $"Season already has an event with slug '{slug}'."));
        }

        if (request.RoundNumber is not null && await _context.RaceEvents.AnyAsync(
                existing => existing.SeasonId == season.Id && existing.RoundNumber == request.RoundNumber,
                cancellationToken))
        {
            return Result.Failure<RaceEventDetailResponse>(
                Error.Conflict("RaceEvent.DuplicateRound", $"Season already has a round {request.RoundNumber}."));
        }

        var raceEvent = new RaceEvent
        {
            SeasonId = season.Id,
            TrackLayoutId = layout.Id,
            Name = request.Name,
            OfficialName = request.OfficialName,
            ShortName = request.ShortName,
            Slug = slug,
            RoundNumber = request.RoundNumber,
            TimeZoneId = timeZoneId,
            Status = request.Status,
            ShortDescription = request.ShortDescription,
            Description = request.Description,
            PosterImageUrl = request.PosterImageUrl,
            BannerImageUrl = request.BannerImageUrl,
            ScheduledDistanceMeters = request.ScheduledDistanceMeters,
            ScheduledDurationMinutes = request.ScheduledDurationMinutes,
            HasSprint = request.HasSprint,
            IsNonChampionship = request.IsNonChampionship,
            IsPublished = request.IsPublished,
            PublishedAtUtc = request.IsPublished ? _clock.UtcNow : null
        };

        var sessionResult = AddSessions(raceEvent, request.Sessions);

        if (sessionResult.IsFailure)
        {
            return Result.Failure<RaceEventDetailResponse>(sessionResult.Error);
        }

        _context.RaceEvents.Add(raceEvent);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(raceEvent.Id, null, true, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<RaceEventDetailResponse>> UpdateAsync(
        Guid id,
        UpdateRaceEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var raceEvent = await _context.RaceEvents
            .AsTracking()
            .Include(candidate => candidate.Sessions)
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (raceEvent is null)
        {
            return Result.Failure<RaceEventDetailResponse>(
                Error.NotFound("RaceEvent.NotFound", $"No race event with id '{id}'."));
        }

        var layout = await _context.TrackLayouts
            .Include(candidate => candidate.Track)
            .FirstOrDefaultAsync(candidate => candidate.Id == request.TrackLayoutId, cancellationToken);

        if (layout is null)
        {
            return Result.Failure<RaceEventDetailResponse>(
                Error.NotFound("TrackLayout.NotFound", $"No track layout with id '{request.TrackLayoutId}'."));
        }

        var timeZoneId = request.TimeZoneId ?? layout.Track.TimeZoneId;

        if (!_timeZones.IsValid(timeZoneId))
        {
            return Result.Failure<RaceEventDetailResponse>(
                Error.Validation("RaceEvent.InvalidTimeZone", $"'{timeZoneId}' is not a known IANA time zone."));
        }

        if (request.RoundNumber is not null && await _context.RaceEvents.AnyAsync(
                existing => existing.SeasonId == raceEvent.SeasonId
                    && existing.RoundNumber == request.RoundNumber
                    && existing.Id != raceEvent.Id,
                cancellationToken))
        {
            return Result.Failure<RaceEventDetailResponse>(
                Error.Conflict("RaceEvent.DuplicateRound", $"Season already has a round {request.RoundNumber}."));
        }

        raceEvent.TrackLayoutId = layout.Id;
        raceEvent.Name = request.Name;
        raceEvent.OfficialName = request.OfficialName;
        raceEvent.ShortName = request.ShortName;
        raceEvent.RoundNumber = request.RoundNumber;
        raceEvent.TimeZoneId = timeZoneId;
        raceEvent.ShortDescription = request.ShortDescription;
        raceEvent.Description = request.Description;
        raceEvent.PosterImageUrl = request.PosterImageUrl;
        raceEvent.BannerImageUrl = request.BannerImageUrl;
        raceEvent.ScheduledDistanceMeters = request.ScheduledDistanceMeters;
        raceEvent.ScheduledDurationMinutes = request.ScheduledDurationMinutes;
        raceEvent.HasSprint = request.HasSprint;
        raceEvent.IsNonChampionship = request.IsNonChampionship;

        if (request.IsPublished && !raceEvent.IsPublished)
        {
            raceEvent.PublishedAtUtc = _clock.UtcNow;
        }

        raceEvent.IsPublished = request.IsPublished;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(raceEvent.Id, null, true, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result> UpdateStatusAsync(
        Guid id,
        UpdateEventStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var raceEvent = await _context.RaceEvents
            .AsTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (raceEvent is null)
        {
            return Result.Failure(Error.NotFound("RaceEvent.NotFound", $"No race event with id '{id}'."));
        }

        if (request.Status is EventStatus.Cancelled or EventStatus.Postponed && string.IsNullOrWhiteSpace(request.Reason))
        {
            return Result.Failure(
                Error.Validation("RaceEvent.ReasonRequired", "Cancelling or postponing an event requires a reason."));
        }

        raceEvent.Status = request.Status;
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var raceEvent = await _context.RaceEvents
            .AsTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (raceEvent is null)
        {
            return Result.Failure(Error.NotFound("RaceEvent.NotFound", $"No race event with id '{id}'."));
        }

        // Remove() on a soft-deletable entity is rewritten into a flag update by the auditing
        // interceptor, so this archives rather than destroys the weekend and its results.
        _context.RaceEvents.Remove(raceEvent);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private IQueryable<RaceEvent> BuildQuery(RaceEventQuery query, bool isAdmin)
    {
        var events = _context.RaceEvents.AsQueryable();

        // Unpublished drafts are visible only to administrators who explicitly ask. A caller
        // without the role cannot reach them by setting the flag.
        if (!isAdmin || !query.IncludeUnpublished)
        {
            events = events.Where(raceEvent => raceEvent.IsPublished);
        }

        if (query.SeasonId is not null)
        {
            events = events.Where(raceEvent => raceEvent.SeasonId == query.SeasonId);
        }

        if (query.SeriesId is not null)
        {
            events = events.Where(raceEvent => raceEvent.Season.SeriesId == query.SeriesId);
        }

        if (!string.IsNullOrWhiteSpace(query.SeriesSlug))
        {
            events = events.Where(raceEvent => raceEvent.Season.Series.Slug == query.SeriesSlug);
        }

        if (query.Year is not null)
        {
            events = events.Where(raceEvent => raceEvent.Season.Year == query.Year);
        }

        if (query.TrackId is not null)
        {
            events = events.Where(raceEvent => raceEvent.TrackLayout.TrackId == query.TrackId);
        }

        if (query.Status is not null)
        {
            events = events.Where(raceEvent => raceEvent.Status == query.Status);
        }

        if (query.FromUtc is not null)
        {
            events = events.Where(raceEvent => raceEvent.StartTimeUtc >= query.FromUtc);
        }

        if (query.ToUtc is not null)
        {
            events = events.Where(raceEvent => raceEvent.StartTimeUtc < query.ToUtc);
        }

        if (!string.IsNullOrWhiteSpace(query.Country))
        {
            events = events.Where(raceEvent => raceEvent.TrackLayout.Track.Country == query.Country);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";

            // EF.Functions.ILike maps to Postgres ILIKE, which is a case-insensitive match done
            // by the database. Doing this with ToLower().Contains() would work but would defeat
            // any index and force a sequential scan.
            events = events.Where(raceEvent =>
                EF.Functions.ILike(raceEvent.Name, term)
                || EF.Functions.ILike(raceEvent.TrackLayout.Track.Name, term));
        }

        return events;
    }

    private IQueryable<RaceEvent> DetailQuery(bool isAdmin)
    {
        // Neither Season.Events nor Track.Layouts is included here, even though the response
        // needs a round count and a layout length. Both paths loop straight back to the type they
        // started from, and EF rejects a cycle in a no-tracking query. The two figures are cheap
        // scalar counts instead, which is also less data off the wire than the collections were.
        var events = _context.RaceEvents
            .Include(raceEvent => raceEvent.Season).ThenInclude(season => season.Series)
            .Include(raceEvent => raceEvent.TrackLayout).ThenInclude(layout => layout.Track)
            .Include(raceEvent => raceEvent.TrackLayout).ThenInclude(layout => layout.LapRecords).ThenInclude(record => record.Driver)
            .Include(raceEvent => raceEvent.TrackLayout).ThenInclude(layout => layout.LapRecords).ThenInclude(record => record.Series)
            .Include(raceEvent => raceEvent.TrackLayout).ThenInclude(layout => layout.LapRecords).ThenInclude(record => record.Team)
            .Include(raceEvent => raceEvent.TrackLayout).ThenInclude(layout => layout.LapRecords).ThenInclude(record => record.VehicleClass)
            .Include(raceEvent => raceEvent.Sessions)
            .AsSplitQuery();

        return isAdmin ? events : events.Where(raceEvent => raceEvent.IsPublished);
    }

    /// <summary>
    /// Flat projection shared by every list endpoint. Written as a projection rather than an
    /// entity load so the database returns only the dozen columns a calendar tile shows, instead
    /// of hydrating whole event, season, series and track graphs per row.
    /// </summary>
    private static System.Linq.Expressions.Expression<Func<RaceEvent, RaceEventSummaryResponse>> SummaryProjection()
        => raceEvent => new RaceEventSummaryResponse
        {
            Id = raceEvent.Id,
            Name = raceEvent.Name,
            ShortName = raceEvent.ShortName,
            Slug = raceEvent.Slug,
            RoundNumber = raceEvent.RoundNumber,
            Status = raceEvent.Status,
            StartTime = new LocalizedTime { Utc = raceEvent.StartTimeUtc, VenueTimeZone = raceEvent.TimeZoneId },
            EndTime = new LocalizedTime { Utc = raceEvent.EndTimeUtc, VenueTimeZone = raceEvent.TimeZoneId },
            SeasonId = raceEvent.SeasonId,
            SeasonYear = raceEvent.Season.Year,
            SeriesId = raceEvent.Season.SeriesId,
            SeriesName = raceEvent.Season.Series.Name,
            SeriesShortName = raceEvent.Season.Series.ShortName,
            SeriesSlug = raceEvent.Season.Series.Slug,
            SeriesColorHex = raceEvent.Season.Series.PrimaryColorHex,
            TrackId = raceEvent.TrackLayout.TrackId,
            TrackName = raceEvent.TrackLayout.Track.Name,
            TrackSlug = raceEvent.TrackLayout.Track.Slug,
            TrackCountry = raceEvent.TrackLayout.Track.Country,
            TrackCountryCode = raceEvent.TrackLayout.Track.CountryCode,
            PosterImageUrl = raceEvent.PosterImageUrl,
            HasSprint = raceEvent.HasSprint,
            SessionCount = raceEvent.Sessions.Count,
            EntryCount = raceEvent.Entries.Count,
            HasResults = raceEvent.Sessions.Any(session => session.ResultsStatus != ResultsStatus.NotAvailable),
            FeatureSession = raceEvent.Sessions
                .Where(session => session.Type == SessionType.Race)
                .OrderBy(session => session.WeekendOrder)
                .Select(session => new SessionSummaryResponse
                {
                    Id = session.Id,
                    RaceEventId = session.RaceEventId,
                    Type = session.Type,
                    SequenceNumber = session.SequenceNumber,
                    Name = session.Name,
                    WeekendOrder = session.WeekendOrder,
                    ScheduledStart = new LocalizedTime { Utc = session.ScheduledStartUtc, VenueTimeZone = raceEvent.TimeZoneId },
                    ScheduledEndUtc = session.ScheduledEndUtc,
                    Status = session.Status,
                    ResultsStatus = session.ResultsStatus,
                    ScheduledDurationMinutes = session.ScheduledDurationMinutes,
                    ScheduledLaps = session.ScheduledLaps,
                    IsPointsScoring = session.IsPointsScoring,
                    HasResults = session.ResultsStatus != ResultsStatus.NotAvailable
                })
                .FirstOrDefault()
        };

    /// <summary>
    /// Fills in the local-time views. Done after the query rather than inside it because time
    /// zone conversion cannot be translated to SQL, and doing it per row in memory over one page
    /// is trivially cheap.
    /// </summary>
    private RaceEventSummaryResponse Localize(RaceEventSummaryResponse row, string? viewerTimeZone)
        => row with
        {
            StartTime = _timeZones.Localize(row.StartTime.Utc, row.StartTime.VenueTimeZone, viewerTimeZone),
            EndTime = _timeZones.Localize(row.EndTime.Utc, row.EndTime.VenueTimeZone, viewerTimeZone),
            FeatureSession = row.FeatureSession is null
                ? null
                : row.FeatureSession with
                {
                    ScheduledStart = _timeZones.Localize(
                        row.FeatureSession.ScheduledStart.Utc,
                        row.FeatureSession.ScheduledStart.VenueTimeZone,
                        viewerTimeZone)
                }
        };

    private async Task<RaceEventDetailResponse> ToDetailAsync(
        RaceEvent raceEvent,
        string? viewerTimeZone,
        CancellationToken cancellationToken)
    {
        var venueZone = raceEvent.TimeZoneId;

        var roundCount = await _context.RaceEvents
            .CountAsync(sibling => sibling.SeasonId == raceEvent.SeasonId, cancellationToken);

        var entryCount = await _context.EventEntries
            .CountAsync(entry => entry.RaceEventId == raceEvent.Id, cancellationToken);

        var track = raceEvent.TrackLayout.Track;

        return new RaceEventDetailResponse
        {
            Id = raceEvent.Id,
            Name = raceEvent.Name,
            OfficialName = raceEvent.OfficialName,
            ShortName = raceEvent.ShortName,
            Slug = raceEvent.Slug,
            RoundNumber = raceEvent.RoundNumber,
            Status = raceEvent.Status,
            StartTime = _timeZones.Localize(raceEvent.StartTimeUtc, venueZone, viewerTimeZone),
            EndTime = _timeZones.Localize(raceEvent.EndTimeUtc, venueZone, viewerTimeZone),
            TimeZoneId = venueZone,
            Season = raceEvent.Season.ToSummary() with { RoundCount = roundCount },
            TrackLayout = raceEvent.TrackLayout.ToResponse(),
            Track = new TrackSummaryResponse
            {
                Id = track.Id,
                Name = track.Name,
                Slug = track.Slug,
                City = track.City,
                Country = track.Country,
                CountryCode = track.CountryCode,
                TimeZoneId = track.TimeZoneId,
                ImageUrl = track.ImageUrl,
                CurrentLayoutLengthMeters = raceEvent.TrackLayout.IsCurrent ? raceEvent.TrackLayout.LengthMeters : null
            },
            ShortDescription = raceEvent.ShortDescription,
            Description = raceEvent.Description,
            PosterImageUrl = raceEvent.PosterImageUrl,
            BannerImageUrl = raceEvent.BannerImageUrl,
            ScheduledDistanceMeters = raceEvent.ScheduledDistanceMeters,
            ScheduledDurationMinutes = raceEvent.ScheduledDurationMinutes,
            HasSprint = raceEvent.HasSprint,
            IsNonChampionship = raceEvent.IsNonChampionship,
            IsPublished = raceEvent.IsPublished,
            PreviousEditionId = raceEvent.PreviousEditionId,
            EntryCount = entryCount,
            Sessions = [.. raceEvent.Sessions
                .OrderBy(session => session.WeekendOrder)
                .Select(session => new SessionSummaryResponse
                {
                    Id = session.Id,
                    RaceEventId = session.RaceEventId,
                    Type = session.Type,
                    SequenceNumber = session.SequenceNumber,
                    Name = session.Name,
                    WeekendOrder = session.WeekendOrder,
                    ScheduledStart = _timeZones.Localize(session.ScheduledStartUtc, venueZone, viewerTimeZone),
                    ScheduledEndUtc = session.ScheduledEndUtc,
                    Status = session.Status,
                    ResultsStatus = session.ResultsStatus,
                    ScheduledDurationMinutes = session.ScheduledDurationMinutes,
                    ScheduledLaps = session.ScheduledLaps,
                    IsPointsScoring = session.IsPointsScoring,
                    HasResults = session.ResultsStatus != ResultsStatus.NotAvailable
                })]
        };
    }

    /// <summary>
    /// Attaches sessions to a new event and derives the event's own window from them, so the
    /// denormalised start and end can never disagree with the schedule they summarise.
    /// </summary>
    private static Result AddSessions(RaceEvent raceEvent, IReadOnlyList<CreateSessionRequest> requests)
    {
        if (requests.Count == 0)
        {
            return Result.Success();
        }

        var duplicates = requests
            .GroupBy(request => (request.Type, request.SequenceNumber))
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key.Type} {group.Key.SequenceNumber}")
            .ToList();

        if (duplicates.Count > 0)
        {
            return Result.Failure(Error.Conflict(
                "Session.Duplicate",
                $"Duplicate session type and sequence: {string.Join(", ", duplicates)}."));
        }

        var order = 1;

        foreach (var request in requests.OrderBy(request => request.ScheduledStartUtc))
        {
            raceEvent.Sessions.Add(new Session
            {
                RaceEvent = raceEvent,
                Type = request.Type,
                SequenceNumber = request.SequenceNumber,
                Name = string.IsNullOrWhiteSpace(request.Name)
                    ? SessionNaming.Derive(request.Type, request.SequenceNumber)
                    : request.Name,
                WeekendOrder = order++,
                ScheduledStartUtc = DateTime.SpecifyKind(request.ScheduledStartUtc, DateTimeKind.Utc),
                ScheduledEndUtc = request.ScheduledEndUtc is null
                    ? null
                    : DateTime.SpecifyKind(request.ScheduledEndUtc.Value, DateTimeKind.Utc),
                ScheduledDurationMinutes = request.ScheduledDurationMinutes,
                ScheduledLaps = request.ScheduledLaps,
                IsPointsScoring = request.IsPointsScoring
                    ?? request.Type is SessionType.Race or SessionType.Sprint
            });
        }

        raceEvent.StartTimeUtc = raceEvent.Sessions.Min(session => session.ScheduledStartUtc);
        raceEvent.EndTimeUtc = raceEvent.Sessions.Max(session =>
            session.ScheduledEndUtc ?? session.ScheduledStartUtc.AddHours(2));

        return Result.Success();
    }

    private (DateTime From, DateTime To) ResolveCalendarWindow(CalendarQuery query)
    {
        if (query.FromUtc is not null && query.ToUtc is not null)
        {
            return (query.FromUtc.Value, query.ToUtc.Value);
        }

        var year = query.Year ?? _clock.UtcNow.Year;

        if (query.Month is >= 1 and <= 12)
        {
            var monthStart = new DateTime(year, query.Month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
            return (monthStart, monthStart.AddMonths(1));
        }

        var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (query.FromUtc ?? yearStart, query.ToUtc ?? yearStart.AddYears(1));
    }
}
