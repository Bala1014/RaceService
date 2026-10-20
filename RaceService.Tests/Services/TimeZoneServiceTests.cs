using FluentAssertions;
using RaceService.Application.Services;

namespace RaceService.Tests.Services;

/// <summary>
/// Showing the correct local start time is a headline requirement of the site, so the conversion
/// is tested across a daylight-saving boundary rather than only at a fixed offset.
/// </summary>
public class TimeZoneServiceTests
{
    private readonly TimeZoneService _timeZones = new();

    [Theory]
    [InlineData("Europe/Monaco")]
    [InlineData("Australia/Melbourne")]
    [InlineData("America/New_York")]
    [InlineData("UTC")]
    public void Accepts_known_iana_identifiers(string id)
        => _timeZones.IsValid(id).Should().BeTrue();

    [Theory]
    [InlineData("Mars/Olympus")]
    [InlineData("Not A Zone")]
    [InlineData("")]
    [InlineData(null)]
    public void Rejects_unknown_identifiers(string? id)
        => _timeZones.IsValid(id).Should().BeFalse();

    [Fact]
    public void Converts_a_summer_instant_to_the_venue_and_viewer_zones()
    {
        var utc = new DateTime(2026, 5, 24, 13, 0, 0, DateTimeKind.Utc);

        var localized = _timeZones.Localize(utc, "Europe/Monaco", "America/New_York");

        localized.Utc.Should().Be(utc);
        localized.AtVenue!.Value.Offset.Should().Be(TimeSpan.FromHours(2), "Monaco is on CEST in May");
        localized.AtVenue!.Value.Hour.Should().Be(15);
        localized.AtViewer!.Value.Offset.Should().Be(TimeSpan.FromHours(-4), "New York is on EDT in May");
        localized.AtViewer!.Value.Hour.Should().Be(9);
    }

    [Fact]
    public void Converts_a_winter_instant_using_the_standard_offset()
    {
        var utc = new DateTime(2026, 12, 6, 13, 0, 0, DateTimeKind.Utc);

        var localized = _timeZones.Localize(utc, "Europe/Monaco", "America/New_York");

        localized.AtVenue!.Value.Offset.Should().Be(TimeSpan.FromHours(1), "Monaco is on CET in December");
        localized.AtViewer!.Value.Offset.Should().Be(TimeSpan.FromHours(-5), "New York is on EST in December");
    }

    [Fact]
    public void Omits_the_viewer_view_when_no_zone_was_requested()
    {
        var localized = _timeZones.Localize(new DateTime(2026, 5, 24, 13, 0, 0, DateTimeKind.Utc), "Europe/Monaco", null);

        localized.AtVenue.Should().NotBeNull();
        localized.AtViewer.Should().BeNull();
        localized.ViewerTimeZone.Should().BeNull();
    }

    [Fact]
    public void Treats_an_unspecified_kind_as_utc_rather_than_local()
    {
        // A value read from the database arrives without a kind on some paths. Guessing "local"
        // there would shift every race time by the server's offset.
        var unspecified = new DateTime(2026, 5, 24, 13, 0, 0, DateTimeKind.Unspecified);

        var localized = _timeZones.Localize(unspecified, "UTC", null);

        localized.Utc.Hour.Should().Be(13);
    }
}
