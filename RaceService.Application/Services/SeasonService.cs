using Microsoft.EntityFrameworkCore;
using RaceService.Application.Common;
using RaceService.Application.Database;
using RaceService.Application.Domain.Entities.Catalog;
using RaceService.Application.Mapping;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Application.Services;

/// <inheritdoc />
public sealed class SeasonService : ISeasonService
{
    private readonly ApplicationDbContext _context;
    private readonly ISlugService _slugs;
    private readonly IRaceEventService _events;

    /// <summary>Initialises the service.</summary>
    public SeasonService(ApplicationDbContext context, ISlugService slugs, IRaceEventService events)
    {
        _context = context;
        _slugs = slugs;
        _events = events;
    }

    /// <inheritdoc />
    public Task<Result<SeasonDetailResponse>> GetByIdAsync(
        Guid id,
        string? viewerTimeZone,
        CancellationToken cancellationToken = default)
        => GetAsync(season => season.Id == id, $"id '{id}'", viewerTimeZone, cancellationToken);

    /// <inheritdoc />
    public Task<Result<SeasonDetailResponse>> GetBySlugAsync(
        string seriesSlug,
        int year,
        string? viewerTimeZone,
        CancellationToken cancellationToken = default)
        => GetAsync(
            season => season.Series.Slug == seriesSlug && season.Year == year,
            $"series '{seriesSlug}' and year {year}",
            viewerTimeZone,
            cancellationToken);

    /// <inheritdoc />
    public async Task<Result<SeasonDetailResponse>> CreateAsync(
        CreateSeasonRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await _context.Series.AnyAsync(series => series.Id == request.SeriesId, cancellationToken))
        {
            return Result.Failure<SeasonDetailResponse>(
                Error.NotFound("Series.NotFound", $"No series with id '{request.SeriesId}'."));
        }

        if (request.StartDate is not null && request.EndDate is not null && request.StartDate > request.EndDate)
        {
            return Result.Failure<SeasonDetailResponse>(
                Error.Validation("Season.InvalidDateRange", "The season's start date must not be after its end date."));
        }

        if (await _context.Seasons.AnyAsync(
                season => season.SeriesId == request.SeriesId && season.Year == request.Year,
                cancellationToken))
        {
            return Result.Failure<SeasonDetailResponse>(
                Error.Conflict("Season.DuplicateYear", $"That series already has a {request.Year} season."));
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? request.Year.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : _slugs.Generate(request.Slug);

        if (await _context.Seasons.AnyAsync(
                season => season.SeriesId == request.SeriesId && season.Slug == slug,
                cancellationToken))
        {
            return Result.Failure<SeasonDetailResponse>(
                Error.Conflict("Season.DuplicateSlug", $"That series already has a season with slug '{slug}'."));
        }

        var newSeason = new Season
        {
            SeriesId = request.SeriesId,
            Year = request.Year,
            Name = request.Name,
            Slug = slug,
            Status = request.Status,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Description = request.Description,
            BannerImageUrl = request.BannerImageUrl,
            PlannedRoundCount = request.PlannedRoundCount,
            DroppedScoresCount = request.DroppedScoresCount
        };

        _context.Seasons.Add(newSeason);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(newSeason.Id, null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<SeasonDetailResponse>> UpdateAsync(
        Guid id,
        UpdateSeasonRequest request,
        CancellationToken cancellationToken = default)
    {
        var season = await _context.Seasons
            .AsTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (season is null)
        {
            return Result.Failure<SeasonDetailResponse>(Error.NotFound("Season.NotFound", $"No season with id '{id}'."));
        }

        if (request.StartDate is not null && request.EndDate is not null && request.StartDate > request.EndDate)
        {
            return Result.Failure<SeasonDetailResponse>(
                Error.Validation("Season.InvalidDateRange", "The season's start date must not be after its end date."));
        }

        season.Name = request.Name;
        season.Status = request.Status;
        season.StartDate = request.StartDate;
        season.EndDate = request.EndDate;
        season.Description = request.Description;
        season.BannerImageUrl = request.BannerImageUrl;
        season.PlannedRoundCount = request.PlannedRoundCount;
        season.DroppedScoresCount = request.DroppedScoresCount;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, null, cancellationToken);
    }

    private async Task<Result<SeasonDetailResponse>> GetAsync(
        System.Linq.Expressions.Expression<Func<Season, bool>> predicate,
        string describedBy,
        string? viewerTimeZone,
        CancellationToken cancellationToken)
    {
        var season = await _context.Seasons
            .Include(candidate => candidate.Series).ThenInclude(series => series.VehicleClasses)
            .Include(candidate => candidate.Events)
            .FirstOrDefaultAsync(predicate, cancellationToken);

        if (season is null)
        {
            return Result.Failure<SeasonDetailResponse>(
                Error.NotFound("Season.NotFound", $"No season with {describedBy}."));
        }

        // The calendar is fetched through the event service so it carries the same localisation
        // and projection as every other calendar view, rather than a second, divergent shape.
        var calendar = await _events.SearchAsync(
            new RaceEventQuery { SeasonId = season.Id, PageSize = 100, TimeZone = viewerTimeZone },
            isAdmin: false,
            cancellationToken);

        var entryCount = await _context.Entries.CountAsync(entry => entry.SeasonId == season.Id, cancellationToken);

        return Result.Success(new SeasonDetailResponse
        {
            Id = season.Id,
            SeriesId = season.SeriesId,
            SeriesName = season.Series.Name,
            SeriesSlug = season.Series.Slug,
            Year = season.Year,
            Name = season.Name,
            Slug = season.Slug,
            Status = season.Status,
            StartDate = season.StartDate,
            EndDate = season.EndDate,
            Description = season.Description,
            BannerImageUrl = season.BannerImageUrl,
            PlannedRoundCount = season.PlannedRoundCount,
            DroppedScoresCount = season.DroppedScoresCount,
            EntryCount = entryCount,
            Calendar = calendar.Items,
            VehicleClasses = [.. season.Series.VehicleClasses
                .OrderBy(vehicleClass => vehicleClass.DisplayOrder)
                .Select(ResponseMappings.ToResponse)]
        });
    }
}
