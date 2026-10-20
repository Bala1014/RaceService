using Microsoft.EntityFrameworkCore;
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
public sealed class EntryService : IEntryService
{
    private readonly ApplicationDbContext _context;

    /// <summary>Initialises the service.</summary>
    public EntryService(ApplicationDbContext context) => _context = context;

    /// <inheritdoc />
    public async Task<Result<PagedResult<EntryResponse>>> GetSeasonEntriesAsync(
        Guid seasonId,
        EntryQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!await _context.Seasons.AnyAsync(season => season.Id == seasonId, cancellationToken))
        {
            return Result.Failure<PagedResult<EntryResponse>>(
                Error.NotFound("Season.NotFound", $"No season with id '{seasonId}'."));
        }

        var entries = _context.Entries
            .Include(entry => entry.Season).ThenInclude(season => season.Series)
            .Include(entry => entry.Team)
            .Include(entry => entry.VehicleClass)
            .Include(entry => entry.ChassisManufacturer)
            .Include(entry => entry.EngineManufacturer)
            .Include(entry => entry.TyreSupplier)
            .Where(entry => entry.SeasonId == seasonId);

        if (query.TeamId is not null)
        {
            entries = entries.Where(entry => entry.TeamId == query.TeamId);
        }

        if (query.VehicleClassId is not null)
        {
            entries = entries.Where(entry => entry.VehicleClassId == query.VehicleClassId);
        }

        if (query.Status is not null)
        {
            entries = entries.Where(entry => entry.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            entries = entries.Where(entry =>
                EF.Functions.ILike(entry.EntrantName, term)
                || EF.Functions.ILike(entry.CarNumber, term)
                || EF.Functions.ILike(entry.Team.Name, term));
        }

        var total = await entries.LongCountAsync(cancellationToken);

        var page = await entries
            // Grouped by class then number, which is the order a published entry list uses.
            .OrderBy(entry => entry.VehicleClass!.DisplayOrder)
            .ThenBy(entry => entry.CarNumber.Length)
            .ThenBy(entry => entry.CarNumber)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return Result.Success(PagedResult<EntryResponse>.Create(
            [.. page.Select(ResponseMappings.ToResponse)],
            query.Page,
            query.PageSize,
            total));
    }

    /// <inheritdoc />
    public async Task<Result<EntryResponse>> CreateSeasonEntryAsync(
        Guid seasonId,
        CreateEntryRequest request,
        Guid? submittedByUserId,
        CancellationToken cancellationToken = default)
    {
        var season = await _context.Seasons
            .FirstOrDefaultAsync(candidate => candidate.Id == seasonId, cancellationToken);

        if (season is null)
        {
            return Result.Failure<EntryResponse>(Error.NotFound("Season.NotFound", $"No season with id '{seasonId}'."));
        }

        if (season.Status is SeasonStatus.Completed or SeasonStatus.Cancelled)
        {
            return Result.Failure<EntryResponse>(Error.Unprocessable(
                "Entry.SeasonClosed",
                "Entries cannot be added to a completed or cancelled season."));
        }

        if (!await _context.Teams.AnyAsync(team => team.Id == request.TeamId, cancellationToken))
        {
            return Result.Failure<EntryResponse>(Error.NotFound("Team.NotFound", $"No team with id '{request.TeamId}'."));
        }

        if (await _context.Entries.AnyAsync(
                entry => entry.SeasonId == seasonId
                    && entry.CarNumber == request.CarNumber
                    && entry.Status != EntryStatus.Withdrawn,
                cancellationToken))
        {
            return Result.Failure<EntryResponse>(Error.Conflict(
                "Entry.DuplicateCarNumber",
                $"Car number '{request.CarNumber}' is already taken in this season."));
        }

        var entry = new Entry
        {
            SeasonId = seasonId,
            TeamId = request.TeamId,
            CarNumber = request.CarNumber,
            EntrantName = request.EntrantName,
            VehicleClassId = request.VehicleClassId,
            ChassisManufacturerId = request.ChassisManufacturerId,
            ChassisModel = request.ChassisModel,
            EngineManufacturerId = request.EngineManufacturerId,
            EngineModel = request.EngineModel,
            TyreSupplierId = request.TyreSupplierId,
            Status = request.Status,
            LiveryImageUrl = request.LiveryImageUrl,
            PrimaryColorHex = request.PrimaryColorHex,
            IsEligibleForPoints = request.IsEligibleForPoints,
            SubmittedByUserId = submittedByUserId
        };

        _context.Entries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);

