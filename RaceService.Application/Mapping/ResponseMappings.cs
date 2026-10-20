using RaceService.Application.Common;
using RaceService.Application.Domain.Entities.Catalog;
using RaceService.Application.Domain.Entities.Competitors;
using RaceService.Application.Domain.Entities.Results;
using RaceService.Application.Domain.Entities.Venues;
using RaceService.Contracts.Responses;

namespace RaceService.Application.Mapping;

/// <summary>
/// Converts entities into response contracts.
/// </summary>
/// <remarks>
/// Hand-written rather than produced by a mapping library. At this size the reflection-based
/// alternative costs more than it saves: it hides which fields are exposed, which is precisely
/// the thing that matters when the difference between an entity and a DTO is what stops internal
/// state leaking out of the API.
/// </remarks>
public static class ResponseMappings
{
    /// <summary>Maps a venue to its list form.</summary>
    public static TrackSummaryResponse ToSummary(this Track track) => new()
    {
        Id = track.Id,
        Name = track.Name,
        Slug = track.Slug,
        City = track.City,
        Country = track.Country,
        CountryCode = track.CountryCode,
        TimeZoneId = track.TimeZoneId,
        ImageUrl = track.ImageUrl,
        CurrentLayoutLengthMeters = track.Layouts
            .Where(layout => layout.IsCurrent)
            .Select(layout => (int?)layout.LengthMeters)
            .FirstOrDefault()
    };

    /// <summary>Maps a venue to its detail form.</summary>
    public static TrackDetailResponse ToDetail(this Track track) => new()
    {
        Id = track.Id,
        Name = track.Name,
        OfficialName = track.OfficialName,
        Slug = track.Slug,
        City = track.City,
        Country = track.Country,
        CountryCode = track.CountryCode,
        TimeZoneId = track.TimeZoneId,
        Latitude = track.Latitude,
        Longitude = track.Longitude,
        OpenedYear = track.OpenedYear,
        Description = track.Description,
        ImageUrl = track.ImageUrl,
        WebsiteUrl = track.WebsiteUrl,
        Layouts = [.. track.Layouts.OrderByDescending(layout => layout.IsCurrent).ThenBy(layout => layout.Name).Select(ToResponse)]
    };

    /// <summary>Maps a layout.</summary>
    public static TrackLayoutResponse ToResponse(this TrackLayout layout) => new()
    {
        Id = layout.Id,
        TrackId = layout.TrackId,
        Name = layout.Name,
        Slug = layout.Slug,
        LengthMeters = layout.LengthMeters,
        LengthKm = Math.Round(layout.LengthMeters / 1000m, 3),
        NumberOfTurns = layout.NumberOfTurns,
        Direction = layout.Direction,
        TrackType = layout.TrackType,
        ElevationChangeMeters = layout.ElevationChangeMeters,
        LongestStraightMeters = layout.LongestStraightMeters,
        MapImageUrl = layout.MapImageUrl,
        IsCurrent = layout.IsCurrent,
        LapRecords = [.. layout.LapRecords.Where(record => record.IsCurrent).Select(ToResponse)]
    };

    /// <summary>Maps a lap record.</summary>
    public static LapRecordResponse ToResponse(this LapRecord record) => new()
    {
        Id = record.Id,
        RecordType = record.RecordType,
        LapTimeMs = record.LapTimeMs,
        LapTimeDisplay = TimeFormatting.ToLapTime(record.LapTimeMs) ?? string.Empty,
        SeriesId = record.SeriesId,
        SeriesName = record.Series?.Name,
        VehicleClassName = record.VehicleClass?.Name,
        DriverId = record.DriverId,
        DriverName = record.Driver?.FullName,
        TeamName = record.Team?.Name,
        VehicleDescription = record.VehicleDescription,
        SetOn = record.SetOn,
        IsCurrent = record.IsCurrent
    };

