using RaceService.Contracts.Enums;

namespace RaceService.Application.Database;

/// <summary>
/// Shape of <c>SeedData/seed.json</c>.
/// </summary>
/// <remarks>
/// Deliberately separate from the domain entities. The seed file is an input format that should
/// be pleasant to hand-edit — short keys, nesting, no GUIDs — and coupling it to the entities
/// would mean every rename in the domain silently invalidates the file.
/// </remarks>
public sealed class SeedDocument
{
    public List<ManufacturerSeed> Manufacturers { get; set; } = [];
    public List<TrackSeed> Tracks { get; set; } = [];
    public List<TeamSeed> Teams { get; set; } = [];
    public List<DriverSeed> Drivers { get; set; } = [];
    public List<SeriesSeed> Series { get; set; } = [];
}

/// <summary>A marque in the seed file.</summary>
public sealed class ManufacturerSeed
{
    /// <summary>Local reference key used elsewhere in the file, e.g. "ferrari".</summary>
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? CountryCode { get; set; }
    public bool SuppliesChassis { get; set; }
    public bool SuppliesEngines { get; set; }
    public bool SuppliesTyres { get; set; }
}

/// <summary>A venue and its layouts.</summary>
public sealed class TrackSeed
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? OfficialName { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? City { get; set; }
    public string Country { get; set; } = string.Empty;
    public string? CountryCode { get; set; }
    public string TimeZoneId { get; set; } = "UTC";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? OpenedYear { get; set; }
    public string? ImageUrl { get; set; }
    public List<TrackLayoutSeed> Layouts { get; set; } = [];
}

/// <summary>One configuration of a venue.</summary>
public sealed class TrackLayoutSeed
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int LengthMeters { get; set; }
    public int? NumberOfTurns { get; set; }
    public TrackDirection Direction { get; set; } = TrackDirection.Clockwise;
    public TrackType TrackType { get; set; } = TrackType.PermanentCircuit;
    public bool IsCurrent { get; set; } = true;
    public List<LapRecordSeed> LapRecords { get; set; } = [];
}

/// <summary>A competing organisation.</summary>
public sealed class TeamSeed
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? OfficialName { get; set; }
    public string ShortName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? CountryCode { get; set; }
    public string? BaseCity { get; set; }
    public string? BaseCountry { get; set; }
    public int? FoundedYear { get; set; }
    public string? PrimaryColorHex { get; set; }
}

