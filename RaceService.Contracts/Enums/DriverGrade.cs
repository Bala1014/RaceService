namespace RaceService.Contracts.Enums;

/// <summary>
/// FIA driver categorisation. Endurance and GT championships mandate a minimum crew
/// composition (an Am in every Pro-Am car, for example), so this belongs on the lineup.
/// </summary>
public enum DriverGrade
{
    NotCategorised = 0,
    Platinum = 1,
    Gold = 2,
    Silver = 3,
    Bronze = 4
}
