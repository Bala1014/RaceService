using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RaceService.Application.Database.Conventions;

/// <summary>
/// Forces every <see cref="DateTime"/> to be UTC on the way in and to come back tagged as UTC on
/// the way out.
/// </summary>
/// <remarks>
/// Npgsql maps <c>DateTime</c> to <c>timestamptz</c> and rejects a value whose <c>Kind</c> is
/// <c>Local</c>. More importantly, a value read back with <c>Kind.Unspecified</c> silently
/// misbehaves the moment anything converts it to a time zone. Pinning the kind at the boundary
/// means a session start time is unambiguous everywhere above this layer, which is the whole
/// premise of showing correct local start times to a worldwide audience.
/// </remarks>
public sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    /// <summary>Initialises the converter.</summary>
    public UtcDateTimeConverter()
        : base(
            value => value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime(),
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
    {
    }
}
