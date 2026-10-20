using Microsoft.EntityFrameworkCore;
using RaceService.Application.Common;
using RaceService.Application.Database;
using RaceService.Application.Domain.Entities.Venues;
using RaceService.Application.Mapping;
using RaceService.Contracts.Common;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Application.Services;

/// <inheritdoc />
public sealed class TrackService : ITrackService
{
    private readonly ApplicationDbContext _context;
    private readonly ISlugService _slugs;
    private readonly ITimeZoneService _timeZones;

    /// <summary>Initialises the service.</summary>
    public TrackService(ApplicationDbContext context, ISlugService slugs, ITimeZoneService timeZones)
    {
        _context = context;
        _slugs = slugs;
        _timeZones = timeZones;
    }

    /// <inheritdoc />
    public async Task<PagedResult<TrackSummaryResponse>> SearchAsync(
        TrackQuery query,
        CancellationToken cancellationToken = default)
    {
        var tracks = _context.Tracks.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Country))
        {
            tracks = tracks.Where(track => track.Country == query.Country);
        }

        if (!string.IsNullOrWhiteSpace(query.CountryCode))
        {
            tracks = tracks.Where(track => track.CountryCode == query.CountryCode);
        }

        if (query.IsActive is not null)
        {
            tracks = tracks.Where(track => track.IsActive == query.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            tracks = tracks.Where(track =>
                EF.Functions.ILike(track.Name, term)
                || EF.Functions.ILike(track.Country, term)
                || (track.City != null && EF.Functions.ILike(track.City, term)));
        }

        var total = await tracks.LongCountAsync(cancellationToken);

        var page = await tracks
            .OrderBy(track => track.Country)
            .ThenBy(track => track.Name)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .Select(track => new TrackSummaryResponse
            {
                Id = track.Id,
                Name = track.Name,
                Slug = track.Slug,
                City = track.City,
                Country = track.Country,
                CountryCode = track.CountryCode,
                TimeZoneId = track.TimeZoneId,
                ImageUrl = track.ImageUrl,
                CurrentLayoutLengthMeters = track.Layouts
                    .Where(layout => layout.IsCurrent)
                    .Select(layout => (int?)layout.LengthMeters)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return PagedResult<TrackSummaryResponse>.Create(page, query.Page, query.PageSize, total);
    }

    /// <inheritdoc />
    public Task<Result<TrackDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => GetAsync(track => track.Id == id, $"id '{id}'", cancellationToken);

    /// <inheritdoc />
    public Task<Result<TrackDetailResponse>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => GetAsync(track => track.Slug == slug, $"slug '{slug}'", cancellationToken);

    /// <inheritdoc />
    public async Task<Result<TrackDetailResponse>> CreateAsync(
        CreateTrackRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_timeZones.IsValid(request.TimeZoneId))
        {
            return Result.Failure<TrackDetailResponse>(
                Error.Validation("Track.InvalidTimeZone", $"'{request.TimeZoneId}' is not a known IANA time zone."));
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? await _slugs.GenerateUniqueAsync(
                request.Name,
                candidate => _context.Tracks.AnyAsync(existing => existing.Slug == candidate, cancellationToken))
            : _slugs.Generate(request.Slug);

        if (await _context.Tracks.AnyAsync(existing => existing.Slug == slug, cancellationToken))
        {
            return Result.Failure<TrackDetailResponse>(
                Error.Conflict("Track.DuplicateSlug", $"A track with slug '{slug}' already exists."));
        }

        var track = new Track
        {
            Name = request.Name,
            OfficialName = request.OfficialName,
            Slug = slug,
            City = request.City,
            Country = request.Country,
            CountryCode = request.CountryCode,
            TimeZoneId = request.TimeZoneId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            OpenedYear = request.OpenedYear,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            WebsiteUrl = request.WebsiteUrl
        };

        // A venue with no layout cannot host an event, so creating one alongside is offered here
        // rather than left as a second call the caller can forget.
        if (request.InitialLayout is not null)
        {
            track.Layouts.Add(BuildLayout(track, request.InitialLayout));
        }

        _context.Tracks.Add(track);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(track.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<TrackDetailResponse>> UpdateAsync(
        Guid id,
        UpdateTrackRequest request,
        CancellationToken cancellationToken = default)
    {
        var track = await _context.Tracks
            .AsTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (track is null)
        {
            return Result.Failure<TrackDetailResponse>(Error.NotFound("Track.NotFound", $"No track with id '{id}'."));
        }

        if (!_timeZones.IsValid(request.TimeZoneId))
        {
            return Result.Failure<TrackDetailResponse>(
                Error.Validation("Track.InvalidTimeZone", $"'{request.TimeZoneId}' is not a known IANA time zone."));
        }

        track.Name = request.Name;
        track.OfficialName = request.OfficialName;
        track.City = request.City;
        track.Country = request.Country;
        track.CountryCode = request.CountryCode;
        track.TimeZoneId = request.TimeZoneId;
        track.Latitude = request.Latitude;
        track.Longitude = request.Longitude;
        track.OpenedYear = request.OpenedYear;
        track.Description = request.Description;
        track.ImageUrl = request.ImageUrl;
        track.WebsiteUrl = request.WebsiteUrl;
        track.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<TrackLayoutResponse>> AddLayoutAsync(
        Guid trackId,
        CreateTrackLayoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var track = await _context.Tracks
            .AsTracking()
            .Include(candidate => candidate.Layouts)
            .FirstOrDefaultAsync(candidate => candidate.Id == trackId, cancellationToken);

        if (track is null)
        {
            return Result.Failure<TrackLayoutResponse>(Error.NotFound("Track.NotFound", $"No track with id '{trackId}'."));
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug) ? _slugs.Generate(request.Name) : _slugs.Generate(request.Slug);

        if (track.Layouts.Any(layout => layout.Slug == slug))
        {
            return Result.Failure<TrackLayoutResponse>(
                Error.Conflict("TrackLayout.DuplicateSlug", $"That track already has a layout with slug '{slug}'."));
        }

        // Only one layout may be current, and the database enforces it with a partial unique
        // index. Demoting the incumbent here means the caller gets the obvious behaviour instead
        // of a constraint violation.
        if (request.IsCurrent)
        {
            foreach (var existing in track.Layouts.Where(layout => layout.IsCurrent))
            {
                existing.IsCurrent = false;
            }
        }

        var newLayout = BuildLayout(track, request with { Slug = slug });

        track.Layouts.Add(newLayout);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(newLayout.ToResponse());
    }

    /// <inheritdoc />
    public async Task<Result> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var track = await _context.Tracks
            .AsTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (track is null)
        {
            return Result.Failure(Error.NotFound("Track.NotFound", $"No track with id '{id}'."));
        }

        if (await _context.RaceEvents.AnyAsync(raceEvent => raceEvent.TrackLayout.TrackId == id, cancellationToken))
        {
            return Result.Failure(Error.Conflict(
                "Track.HasEvents",
                "A track that has hosted events cannot be archived. Set it inactive instead."));
        }

        _context.Tracks.Remove(track);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private TrackLayout BuildLayout(Track track, CreateTrackLayoutRequest request) => new()
    {
        Track = track,
        Name = request.Name,
        Slug = string.IsNullOrWhiteSpace(request.Slug) ? _slugs.Generate(request.Name) : request.Slug,
        LengthMeters = request.LengthMeters,
        NumberOfTurns = request.NumberOfTurns,
        Direction = request.Direction,
        TrackType = request.TrackType,
        ElevationChangeMeters = request.ElevationChangeMeters,
        LongestStraightMeters = request.LongestStraightMeters,
        MapImageUrl = request.MapImageUrl,
        IsCurrent = request.IsCurrent
    };

    private async Task<Result<TrackDetailResponse>> GetAsync(
        System.Linq.Expressions.Expression<Func<Track, bool>> predicate,
        string describedBy,
        CancellationToken cancellationToken)
    {
        var track = await _context.Tracks
            .Include(candidate => candidate.Layouts).ThenInclude(layout => layout.LapRecords).ThenInclude(record => record.Driver)
            .Include(candidate => candidate.Layouts).ThenInclude(layout => layout.LapRecords).ThenInclude(record => record.Series)
            .Include(candidate => candidate.Layouts).ThenInclude(layout => layout.LapRecords).ThenInclude(record => record.Team)
            .Include(candidate => candidate.Layouts).ThenInclude(layout => layout.LapRecords).ThenInclude(record => record.VehicleClass)
            .AsSplitQuery()
            .FirstOrDefaultAsync(predicate, cancellationToken);

        return track is null
            ? Result.Failure<TrackDetailResponse>(Error.NotFound("Track.NotFound", $"No track with {describedBy}."))
            : Result.Success(track.ToDetail());
    }
}