/// <summary>A competitor.</summary>
public sealed class DriverSeed
{
    public string Key { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Abbreviation { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? CountryCode { get; set; }
    public string? Nationality { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? PlaceOfBirth { get; set; }
    public int? PermanentNumber { get; set; }
}

/// <summary>A championship and everything under it.</summary>
public sealed class SeriesSeed
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Discipline Discipline { get; set; }
    public string? SanctioningBody { get; set; }
    public string? Description { get; set; }
    public string? PrimaryColorHex { get; set; }
    public int? InauguralYear { get; set; }
    public int DisplayOrder { get; set; }
    public List<VehicleClassSeed> VehicleClasses { get; set; } = [];
    public List<SeasonSeed> Seasons { get; set; } = [];
}

/// <summary>A class within a series.</summary>
public sealed class VehicleClassSeed
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>One running of a championship.</summary>
public sealed class SeasonSeed
{
    public int Year { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public SeasonStatus Status { get; set; } = SeasonStatus.Announced;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? PlannedRoundCount { get; set; }
    public List<PointsSchemeSeed> PointsSchemes { get; set; } = [];
    public List<EntrySeed> Entries { get; set; } = [];
    public List<RaceEventSeed> Events { get; set; } = [];
}

/// <summary>A points table.</summary>
public sealed class PointsSchemeSeed
{
    public string Name { get; set; } = string.Empty;
    public SessionType AppliesToSessionType { get; set; } = SessionType.Race;
    public ChampionshipType ChampionshipType { get; set; } = ChampionshipType.Drivers;
    public decimal FastestLapPoints { get; set; }
    public int? FastestLapMaxPosition { get; set; }
    public decimal PolePositionPoints { get; set; }
    public decimal MinimumDistancePercent { get; set; }
    public decimal MinimumDriverDistancePercent { get; set; }

    /// <summary>Position-to-points map, keyed by position as a string, e.g. <c>{ "1": 25 }</c>.</summary>
    public Dictionary<string, decimal> Positions { get; set; } = [];
}

/// <summary>A car registered for a season.</summary>
public sealed class EntrySeed
{
    public string Key { get; set; } = string.Empty;
    public string TeamKey { get; set; } = string.Empty;
    public string CarNumber { get; set; } = string.Empty;
    public string EntrantName { get; set; } = string.Empty;
    public string? VehicleClassKey { get; set; }
    public string? ChassisManufacturerKey { get; set; }
    public string? ChassisModel { get; set; }
    public string? EngineManufacturerKey { get; set; }
    public string? EngineModel { get; set; }
    public string? TyreSupplierKey { get; set; }
    public EntryStatus Status { get; set; } = EntryStatus.Confirmed;
}

/// <summary>A race weekend.</summary>
public sealed class RaceEventSeed
{
    public string Name { get; set; } = string.Empty;
    public string? OfficialName { get; set; }
    public string? ShortName { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string TrackLayoutKey { get; set; } = string.Empty;
    public int? RoundNumber { get; set; }
    public string? TimeZoneId { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Scheduled;
    public string? ShortDescription { get; set; }
    public int? ScheduledDistanceMeters { get; set; }
    public int? ScheduledDurationMinutes { get; set; }
    public bool HasSprint { get; set; }
    public bool IsPublished { get; set; } = true;
    public List<EventEntrySeed> Entries { get; set; } = [];
    public List<SessionSeed> Sessions { get; set; } = [];
}

/// <summary>A car taking part in one round.</summary>
public sealed class EventEntrySeed
{
    public string EntryKey { get; set; } = string.Empty;
    public EntryStatus Status { get; set; } = EntryStatus.Confirmed;
    public int? StartingGridPosition { get; set; }
    public List<string> DriverKeys { get; set; } = [];
}

/// <summary>One piece of on-track running.</summary>
public sealed class SessionSeed
{
    public SessionType Type { get; set; }
    public int SequenceNumber { get; set; } = 1;
    public string Name { get; set; } = string.Empty;
    public DateTime ScheduledStartUtc { get; set; }
    public DateTime? ScheduledEndUtc { get; set; }
    public int? ScheduledDurationMinutes { get; set; }
    public int? ScheduledLaps { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Scheduled;
    public bool IsPointsScoring { get; set; }
    public ResultsStatus ResultsStatus { get; set; } = ResultsStatus.NotAvailable;
    public List<SessionResultSeed> Results { get; set; } = [];
    public List<SessionLapSeed> Laps { get; set; } = [];
    public List<PitStopSeed> PitStops { get; set; } = [];
    public List<PenaltySeed> Penalties { get; set; } = [];
}

/// <summary>A classification row.</summary>
public sealed class SessionResultSeed
{
    public string EntryKey { get; set; } = string.Empty;
    public List<string> DriverKeys { get; set; } = [];
    public int? Position { get; set; }
    public int? ClassPosition { get; set; }
    public int? StartingPosition { get; set; }
    public int LapsCompleted { get; set; }
    public int LapsLed { get; set; }
    public long? TotalTimeMs { get; set; }
    public long? GapToLeaderMs { get; set; }
    public int? GapToLeaderLaps { get; set; }
    public int? BestLapTimeMs { get; set; }
    public int? BestLapNumber { get; set; }
    public ResultStatus Status { get; set; } = ResultStatus.Classified;
    public string? StatusDetail { get; set; }
    public bool IsClassified { get; set; } = true;
    public decimal PointsScored { get; set; }
    public bool IsFastestLap { get; set; }
    public bool IsPolePosition { get; set; }
    public int PitStopCount { get; set; }
}

/// <summary>One car's timed lap within a session.</summary>
public sealed class SessionLapSeed
{
    public string EntryKey { get; set; } = string.Empty;
    public string? DriverKey { get; set; }
    public int LapNumber { get; set; }
    public int? LapTimeMs { get; set; }
    public int? Sector1Ms { get; set; }
    public int? Sector2Ms { get; set; }
    public int? Sector3Ms { get; set; }
    public int? Position { get; set; }
    public bool IsPitInLap { get; set; }
    public bool IsPitOutLap { get; set; }
    public bool IsPersonalBest { get; set; }
    public bool IsSessionBest { get; set; }
}

/// <summary>One visit to the pit lane.</summary>
public sealed class PitStopSeed
{
    public string EntryKey { get; set; } = string.Empty;
    public int StopNumber { get; set; } = 1;
    public int LapNumber { get; set; }
    public int? StationaryTimeMs { get; set; }
    public int? TotalPitLaneTimeMs { get; set; }
    public string? TyreCompoundFitted { get; set; }
    public bool RefuelledStop { get; set; }
    public bool DriverChanged { get; set; }
    public string? DriverInKey { get; set; }
    public string? DriverOutKey { get; set; }
}

/// <summary>A stewards' decision against a car or driver.</summary>
public sealed class PenaltySeed
{
    public string EntryKey { get; set; } = string.Empty;
    public string? DriverKey { get; set; }
    public PenaltyType Type { get; set; }
    public int? TimePenaltySeconds { get; set; }
    public int? GridPositionDrop { get; set; }
    public string? Reason { get; set; }
    public int? LapNumber { get; set; }
    public bool IsServed { get; set; }
}

/// <summary>A standing circuit record, independent of any one session.</summary>
public sealed class LapRecordSeed
{
    public RecordType RecordType { get; set; }
    public int LapTimeMs { get; set; }
    public string? SeriesKey { get; set; }
    public string? VehicleClassKey { get; set; }
    public string? DriverKey { get; set; }
    public string? TeamKey { get; set; }
    public DateOnly SetOn { get; set; }
    public string? VehicleDescription { get; set; }
    public bool IsCurrent { get; set; } = true;
}
