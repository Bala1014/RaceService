using System.Globalization;

namespace RaceService.Application.Common;

/// <summary>
/// Renders millisecond durations the way timing screens do.
/// </summary>
/// <remarks>
/// Formatting lives on the server so that every client — web, mobile, an embedded widget —
/// shows "1:27.456" identically. Leaving it to callers reliably produces three different
/// roundings of the same lap.
/// </remarks>
public static class TimeFormatting
{
    /// <summary>Formats a lap time as m:ss.fff, e.g. "1:27.456".</summary>
    public static string? ToLapTime(int? milliseconds)
    {
        if (milliseconds is null or < 0)
        {
            return null;
        }

        var time = TimeSpan.FromMilliseconds(milliseconds.Value);

        return time.TotalHours >= 1
            ? string.Format(CultureInfo.InvariantCulture, "{0}:{1:D2}:{2:D2}.{3:D3}", (int)time.TotalHours, time.Minutes, time.Seconds, time.Milliseconds)
            : string.Format(CultureInfo.InvariantCulture, "{0}:{1:D2}.{2:D3}", (int)time.TotalMinutes, time.Seconds, time.Milliseconds);
    }

    /// <summary>Formats a total race time as h:mm:ss.fff.</summary>
    public static string? ToTotalTime(long? milliseconds)
    {
        if (milliseconds is null or < 0)
        {
            return null;
        }

        var time = TimeSpan.FromMilliseconds(milliseconds.Value);

        return string.Format(
            CultureInfo.InvariantCulture,
            "{0}:{1:D2}:{2:D2}.{3:D3}",
            (int)time.TotalHours,
            time.Minutes,
            time.Seconds,
            time.Milliseconds);
    }

    /// <summary>
    /// Formats the gap column of a classification. A car on the lead lap shows a time; one that
    /// is lapped shows the lap count, because a time gap is meaningless once you are a lap down.
    /// </summary>
    public static string? ToGap(int? position, long? gapMs, int? gapLaps)
    {
        if (position == 1)
        {
            return null;
        }

        if (gapLaps is > 0)
        {
            return gapLaps == 1 ? "+1 lap" : $"+{gapLaps} laps";
        }

        if (gapMs is null or < 0)
        {
            return null;
        }

        var time = TimeSpan.FromMilliseconds(gapMs.Value);

        return time.TotalMinutes >= 1
            ? string.Format(CultureInfo.InvariantCulture, "+{0}:{1:D2}.{2:D3}", (int)time.TotalMinutes, time.Seconds, time.Milliseconds)
            : string.Format(CultureInfo.InvariantCulture, "+{0}.{1:D3}", time.Seconds, time.Milliseconds);
    }

    /// <summary>Formats a pit stop duration as s.fff, e.g. "2.412".</summary>
    public static string? ToStopTime(int? milliseconds)
        => milliseconds is null or < 0
            ? null
            : string.Format(CultureInfo.InvariantCulture, "{0:F3}", milliseconds.Value / 1000m);
}