    /// <summary>Maps a class.</summary>
    public static VehicleClassResponse ToResponse(this VehicleClass vehicleClass) => new()
    {
        Id = vehicleClass.Id,
        SeriesId = vehicleClass.SeriesId,
        Name = vehicleClass.Name,
        ShortName = vehicleClass.ShortName,
        Slug = vehicleClass.Slug,
        Description = vehicleClass.Description,
        ColorHex = vehicleClass.ColorHex,
        DisplayOrder = vehicleClass.DisplayOrder
    };

    /// <summary>Maps a championship to its list form.</summary>
    public static SeriesSummaryResponse ToSummary(this Series series) => new()
    {
        Id = series.Id,
        Name = series.Name,
        ShortName = series.ShortName,
        Slug = series.Slug,
        Discipline = series.Discipline,
        SanctioningBody = series.SanctioningBody,
        LogoUrl = series.LogoUrl,
        PrimaryColorHex = series.PrimaryColorHex,
        IsActive = series.IsActive,
        LatestSeasonYear = series.Seasons.Count == 0 ? null : series.Seasons.Max(season => season.Year)
    };

    /// <summary>Maps a season to its list form.</summary>
    public static SeasonSummaryResponse ToSummary(this Season season) => new()
    {
        Id = season.Id,
        SeriesId = season.SeriesId,
        SeriesName = season.Series?.Name ?? string.Empty,
        SeriesSlug = season.Series?.Slug ?? string.Empty,
        Year = season.Year,
        Name = season.Name,
        Slug = season.Slug,
        Status = season.Status,
        StartDate = season.StartDate,
        EndDate = season.EndDate,
        RoundCount = season.Events.Count,
        PlannedRoundCount = season.PlannedRoundCount
    };

    /// <summary>Maps a competitor to its list form.</summary>
    public static DriverSummaryResponse ToSummary(this Driver driver) => new()
    {
        Id = driver.Id,
        FullName = driver.FullName,
        Abbreviation = driver.Abbreviation,
        Slug = driver.Slug,
        CountryCode = driver.CountryCode,
        Nationality = driver.Nationality,
        PermanentNumber = driver.PermanentNumber,
        HeadshotUrl = driver.HeadshotUrl,
        IsActive = driver.IsActive
    };

    /// <summary>Maps a competitor to its detail form.</summary>
    public static DriverDetailResponse ToDetail(this Driver driver, DateTime nowUtc) => new()
    {
        Id = driver.Id,
        FirstName = driver.FirstName,
        LastName = driver.LastName,
        FullName = driver.FullName,
        Abbreviation = driver.Abbreviation,
        Slug = driver.Slug,
        CountryCode = driver.CountryCode,
        Nationality = driver.Nationality,
        DateOfBirth = driver.DateOfBirth,
        DateOfDeath = driver.DateOfDeath,
        Age = CalculateAge(driver.DateOfBirth, driver.DateOfDeath, nowUtc),
        PlaceOfBirth = driver.PlaceOfBirth,
        PermanentNumber = driver.PermanentNumber,
        ImageUrl = driver.ImageUrl,
        HeadshotUrl = driver.HeadshotUrl,
        Biography = driver.Biography,
        WebsiteUrl = driver.WebsiteUrl,
        IsActive = driver.IsActive,
        CareerStats = [.. driver.CareerStats.Select(ToResponse)]
    };

    /// <summary>Maps career totals.</summary>
    public static DriverCareerStatResponse ToResponse(this DriverCareerStat stat) => new()
    {
        SeriesId = stat.SeriesId,
        SeriesName = stat.Series?.Name,
        Starts = stat.Starts,
        Wins = stat.Wins,
        Podiums = stat.Podiums,
        PolePositions = stat.PolePositions,
        FastestLaps = stat.FastestLaps,
        Retirements = stat.Retirements,
        LapsCompleted = stat.LapsCompleted,
        LapsLed = stat.LapsLed,
        CareerPoints = stat.CareerPoints,
        ChampionshipsWon = stat.ChampionshipsWon,
        BestFinishPosition = stat.BestFinishPosition,
        FirstSeasonYear = stat.FirstSeasonYear,
        LastSeasonYear = stat.LastSeasonYear,
        RecalculatedAtUtc = stat.RecalculatedAtUtc
    };

