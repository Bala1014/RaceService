using FluentAssertions;
using RaceService.Application.Common;

namespace RaceService.Tests.Services;

/// <summary>
/// Timing displays are formatted server-side precisely so every client agrees. These tests are
/// what stop that formatting drifting.
/// </summary>
public class TimeFormattingTests
{
    [Theory]
    [InlineData(70270, "1:10.270")]
    [InlineData(74231, "1:14.231")]
    [InlineData(205412, "3:25.412")]
    [InlineData(59999, "0:59.999")]
    public void Formats_a_lap_time_as_minutes_seconds_milliseconds(int ms, string expected)
        => TimeFormatting.ToLapTime(ms).Should().Be(expected);

    [Fact]
    public void Formats_a_lap_over_an_hour_with_an_hours_component()
        => TimeFormatting.ToLapTime(3_723_456).Should().Be("1:02:03.456");

    [Fact]
    public void Returns_null_for_a_missing_lap_time()
        => TimeFormatting.ToLapTime(null).Should().BeNull();

    [Theory]
    [InlineData(6234512, "1:43:54.512")]
    [InlineData(86412334, "24:00:12.334")]
    public void Formats_a_total_race_time_with_hours(long ms, string expected)
        => TimeFormatting.ToTotalTime(ms).Should().Be(expected);

    [Fact]
    public void Shows_no_gap_for_the_winner()
        => TimeFormatting.ToGap(1, null, null).Should().BeNull();

    [Theory]
    [InlineData(7614, "+7.614")]
    [InlineData(12480, "+12.480")]
    public void Shows_a_time_gap_for_a_car_on_the_lead_lap(long ms, string expected)
        => TimeFormatting.ToGap(2, ms, null).Should().Be(expected);

    [Fact]
    public void Shows_a_minutes_gap_when_the_deficit_is_large()
        => TimeFormatting.ToGap(5, 91_500, null).Should().Be("+1:31.500");

    [Theory]
    [InlineData(1, "+1 lap")]
    [InlineData(15, "+15 laps")]
    public void Prefers_a_lap_count_once_a_car_is_lapped(int laps, string expected)
        => TimeFormatting.ToGap(3, null, laps).Should().Be(expected);

    [Fact]
    public void Formats_a_pit_stop_to_three_decimal_places()
        => TimeFormatting.ToStopTime(2412).Should().Be("2.412");
}
