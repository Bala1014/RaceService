using RaceService.Application.Domain.Entities.Catalog;
using RaceService.Application.Domain.Entities.Events;
using RaceService.Application.Domain.Entities.Results;

namespace RaceService.Application.Services;

/// <summary>Awards championship points to a session's classification.</summary>
public interface IPointsCalculator
{
    /// <summary>
    /// Writes <c>PointsScored</c> onto every result and per-driver row for a session, from the
    /// season's points scheme.
    /// </summary>
    /// <param name="session">Session being scored. Must have its results loaded.</param>
    /// <param name="scheme">Scheme for this session type, or null if none applies.</param>
    void Apply(Session session, PointsScheme? scheme);
}

/// <inheritdoc />
/// <remarks>
/// Points are computed rather than typed in because the regulations are the source of truth and
/// a human transcribing a points column will eventually differ from them. Recomputing also means
/// that when the stewards amend a result the whole championship follows automatically.
/// </remarks>
public sealed class PointsCalculator : IPointsCalculator
{
    /// <inheritdoc />
    public void Apply(Session session, PointsScheme? scheme)
    {
        if (scheme is null || !session.IsPointsScoring)
        {
            foreach (var result in session.Results)
            {
                result.PointsScored = 0;

                foreach (var driver in result.Drivers)
                {
                    driver.PointsScored = 0;
                }
            }

            return;
        }

        var table = scheme.Entries.ToDictionary(entry => entry.Position, entry => entry.Points);

        // Distance threshold is measured against the winner, not against the scheduled distance:
        // a race stopped early still has a legitimate leader to measure everyone else by.
        var leaderLaps = session.Results
            .Where(result => result.Position is not null)
            .OrderBy(result => result.Position)
            .Select(result => result.LapsCompleted)
            .FirstOrDefault();

        foreach (var result in session.Results)
        {
            result.PointsScored = CalculateForCar(result, scheme, table, leaderLaps);
            DistributeToDrivers(result, scheme, leaderLaps);
        }
    }

    private static decimal CalculateForCar(
        SessionResult result,
        PointsScheme scheme,
        IReadOnlyDictionary<int, decimal> table,
        int leaderLaps)
    {
        if (!result.IsClassified || result.Position is null)
        {
            return 0;
        }

        if (result.EventEntry is { IsEligibleForPoints: false })
        {
            return 0;
        }

        if (!MeetsDistance(result.LapsCompleted, leaderLaps, scheme.MinimumDistancePercent))
        {
            return 0;
        }

        var points = table.TryGetValue(result.Position.Value, out var positionPoints) ? positionPoints : 0m;

        points *= scheme.PointsMultiplier;

        if (result.IsFastestLap
            && scheme.FastestLapPoints > 0
            && (scheme.FastestLapMaxPosition is null || result.Position <= scheme.FastestLapMaxPosition))
        {
            points += scheme.FastestLapPoints;
        }

        if (result.IsPolePosition && scheme.PolePositionPoints > 0)
        {
            points += scheme.PolePositionPoints;
        }

        return points;
    }

    /// <summary>
    /// Splits a car's points among its crew. Each qualifying driver takes the full amount rather
    /// than a share — that is how endurance championships work, and it is why a co-driver who did
    /// not meet the minimum drive time scores nothing while their team-mates score everything.
    /// </summary>
    private static void DistributeToDrivers(SessionResult result, PointsScheme scheme, int leaderLaps)
    {
        foreach (var driver in result.Drivers)
        {
            var eligible = result.PointsScored > 0
                && MeetsDistance(driver.LapsDriven, leaderLaps, scheme.MinimumDriverDistancePercent);

            driver.IsEligibleForPoints = eligible;
            driver.PointsScored = eligible ? result.PointsScored : 0;
        }
    }

    private static bool MeetsDistance(int laps, int leaderLaps, decimal minimumPercent)
    {
        if (minimumPercent <= 0 || leaderLaps <= 0)
        {
            return true;
        }

        return laps * 100m / leaderLaps >= minimumPercent;
    }
}
