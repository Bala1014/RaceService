namespace RaceService.Contracts.Common;

/// <summary>
/// One instant expressed three ways: the canonical UTC value, the time at the circuit, and
/// optionally the time in the caller's own zone.
/// </summary>
/// <remarks>
/// Both local views are returned as offset-carrying values rather than naive local timestamps,
/// so a client can tell a UTC+1 winter start from a UTC+2 summer one without consulting a
/// tz database of its own.
/// </remarks>
public sealed record LocalizedTime
{
    /// <summary>Canonical instant. This is the value to compare and sort on.</summary>
    public DateTime Utc { get; init; }

    /// <summary>The same instant at the circuit.</summary>
    public DateTimeOffset? AtVenue { get; init; }

    /// <summary>IANA zone <see cref="AtVenue"/> is expressed in.</summary>
    public string? VenueTimeZone { get; init; }

    /// <summary>The same instant in the zone the caller asked for, when they asked for one.</summary>
    public DateTimeOffset? AtViewer { get; init; }

    /// <summary>IANA zone <see cref="AtViewer"/> is expressed in.</summary>
    public string? ViewerTimeZone { get; init; }
}