        return await LoadEntryAsync(entry.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<EntryResponse>> UpdateSeasonEntryAsync(
        Guid entryId,
        UpdateEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        var entry = await _context.Entries
            .AsTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == entryId, cancellationToken);

        if (entry is null)
        {
            return Result.Failure<EntryResponse>(Error.NotFound("Entry.NotFound", $"No entry with id '{entryId}'."));
        }

        if (entry.CarNumber != request.CarNumber && await _context.Entries.AnyAsync(
                other => other.SeasonId == entry.SeasonId
                    && other.CarNumber == request.CarNumber
                    && other.Id != entryId
                    && other.Status != EntryStatus.Withdrawn,
                cancellationToken))
        {
            return Result.Failure<EntryResponse>(Error.Conflict(
                "Entry.DuplicateCarNumber",
                $"Car number '{request.CarNumber}' is already taken in this season."));
        }

        entry.TeamId = request.TeamId;
        entry.CarNumber = request.CarNumber;
        entry.EntrantName = request.EntrantName;
        entry.VehicleClassId = request.VehicleClassId;
        entry.ChassisManufacturerId = request.ChassisManufacturerId;
        entry.ChassisModel = request.ChassisModel;
        entry.EngineManufacturerId = request.EngineManufacturerId;
        entry.EngineModel = request.EngineModel;
        entry.TyreSupplierId = request.TyreSupplierId;
        entry.LiveryImageUrl = request.LiveryImageUrl;
        entry.PrimaryColorHex = request.PrimaryColorHex;
        entry.IsEligibleForPoints = request.IsEligibleForPoints;

        await _context.SaveChangesAsync(cancellationToken);

        return await LoadEntryAsync(entryId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result> UpdateSeasonEntryStatusAsync(
        Guid entryId,
        UpdateEntryStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var entry = await _context.Entries
            .AsTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == entryId, cancellationToken);

        if (entry is null)
        {
            return Result.Failure(Error.NotFound("Entry.NotFound", $"No entry with id '{entryId}'."));
        }

        entry.Status = request.Status;
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<EventEntryResponse>>> GetEventEntriesAsync(
        Guid raceEventId,
        CancellationToken cancellationToken = default)
    {
        if (!await _context.RaceEvents.AnyAsync(raceEvent => raceEvent.Id == raceEventId, cancellationToken))
        {
            return Result.Failure<IReadOnlyList<EventEntryResponse>>(
                Error.NotFound("RaceEvent.NotFound", $"No race event with id '{raceEventId}'."));
        }

        var entries = await EventEntryQuery()
            .Where(eventEntry => eventEntry.RaceEventId == raceEventId)
            .ToListAsync(cancellationToken);

        var ordered = entries
            .OrderBy(eventEntry => eventEntry.Entry.VehicleClass?.DisplayOrder ?? 0)
            .ThenBy(eventEntry => eventEntry.StartingGridPosition ?? int.MaxValue)
            .ThenBy(eventEntry => eventEntry.CarNumberOverride ?? eventEntry.Entry.CarNumber)
            .Select(ResponseMappings.ToResponse)
            .ToList();

        return Result.Success<IReadOnlyList<EventEntryResponse>>(ordered);
    }

    /// <inheritdoc />
    public async Task<Result<EventEntryResponse>> AddEventEntryAsync(
        Guid raceEventId,
        CreateEventEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        var raceEvent = await _context.RaceEvents
            .FirstOrDefaultAsync(candidate => candidate.Id == raceEventId, cancellationToken);

        if (raceEvent is null)
        {
            return Result.Failure<EventEntryResponse>(
                Error.NotFound("RaceEvent.NotFound", $"No race event with id '{raceEventId}'."));
        }

        if (raceEvent.Status is EventStatus.Cancelled or EventStatus.Archived)
        {
            return Result.Failure<EventEntryResponse>(Error.Unprocessable(
                "EventEntry.EventClosed",
                "Entries cannot be added to a cancelled or archived event."));
        }

        var entry = await _context.Entries
            .FirstOrDefaultAsync(candidate => candidate.Id == request.EntryId, cancellationToken);

        if (entry is null)
        {
            return Result.Failure<EventEntryResponse>(
                Error.NotFound("Entry.NotFound", $"No season entry with id '{request.EntryId}'."));
        }

        // The entry and the event must belong to the same season, or a car from one championship
        // would appear on another's entry list.
        if (entry.SeasonId != raceEvent.SeasonId)
        {
            return Result.Failure<EventEntryResponse>(Error.Validation(
                "EventEntry.SeasonMismatch",
                "That entry belongs to a different season than this event."));
        }

        if (await _context.EventEntries.AnyAsync(
                existing => existing.RaceEventId == raceEventId && existing.EntryId == request.EntryId,
                cancellationToken))
        {
            return Result.Failure<EventEntryResponse>(Error.Conflict(
                "EventEntry.Duplicate",
                "That car is already on this event's entry list."));
        }

        var lineupValidation = await ValidateLineupAsync(request.Drivers, cancellationToken);

        if (lineupValidation.IsFailure)
        {
            return Result.Failure<EventEntryResponse>(lineupValidation.Error);
        }

        var eventEntry = new EventEntry
        {
            RaceEventId = raceEventId,
            EntryId = request.EntryId,
            CarNumberOverride = request.CarNumberOverride,
            Status = request.Status,
            StartingGridPosition = request.StartingGridPosition,
            StartsFromPitLane = request.StartsFromPitLane,
            IsEligibleForPoints = request.IsEligibleForPoints
        };

        foreach (var driver in request.Drivers)
        {
            eventEntry.Drivers.Add(new EventEntryDriver
            {
                EventEntry = eventEntry,
                DriverId = driver.DriverId,
                LineupPosition = driver.LineupPosition,
                Role = driver.Role,
                Grade = driver.Grade,
                IsStartingDriver = driver.IsStartingDriver
            });
        }

        EnsureSingleStarter(eventEntry);

        _context.EventEntries.Add(eventEntry);
        await _context.SaveChangesAsync(cancellationToken);

        return await LoadEventEntryAsync(eventEntry.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<EventEntryResponse>> UpdateLineupAsync(
        Guid eventEntryId,
        UpdateEventEntryLineupRequest request,
        CancellationToken cancellationToken = default)
    {
        var eventEntry = await _context.EventEntries
            .AsTracking()
            .Include(candidate => candidate.Drivers)
            .FirstOrDefaultAsync(candidate => candidate.Id == eventEntryId, cancellationToken);

        if (eventEntry is null)
        {
            return Result.Failure<EventEntryResponse>(
                Error.NotFound("EventEntry.NotFound", $"No event entry with id '{eventEntryId}'."));
        }

        var validation = await ValidateLineupAsync(request.Drivers, cancellationToken);

        if (validation.IsFailure)
        {
            return Result.Failure<EventEntryResponse>(validation.Error);
        }

        _context.EventEntryDrivers.RemoveRange(eventEntry.Drivers);
        eventEntry.Drivers.Clear();

        foreach (var driver in request.Drivers)
        {
            eventEntry.Drivers.Add(new EventEntryDriver
            {
                EventEntry = eventEntry,
                DriverId = driver.DriverId,
                LineupPosition = driver.LineupPosition,
                Role = driver.Role,
                Grade = driver.Grade,
                IsStartingDriver = driver.IsStartingDriver
            });
        }

        EnsureSingleStarter(eventEntry);

        await _context.SaveChangesAsync(cancellationToken);

        return await LoadEventEntryAsync(eventEntryId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result> RemoveEventEntryAsync(Guid eventEntryId, CancellationToken cancellationToken = default)
    {
        var eventEntry = await _context.EventEntries
            .AsTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == eventEntryId, cancellationToken);

        if (eventEntry is null)
        {
            return Result.Failure(Error.NotFound("EventEntry.NotFound", $"No event entry with id '{eventEntryId}'."));
        }

        // Once a car has a classification it is part of the historical record. Withdrawing it
        // keeps the result intact; removing the row would orphan or destroy it.
        if (await _context.SessionResults.AnyAsync(result => result.EventEntryId == eventEntryId, cancellationToken))
        {
            return Result.Failure(Error.Conflict(
                "EventEntry.HasResults",
                "A car with recorded results cannot be removed. Set its status to Withdrawn instead."));
        }

        _context.EventEntries.Remove(eventEntry);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result> ValidateLineupAsync(
        IReadOnlyList<EventEntryDriverRequest> drivers,
        CancellationToken cancellationToken)
    {
        if (drivers.Count == 0)
        {
            return Result.Failure(Error.Validation("EventEntry.EmptyLineup", "An entry needs at least one driver."));
        }

        var duplicateDrivers = drivers
            .GroupBy(driver => driver.DriverId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateDrivers.Count > 0)
        {
            return Result.Failure(Error.Conflict(
                "EventEntry.DuplicateDriver",
                $"A driver can appear once per car; these appear more than once: {string.Join(", ", duplicateDrivers)}."));
        }

        var duplicatePositions = drivers
            .GroupBy(driver => driver.LineupPosition)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicatePositions.Count > 0)
        {
            return Result.Failure(Error.Conflict(
                "EventEntry.DuplicateLineupPosition",
                $"Lineup positions must be unique; these are repeated: {string.Join(", ", duplicatePositions)}."));
        }

        var driverIds = drivers.Select(driver => driver.DriverId).ToList();

        var known = await _context.Drivers
            .Where(driver => driverIds.Contains(driver.Id))
            .Select(driver => driver.Id)
            .ToHashSetAsync(cancellationToken);

        var unknown = driverIds.Where(id => !known.Contains(id)).ToList();

        if (unknown.Count > 0)
        {
            return Result.Failure(Error.NotFound(
                "Driver.NotFound",
                $"These drivers do not exist: {string.Join(", ", unknown)}."));
        }

        if (drivers.Count(driver => driver.IsStartingDriver) > 1)
        {
            return Result.Failure(Error.Conflict(
                "EventEntry.MultipleStarters",
                "Only one driver in a crew can take the start."));
        }

        return Result.Success();
    }

    /// <summary>
    /// Guarantees exactly one starting driver, since the database enforces at most one and a
    /// crew with none would leave a race result unable to say who started.
    /// </summary>
    private static void EnsureSingleStarter(EventEntry eventEntry)
    {
        if (eventEntry.Drivers.Any(driver => driver.IsStartingDriver))
        {
            return;
        }

        var lead = eventEntry.Drivers
            .Where(driver => driver.Role == DriverRole.Race)
            .OrderBy(driver => driver.LineupPosition)
            .FirstOrDefault()
            ?? eventEntry.Drivers.OrderBy(driver => driver.LineupPosition).First();

        lead.IsStartingDriver = true;
    }

    private IQueryable<EventEntry> EventEntryQuery()
        => _context.EventEntries
            .Include(eventEntry => eventEntry.Entry).ThenInclude(entry => entry.Team)
            .Include(eventEntry => eventEntry.Entry).ThenInclude(entry => entry.VehicleClass)
            .Include(eventEntry => eventEntry.Drivers).ThenInclude(driver => driver.Driver)
            .AsSplitQuery();

    private async Task<Result<EntryResponse>> LoadEntryAsync(Guid entryId, CancellationToken cancellationToken)
    {
        var entry = await _context.Entries
            .Include(candidate => candidate.Season).ThenInclude(season => season.Series)
            .Include(candidate => candidate.Team)
            .Include(candidate => candidate.VehicleClass)
            .Include(candidate => candidate.ChassisManufacturer)
            .Include(candidate => candidate.EngineManufacturer)
            .Include(candidate => candidate.TyreSupplier)
            .AsSplitQuery()
            .FirstOrDefaultAsync(candidate => candidate.Id == entryId, cancellationToken);

        return entry is null
            ? Result.Failure<EntryResponse>(Error.NotFound("Entry.NotFound", $"No entry with id '{entryId}'."))
            : Result.Success(entry.ToResponse());
    }

    private async Task<Result<EventEntryResponse>> LoadEventEntryAsync(Guid eventEntryId, CancellationToken cancellationToken)
    {
        var eventEntry = await EventEntryQuery()
            .FirstOrDefaultAsync(candidate => candidate.Id == eventEntryId, cancellationToken);

        return eventEntry is null
            ? Result.Failure<EventEntryResponse>(Error.NotFound("EventEntry.NotFound", $"No event entry with id '{eventEntryId}'."))
            : Result.Success(eventEntry.ToResponse());
    }
}
