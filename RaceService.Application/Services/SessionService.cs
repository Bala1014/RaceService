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
public sealed class SessionService : ISessionService
{
    private readonly ApplicationDbContext _context;
    private readonly ITimeZoneService _timeZones;
    private readonly IDateTimeProvider _clock;

    /// <summary>Initialises the service.</summary>
    public SessionService(ApplicationDbContext context, ITimeZoneService timeZones, IDateTimeProvider clock)
    {
        _context = context;
        _timeZones = timeZones;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<SessionSummaryResponse>>> GetForEventAsync(
        Guid raceEventId,
        string? viewerTimeZone,
        CancellationToken cancellationToken = default)
    {
        if (!await _context.RaceEvents.AnyAsync(raceEvent => raceEvent.Id == raceEventId, cancellationToken))
        {
            return Result.Failure<IReadOnlyList<SessionSummaryResponse>>(
                Error.NotFound("RaceEvent.NotFound", $"No race event with id '{raceEventId}'."));
        }

        var sessions = await _context.Sessions
            .Where(session => session.RaceEventId == raceEventId)
            .OrderBy(session => session.WeekendOrder)
            .Select(SummaryProjection())
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<SessionSummaryResponse>>(
            [.. sessions.Select(row => Localize(row, viewerTimeZone))]);
    }

    /// <inheritdoc />
    public async Task<Result<SessionDetailResponse>> GetByIdAsync(
        Guid id,
        string? viewerTimeZone,
        CancellationToken cancellationToken = default)
    {
        var session = await _context.Sessions
            .Include(candidate => candidate.RaceEvent)
            .Include(candidate => candidate.Results).ThenInclude(result => result.EventEntry).ThenInclude(entry => entry.Entry).ThenInclude(entry => entry.Team)
            .Include(candidate => candidate.Results).ThenInclude(result => result.EventEntry).ThenInclude(entry => entry.Entry).ThenInclude(entry => entry.VehicleClass)
            .Include(candidate => candidate.Results).ThenInclude(result => result.Drivers).ThenInclude(driver => driver.Driver)
            .AsSplitQuery()
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (session is null)
        {
            return Result.Failure<SessionDetailResponse>(Error.NotFound("Session.NotFound", $"No session with id '{id}'."));
        }

        return Result.Success(new SessionDetailResponse
        {
            Id = session.Id,
            RaceEventId = session.RaceEventId,
            RaceEventName = session.RaceEvent.Name,
            RaceEventSlug = session.RaceEvent.Slug,
            Type = session.Type,
            SequenceNumber = session.SequenceNumber,
            Name = session.Name,
            ScheduledStart = _timeZones.Localize(session.ScheduledStartUtc, session.RaceEvent.TimeZoneId, viewerTimeZone),
            ScheduledEndUtc = session.ScheduledEndUtc,
            ActualStartUtc = session.ActualStartUtc,
            ActualEndUtc = session.ActualEndUtc,
            Status = session.Status,
            StatusNote = session.StatusNote,
            ResultsStatus = session.ResultsStatus,
            ResultsPublishedAtUtc = session.ResultsPublishedAtUtc,
            ScheduledDurationMinutes = session.ScheduledDurationMinutes,
            ScheduledLaps = session.ScheduledLaps,
            CompletedLaps = session.CompletedLaps,
            IsPointsScoring = session.IsPointsScoring,
            WeatherSummary = session.WeatherSummary,
            AirTemperatureC = session.AirTemperatureC,
            TrackTemperatureC = session.TrackTemperatureC,
            HumidityPercent = session.HumidityPercent,
            RedFlagCount = session.RedFlagCount,
            SafetyCarCount = session.SafetyCarCount,
            Results = [.. session.Results
                .OrderBy(result => result.Position is null)
                .ThenBy(result => result.Position)
                .Select(ResponseMappings.ToResponse)]
        });
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SessionSummaryResponse>> GetLiveAndNextAsync(
        int count,
        string? viewerTimeZone,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;

        // Anything currently running comes first regardless of its scheduled time, then the next
        // scheduled sessions. This is the query behind a "what's on now" strip.
        var sessions = await _context.Sessions
            .Where(session => session.RaceEvent.IsPublished)
            .Where(session => session.Status == SessionStatus.Running
                || session.Status == SessionStatus.Suspended
                || (session.Status == SessionStatus.Scheduled && session.ScheduledStartUtc >= now.AddHours(-4)))
            .OrderByDescending(session => session.Status == SessionStatus.Running || session.Status == SessionStatus.Suspended)
            .ThenBy(session => session.ScheduledStartUtc)
            .Take(Math.Clamp(count, 1, 50))
            .Select(SummaryProjection())
            .ToListAsync(cancellationToken);

        return [.. sessions.Select(row => Localize(row, viewerTimeZone))];
    }

    /// <inheritdoc />
    public async Task<Result<SessionSummaryResponse>> CreateAsync(
        Guid raceEventId,
        CreateSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var raceEvent = await _context.RaceEvents
            .AsTracking()
            .Include(candidate => candidate.Sessions)
            .FirstOrDefaultAsync(candidate => candidate.Id == raceEventId, cancellationToken);

        if (raceEvent is null)
        {
            return Result.Failure<SessionSummaryResponse>(
                Error.NotFound("RaceEvent.NotFound", $"No race event with id '{raceEventId}'."));
        }

        if (raceEvent.Sessions.Any(session =>
                session.Type == request.Type && session.SequenceNumber == request.SequenceNumber))
        {
            return Result.Failure<SessionSummaryResponse>(Error.Conflict(
                "Session.Duplicate",
                $"This event already has a {request.Type} session with sequence {request.SequenceNumber}."));
        }

        var session = new Session
        {
            RaceEvent = raceEvent,
            Type = request.Type,
            SequenceNumber = request.SequenceNumber,
            Name = string.IsNullOrWhiteSpace(request.Name)
                ? SessionNaming.Derive(request.Type, request.SequenceNumber)
                : request.Name,
            ScheduledStartUtc = DateTime.SpecifyKind(request.ScheduledStartUtc, DateTimeKind.Utc),
            ScheduledEndUtc = request.ScheduledEndUtc is null
                ? null
                : DateTime.SpecifyKind(request.ScheduledEndUtc.Value, DateTimeKind.Utc),
            ScheduledDurationMinutes = request.ScheduledDurationMinutes,
            ScheduledLaps = request.ScheduledLaps,
            IsPointsScoring = request.IsPointsScoring ?? request.Type is SessionType.Race or SessionType.Sprint
        };

        raceEvent.Sessions.Add(session);
        ResequenceWeekend(raceEvent);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(Localize(ToSummary(session, raceEvent.TimeZoneId), null));
    }

    /// <inheritdoc />
    public async Task<Result<SessionSummaryResponse>> UpdateAsync(
        Guid id,
        UpdateSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _context.Sessions
            .AsTracking()
            .Include(candidate => candidate.RaceEvent).ThenInclude(raceEvent => raceEvent.Sessions)
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (session is null)
        {
            return Result.Failure<SessionSummaryResponse>(Error.NotFound("Session.NotFound", $"No session with id '{id}'."));
        }

        if (request.ScheduledEndUtc is not null && request.ScheduledEndUtc < request.ScheduledStartUtc)
        {
            return Result.Failure<SessionSummaryResponse>(
                Error.Validation("Session.InvalidTimeRange", "A session cannot end before it starts."));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            session.Name = request.Name;
        }

        session.ScheduledStartUtc = DateTime.SpecifyKind(request.ScheduledStartUtc, DateTimeKind.Utc);
        session.ScheduledEndUtc = request.ScheduledEndUtc is null
            ? null
            : DateTime.SpecifyKind(request.ScheduledEndUtc.Value, DateTimeKind.Utc);
        session.ScheduledDurationMinutes = request.ScheduledDurationMinutes;
        session.ScheduledLaps = request.ScheduledLaps;
        session.IsPointsScoring = request.IsPointsScoring;

        ResequenceWeekend(session.RaceEvent);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(Localize(ToSummary(session, session.RaceEvent.TimeZoneId), null));
    }

    /// <inheritdoc />
    public async Task<Result> UpdateStatusAsync(
        Guid id,
        UpdateSessionStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _context.Sessions
            .AsTracking()
            .Include(candidate => candidate.RaceEvent)
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (session is null)
        {
            return Result.Failure(Error.NotFound("Session.NotFound", $"No session with id '{id}'."));
        }

        session.Status = request.Status;
        session.ActualStartUtc = request.ActualStartUtc is null
            ? session.ActualStartUtc
            : DateTime.SpecifyKind(request.ActualStartUtc.Value, DateTimeKind.Utc);
        session.ActualEndUtc = request.ActualEndUtc is null
            ? session.ActualEndUtc
            : DateTime.SpecifyKind(request.ActualEndUtc.Value, DateTimeKind.Utc);
        session.CompletedLaps = request.CompletedLaps ?? session.CompletedLaps;
        session.WeatherSummary = request.WeatherSummary ?? session.WeatherSummary;
        session.AirTemperatureC = request.AirTemperatureC ?? session.AirTemperatureC;
        session.TrackTemperatureC = request.TrackTemperatureC ?? session.TrackTemperatureC;
        session.HumidityPercent = request.HumidityPercent ?? session.HumidityPercent;
        session.RedFlagCount = request.RedFlagCount ?? session.RedFlagCount;
        session.SafetyCarCount = request.SafetyCarCount ?? session.SafetyCarCount;
        session.StatusNote = request.StatusNote ?? session.StatusNote;

        // The weekend's own status follows its sessions, so an event never claims to be scheduled
        // while a session of it is running.
        await SyncEventStatusAsync(session.RaceEvent, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var session = await _context.Sessions
            .AsTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (session is null)
        {
            return Result.Failure(Error.NotFound("Session.NotFound", $"No session with id '{id}'."));
        }

        // Deleting a session cascades to its results, which is destructive and irreversible.
        // Refusing once a classification exists is the safe default; an admin who really means it
        // clears the results first.
        if (await _context.SessionResults.AnyAsync(result => result.SessionId == id, cancellationToken))
        {
            return Result.Failure(Error.Conflict(
                "Session.HasResults",
                "A session with recorded results cannot be deleted. Clear its results first."));
        }

        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <summary>
    /// Renumbers a weekend's running order by scheduled time and refreshes the event's window.
    /// Called after any schedule change so the two never drift apart.
    /// </summary>
    private static void ResequenceWeekend(RaceEvent raceEvent)
    {
        var order = 1;

        foreach (var session in raceEvent.Sessions.OrderBy(session => session.ScheduledStartUtc))
        {
            session.WeekendOrder = order++;
        }

        if (raceEvent.Sessions.Count == 0)
        {
            return;
        }

        raceEvent.StartTimeUtc = raceEvent.Sessions.Min(session => session.ScheduledStartUtc);
        raceEvent.EndTimeUtc = raceEvent.Sessions.Max(session =>
            session.ScheduledEndUtc ?? session.ScheduledStartUtc.AddHours(2));
    }

    private async Task SyncEventStatusAsync(RaceEvent raceEvent, CancellationToken cancellationToken)
    {
        var statuses = await _context.Sessions
            .Where(session => session.RaceEventId == raceEvent.Id)
            .Select(session => session.Status)
            .ToListAsync(cancellationToken);

        if (statuses.Count == 0 || raceEvent.Status is EventStatus.Cancelled or EventStatus.Postponed)
        {
            return;
        }

        if (statuses.Any(status => status is SessionStatus.Running or SessionStatus.Suspended))
        {
            raceEvent.Status = EventStatus.InProgress;
            return;
        }

        if (statuses.All(status => status is SessionStatus.Finished or SessionStatus.Cancelled or SessionStatus.Abandoned))
        {
            raceEvent.Status = EventStatus.Completed;
        }
    }

    private static SessionSummaryResponse ToSummary(Session session, string venueTimeZone) => new()
    {
        Id = session.Id,
        RaceEventId = session.RaceEventId,
        Type = session.Type,
        SequenceNumber = session.SequenceNumber,
        Name = session.Name,
        WeekendOrder = session.WeekendOrder,
        ScheduledStart = new LocalizedTime { Utc = session.ScheduledStartUtc, VenueTimeZone = venueTimeZone },
        ScheduledEndUtc = session.ScheduledEndUtc,
        Status = session.Status,
        ResultsStatus = session.ResultsStatus,
        ScheduledDurationMinutes = session.ScheduledDurationMinutes,
        ScheduledLaps = session.ScheduledLaps,
        IsPointsScoring = session.IsPointsScoring,
        HasResults = session.ResultsStatus != ResultsStatus.NotAvailable
    };

    private static System.Linq.Expressions.Expression<Func<Session, SessionSummaryResponse>> SummaryProjection()
        => session => new SessionSummaryResponse
        {
            Id = session.Id,
            RaceEventId = session.RaceEventId,
            Type = session.Type,
            SequenceNumber = session.SequenceNumber,
            Name = session.Name,
            WeekendOrder = session.WeekendOrder,
            ScheduledStart = new LocalizedTime
            {
                Utc = session.ScheduledStartUtc,
                VenueTimeZone = session.RaceEvent.TimeZoneId
            },
            ScheduledEndUtc = session.ScheduledEndUtc,
            Status = session.Status,
            ResultsStatus = session.ResultsStatus,
            ScheduledDurationMinutes = session.ScheduledDurationMinutes,
            ScheduledLaps = session.ScheduledLaps,
            IsPointsScoring = session.IsPointsScoring,
            HasResults = session.ResultsStatus != ResultsStatus.NotAvailable
        };

    private SessionSummaryResponse Localize(SessionSummaryResponse row, string? viewerTimeZone)
        => row with
        {
            ScheduledStart = _timeZones.Localize(
                row.ScheduledStart.Utc,
                row.ScheduledStart.VenueTimeZone,
                viewerTimeZone)
        };
}
