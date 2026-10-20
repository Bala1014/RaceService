using Microsoft.EntityFrameworkCore;
using RaceService.Application.Abstractions;
using RaceService.Application.Common;
using RaceService.Application.Database;
using RaceService.Application.Domain.Entities.Standings;
using RaceService.Contracts.Enums;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Application.Services;

/// <inheritdoc />
/// <remarks>
/// Standings are a projection over results, rebuilt whenever a session's classification becomes
/// official or is amended. Keeping them as stored rows rather than computing them per request is
/// what lets the championship page load in one indexed read instead of aggregating every result
/// of the season on every view.
/// </remarks>
public sealed class StandingsService : IStandingsService
{
    private readonly ApplicationDbContext _context;
    private readonly IDateTimeProvider _clock;

    /// <summary>Initialises the service.</summary>
    public StandingsService(ApplicationDbContext context, IDateTimeProvider clock)
    {
        _context = context;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<DriverStandingResponse>>> GetDriverStandingsAsync(
        Guid seasonId,
        StandingsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!await _context.Seasons.AnyAsync(season => season.Id == seasonId, cancellationToken))
        {
            return Result.Failure<IReadOnlyList<DriverStandingResponse>>(
                Error.NotFound("Season.NotFound", $"No season with id '{seasonId}'."));
        }

        var standings = await _context.DriverStandings
            .Include(standing => standing.Driver)
            .Include(standing => standing.VehicleClass)
            .Where(standing => standing.SeasonId == seasonId
                && standing.AfterEventId == query.AfterEventId
                && standing.VehicleClassId == query.VehicleClassId)
            .OrderBy(standing => standing.Position)
            .ToListAsync(cancellationToken);

        // Teams are gathered separately: a driver who moved mid-season belongs to more than one,
        // and folding that into the standings row would either lose information or duplicate the
        // driver in the table.
        var teamsByDriver = await LoadTeamsByDriverAsync(seasonId, cancellationToken);

        var rows = standings.Select(standing => new DriverStandingResponse
        {
            Position = standing.Position,
            PreviousPosition = standing.PreviousPosition,
            PositionChange = standing.PreviousPosition is null ? null : standing.PreviousPosition - standing.Position,
            DriverId = standing.DriverId,
            DriverName = standing.Driver.FullName,
            DriverSlug = standing.Driver.Slug,
            Abbreviation = standing.Driver.Abbreviation,
            CountryCode = standing.Driver.CountryCode,
            HeadshotUrl = standing.Driver.HeadshotUrl,
            TeamNames = teamsByDriver.TryGetValue(standing.DriverId, out var teams) ? teams : [],
            VehicleClassName = standing.VehicleClass?.Name,
            Points = standing.Points,
            PointsBeforeDrops = standing.PointsBeforeDrops,
            Starts = standing.Starts,
            Wins = standing.Wins,
            Podiums = standing.Podiums,
            PolePositions = standing.PolePositions,
            FastestLaps = standing.FastestLaps,
            Retirements = standing.Retirements,
            RecalculatedAtUtc = standing.RecalculatedAtUtc
        }).ToList();

        return Result.Success<IReadOnlyList<DriverStandingResponse>>(rows);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<TeamStandingResponse>>> GetTeamStandingsAsync(
        Guid seasonId,
        StandingsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!await _context.Seasons.AnyAsync(season => season.Id == seasonId, cancellationToken))
        {
            return Result.Failure<IReadOnlyList<TeamStandingResponse>>(
                Error.NotFound("Season.NotFound", $"No season with id '{seasonId}'."));
        }

        var standings = await _context.TeamStandings
            .Include(standing => standing.Team)
            .Include(standing => standing.VehicleClass)
            .Where(standing => standing.SeasonId == seasonId
                && standing.AfterEventId == query.AfterEventId
                && standing.VehicleClassId == query.VehicleClassId)
            .OrderBy(standing => standing.Position)
            .ToListAsync(cancellationToken);

        var rows = standings.Select(standing => new TeamStandingResponse
        {
            Position = standing.Position,
            PreviousPosition = standing.PreviousPosition,
            PositionChange = standing.PreviousPosition is null ? null : standing.PreviousPosition - standing.Position,
            TeamId = standing.TeamId,
            TeamName = standing.Team.Name,
            TeamSlug = standing.Team.Slug,
            CountryCode = standing.Team.CountryCode,
            LogoUrl = standing.Team.LogoUrl,
            PrimaryColorHex = standing.Team.PrimaryColorHex,
            VehicleClassName = standing.VehicleClass?.Name,
            Points = standing.Points,
            PointsBeforeDrops = standing.PointsBeforeDrops,
            Starts = standing.Starts,
            Wins = standing.Wins,
            Podiums = standing.Podiums,
            PolePositions = standing.PolePositions,
            FastestLaps = standing.FastestLaps,
            RecalculatedAtUtc = standing.RecalculatedAtUtc
        }).ToList();

        return Result.Success<IReadOnlyList<TeamStandingResponse>>(rows);
    }

