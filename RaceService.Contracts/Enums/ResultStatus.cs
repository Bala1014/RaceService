namespace RaceService.Contracts.Enums;

/// <summary>
/// Why a car appears where it does in the classification. Kept separate from the
/// free-text <c>StatusDetail</c> ("Gearbox", "Accident damage") so it stays queryable.
/// </summary>
public enum ResultStatus
{
    Unknown = 0,
    /// <summary>Ran the required race distance and holds a classified finishing position.</summary>
    Classified = 1,
    /// <summary>Reached the flag but short of the minimum distance, so not classified.</summary>
    NotClassified = 2,
    Retired = 3,
    DidNotStart = 4,
    DidNotQualify = 5,
    DidNotPractice = 6,
    Disqualified = 7,
    Withdrawn = 8
}
