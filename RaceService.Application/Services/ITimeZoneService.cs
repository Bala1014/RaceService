using RaceService.Contracts.Common;
using TimeZoneConverter;

namespace RaceService.Application.Services;

/// <summary>Validates IANA time zones and projects UTC instants into them.</summary>
public interface ITimeZoneService
{
    /// <summary>Reports whether an IANA identifier is one this machine can resolve.</summary>
    bool IsValid(string? timeZoneId);

    /// <summary>Resolves an IANA identifier, returning null when it is unknown.</summary>
    TimeZoneInfo? Resolve(string? timeZoneId);

    /// <summary>Expresses one UTC instant at the venue and, optionally, for the viewer.</summary>
    LocalizedTime Localize(DateTime utc, string? venueTimeZoneId, string? viewerTimeZoneId);
}

/// <inheritdoc />
/// <remarks>
/// Resolution goes through TimeZoneConverter because Windows and Linux disagree about time zone
/// identifiers: "Europe/Monaco" is unknown to a stock Windows machine and "W. Europe Standard
/// Time" is unknown to Linux. Since development happens on one and Cloud Run on the other, going
/// direct to <see cref="TimeZoneInfo.FindSystemTimeZoneById"/> would work locally and fail in
/// production, or the reverse.
/// </remarks>
public sealed class TimeZoneService : ITimeZoneService
{
    // Resolution walks the tz database on disk, so results are cached. The set of zones is small
    // and fixed for the life of the process.
    private readonly Dictionary<string, TimeZoneInfo?> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _gate = new();

    /// <inheritdoc />
    public bool IsValid(string? timeZoneId) => Resolve(timeZoneId) is not null;

    /// <inheritdoc />
    public TimeZoneInfo? Resolve(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return null;
        }

        lock (_gate)
        {
            if (_cache.TryGetValue(timeZoneId, out var cached))
            {
                return cached;
            }

            TimeZoneInfo? resolved = null;

            if (TZConvert.TryGetTimeZoneInfo(timeZoneId, out var zone))
            {
                resolved = zone;
            }

            _cache[timeZoneId] = resolved;
            return resolved;
        }
    }

    /// <inheritdoc />
    public LocalizedTime Localize(DateTime utc, string? venueTimeZoneId, string? viewerTimeZoneId)
    {
        var utcInstant = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        var venueZone = Resolve(venueTimeZoneId);
        var viewerZone = Resolve(viewerTimeZoneId);

        return new LocalizedTime
        {
            Utc = utcInstant,
            AtVenue = venueZone is null ? null : TimeZoneInfo.ConvertTime(new DateTimeOffset(utcInstant), venueZone),
            VenueTimeZone = venueZone is null ? null : venueTimeZoneId,
            AtViewer = viewerZone is null ? null : TimeZoneInfo.ConvertTime(new DateTimeOffset(utcInstant), viewerZone),
            ViewerTimeZone = viewerZone is null ? null : viewerTimeZoneId
        };
    }
}
