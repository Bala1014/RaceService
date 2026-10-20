namespace RaceService.Contracts.Common;

/// <summary>
/// Adds an optional display time zone to a read request.
/// </summary>
/// <remarks>
/// Storage is always UTC. This lets a caller ask for pre-converted local values so that a browser
/// in Melbourne and a mobile client in Monaco both render the same session correctly without
/// each reimplementing the conversion — and without the server ever storing a duplicated local
/// timestamp, which would go stale the moment a session is rescheduled or a country changes its
/// daylight saving rules.
/// </remarks>
public record TimeZoneAwareRequest : PagedRequest
{
    /// <summary>IANA time zone to render local times in, e.g. "Australia/Melbourne".</summary>
    public string? TimeZone { get; init; }
}
