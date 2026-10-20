namespace RaceService.Contracts.Enums;

/// <summary>
/// Which category a circuit lap record belongs to. An outright qualifying lap and the
/// fastest race lap are different records and must not overwrite each other.
/// </summary>
public enum RecordType
{
    Unknown = 0,
    /// <summary>Fastest lap ever set on the layout, in any session.</summary>
    Outright = 1,
    RaceLap = 2,
    QualifyingLap = 3
}