    /// <inheritdoc />
    public async Task<Result> RecalculateSeasonAsync(Guid seasonId, CancellationToken cancellationToken = default)
    {
        var season = await _context.Seasons
            .FirstOrDefaultAsync(candidate => candidate.Id == seasonId, cancellationToken);

        if (season is null)
        {
            return Result.Failure(Error.NotFound("Season.NotFound", $"No season with id '{seasonId}'."));
        }

        // Only official and amended classifications count. A provisional result that is still
        // under appeal must not move the championship.
        var rows = await _context.SessionResultDrivers
            .Where(resultDriver =>
                resultDriver.SessionResult.Session.RaceEvent.SeasonId == seasonId
                && resultDriver.SessionResult.Session.IsPointsScoring
                && (resultDriver.SessionResult.Session.ResultsStatus == ResultsStatus.Official
                    || resultDriver.SessionResult.Session.ResultsStatus == ResultsStatus.Amended))
            .Select(resultDriver => new ScoringRow
            {
                DriverId = resultDriver.DriverId,
                TeamId = resultDriver.SessionResult.EventEntry.Entry.TeamId,
                VehicleClassId = resultDriver.SessionResult.EventEntry.Entry.VehicleClassId,
                Points = resultDriver.PointsScored,
                Position = resultDriver.SessionResult.Position,
                ClassPosition = resultDriver.SessionResult.ClassPosition,
                IsFastestLap = resultDriver.SessionResult.IsFastestLap,
                IsPolePosition = resultDriver.SessionResult.IsPolePosition,
                Status = resultDriver.SessionResult.Status,
                SessionType = resultDriver.SessionResult.Session.Type
            })
            .ToListAsync(cancellationToken);

        var now = _clock.UtcNow;

        var previousDriverPositions = await _context.DriverStandings
            .Where(standing => standing.SeasonId == seasonId && standing.AfterEventId == null)
            .ToDictionaryAsync(
                standing => (standing.DriverId, standing.VehicleClassId),
                standing => standing.Position,
                cancellationToken);

        var previousTeamPositions = await _context.TeamStandings
            .Where(standing => standing.SeasonId == seasonId && standing.AfterEventId == null)
            .ToDictionaryAsync(
                standing => (standing.TeamId, standing.VehicleClassId),
                standing => standing.Position,
                cancellationToken);

        await _context.DriverStandings
            .Where(standing => standing.SeasonId == seasonId && standing.AfterEventId == null)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.TeamStandings
            .Where(standing => standing.SeasonId == seasonId && standing.AfterEventId == null)
            .ExecuteDeleteAsync(cancellationToken);

        // Two tables are produced, not one. The outright championship uses a null class and is
        // what a caller gets by default; a multi-class series additionally gets one table per
        // class. Without the outright rows a WEC season would have standings that no default
        // query could find, and without the per-class rows an LMP2 crew would be ranked against
        // Hypercars they were never competing with.
        var driverPartitions = BuildPartitions(rows);

        var driverStandings = driverPartitions
            .SelectMany(partition =>
            {
                var standings = partition.Rows
                    .GroupBy(row => row.DriverId)
                    .Select(group => new DriverStanding
                    {
                        SeasonId = seasonId,
                        DriverId = group.Key,
                        VehicleClassId = partition.VehicleClassId,
                        AfterEventId = null,
                        Points = group.Sum(row => row.Points),
                        PointsBeforeDrops = group.Sum(row => row.Points),
                        Starts = group.Count(row => row.SessionType == SessionType.Race),
                        Wins = group.Count(row => partition.RankingPosition(row) == 1 && row.SessionType == SessionType.Race),
                        Podiums = group.Count(row => partition.RankingPosition(row) <= 3 && row.SessionType == SessionType.Race),
                        PolePositions = group.Count(row => row.IsPolePosition),
                        FastestLaps = group.Count(row => row.IsFastestLap),
                        Retirements = group.Count(row => row.Status == ResultStatus.Retired),
                        PreviousPosition = previousDriverPositions.TryGetValue((group.Key, partition.VehicleClassId), out var previous)
                            ? previous
                            : null,
                        RecalculatedAtUtc = now
                    })
                    .ToList();

                // Positions are assigned inside a partition, so a class winner is P1 of that
                // class rather than of the whole field.
                AssignPositions(standings);
                return standings;
            })
            .ToList();

        var teamStandings = driverPartitions
            .SelectMany(partition =>
            {
                var standings = partition.Rows
                    .GroupBy(row => row.TeamId)
                    .Select(group => new TeamStanding
                    {
                        SeasonId = seasonId,
                        TeamId = group.Key,
                        VehicleClassId = partition.VehicleClassId,
                        AfterEventId = null,
                        Points = group.Sum(row => row.Points),
                        PointsBeforeDrops = group.Sum(row => row.Points),
                        Starts = group.Count(row => row.SessionType == SessionType.Race),
                        Wins = group.Count(row => partition.RankingPosition(row) == 1 && row.SessionType == SessionType.Race),
                        Podiums = group.Count(row => partition.RankingPosition(row) <= 3 && row.SessionType == SessionType.Race),
                        PolePositions = group.Count(row => row.IsPolePosition),
                        FastestLaps = group.Count(row => row.IsFastestLap),
                        PreviousPosition = previousTeamPositions.TryGetValue((group.Key, partition.VehicleClassId), out var previous)
                            ? previous
                            : null,
                        RecalculatedAtUtc = now
                    })
                    .ToList();

                AssignPositions(standings);
                return standings;
            })
            .ToList();

        _context.DriverStandings.AddRange(driverStandings);
        _context.TeamStandings.AddRange(teamStandings);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <summary>
    /// Splits the scoring rows into the partitions a season needs a table for: always the
    /// outright championship, plus one per class when the series runs more than one.
    /// </summary>
    private static List<StandingsPartition> BuildPartitions(List<ScoringRow> rows)
    {
        var partitions = new List<StandingsPartition>
        {
            new(null, rows)
        };

        partitions.AddRange(rows
            .Where(row => row.VehicleClassId is not null)
            .GroupBy(row => row.VehicleClassId)
            .Select(group => new StandingsPartition(group.Key, [.. group])));

        return partitions;
    }

    /// <summary>One championship table's worth of scoring rows.</summary>
    private sealed record StandingsPartition(Guid? VehicleClassId, List<ScoringRow> Rows)
    {
        /// <summary>
        /// The position that counts in this table. An outright table ranks on the overall
        /// finishing position; a class table ranks on the class position, so an LMP2 crew that
        /// won its class is credited with a win even though a Hypercar finished ahead of it.
        /// </summary>
        public int? RankingPosition(ScoringRow row)
            => VehicleClassId is null ? row.Position : row.ClassPosition ?? row.Position;
    }

    /// <summary>
    /// Orders a table and stamps positions. The tie-break is countback — most wins, then most
    /// second places, and so on — which is what motorsport regulations almost universally use,
    /// rather than leaving two competitors sharing a position.
    /// </summary>
    private static void AssignPositions<T>(List<T> standings)
        where T : class
    {
        var ordered = standings
            .OrderByDescending(GetPoints)
            .ThenByDescending(GetWins)
            .ThenByDescending(GetPodiums)
            .ThenByDescending(GetPoles)
            .ToList();

        for (var index = 0; index < ordered.Count; index++)
        {
            SetPosition(ordered[index], index + 1);
        }

        static decimal GetPoints(T standing) => standing switch
        {
            DriverStanding driver => driver.Points,
            TeamStanding team => team.Points,
            _ => 0
        };

        static int GetWins(T standing) => standing switch
        {
            DriverStanding driver => driver.Wins,
            TeamStanding team => team.Wins,
            _ => 0
        };

        static int GetPodiums(T standing) => standing switch
        {
            DriverStanding driver => driver.Podiums,
            TeamStanding team => team.Podiums,
            _ => 0
        };

        static int GetPoles(T standing) => standing switch
        {
            DriverStanding driver => driver.PolePositions,
            TeamStanding team => team.PolePositions,
            _ => 0
        };

        static void SetPosition(T standing, int position)
        {
            switch (standing)
            {
                case DriverStanding driver:
                    driver.Position = position;
                    break;
                case TeamStanding team:
                    team.Position = position;
                    break;
            }
        }
    }

    private async Task<Dictionary<Guid, List<string>>> LoadTeamsByDriverAsync(
        Guid seasonId,
        CancellationToken cancellationToken)
    {
        var rows = await _context.EventEntryDrivers
            .Where(entryDriver => entryDriver.EventEntry.Entry.SeasonId == seasonId)
            .Select(entryDriver => new
            {
                entryDriver.DriverId,
                TeamName = entryDriver.EventEntry.Entry.Team.Name
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(row => row.DriverId)
            .ToDictionary(group => group.Key, group => group.Select(row => row.TeamName).ToList());
    }

    /// <summary>Flat shape the standings rebuild aggregates over.</summary>
    private sealed class ScoringRow
    {
        public Guid DriverId { get; init; }
        public Guid TeamId { get; init; }
        public Guid? VehicleClassId { get; init; }
        public decimal Points { get; init; }
        public int? Position { get; init; }
        public int? ClassPosition { get; init; }
        public bool IsFastestLap { get; init; }
        public bool IsPolePosition { get; init; }
        public ResultStatus Status { get; init; }
        public SessionType SessionType { get; init; }
    }
}