    /// <summary>Maps a team to its list form.</summary>
    public static TeamSummaryResponse ToSummary(this Team team) => new()
    {
        Id = team.Id,
        Name = team.Name,
        ShortName = team.ShortName,
        Slug = team.Slug,
        CountryCode = team.CountryCode,
        LogoUrl = team.LogoUrl,
        PrimaryColorHex = team.PrimaryColorHex,
        IsActive = team.IsActive
    };

    /// <summary>Maps a season entry.</summary>
    public static EntryResponse ToResponse(this Entry entry) => new()
    {
        Id = entry.Id,
        SeasonId = entry.SeasonId,
        SeasonYear = entry.Season?.Year ?? 0,
        SeriesName = entry.Season?.Series?.Name ?? string.Empty,
        CarNumber = entry.CarNumber,
        TeamId = entry.TeamId,
        TeamName = entry.Team?.Name ?? string.Empty,
        TeamLogoUrl = entry.Team?.LogoUrl,
        EntrantName = entry.EntrantName,
        VehicleClassId = entry.VehicleClassId,
        VehicleClassName = entry.VehicleClass?.Name,
        VehicleClassColorHex = entry.VehicleClass?.ColorHex,
        ChassisManufacturerName = entry.ChassisManufacturer?.Name,
        ChassisModel = entry.ChassisModel,
        EngineManufacturerName = entry.EngineManufacturer?.Name,
        EngineModel = entry.EngineModel,
        TyreSupplierName = entry.TyreSupplier?.Name,
        Status = entry.Status,
        LiveryImageUrl = entry.LiveryImageUrl,
        PrimaryColorHex = entry.PrimaryColorHex,
        IsEligibleForPoints = entry.IsEligibleForPoints
    };

    /// <summary>Maps a car at one round, including its crew.</summary>
    public static EventEntryResponse ToResponse(this EventEntry eventEntry) => new()
    {
        Id = eventEntry.Id,
        RaceEventId = eventEntry.RaceEventId,
        EntryId = eventEntry.EntryId,
        // The override wins where present; this is the only place the effective number is decided.
        CarNumber = eventEntry.CarNumberOverride ?? eventEntry.Entry?.CarNumber ?? string.Empty,
        TeamId = eventEntry.Entry?.TeamId ?? Guid.Empty,
        TeamName = eventEntry.Entry?.Team?.Name ?? string.Empty,
        TeamLogoUrl = eventEntry.Entry?.Team?.LogoUrl,
        EntrantName = eventEntry.Entry?.EntrantName ?? string.Empty,
        VehicleClassId = eventEntry.Entry?.VehicleClassId,
        VehicleClassName = eventEntry.Entry?.VehicleClass?.Name,
        VehicleClassColorHex = eventEntry.Entry?.VehicleClass?.ColorHex,
        ChassisModel = eventEntry.Entry?.ChassisModel,
        EngineModel = eventEntry.Entry?.EngineModel,
        Status = eventEntry.Status,
        StatusNote = eventEntry.StatusNote,
        StartingGridPosition = eventEntry.StartingGridPosition,
        StartsFromPitLane = eventEntry.StartsFromPitLane,
        IsEligibleForPoints = eventEntry.IsEligibleForPoints,
        LiveryImageUrl = eventEntry.Entry?.LiveryImageUrl,
        Drivers = [.. eventEntry.Drivers
            .OrderBy(entryDriver => entryDriver.LineupPosition)
            .Select(entryDriver => new EventEntryDriverResponse
            {
                DriverId = entryDriver.DriverId,
                FullName = entryDriver.Driver?.FullName ?? string.Empty,
                Abbreviation = entryDriver.Driver?.Abbreviation,
                Slug = entryDriver.Driver?.Slug ?? string.Empty,
                CountryCode = entryDriver.Driver?.CountryCode,
                HeadshotUrl = entryDriver.Driver?.HeadshotUrl,
                LineupPosition = entryDriver.LineupPosition,
                Role = entryDriver.Role,
                Grade = entryDriver.Grade,
                IsStartingDriver = entryDriver.IsStartingDriver
            })]
    };

