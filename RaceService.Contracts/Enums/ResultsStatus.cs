namespace RaceService.Contracts.Enums;

/// <summary>
/// How much trust to place in the result rows attached to a session. Motorsport results are
/// routinely amended hours after the flag once stewards rule on protests, so a published
/// classification must say which stage it is at.
/// </summary>
public enum ResultsStatus
{
    /// <summary>No classification has been recorded yet.</summary>
    NotAvailable = 0,
    Provisional = 1,
    Official = 2,
    /// <summary>Official results later changed by the stewards.</summary>
    Amended = 3
}
