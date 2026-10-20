using Microsoft.EntityFrameworkCore;
using RaceService.Application.Abstractions;
using RaceService.Application.Common;
using RaceService.Application.Database;
using RaceService.Application.Domain.Entities.Events;
using RaceService.Application.Domain.Entities.Results;
using RaceService.Application.Mapping;
using RaceService.Contracts.Enums;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Application.Services;

/// <inheritdoc />
public sealed class ResultService : IResultService
{
    private readonly ApplicationDbContext _context;
    private readonly IPointsCalculator _points;
    private readonly IStandingsService _standings;
    private readonly IDateTimeProvider _clock;

    /// <summary>Initialises the service.</summary>
    public ResultService(
        ApplicationDbContext context,
        IPointsCalculator points,
        IStandingsService standings,
        IDateTimeProvider clock)
    {
        _context = context;
        _points = points;
        _standings = standings;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<SessionResultResponse>>> GetResultsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        if (!await _context.Sessions.AnyAsync(session => session.Id == sessionId, cancellationToken))
        {
            return Result.Failure<IReadOnlyList<SessionResultResponse>>(
                Error.NotFound("Session.NotFound", $"No session with id '{sessionId}'."));
        }

        var results = await ResultsQuery(sessionId).ToListAsync(cancellationToken);

        // Unclassified cars sort after classified ones, which is how a results table reads: the
        // finishers in order, then the retirements.
        var ordered = results
            .OrderBy(result => result.Position is null)
            .ThenBy(result => result.Position)
            .ThenBy(result => result.LapsCompleted)
            .Select(ResponseMappings.ToResponse)
            .ToList();

        return Result.Success<IReadOnlyList<SessionResultResponse>>(ordered);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<SessionResultResponse>>> ReplaceResultsAsync(
        Guid sessionId,
        ReplaceSessionResultsRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _context.Sessions
            .AsTracking()
            .Include(candidate => candidate.Results).ThenInclude(result => result.Drivers)
            .Include(candidate => candidate.RaceEvent).ThenInclude(raceEvent => raceEvent.Season)
            .FirstOrDefaultAsync(candidate => candidate.Id == sessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure<IReadOnlyList<SessionResultResponse>>(
                Error.NotFound("Session.NotFound", $"No session with id '{sessionId}'."));
        }

        var validation = await ValidateAsync(session, request, cancellationToken);

        if (validation.IsFailure)
        {
            return Result.Failure<IReadOnlyList<SessionResultResponse>>(validation.Error);
        }

        var lineups = await LoadLineupsAsync(session.RaceEventId, cancellationToken);

        // The whole classification is swapped as one unit, inside a transaction, so a session is
        // never observable in a half-replaced state where positions duplicate or points are
        // partly stale. The transaction runs inside the execution strategy because connection
        // retries are enabled: a retry has to replay the entire swap, not resume a partial one.
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                _context.SessionResults.RemoveRange(session.Results);
                session.Results.Clear();

                foreach (var row in request.Results)
                {
                    var result = new SessionResult
                    {
                        Session = session,
                        EventEntryId = row.EventEntryId,
                        Position = row.Position,
                        ClassPosition = row.ClassPosition,
                        StartingPosition = row.StartingPosition,
                        LapsCompleted = row.LapsCompleted,
                        LapsLed = row.LapsLed,
                        TotalTimeMs = row.TotalTimeMs,
                        GapToLeaderMs = row.GapToLeaderMs,
                        GapToLeaderLaps = row.GapToLeaderLaps,
                        BestLapTimeMs = row.BestLapTimeMs,
                        BestLapNumber = row.BestLapNumber,
                        AverageSpeedKph = row.AverageSpeedKph,
                        TopSpeedKph = row.TopSpeedKph,
                        Status = row.Status,
                        StatusDetail = row.StatusDetail,
                        IsClassified = row.IsClassified,
                        PointsScored = row.PointsScored,
                        IsFastestLap = row.IsFastestLap,
                        IsPolePosition = row.IsPolePosition,
                        PitStopCount = row.PitStopCount,
                        PenaltyTimeMs = row.PenaltyTimeMs
                    };

                    // A single-driver car need not restate its crew: it is already on the entry, and
                    // requiring it again would be an invitation to state it inconsistently.
                    var driverRows = row.Drivers.Count > 0
                        ? row.Drivers
                        : lineups.TryGetValue(row.EventEntryId, out var lineup)
                            ? [.. lineup.Select(driverId => new SessionResultDriverRequest
                            {
                                DriverId = driverId,
                                LapsDriven = row.LapsCompleted
                            })]
                            : (IReadOnlyList<SessionResultDriverRequest>)[];

                    foreach (var driverRow in driverRows)
                    {
                        result.Drivers.Add(new SessionResultDriver
                        {
                            SessionResult = result,
                            DriverId = driverRow.DriverId,
                            LapsDriven = driverRow.LapsDriven,
                            BestLapTimeMs = driverRow.BestLapTimeMs,
                            TimeDrivenMs = driverRow.TimeDrivenMs,
                            SetBestLap = driverRow.SetBestLap,
                            PointsScored = row.PointsScored,
                            IsEligibleForPoints = true
                        });
                    }

                    session.Results.Add(result);
                }

                if (request.RecalculatePoints)
                {
                    var scheme = await LoadPointsSchemeAsync(session, cancellationToken);
                    _points.Apply(session, scheme);
                }

                session.ResultsStatus = request.ResultsStatus;
                session.ResultsPublishedAtUtc = request.ResultsStatus is ResultsStatus.Official or ResultsStatus.Amended
                    ? _clock.UtcNow
                    : session.ResultsPublishedAtUtc;

                // Qualifying settles the grid, so publishing it writes each car's starting slot back
                // onto its event entry. Doing it here means a race result table can show "started P4"
                // without reaching into another session's classification.
                if (session.Type is SessionType.Qualifying or SessionType.Hyperpole)
                {
                    await ApplyGridFromQualifyingAsync(session, cancellationToken);
                }

                await _context.SaveChangesAsync(cancellationToken);

                if (session.IsPointsScoring && request.ResultsStatus is ResultsStatus.Official or ResultsStatus.Amended)
                {
                    await _standings.RecalculateSeasonAsync(session.RaceEvent.SeasonId, cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });

        return await GetResultsAsync(sessionId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<SessionLapResponse>>> GetLapsAsync(
        Guid sessionId,
        Guid? eventEntryId,
        CancellationToken cancellationToken = default)
    {
        if (!await _context.Sessions.AnyAsync(session => session.Id == sessionId, cancellationToken))
        {
            return Result.Failure<IReadOnlyList<SessionLapResponse>>(
                Error.NotFound("Session.NotFound", $"No session with id '{sessionId}'."));
        }

        var laps = _context.SessionLaps
            .Include(lap => lap.EventEntry).ThenInclude(entry => entry.Entry)
            .Include(lap => lap.Driver)
            .Where(lap => lap.SessionId == sessionId);

        if (eventEntryId is not null)
        {
            laps = laps.Where(lap => lap.EventEntryId == eventEntryId);
        }

        var rows = await laps
            .OrderBy(lap => lap.LapNumber)
            .ThenBy(lap => lap.Position)
            .Take(20_000)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<SessionLapResponse>>([.. rows.Select(ResponseMappings.ToResponse)]);
    }

    /// <inheritdoc />
    public async Task<Result> ReplaceLapsAsync(
        Guid sessionId,
        ReplaceSessionLapsRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _context.Sessions
            .AsTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == sessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure(Error.NotFound("Session.NotFound", $"No session with id '{sessionId}'."));
        }

        var validEntryIds = await _context.EventEntries
            .Where(entry => entry.RaceEventId == session.RaceEventId)
            .Select(entry => entry.Id)
            .ToHashSetAsync(cancellationToken);

        var unknown = request.Laps
            .Select(lap => lap.EventEntryId)
            .Distinct()
            .Where(id => !validEntryIds.Contains(id))
            .ToList();

        if (unknown.Count > 0)
        {
            return Result.Failure(Error.Validation(
                "SessionLap.UnknownEntry",
                $"These entries are not on this event's entry list: {string.Join(", ", unknown)}."));
        }

        var duplicates = request.Laps
            .GroupBy(lap => (lap.EventEntryId, lap.LapNumber))
            .Where(group => group.Count() > 1)
            .ToList();

        if (duplicates.Count > 0)
        {
            return Result.Failure(Error.Conflict(
                "SessionLap.Duplicate",
                $"{duplicates.Count} car/lap-number combination(s) appear more than once."));
        }

        await _context.SessionLaps
            .Where(lap => lap.SessionId == sessionId)
            .ExecuteDeleteAsync(cancellationToken);

        // A session's fastest lap and each car's personal best are derived here rather than
        // trusted from the upload, so the flags cannot contradict the times they describe.
        var timed = request.Laps
            .Where(lap => lap.LapTimeMs is > 0 && !lap.IsDeletedByStewards)
            .ToList();

        var sessionBest = timed.Count == 0 ? null : timed.Min(lap => lap.LapTimeMs);

        var personalBests = timed
            .GroupBy(lap => lap.EventEntryId)
            .ToDictionary(group => group.Key, group => group.Min(lap => lap.LapTimeMs));

        var laps = request.Laps.Select(lap => new SessionLap
        {
            SessionId = sessionId,
            EventEntryId = lap.EventEntryId,
            DriverId = lap.DriverId,
            LapNumber = lap.LapNumber,
            LapTimeMs = lap.LapTimeMs,
            Sector1Ms = lap.Sector1Ms,
            Sector2Ms = lap.Sector2Ms,
            Sector3Ms = lap.Sector3Ms,
            Position = lap.Position,
            SpeedTrapKph = lap.SpeedTrapKph,
            TyreCompound = lap.TyreCompound,
            TyreAgeLaps = lap.TyreAgeLaps,
            IsPitInLap = lap.IsPitInLap,
            IsPitOutLap = lap.IsPitOutLap,
            IsUnderCaution = lap.IsUnderCaution,
            IsDeletedByStewards = lap.IsDeletedByStewards,
            IsPersonalBest = lap.LapTimeMs is not null
                && personalBests.TryGetValue(lap.EventEntryId, out var best)
                && lap.LapTimeMs == best,
            IsSessionBest = lap.LapTimeMs is not null && lap.LapTimeMs == sessionBest,
            CreatedAtUtc = _clock.UtcNow
        });

        _context.SessionLaps.AddRange(laps);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<PitStopResponse>>> GetPitStopsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        if (!await _context.Sessions.AnyAsync(session => session.Id == sessionId, cancellationToken))
        {
            return Result.Failure<IReadOnlyList<PitStopResponse>>(
                Error.NotFound("Session.NotFound", $"No session with id '{sessionId}'."));
        }

        var stops = await _context.PitStops
            .Include(stop => stop.EventEntry).ThenInclude(entry => entry.Entry)
            .Include(stop => stop.DriverIn)
            .Include(stop => stop.DriverOut)
            .Where(stop => stop.SessionId == sessionId)
            .OrderBy(stop => stop.LapNumber)
            .ThenBy(stop => stop.StopNumber)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<PitStopResponse>>([.. stops.Select(ResponseMappings.ToResponse)]);
    }

    /// <inheritdoc />
    public async Task<Result<PitStopResponse>> AddPitStopAsync(
        Guid sessionId,
        CreatePitStopRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _context.Sessions
            .FirstOrDefaultAsync(candidate => candidate.Id == sessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure<PitStopResponse>(Error.NotFound("Session.NotFound", $"No session with id '{sessionId}'."));
        }

        if (!await _context.EventEntries.AnyAsync(
                entry => entry.Id == request.EventEntryId && entry.RaceEventId == session.RaceEventId,
                cancellationToken))
        {
            return Result.Failure<PitStopResponse>(Error.Validation(
                "PitStop.UnknownEntry",
                "That entry is not on this event's entry list."));
        }

        if (await _context.PitStops.AnyAsync(
                stop => stop.SessionId == sessionId
                    && stop.EventEntryId == request.EventEntryId
                    && stop.StopNumber == request.StopNumber,
                cancellationToken))
        {
            return Result.Failure<PitStopResponse>(Error.Conflict(
                "PitStop.Duplicate",
                $"Stop number {request.StopNumber} is already recorded for that car."));
        }

        var pitStop = new PitStop
        {
            SessionId = sessionId,
            EventEntryId = request.EventEntryId,
            StopNumber = request.StopNumber,
            LapNumber = request.LapNumber,
            StationaryTimeMs = request.StationaryTimeMs,
            TotalPitLaneTimeMs = request.TotalPitLaneTimeMs,
            TyreCompoundFitted = request.TyreCompoundFitted,
            RefuelledStop = request.RefuelledStop,
            DriverChanged = request.DriverChanged,
            DriverInId = request.DriverInId,
            DriverOutId = request.DriverOutId,
            IsUnderCaution = request.IsUnderCaution,
            IsPenaltyServed = request.IsPenaltyServed,
            Note = request.Note,
            CreatedAtUtc = _clock.UtcNow
        };

        _context.PitStops.Add(pitStop);

        // Keep the denormalised stop count on the classification in step with the stop rows.
        var result = await _context.SessionResults
            .AsTracking()
            .FirstOrDefaultAsync(
                candidate => candidate.SessionId == sessionId && candidate.EventEntryId == request.EventEntryId,
                cancellationToken);

        if (result is not null)
        {
            result.PitStopCount = await _context.PitStops.CountAsync(
                stop => stop.SessionId == sessionId && stop.EventEntryId == request.EventEntryId,
                cancellationToken) + 1;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var saved = await _context.PitStops
            .Include(stop => stop.EventEntry).ThenInclude(entry => entry.Entry)
            .Include(stop => stop.DriverIn)
            .Include(stop => stop.DriverOut)
            .FirstAsync(stop => stop.Id == pitStop.Id, cancellationToken);

        return Result.Success(saved.ToResponse());
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<PenaltyResponse>>> GetPenaltiesAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        if (!await _context.Sessions.AnyAsync(session => session.Id == sessionId, cancellationToken))
        {
            return Result.Failure<IReadOnlyList<PenaltyResponse>>(
                Error.NotFound("Session.NotFound", $"No session with id '{sessionId}'."));
        }

        var penalties = await _context.Penalties
            .Include(penalty => penalty.EventEntry).ThenInclude(entry => entry.Entry)
            .Include(penalty => penalty.Driver)
            .Where(penalty => penalty.SessionId == sessionId)
            .OrderBy(penalty => penalty.LapNumber)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<PenaltyResponse>>([.. penalties.Select(ResponseMappings.ToResponse)]);
    }

    /// <inheritdoc />
    public async Task<Result<PenaltyResponse>> AddPenaltyAsync(
        Guid sessionId,
        CreatePenaltyRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _context.Sessions
            .FirstOrDefaultAsync(candidate => candidate.Id == sessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure<PenaltyResponse>(Error.NotFound("Session.NotFound", $"No session with id '{sessionId}'."));
        }

        if (!await _context.EventEntries.AnyAsync(
                entry => entry.Id == request.EventEntryId && entry.RaceEventId == session.RaceEventId,
                cancellationToken))
        {
            return Result.Failure<PenaltyResponse>(Error.Validation(
                "Penalty.UnknownEntry",
                "That entry is not on this event's entry list."));
        }

        var penalty = new Penalty
        {
            SessionId = sessionId,
            EventEntryId = request.EventEntryId,
            DriverId = request.DriverId,
            Type = request.Type,
            TimePenaltySeconds = request.TimePenaltySeconds,
            GridPositionDrop = request.GridPositionDrop,
            LicencePoints = request.LicencePoints,
            FineAmount = request.FineAmount,
            FineCurrency = request.FineCurrency,
            Reason = request.Reason,
            LapNumber = request.LapNumber,
            IssuedAtUtc = request.IssuedAtUtc ?? _clock.UtcNow,
            AppliedToSessionId = request.AppliedToSessionId
        };

        _context.Penalties.Add(penalty);
        await _context.SaveChangesAsync(cancellationToken);

        var saved = await _context.Penalties
            .Include(candidate => candidate.EventEntry).ThenInclude(entry => entry.Entry)
            .Include(candidate => candidate.Driver)
            .FirstAsync(candidate => candidate.Id == penalty.Id, cancellationToken);

        return Result.Success(saved.ToResponse());
    }

    private IQueryable<SessionResult> ResultsQuery(Guid sessionId)
        => _context.SessionResults
            .Include(result => result.EventEntry).ThenInclude(entry => entry.Entry).ThenInclude(entry => entry.Team)
            .Include(result => result.EventEntry).ThenInclude(entry => entry.Entry).ThenInclude(entry => entry.VehicleClass)
            .Include(result => result.Drivers).ThenInclude(driver => driver.Driver)
            .Where(result => result.SessionId == sessionId)
            .AsSplitQuery();

    /// <summary>
    /// Checks the classification as a set. These are the invariants the database also enforces,
    /// checked up front so the caller gets one clear message rather than a constraint violation.
    /// </summary>
    private async Task<Result> ValidateAsync(
        Session session,
        ReplaceSessionResultsRequest request,
        CancellationToken cancellationToken)
    {
        var entryIds = request.Results.Select(row => row.EventEntryId).ToList();

        var duplicateEntries = entryIds
            .GroupBy(id => id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateEntries.Count > 0)
        {
            return Result.Failure(Error.Conflict(
                "SessionResult.DuplicateEntry",
                $"A car may appear once in a classification; these appear more than once: {string.Join(", ", duplicateEntries)}."));
        }

        var validEntryIds = await _context.EventEntries
            .Where(entry => entry.RaceEventId == session.RaceEventId)
            .Select(entry => entry.Id)
            .ToHashSetAsync(cancellationToken);

        var unknown = entryIds.Where(id => !validEntryIds.Contains(id)).ToList();

        if (unknown.Count > 0)
        {
            return Result.Failure(Error.Validation(
                "SessionResult.UnknownEntry",
                $"These entries are not on this event's entry list: {string.Join(", ", unknown)}."));
        }

        var positions = request.Results
            .Where(row => row.Position is not null)
            .Select(row => row.Position!.Value)
            .ToList();

        var duplicatePositions = positions
            .GroupBy(position => position)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicatePositions.Count > 0)
        {
            return Result.Failure(Error.Conflict(
                "SessionResult.DuplicatePosition",
                $"Finishing positions must be unique; these are repeated: {string.Join(", ", duplicatePositions)}."));
        }

        // Positions must form 1..n with no holes. A gap almost always means a row was dropped
        // during data entry, and silently accepting it corrupts every points total downstream.
        if (positions.Count > 0)
        {
            var expected = Enumerable.Range(1, positions.Count).ToHashSet();
            var missing = expected.Except(positions).OrderBy(position => position).ToList();

            if (missing.Count > 0)
            {
                return Result.Failure(Error.Validation(
                    "SessionResult.PositionGap",
                    $"Finishing positions must run from 1 with no gaps; missing: {string.Join(", ", missing)}."));
            }
        }

        if (request.Results.Count(row => row.IsFastestLap) > 1)
        {
            return Result.Failure(Error.Conflict(
                "SessionResult.MultipleFastestLaps",
                "Only one car can hold the fastest lap of a session."));
        }

        if (request.Results.Count(row => row.IsPolePosition) > 1)
        {
            return Result.Failure(Error.Conflict(
                "SessionResult.MultiplePoles",
                "Only one car can hold pole position."));
        }

        foreach (var row in request.Results.Where(row => row.LapsLed > row.LapsCompleted))
        {
            return Result.Failure(Error.Validation(
                "SessionResult.LapsLedExceedsCompleted",
                $"Entry '{row.EventEntryId}' is recorded as leading more laps than it completed."));
        }

        return Result.Success();
    }

    private async Task<Dictionary<Guid, List<Guid>>> LoadLineupsAsync(Guid raceEventId, CancellationToken cancellationToken)
    {
        var rows = await _context.EventEntryDrivers
            .Where(entryDriver => entryDriver.EventEntry.RaceEventId == raceEventId
                && entryDriver.Role == DriverRole.Race)
            .OrderBy(entryDriver => entryDriver.LineupPosition)
            .Select(entryDriver => new { entryDriver.EventEntryId, entryDriver.DriverId })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(row => row.EventEntryId)
            .ToDictionary(group => group.Key, group => group.Select(row => row.DriverId).ToList());
    }

    private async Task<Domain.Entities.Catalog.PointsScheme?> LoadPointsSchemeAsync(
        Session session,
        CancellationToken cancellationToken)
        => await _context.PointsSchemes
            .Include(scheme => scheme.Entries)
            .FirstOrDefaultAsync(
                scheme => scheme.SeasonId == session.RaceEvent.SeasonId
                    && scheme.AppliesToSessionType == session.Type
                    && scheme.ChampionshipType == ChampionshipType.Drivers,
                cancellationToken);

    private async Task ApplyGridFromQualifyingAsync(Session session, CancellationToken cancellationToken)
    {
        var grid = session.Results
            .Where(result => result.Position is not null)
            .ToDictionary(result => result.EventEntryId, result => result.Position!.Value);

        if (grid.Count == 0)
        {
            return;
        }

        var entries = await _context.EventEntries
            .AsTracking()
            .Where(entry => entry.RaceEventId == session.RaceEventId && grid.Keys.Contains(entry.Id))
            .ToListAsync(cancellationToken);

        foreach (var entry in entries)
        {
            entry.StartingGridPosition = grid[entry.Id];
        }
    }
}