    /// <summary>Maps a classification row.</summary>
    public static SessionResultResponse ToResponse(this SessionResult result) => new()
    {
        Id = result.Id,
        SessionId = result.SessionId,
        EventEntryId = result.EventEntryId,
        Position = result.Position,
        ClassPosition = result.ClassPosition,
        StartingPosition = result.StartingPosition,
        PositionsGained = result.StartingPosition is null || result.Position is null
            ? null
            : result.StartingPosition - result.Position,
        CarNumber = result.EventEntry?.CarNumberOverride ?? result.EventEntry?.Entry?.CarNumber ?? string.Empty,
        TeamId = result.EventEntry?.Entry?.TeamId ?? Guid.Empty,
        TeamName = result.EventEntry?.Entry?.Team?.Name ?? string.Empty,
        EntrantName = result.EventEntry?.Entry?.EntrantName ?? string.Empty,
        VehicleClassName = result.EventEntry?.Entry?.VehicleClass?.Name,
        VehicleClassColorHex = result.EventEntry?.Entry?.VehicleClass?.ColorHex,
        ChassisModel = result.EventEntry?.Entry?.ChassisModel,
        LapsCompleted = result.LapsCompleted,
        LapsLed = result.LapsLed,
        TotalTimeMs = result.TotalTimeMs,
        TotalTimeDisplay = TimeFormatting.ToTotalTime(result.TotalTimeMs),
        GapToLeaderMs = result.GapToLeaderMs,
        GapToLeaderLaps = result.GapToLeaderLaps,
        GapDisplay = TimeFormatting.ToGap(result.Position, result.GapToLeaderMs, result.GapToLeaderLaps),
        BestLapTimeMs = result.BestLapTimeMs,
        BestLapDisplay = TimeFormatting.ToLapTime(result.BestLapTimeMs),
        BestLapNumber = result.BestLapNumber,
        AverageSpeedKph = result.AverageSpeedKph,
        TopSpeedKph = result.TopSpeedKph,
        Status = result.Status,
        StatusDetail = result.StatusDetail,
        IsClassified = result.IsClassified,
        PointsScored = result.PointsScored,
        IsFastestLap = result.IsFastestLap,
        IsPolePosition = result.IsPolePosition,
        PitStopCount = result.PitStopCount,
        PenaltyTimeMs = result.PenaltyTimeMs,
        Drivers = [.. result.Drivers.Select(resultDriver => new SessionResultDriverResponse
        {
            DriverId = resultDriver.DriverId,
            FullName = resultDriver.Driver?.FullName ?? string.Empty,
            Abbreviation = resultDriver.Driver?.Abbreviation,
            Slug = resultDriver.Driver?.Slug ?? string.Empty,
            CountryCode = resultDriver.Driver?.CountryCode,
            LapsDriven = resultDriver.LapsDriven,
            BestLapTimeMs = resultDriver.BestLapTimeMs,
            BestLapDisplay = TimeFormatting.ToLapTime(resultDriver.BestLapTimeMs),
            TimeDrivenMs = resultDriver.TimeDrivenMs,
            PointsScored = resultDriver.PointsScored,
            IsEligibleForPoints = resultDriver.IsEligibleForPoints,
            SetBestLap = resultDriver.SetBestLap
        })]
    };

