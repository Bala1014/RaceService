using Microsoft.EntityFrameworkCore;
using RaceService.Application.Abstractions;
using RaceService.Application.Common;
using RaceService.Application.Database;
using RaceService.Application.Domain.Entities.Competitors;
using RaceService.Application.Mapping;
using RaceService.Contracts.Common;
using RaceService.Contracts.Enums;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Application.Services;

/// <inheritdoc />
public sealed class DriverService : IDriverService
{
    private readonly ApplicationDbContext _context;
    private readonly ISlugService _slugs;
    private readonly IDateTimeProvider _clock;

    /// <summary>Initialises the service.</summary>
    public DriverService(ApplicationDbContext context, ISlugService slugs, IDateTimeProvider clock)
    {
        _context = context;
        _slugs = slugs;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task<PagedResult<DriverSummaryResponse>> SearchAsync(
        DriverQuery query,
        CancellationToken cancellationToken = default)
    {
        var drivers = _context.Drivers.AsQueryable();

        if (query.IsActive is not null)
        {
            drivers = drivers.Where(driver => driver.IsActive == query.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(query.CountryCode))
        {
            drivers = drivers.Where(driver => driver.CountryCode == query.CountryCode);
        }

        if (query.SeasonId is not null)
        {
            drivers = drivers.Where(driver => driver.EventEntries.Any(entry =>
                entry.EventEntry.Entry.SeasonId == query.SeasonId));
        }

        if (query.TeamId is not null)
        {
            drivers = drivers.Where(driver => driver.EventEntries.Any(entry =>
                entry.EventEntry.Entry.TeamId == query.TeamId));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            drivers = drivers.Where(driver =>
                EF.Functions.ILike(driver.FullName, term)
                || (driver.Abbreviation != null && EF.Functions.ILike(driver.Abbreviation, term)));
        }

        var total = await drivers.LongCountAsync(cancellationToken);

        var page = await drivers
            .OrderBy(driver => driver.LastName)
            .ThenBy(driver => driver.FirstName)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .Select(driver => new DriverSummaryResponse
            {
                Id = driver.Id,
                FullName = driver.FullName,
                Abbreviation = driver.Abbreviation,
                Slug = driver.Slug,
                CountryCode = driver.CountryCode,
                Nationality = driver.Nationality,
                PermanentNumber = driver.PermanentNumber,
                HeadshotUrl = driver.HeadshotUrl,
                IsActive = driver.IsActive
            })
            .ToListAsync(cancellationToken);

        return PagedResult<DriverSummaryResponse>.Create(page, query.Page, query.PageSize, total);
    }

    /// <inheritdoc />
    public Task<Result<DriverDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => GetAsync(driver => driver.Id == id, $"id '{id}'", cancellationToken);

    /// <inheritdoc />
    public Task<Result<DriverDetailResponse>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => GetAsync(driver => driver.Slug == slug, $"slug '{slug}'", cancellationToken);

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<SessionResultResponse>>> GetRecentResultsAsync(
        Guid driverId,
        int count,
        CancellationToken cancellationToken = default)
    {
        if (!await _context.Drivers.AnyAsync(driver => driver.Id == driverId, cancellationToken))
        {
            return Result.Failure<IReadOnlyList<SessionResultResponse>>(
                Error.NotFound("Driver.NotFound", $"No driver with id '{driverId}'."));
        }

        var results = await _context.SessionResults
            .Include(result => result.EventEntry).ThenInclude(entry => entry.Entry).ThenInclude(entry => entry.Team)
            .Include(result => result.EventEntry).ThenInclude(entry => entry.Entry).ThenInclude(entry => entry.VehicleClass)
            .Include(result => result.Drivers).ThenInclude(driver => driver.Driver)
            .Where(result => result.Drivers.Any(driver => driver.DriverId == driverId)
                && result.Session.Type == SessionType.Race)
            .OrderByDescending(result => result.Session.ScheduledStartUtc)
            .Take(Math.Clamp(count, 1, 100))
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<SessionResultResponse>>([.. results.Select(ResponseMappings.ToResponse)]);
    }

    /// <inheritdoc />
    public async Task<Result<DriverDetailResponse>> CreateAsync(
        CreateDriverRequest request,
        CancellationToken cancellationToken = default)
    {
        var fullName = string.IsNullOrWhiteSpace(request.FullName)
            ? $"{request.FirstName} {request.LastName}".Trim()
            : request.FullName;

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? await _slugs.GenerateUniqueAsync(
                fullName,
                candidate => _context.Drivers.AnyAsync(existing => existing.Slug == candidate, cancellationToken))
            : _slugs.Generate(request.Slug);

        if (await _context.Drivers.AnyAsync(existing => existing.Slug == slug, cancellationToken))
        {
            return Result.Failure<DriverDetailResponse>(
                Error.Conflict("Driver.DuplicateSlug", $"A driver with slug '{slug}' already exists."));
        }

        var driver = new Driver
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            FullName = fullName,
            Abbreviation = request.Abbreviation?.ToUpperInvariant(),
            Slug = slug,
            CountryCode = request.CountryCode?.ToUpperInvariant(),
            Nationality = request.Nationality,
            DateOfBirth = request.DateOfBirth,
            PlaceOfBirth = request.PlaceOfBirth,
            PermanentNumber = request.PermanentNumber,
            ImageUrl = request.ImageUrl,
            HeadshotUrl = request.HeadshotUrl,
            Biography = request.Biography,
            WebsiteUrl = request.WebsiteUrl
        };

        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(driver.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<DriverDetailResponse>> UpdateAsync(
        Guid id,
        UpdateDriverRequest request,
        CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers
            .AsTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (driver is null)
        {
            return Result.Failure<DriverDetailResponse>(Error.NotFound("Driver.NotFound", $"No driver with id '{id}'."));
        }

        if (request.DateOfBirth is not null
            && request.DateOfDeath is not null
            && request.DateOfDeath < request.DateOfBirth)
        {
            return Result.Failure<DriverDetailResponse>(
                Error.Validation("Driver.InvalidDates", "A date of death cannot precede a date of birth."));
        }

        driver.FirstName = request.FirstName;
        driver.LastName = request.LastName;
        driver.FullName = string.IsNullOrWhiteSpace(request.FullName)
            ? $"{request.FirstName} {request.LastName}".Trim()
            : request.FullName;
        driver.Abbreviation = request.Abbreviation?.ToUpperInvariant();
        driver.CountryCode = request.CountryCode?.ToUpperInvariant();
        driver.Nationality = request.Nationality;
        driver.DateOfBirth = request.DateOfBirth;
        driver.DateOfDeath = request.DateOfDeath;
        driver.PlaceOfBirth = request.PlaceOfBirth;
        driver.PermanentNumber = request.PermanentNumber;
        driver.ImageUrl = request.ImageUrl;
        driver.HeadshotUrl = request.HeadshotUrl;
        driver.Biography = request.Biography;
        driver.WebsiteUrl = request.WebsiteUrl;
        driver.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers
            .AsTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (driver is null)
        {
            return Result.Failure(Error.NotFound("Driver.NotFound", $"No driver with id '{id}'."));
        }

        if (await _context.SessionResultDrivers.AnyAsync(result => result.DriverId == id, cancellationToken))
        {
            return Result.Failure(Error.Conflict(
                "Driver.HasResults",
                "A driver with recorded results cannot be archived. Set them inactive instead."));
        }

        _context.Drivers.Remove(driver);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> RecalculateCareerStatsAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        if (!await _context.Drivers.AnyAsync(driver => driver.Id == driverId, cancellationToken))
        {
            return Result.Failure(Error.NotFound("Driver.NotFound", $"No driver with id '{driverId}'."));
        }

        // Only official classifications count toward a career total, for the same reason they are
        // the only ones that move a championship.
        var rows = await _context.SessionResultDrivers
            .Where(resultDriver => resultDriver.DriverId == driverId
                && resultDriver.SessionResult.Session.Type == SessionType.Race
                && (resultDriver.SessionResult.Session.ResultsStatus == ResultsStatus.Official
                    || resultDriver.SessionResult.Session.ResultsStatus == ResultsStatus.Amended))
            .Select(resultDriver => new
            {
                SeriesId = resultDriver.SessionResult.Session.RaceEvent.Season.SeriesId,
                Year = resultDriver.SessionResult.Session.RaceEvent.Season.Year,
                resultDriver.SessionResult.Position,
                resultDriver.SessionResult.LapsCompleted,
                resultDriver.SessionResult.LapsLed,
                resultDriver.SessionResult.IsFastestLap,
                resultDriver.SessionResult.IsPolePosition,
                resultDriver.SessionResult.Status,
                resultDriver.PointsScored
            })
            .ToListAsync(cancellationToken);

        var now = _clock.UtcNow;

        await _context.DriverCareerStats
            .Where(stat => stat.DriverId == driverId)
            .ExecuteDeleteAsync(cancellationToken);

        // One row per series, plus a null-series row holding the all-series career total.
        var groups = rows
            .GroupBy(row => (Guid?)row.SeriesId)
            .Concat([rows.GroupBy(_ => (Guid?)null).FirstOrDefault()])
            .Where(group => group is not null)
            .Select(group => group!);

        var stats = groups.Select(group => new DriverCareerStat
        {
            DriverId = driverId,
            SeriesId = group.Key,
            Starts = group.Count(),
            Wins = group.Count(row => row.Position == 1),
            Podiums = group.Count(row => row.Position <= 3),
            PolePositions = group.Count(row => row.IsPolePosition),
            FastestLaps = group.Count(row => row.IsFastestLap),
            Retirements = group.Count(row => row.Status == ResultStatus.Retired),
            LapsCompleted = group.Sum(row => row.LapsCompleted),
            LapsLed = group.Sum(row => row.LapsLed),
            CareerPoints = group.Sum(row => row.PointsScored),
            BestFinishPosition = group.Where(row => row.Position is not null).Select(row => row.Position).Min(),
            FirstSeasonYear = group.Min(row => row.Year),
            LastSeasonYear = group.Max(row => row.Year),
            RecalculatedAtUtc = now
        }).ToList();

        _context.DriverCareerStats.AddRange(stats);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result<DriverDetailResponse>> GetAsync(
        System.Linq.Expressions.Expression<Func<Driver, bool>> predicate,
        string describedBy,
        CancellationToken cancellationToken)
    {
        var driver = await _context.Drivers
            .Include(candidate => candidate.CareerStats).ThenInclude(stat => stat.Series)
            .FirstOrDefaultAsync(predicate, cancellationToken);

        return driver is null
            ? Result.Failure<DriverDetailResponse>(Error.NotFound("Driver.NotFound", $"No driver with {describedBy}."))
            : Result.Success(driver.ToDetail(_clock.UtcNow));
    }
}
