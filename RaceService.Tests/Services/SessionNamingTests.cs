using FluentAssertions;
using RaceService.Application.Services;
using RaceService.Contracts.Enums;

namespace RaceService.Tests.Services;

/// <summary>Session names must read the same however the session was created.</summary>
public class SessionNamingTests
{
    [Theory]
    [InlineData(SessionType.Practice, 1, "Practice 1")]
    [InlineData(SessionType.Practice, 3, "Practice 3")]
    [InlineData(SessionType.Qualifying, 1, "Qualifying")]
    [InlineData(SessionType.Qualifying, 3, "Qualifying 3")]
    [InlineData(SessionType.Race, 1, "Race")]
    [InlineData(SessionType.Sprint, 1, "Sprint")]
    [InlineData(SessionType.Hyperpole, 1, "Hyperpole")]
    [InlineData(SessionType.WarmUp, 1, "Warm Up")]
    public void Derives_the_conventional_name(SessionType type, int sequence, string expected)
        => SessionNaming.Derive(type, sequence).Should().Be(expected);
}