    /// <summary>Maps a lap.</summary>
    public static SessionLapResponse ToResponse(this SessionLap lap) => new()
    {
        Id = lap.Id,
        EventEntryId = lap.EventEntryId,
        CarNumber = lap.EventEntry?.CarNumberOverride ?? lap.EventEntry?.Entry?.CarNumber ?? string.Empty,
        DriverId = lap.DriverId,
        DriverName = lap.Driver?.FullName,
        LapNumber = lap.LapNumber,
        LapTimeMs = lap.LapTimeMs,
        LapTimeDisplay = TimeFormatting.ToLapTime(lap.LapTimeMs),
        Sector1Ms = lap.Sector1Ms,
        Sector2Ms = lap.Sector2Ms,
        Sector3Ms = lap.Sector3Ms,
        Position = lap.Position,
        SpeedTrapKph = lap.SpeedTrapKph,
        TyreCompound = lap.TyreCompound,
        TyreAgeLaps = lap.TyreAgeLaps,
        IsPitInLap = lap.IsPitInLap,
        IsPitOutLap = lap.IsPitOutLap,
        IsPersonalBest = lap.IsPersonalBest,
        IsSessionBest = lap.IsSessionBest,
        IsUnderCaution = lap.IsUnderCaution,
        IsDeletedByStewards = lap.IsDeletedByStewards
    };

    /// <summary>Maps a pit stop.</summary>
    public static PitStopResponse ToResponse(this PitStop stop) => new()
    {
        Id = stop.Id,
        EventEntryId = stop.EventEntryId,
        CarNumber = stop.EventEntry?.CarNumberOverride ?? stop.EventEntry?.Entry?.CarNumber ?? string.Empty,
        StopNumber = stop.StopNumber,
        LapNumber = stop.LapNumber,
        StationaryTimeMs = stop.StationaryTimeMs,
        StationaryTimeDisplay = TimeFormatting.ToStopTime(stop.StationaryTimeMs),
        TotalPitLaneTimeMs = stop.TotalPitLaneTimeMs,
        TyreCompoundFitted = stop.TyreCompoundFitted,
        RefuelledStop = stop.RefuelledStop,
        DriverChanged = stop.DriverChanged,
        DriverInName = stop.DriverIn?.FullName,
        DriverOutName = stop.DriverOut?.FullName,
        IsUnderCaution = stop.IsUnderCaution,
        IsPenaltyServed = stop.IsPenaltyServed,
        Note = stop.Note
    };

    /// <summary>Maps a stewards' decision.</summary>
    public static PenaltyResponse ToResponse(this Penalty penalty) => new()
    {
        Id = penalty.Id,
        SessionId = penalty.SessionId,
        EventEntryId = penalty.EventEntryId,
        CarNumber = penalty.EventEntry?.CarNumberOverride ?? penalty.EventEntry?.Entry?.CarNumber ?? string.Empty,
        DriverId = penalty.DriverId,
        DriverName = penalty.Driver?.FullName,
        Type = penalty.Type,
        TimePenaltySeconds = penalty.TimePenaltySeconds,
        GridPositionDrop = penalty.GridPositionDrop,
        LicencePoints = penalty.LicencePoints,
        FineAmount = penalty.FineAmount,
        FineCurrency = penalty.FineCurrency,
        Reason = penalty.Reason,
        LapNumber = penalty.LapNumber,
        IssuedAtUtc = penalty.IssuedAtUtc,
        AppliedToSessionId = penalty.AppliedToSessionId,
        IsServed = penalty.IsServed,
        IsRescinded = penalty.IsRescinded
    };

    private static int? CalculateAge(DateOnly? birth, DateOnly? death, DateTime nowUtc)
    {
        if (birth is null)
        {
            return null;
        }

        var reference = death ?? DateOnly.FromDateTime(nowUtc);
        var age = reference.Year - birth.Value.Year;

        // Birthday not yet reached in the reference year.
        if (reference < birth.Value.AddYears(age))
        {
            age--;
        }

        return age < 0 ? null : age;
    }
}
