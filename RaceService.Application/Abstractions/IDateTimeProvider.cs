namespace RaceService.Application.Abstractions;

/// <summary>
/// Supplies the current time. Injected rather than calling <c>DateTime.UtcNow</c> directly so
/// that scheduling logic — which race is next, has this session started — can be tested at a
/// fixed instant instead of depending on when the test happens to run.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

/// <inheritdoc />
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
