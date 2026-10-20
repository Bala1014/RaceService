using FluentAssertions;
using RaceService.Application.Domain.Entities.Catalog;
using RaceService.Application.Domain.Entities.Competitors;
using RaceService.Application.Domain.Entities.Events;
using RaceService.Application.Domain.Entities.Results;
using RaceService.Application.Services;
using RaceService.Contracts.Enums;

namespace RaceService.Tests.Services;

/// <summary>
/// Covers the scoring rules that are easy to get subtly wrong and expensive to get wrong in
/// public: distance thresholds, bonus eligibility and how a crew shares a car's points.
/// </summary>
public class PointsCalculatorTests
{
    private readonly PointsCalculator _calculator = new();

    [Fact]
    public void Awards_points_by_finishing_position()
    {
        var session = BuildSession(
            (position: 1, laps: 78, fastestLap: false, drivers: 1),
            (position: 2, laps: 78, fastestLap: false, drivers: 1),
            (position: 3, laps: 78, fastestLap: false, drivers: 1));

        _calculator.Apply(session, StandardScheme());

        session.Results.Select(result => result.PointsScored).Should().Equal(25m, 18m, 15m);
    }

    [Fact]
    public void Awards_nothing_below_the_positions_the_scheme_pays()
    {
        var session = BuildSession((position: 11, laps: 78, fastestLap: false, drivers: 1));

        _calculator.Apply(session, StandardScheme());

        session.Results.Single().PointsScored.Should().Be(0m);
    }

    [Fact]
    public void Withholds_points_from_a_car_short_of_the_minimum_distance()
    {
        // The leader ran 78 laps; 50 is under the 90% the scheme demands, so a car classified
        // second on the road still scores nothing.
        var session = BuildSession(
            (position: 1, laps: 78, fastestLap: false, drivers: 1),
            (position: 2, laps: 50, fastestLap: false, drivers: 1));

        _calculator.Apply(session, StandardScheme());

        session.Results.Last().PointsScored.Should().Be(0m);
    }

    [Fact]
    public void Adds_the_fastest_lap_bonus_only_within_the_eligible_positions()
    {
        var scheme = StandardScheme();
        scheme.FastestLapPoints = 1m;
        scheme.FastestLapMaxPosition = 10;

        var eligible = BuildSession((position: 3, laps: 78, fastestLap: true, drivers: 1));
        _calculator.Apply(eligible, scheme);
        eligible.Results.Single().PointsScored.Should().Be(16m, "15 for third plus the bonus");

        var ineligible = BuildSession((position: 11, laps: 78, fastestLap: true, drivers: 1));
        _calculator.Apply(ineligible, scheme);
        ineligible.Results.Single().PointsScored.Should().Be(0m, "the bonus does not apply outside the top ten");
    }

    [Fact]
    public void Applies_the_multiplier_for_a_double_points_round()
    {
        var scheme = StandardScheme();
        scheme.PointsMultiplier = 2m;

        var session = BuildSession((position: 1, laps: 78, fastestLap: false, drivers: 1));

        _calculator.Apply(session, scheme);

        session.Results.Single().PointsScored.Should().Be(50m);
    }

    [Fact]
    public void Gives_every_qualifying_crew_member_the_cars_full_points()
    {
        var session = BuildSession((position: 1, laps: 300, fastestLap: false, drivers: 3));
        var scheme = StandardScheme();
        scheme.MinimumDriverDistancePercent = 15m;

        foreach (var driver in session.Results.Single().Drivers)
        {
            driver.LapsDriven = 100;
        }

        _calculator.Apply(session, scheme);

        session.Results.Single().Drivers.Should().OnlyContain(driver =>
            driver.PointsScored == 25m && driver.IsEligibleForPoints);
    }

    [Fact]
    public void Withholds_points_from_a_driver_who_did_not_meet_the_minimum_drive_time()
    {
        var session = BuildSession((position: 1, laps: 300, fastestLap: false, drivers: 3));
        var scheme = StandardScheme();
        scheme.MinimumDriverDistancePercent = 15m;

        var drivers = session.Results.Single().Drivers.ToList();
        drivers[0].LapsDriven = 150;
        drivers[1].LapsDriven = 140;
        // Ten of three hundred laps is well under fifteen percent.
        drivers[2].LapsDriven = 10;

        _calculator.Apply(session, scheme);

        drivers[0].PointsScored.Should().Be(25m);
        drivers[1].PointsScored.Should().Be(25m);
        drivers[2].PointsScored.Should().Be(0m);
        drivers[2].IsEligibleForPoints.Should().BeFalse();
    }

    [Fact]
    public void Awards_nothing_for_a_non_scoring_session()
    {
        var session = BuildSession((position: 1, laps: 78, fastestLap: false, drivers: 1));
        session.IsPointsScoring = false;

        _calculator.Apply(session, StandardScheme());

        session.Results.Single().PointsScored.Should().Be(0m);
    }

    [Fact]
    public void Awards_nothing_when_no_scheme_applies()
    {
        var session = BuildSession((position: 1, laps: 78, fastestLap: false, drivers: 1));

        _calculator.Apply(session, null);

        session.Results.Single().PointsScored.Should().Be(0m);
    }

    [Fact]
    public void Awards_nothing_to_a_car_declared_ineligible_for_the_championship()
    {
        var session = BuildSession((position: 1, laps: 78, fastestLap: false, drivers: 1));
        session.Results.Single().EventEntry.IsEligibleForPoints = false;

        _calculator.Apply(session, StandardScheme());

        session.Results.Single().PointsScored.Should().Be(0m);
    }

    private static PointsScheme StandardScheme() => new()
    {
        AppliesToSessionType = SessionType.Race,
        ChampionshipType = ChampionshipType.Drivers,
        MinimumDistancePercent = 90m,
        PointsMultiplier = 1m,
        Entries =
        [
            new() { Position = 1, Points = 25m },
            new() { Position = 2, Points = 18m },
            new() { Position = 3, Points = 15m },
            new() { Position = 4, Points = 12m },
            new() { Position = 5, Points = 10m }
        ]
    };

    private static Session BuildSession(params (int position, int laps, bool fastestLap, int drivers)[] rows)
    {
        var session = new Session
        {
            Type = SessionType.Race,
            IsPointsScoring = true
        };

        foreach (var row in rows)
        {
            var result = new SessionResult
            {
                Session = session,
                EventEntry = new EventEntry { IsEligibleForPoints = true },
                Position = row.position,
                LapsCompleted = row.laps,
                IsClassified = true,
                IsFastestLap = row.fastestLap
            };

            for (var i = 0; i < row.drivers; i++)
            {
                result.Drivers.Add(new SessionResultDriver
                {
                    SessionResult = result,
                    DriverId = Guid.CreateVersion7(),
                    LapsDriven = row.laps / row.drivers
                });
            }

            session.Results.Add(result);
        }

        return session;
    }
}
