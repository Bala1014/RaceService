using Microsoft.EntityFrameworkCore;
using RaceService.Application.Common;
using RaceService.Application.Database;
using RaceService.Application.Domain.Entities.Catalog;
using RaceService.Application.Mapping;
using RaceService.Contracts.Common;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Application.Services;

/// <inheritdoc />
public sealed class SeriesService : ISeriesService
{
    private readonly ApplicationDbContext _context;
    private readonly ISlugService _slugs;

    /// <summary>Initialises the service.</summary>
    public SeriesService(ApplicationDbContext context, ISlugService slugs)
    {
        _context = context;
        _slugs = slugs;
    }

    /// <inheritdoc />
    public async Task<PagedResult<SeriesSummaryResponse>> SearchAsync(
        SeriesQuery query,
        CancellationToken cancellationToken = default)
    {
        var series = _context.Series.AsQueryable();

        if (query.Discipline is not null)
        {
            series = series.Where(candidate => candidate.Discipline == query.Discipline);
        }

        if (query.IsActive is not null)
        {
            series = series.Where(candidate => candidate.IsActive == query.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            series = series.Where(candidate =>
                EF.Functions.ILike(candidate.Name, term) || EF.Functions.ILike(candidate.ShortName, term));
        }

        var total = await series.LongCountAsync(cancellationToken);

        var page = await series
            .OrderBy(candidate => candidate.DisplayOrder)
            .ThenBy(candidate => candidate.Name)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .Select(candidate => new SeriesSummaryResponse
            {
                Id = candidate.Id,
                Name = candidate.Name,
                ShortName = candidate.ShortName,
                Slug = candidate.Slug,
                Discipline = candidate.Discipline,
                SanctioningBody = candidate.SanctioningBody,
                LogoUrl = candidate.LogoUrl,
                PrimaryColorHex = candidate.PrimaryColorHex,
                IsActive = candidate.IsActive,
                LatestSeasonYear = candidate.Seasons
                    .OrderByDescending(season => season.Year)
                    .Select(season => (int?)season.Year)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return PagedResult<SeriesSummaryResponse>.Create(page, query.Page, query.PageSize, total);
    }

    /// <inheritdoc />
    public Task<Result<SeriesDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => GetAsync(series => series.Id == id, $"id '{id}'", cancellationToken);

    /// <inheritdoc />
    public Task<Result<SeriesDetailResponse>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => GetAsync(series => series.Slug == slug, $"slug '{slug}'", cancellationToken);

    /// <inheritdoc />
    public async Task<Result<SeriesDetailResponse>> CreateAsync(
        CreateSeriesRequest request,
        CancellationToken cancellationToken = default)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? await _slugs.GenerateUniqueAsync(
                request.Name,
                candidate => _context.Series.AnyAsync(existing => existing.Slug == candidate, cancellationToken))
            : _slugs.Generate(request.Slug);

        if (await _context.Series.AnyAsync(existing => existing.Slug == slug, cancellationToken))
        {
            return Result.Failure<SeriesDetailResponse>(
                Error.Conflict("Series.DuplicateSlug", $"A series with slug '{slug}' already exists."));
        }

        var series = new Series
        {
            Name = request.Name,
            ShortName = request.ShortName,
            Slug = slug,
            Discipline = request.Discipline,
            SanctioningBody = request.SanctioningBody,
            Description = request.Description,
            LogoUrl = request.LogoUrl,
            PrimaryColorHex = request.PrimaryColorHex,
            WebsiteUrl = request.WebsiteUrl,
            InauguralYear = request.InauguralYear,
            DisplayOrder = request.DisplayOrder
        };

        _context.Series.Add(series);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(series.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<SeriesDetailResponse>> UpdateAsync(
        Guid id,
        UpdateSeriesRequest request,
        CancellationToken cancellationToken = default)
    {
        var series = await _context.Series
            .AsTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (series is null)
        {
            return Result.Failure<SeriesDetailResponse>(Error.NotFound("Series.NotFound", $"No series with id '{id}'."));
        }

        series.Name = request.Name;
        series.ShortName = request.ShortName;
        series.Discipline = request.Discipline;
        series.SanctioningBody = request.SanctioningBody;
        series.Description = request.Description;
        series.LogoUrl = request.LogoUrl;
        series.PrimaryColorHex = request.PrimaryColorHex;
        series.WebsiteUrl = request.WebsiteUrl;
        series.InauguralYear = request.InauguralYear;
        series.DisplayOrder = request.DisplayOrder;
        series.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var series = await _context.Series
            .AsTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (series is null)
        {
            return Result.Failure(Error.NotFound("Series.NotFound", $"No series with id '{id}'."));
        }

        // A series with seasons is never removed, even softly: doing so would hide a decade of
        // race history behind a global query filter. Deactivating is the honest operation.
        if (await _context.Seasons.AnyAsync(season => season.SeriesId == id, cancellationToken))
        {
            return Result.Failure(Error.Conflict(
                "Series.HasSeasons",
                "A series with seasons cannot be archived. Set it inactive instead."));
        }

        _context.Series.Remove(series);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result<SeriesDetailResponse>> GetAsync(
        System.Linq.Expressions.Expression<Func<Series, bool>> predicate,
        string describedBy,
        CancellationToken cancellationToken)
    {
        var series = await _context.Series
            .Include(candidate => candidate.Seasons).ThenInclude(season => season.Events)
            .Include(candidate => candidate.VehicleClasses)
            .AsSplitQuery()
            .FirstOrDefaultAsync(predicate, cancellationToken);

        if (series is null)
        {
            return Result.Failure<SeriesDetailResponse>(Error.NotFound("Series.NotFound", $"No series with {describedBy}."));
        }

        return Result.Success(new SeriesDetailResponse
        {
            Id = series.Id,
            Name = series.Name,
            ShortName = series.ShortName,
            Slug = series.Slug,
            Discipline = series.Discipline,
            SanctioningBody = series.SanctioningBody,
            Description = series.Description,
            LogoUrl = series.LogoUrl,
            PrimaryColorHex = series.PrimaryColorHex,
            WebsiteUrl = series.WebsiteUrl,
            InauguralYear = series.InauguralYear,
            IsActive = series.IsActive,
            Seasons = [.. series.Seasons
                .OrderByDescending(season => season.Year)
                .Select(season =>
                {
                    season.Series = series;
                    return season.ToSummary();
                })],
            VehicleClasses = [.. series.VehicleClasses
                .OrderBy(vehicleClass => vehicleClass.DisplayOrder)
                .Select(ResponseMappings.ToResponse)]
        });
    }
}
