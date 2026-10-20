using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RaceService.Application.Domain.Entities.Catalog;
using RaceService.Application.Domain.Entities.Competitors;
using RaceService.Application.Domain.Entities.Events;
using RaceService.Application.Domain.Entities.Results;
using RaceService.Application.Domain.Entities.Venues;
using RaceService.Application.Services;
using RaceService.Contracts.Enums;

namespace RaceService.Application.Database;

/// <summary>
/// Applies pending migrations at start-up and seeds reference data when the database is empty.
/// </summary>
/// <remarks>
/// The seed is a single JSON document describing one complete object graph rather than the
/// previous file-per-table arrangement. That earlier design made the caller responsible for
/// getting insert order right and for pasting foreign key GUIDs between files by hand, which is
/// unmaintainable the moment the graph is more than three levels deep — and this one is seven.
/// Here the file nests naturally and keys are assigned as the graph is built, so referential
/// integrity is a property of the structure rather than something the author has to remember.
/// </remarks>
public sealed class DatabaseInitializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ApplicationDbContext _context;
    private readonly IPointsCalculator _points;
    private readonly IStandingsService _standings;
    private readonly IDriverService _drivers;
    private readonly ILogger<DatabaseInitializer> _logger;

    /// <summary>Initialises the initializer.</summary>
    public DatabaseInitializer(
        ApplicationDbContext context,
        IPointsCalculator points,
        IStandingsService standings,
        IDriverService drivers,
        ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _points = points;
        _standings = standings;
        _drivers = drivers;
        _logger = logger;
    }

    /// <summary>Migrates the database and seeds it if it is empty.</summary>
    public async Task InitialiseAsync(bool seed, CancellationToken cancellationToken = default)
    {
        var pending = (await _context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

        if (pending.Count > 0)
        {
            _logger.LogInformation("Applying {Count} pending migration(s): {Migrations}", pending.Count, string.Join(", ", pending));
            await _context.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            _logger.LogInformation("Database schema is up to date.");
        }

        if (seed)
        {
            await SeedAsync(cancellationToken);
        }
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        // A single guard on the root of the graph. Seeding is all-or-nothing so a half-applied
        // seed can never leave dangling references.
        if (await _context.Series.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Seed data already present, skipping.");
            return;
        }

        var path = Path.Combine(AppContext.BaseDirectory, "Database", "SeedData", "seed.json");

        if (!File.Exists(path))
        {
            _logger.LogWarning("Seed file not found at {Path}; starting with an empty database.", path);
            return;
        }

        SeedDocument? document;

        await using (var stream = File.OpenRead(path))
        {
            document = await JsonSerializer.DeserializeAsync<SeedDocument>(stream, SerializerOptions, cancellationToken);
        }

        if (document is null)
        {
            _logger.LogWarning("Seed file at {Path} was empty.", path);
            return;
        }

        List<Season> seededSeasons = [];
        List<Guid> seededDriverIds = [];

        // Connection retries are enabled, so an explicit transaction has to run inside the
        // execution strategy: a retry must replay the whole transaction, not resume half of one.
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                (seededSeasons, seededDriverIds) = Build(document);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(exception, "Seeding failed and was rolled back.");
                throw;
            }
        });

        // Standings and career stats are projections over results, so they are built after the
        // results exist rather than restated in the seed file where they could disagree with them.
        foreach (var season in seededSeasons)
        {
            await _standings.RecalculateSeasonAsync(season.Id, cancellationToken);
        }

        foreach (var driverId in seededDriverIds)
        {
            await _drivers.RecalculateCareerStatsAsync(driverId, cancellationToken);
        }

        _logger.LogInformation(
            "Seeded {Series} series, {Tracks} tracks, {Teams} teams, {Drivers} drivers, {Seasons} season(s).",
            document.Series.Count,
            document.Tracks.Count,
            document.Teams.Count,
            document.Drivers.Count,
            seededSeasons.Count);
    }

    /// <summary>
    /// Materialises the seed document into tracked entities. References inside the document are
    /// by human-readable key ("silverstone", "ver") rather than GUID, and are resolved through
    /// these dictionaries as the graph is built.
    /// </summary>
    private (List<Season> Seasons, List<Guid> DriverIds) Build(SeedDocument document)
    {
        var seededSeasons = new List<Season>();
        var manufacturers = new Dictionary<string, Manufacturer>(StringComparer.OrdinalIgnoreCase);
        var tracks = new Dictionary<string, Track>(StringComparer.OrdinalIgnoreCase);
        var layouts = new Dictionary<string, TrackLayout>(StringComparer.OrdinalIgnoreCase);
        var teams = new Dictionary<string, Team>(StringComparer.OrdinalIgnoreCase);
        var drivers = new Dictionary<string, Driver>(StringComparer.OrdinalIgnoreCase);
        var classes = new Dictionary<string, VehicleClass>(StringComparer.OrdinalIgnoreCase);
        var seriesByKey = new Dictionary<string, Series>(StringComparer.OrdinalIgnoreCase);

        foreach (var seed in document.Manufacturers)
        {
            var manufacturer = new Manufacturer
            {
                Name = seed.Name,
                Slug = seed.Slug,
                CountryCode = seed.CountryCode,
                SuppliesChassis = seed.SuppliesChassis,
                SuppliesEngines = seed.SuppliesEngines,
                SuppliesTyres = seed.SuppliesTyres
            };

            manufacturers[seed.Key] = manufacturer;
            _context.Manufacturers.Add(manufacturer);
        }

        foreach (var seed in document.Tracks)
        {
            var track = new Track
            {
                Name = seed.Name,
                OfficialName = seed.OfficialName,
                Slug = seed.Slug,
                City = seed.City,
                Country = seed.Country,
                CountryCode = seed.CountryCode,
                TimeZoneId = seed.TimeZoneId,
                Latitude = seed.Latitude,
                Longitude = seed.Longitude,
                OpenedYear = seed.OpenedYear,
                ImageUrl = seed.ImageUrl
            };

            tracks[seed.Key] = track;
            _context.Tracks.Add(track);

            foreach (var layoutSeed in seed.Layouts)
            {
                var layout = new TrackLayout
                {
                    Track = track,
                    Name = layoutSeed.Name,
                    Slug = layoutSeed.Slug,
                    LengthMeters = layoutSeed.LengthMeters,
                    NumberOfTurns = layoutSeed.NumberOfTurns,
                    Direction = layoutSeed.Direction,
                    TrackType = layoutSeed.TrackType,
                    IsCurrent = layoutSeed.IsCurrent
                };

                layouts[layoutSeed.Key] = layout;
                track.Layouts.Add(layout);
            }
        }

        foreach (var seed in document.Teams)
        {
            var team = new Team
            {
                Name = seed.Name,
                OfficialName = seed.OfficialName,
                ShortName = seed.ShortName,
                Slug = seed.Slug,
                CountryCode = seed.CountryCode,
                BaseCity = seed.BaseCity,
                BaseCountry = seed.BaseCountry,
                FoundedYear = seed.FoundedYear,
                PrimaryColorHex = seed.PrimaryColorHex
            };

            teams[seed.Key] = team;
            _context.Teams.Add(team);
        }

        foreach (var seed in document.Drivers)
        {
            var driver = new Driver
            {
                FirstName = seed.FirstName,
                LastName = seed.LastName,
                FullName = seed.FullName ?? $"{seed.FirstName} {seed.LastName}",
                Abbreviation = seed.Abbreviation,
                Slug = seed.Slug,
                CountryCode = seed.CountryCode,
                Nationality = seed.Nationality,
                DateOfBirth = seed.DateOfBirth,
                PlaceOfBirth = seed.PlaceOfBirth,
                PermanentNumber = seed.PermanentNumber
            };

            drivers[seed.Key] = driver;
            _context.Drivers.Add(driver);
        }

        foreach (var seriesSeed in document.Series)
        {
            var series = new Series
            {
                Name = seriesSeed.Name,
                ShortName = seriesSeed.ShortName,
                Slug = seriesSeed.Slug,
                Discipline = seriesSeed.Discipline,
                SanctioningBody = seriesSeed.SanctioningBody,
                Description = seriesSeed.Description,
                PrimaryColorHex = seriesSeed.PrimaryColorHex,
                InauguralYear = seriesSeed.InauguralYear,
                DisplayOrder = seriesSeed.DisplayOrder
            };

            seriesByKey[seriesSeed.Key] = series;
            _context.Series.Add(series);

            foreach (var classSeed in seriesSeed.VehicleClasses)
            {
                var vehicleClass = new VehicleClass
                {
                    Series = series,
                    Name = classSeed.Name,
                    ShortName = classSeed.ShortName,
                    Slug = classSeed.Slug,
                    ColorHex = classSeed.ColorHex,
                    DisplayOrder = classSeed.DisplayOrder
                };

                classes[classSeed.Key] = vehicleClass;
                series.VehicleClasses.Add(vehicleClass);
            }

            foreach (var seasonSeed in seriesSeed.Seasons)
            {
                var season = new Season
                {
                    Series = series,
                    Year = seasonSeed.Year,
                    Name = seasonSeed.Name,
                    Slug = seasonSeed.Slug,
                    Status = seasonSeed.Status,
                    StartDate = seasonSeed.StartDate,
                    EndDate = seasonSeed.EndDate,
                    PlannedRoundCount = seasonSeed.PlannedRoundCount
                };

                series.Seasons.Add(season);

                foreach (var schemeSeed in seasonSeed.PointsSchemes)
                {
                    var scheme = new PointsScheme
                    {
                        Season = season,
                        Name = schemeSeed.Name,
                        AppliesToSessionType = schemeSeed.AppliesToSessionType,
                        ChampionshipType = schemeSeed.ChampionshipType,
                        FastestLapPoints = schemeSeed.FastestLapPoints,
                        FastestLapMaxPosition = schemeSeed.FastestLapMaxPosition,
                        PolePositionPoints = schemeSeed.PolePositionPoints,
                        MinimumDistancePercent = schemeSeed.MinimumDistancePercent,
                        MinimumDriverDistancePercent = schemeSeed.MinimumDriverDistancePercent
                    };

                    foreach (var (position, points) in schemeSeed.Positions)
                    {
                        scheme.Entries.Add(new PointsSchemeEntry
                        {
                            PointsScheme = scheme,
                            Position = int.Parse(position),
                            Points = points
                        });
                    }

                    season.PointsSchemes.Add(scheme);
                }

                var entries = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);

                foreach (var entrySeed in seasonSeed.Entries)
                {
                    var entry = new Entry
                    {
                        Season = season,
                        Team = teams[entrySeed.TeamKey],
                        CarNumber = entrySeed.CarNumber,
                        EntrantName = entrySeed.EntrantName,
                        Status = entrySeed.Status,
                        ChassisModel = entrySeed.ChassisModel,
                        EngineModel = entrySeed.EngineModel,
                        VehicleClass = entrySeed.VehicleClassKey is null ? null : classes[entrySeed.VehicleClassKey],
                        ChassisManufacturer = entrySeed.ChassisManufacturerKey is null ? null : manufacturers[entrySeed.ChassisManufacturerKey],
                        EngineManufacturer = entrySeed.EngineManufacturerKey is null ? null : manufacturers[entrySeed.EngineManufacturerKey],
                        TyreSupplier = entrySeed.TyreSupplierKey is null ? null : manufacturers[entrySeed.TyreSupplierKey]
                    };

                    entries[entrySeed.Key] = entry;
                    season.Entries.Add(entry);
                }

                foreach (var eventSeed in seasonSeed.Events)
                {
                    var layout = layouts[eventSeed.TrackLayoutKey];

                    var raceEvent = new RaceEvent
                    {
                        Season = season,
                        TrackLayout = layout,
                        Name = eventSeed.Name,
                        OfficialName = eventSeed.OfficialName,
                        ShortName = eventSeed.ShortName,
                        Slug = eventSeed.Slug,
                        RoundNumber = eventSeed.RoundNumber,
                        TimeZoneId = eventSeed.TimeZoneId ?? layout.Track.TimeZoneId,
                        Status = eventSeed.Status,
                        ShortDescription = eventSeed.ShortDescription,
                        ScheduledDistanceMeters = eventSeed.ScheduledDistanceMeters,
                        ScheduledDurationMinutes = eventSeed.ScheduledDurationMinutes,
                        HasSprint = eventSeed.HasSprint,
                        IsPublished = eventSeed.IsPublished,
                        PublishedAtUtc = eventSeed.IsPublished ? DateTime.UtcNow : null
                    };

                    var eventEntries = new Dictionary<string, EventEntry>(StringComparer.OrdinalIgnoreCase);

                    foreach (var eventEntrySeed in eventSeed.Entries)
                    {
                        var eventEntry = new EventEntry
                        {
                            RaceEvent = raceEvent,
                            Entry = entries[eventEntrySeed.EntryKey],
                            Status = eventEntrySeed.Status,
                            StartingGridPosition = eventEntrySeed.StartingGridPosition
                        };

                        var position = 1;

                        foreach (var driverKey in eventEntrySeed.DriverKeys)
                        {
                            eventEntry.Drivers.Add(new EventEntryDriver
                            {
                                EventEntry = eventEntry,
                                Driver = drivers[driverKey],
                                LineupPosition = position,
                                Role = RaceService.Contracts.Enums.DriverRole.Race,
                                IsStartingDriver = position == 1
                            });

                            position++;
                        }

                        eventEntries[eventEntrySeed.EntryKey] = eventEntry;
                        raceEvent.Entries.Add(eventEntry);
                    }

                    var weekendOrder = 1;

                    foreach (var sessionSeed in eventSeed.Sessions)
                    {
                        var session = new Session
                        {
                            RaceEvent = raceEvent,
                            Type = sessionSeed.Type,
                            SequenceNumber = sessionSeed.SequenceNumber,
                            Name = sessionSeed.Name,
                            WeekendOrder = weekendOrder++,
                            ScheduledStartUtc = DateTime.SpecifyKind(sessionSeed.ScheduledStartUtc, DateTimeKind.Utc),
                            ScheduledEndUtc = sessionSeed.ScheduledEndUtc is null
                                ? null
                                : DateTime.SpecifyKind(sessionSeed.ScheduledEndUtc.Value, DateTimeKind.Utc),
                            ScheduledDurationMinutes = sessionSeed.ScheduledDurationMinutes,
                            ScheduledLaps = sessionSeed.ScheduledLaps,
                            Status = sessionSeed.Status,
                            IsPointsScoring = sessionSeed.IsPointsScoring,
                            ResultsStatus = sessionSeed.ResultsStatus
                        };

                        foreach (var resultSeed in sessionSeed.Results)
                        {
                            var result = new SessionResult
                            {
                                Session = session,
                                EventEntry = eventEntries[resultSeed.EntryKey],
                                Position = resultSeed.Position,
                                ClassPosition = resultSeed.ClassPosition,
                                StartingPosition = resultSeed.StartingPosition,
                                LapsCompleted = resultSeed.LapsCompleted,
                                LapsLed = resultSeed.LapsLed,
                                TotalTimeMs = resultSeed.TotalTimeMs,
                                GapToLeaderMs = resultSeed.GapToLeaderMs,
                                GapToLeaderLaps = resultSeed.GapToLeaderLaps,
                                BestLapTimeMs = resultSeed.BestLapTimeMs,
                                BestLapNumber = resultSeed.BestLapNumber,
                                Status = resultSeed.Status,
                                StatusDetail = resultSeed.StatusDetail,
                                IsClassified = resultSeed.IsClassified,
                                PointsScored = resultSeed.PointsScored,
                                IsFastestLap = resultSeed.IsFastestLap,
                                IsPolePosition = resultSeed.IsPolePosition,
                                PitStopCount = resultSeed.PitStopCount
                            };

                            foreach (var driverKey in resultSeed.DriverKeys)
                            {
                                result.Drivers.Add(new SessionResultDriver
                                {
                                    SessionResult = result,
                                    Driver = drivers[driverKey],
                                    // Split evenly across the crew. Real timing data gives exact
                                    // per-driver laps; an even split is a reasonable stand-in for
                                    // seed data and keeps every driver over the minimum-distance
                                    // threshold, which is what the scoring rules test.
                                    LapsDriven = resultSeed.LapsCompleted / Math.Max(resultSeed.DriverKeys.Count, 1),
                                    PointsScored = 0,
                                    IsEligibleForPoints = true
                                });
                            }

                            session.Results.Add(result);
                        }

                        foreach (var lapSeed in sessionSeed.Laps)
                        {
                            session.Laps.Add(new SessionLap
                            {
                                Session = session,
                                EventEntry = eventEntries[lapSeed.EntryKey],
                                Driver = lapSeed.DriverKey is null ? null : drivers[lapSeed.DriverKey],
                                LapNumber = lapSeed.LapNumber,
                                LapTimeMs = lapSeed.LapTimeMs,
                                Sector1Ms = lapSeed.Sector1Ms,
                                Sector2Ms = lapSeed.Sector2Ms,
                                Sector3Ms = lapSeed.Sector3Ms,
                                Position = lapSeed.Position,
                                IsPitInLap = lapSeed.IsPitInLap,
                                IsPitOutLap = lapSeed.IsPitOutLap,
                                IsPersonalBest = lapSeed.IsPersonalBest,
                                IsSessionBest = lapSeed.IsSessionBest
                            });
                        }

                        foreach (var stopSeed in sessionSeed.PitStops)
                        {
                            session.PitStops.Add(new PitStop
                            {
                                Session = session,
                                EventEntry = eventEntries[stopSeed.EntryKey],
                                StopNumber = stopSeed.StopNumber,
                                LapNumber = stopSeed.LapNumber,
                                StationaryTimeMs = stopSeed.StationaryTimeMs,
                                TotalPitLaneTimeMs = stopSeed.TotalPitLaneTimeMs,
                                TyreCompoundFitted = stopSeed.TyreCompoundFitted,
                                RefuelledStop = stopSeed.RefuelledStop,
                                DriverChanged = stopSeed.DriverChanged,
                                DriverIn = stopSeed.DriverInKey is null ? null : drivers[stopSeed.DriverInKey],
                                DriverOut = stopSeed.DriverOutKey is null ? null : drivers[stopSeed.DriverOutKey]
                            });
                        }

                        foreach (var penaltySeed in sessionSeed.Penalties)
                        {
                            session.Penalties.Add(new Penalty
                            {
                                Session = session,
                                EventEntry = eventEntries[penaltySeed.EntryKey],
                                Driver = penaltySeed.DriverKey is null ? null : drivers[penaltySeed.DriverKey],
                                Type = penaltySeed.Type,
                                TimePenaltySeconds = penaltySeed.TimePenaltySeconds,
                                GridPositionDrop = penaltySeed.GridPositionDrop,
                                Reason = penaltySeed.Reason,
                                LapNumber = penaltySeed.LapNumber,
                                IsServed = penaltySeed.IsServed
                            });
                        }

                        // Points come from the season's scheme, exactly as they would through the
                        // API. The seed file states finishing positions; it never states points,
                        // so the two can never contradict each other.
                        if (session.IsPointsScoring)
                        {
                            var scheme = season.PointsSchemes.FirstOrDefault(candidate =>
                                candidate.AppliesToSessionType == session.Type
                                && candidate.ChampionshipType == ChampionshipType.Drivers);

                            _points.Apply(session, scheme);
                        }

                        raceEvent.Sessions.Add(session);
                    }

                    // The event's own window is derived from its sessions rather than restated in
                    // the seed file, which keeps the denormalised columns honest by construction.
                    if (raceEvent.Sessions.Count > 0)
                    {
                        raceEvent.StartTimeUtc = raceEvent.Sessions.Min(session => session.ScheduledStartUtc);
                        raceEvent.EndTimeUtc = raceEvent.Sessions.Max(session =>
                            session.ScheduledEndUtc ?? session.ScheduledStartUtc.AddHours(2));
                    }

                    season.Events.Add(raceEvent);
                }

                seededSeasons.Add(season);
            }
        }

        // Circuit records stand independently of any one session, and reference drivers, teams
        // and series built in the loops above — so they are resolved in a final pass once every
        // lookup is populated, rather than threading series/driver/team resolution into the track
        // loop that runs before any of them exist.
        foreach (var trackSeed in document.Tracks)
        {
            foreach (var layoutSeed in trackSeed.Layouts)
            {
                var layout = layouts[layoutSeed.Key];

                foreach (var lapRecordSeed in layoutSeed.LapRecords)
                {
                    layout.LapRecords.Add(new LapRecord
                    {
                        TrackLayout = layout,
                        RecordType = lapRecordSeed.RecordType,
                        LapTimeMs = lapRecordSeed.LapTimeMs,
                        Series = lapRecordSeed.SeriesKey is null ? null : seriesByKey[lapRecordSeed.SeriesKey],
                        VehicleClass = lapRecordSeed.VehicleClassKey is null ? null : classes[lapRecordSeed.VehicleClassKey],
                        Driver = lapRecordSeed.DriverKey is null ? null : drivers[lapRecordSeed.DriverKey],
                        Team = lapRecordSeed.TeamKey is null ? null : teams[lapRecordSeed.TeamKey],
                        SetOn = lapRecordSeed.SetOn,
                        VehicleDescription = lapRecordSeed.VehicleDescription,
                        IsCurrent = lapRecordSeed.IsCurrent
                    });
                }
            }
        }

        return (seededSeasons, [.. drivers.Values.Select(driver => driver.Id)]);
    }
}
