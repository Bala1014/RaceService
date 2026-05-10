using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RaceService.Application.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RaceService.Application.Database
{
    public class DbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(ApplicationDbContext context, ILogger<DbInitializer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Init()
        {
            try
            {
                var pendingMigrations = _context.Database.GetPendingMigrations();
                if (pendingMigrations.Any())
                {
                    _context.Database.Migrate();
                    _logger.LogInformation("Applied {Count} pending migrations.", pendingMigrations.Count());
                }
                else
                {
                    _logger.LogInformation("No pending migrations found. Database is up to date.");
                }

                SeedDataToDB();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while applying migrations.");
                throw;
            }
        }

        private void SeedDataToDB()
        {
            try
            {
                var seedDataPath = GetSeedDataFolderPath();

                // ── 1. Drivers ────────────────────────────────────────────────────────────
                // No dependencies. Seed first.
                if (!_context.Driver.Any())
                {
                    var drivers = LoadSeedData<DriverSeedDto>(Path.Combine(seedDataPath, "Drivers.json"));
                    var driverEntities = drivers.Select(d => new Driver
                    {
                        Id         = d.Id,
                        Name       = d.Name,
                        Number     = d.Number,
                        Nationality = d.Nationality,
                        Category   = d.Category,
                        RaceWins   = d.RaceWins,    // now int
                        // Podiums    = d.Podiums,
                        // Poles      = d.Poles
                    }).ToList();

                    _context.Driver.AddRange(driverEntities);
                    _context.SaveChanges();
                    _logger.LogInformation("Seeded {Count} drivers.", driverEntities.Count);
                }

                // ── 2. Tracks ─────────────────────────────────────────────────────────────
                // No dependencies. Seed second.
                if (!_context.Track.Any())
                {
                    var tracks = LoadSeedData<TrackSeedDto>(Path.Combine(seedDataPath, "Tracks.json"));
                    var trackEntities = tracks.Select(t => new Track
                    {
                        Id          = t.Id,
                        Name        = t.Name,
                        Location    = t.Location,
                        Country     = t.Country,
                        TimeZone    = t.TimeZone,
                        LengthInKm  = t.LengthInKm,     // decimal
                        ImageUrl    = t.ImageUrl,
                        BestLapTime = t.BestLapTime != null
                                        ? TimeSpan.Parse(t.BestLapTime)
                                        : null           // TimeSpan? from "00:01:27.456" string
                    }).ToList();

                    _context.Track.AddRange(trackEntities);
                    _context.SaveChanges();
                    _logger.LogInformation("Seeded {Count} tracks.", trackEntities.Count);
                }

                // ── 3. Races ──────────────────────────────────────────────────────────────
                // Depends on Tracks (TrackId FK).
                if (!_context.Race.Any())
                {
                    var races = LoadSeedData<RaceDto>(Path.Combine(seedDataPath, "Races.json"));
                    var raceEntities = races.Select(r => new Race
                    {
                        Id           = r.Id,
                        Name         = r.Name,
                        StartTimeUTC = r.StartTimeUTC,
                        Laps         = r.Laps,
                        Status       = (RaceStatus)r.Status,
                        CreatedAt    = r.CreatedAt,
                        TrackId      = Guid.Parse(r.TrackId)
                        // Do NOT set Entries or PastResults here.
                        // They are seeded separately below and linked via their own FK columns.
                    }).ToList();

                    _context.Race.AddRange(raceEntities);
                    _context.SaveChanges();
                    _logger.LogInformation("Seeded {Count} races.", raceEntities.Count);
                }

                // ── 4. Race Entries ───────────────────────────────────────────────────────
                // Depends on Races (RaceId FK).
                if (!_context.RaceEntry.Any())
                {
                    var raceEntries = LoadSeedData<RaceEntryDto>(Path.Combine(seedDataPath, "RaceEntries.json"));
                    var raceEntryEntities = raceEntries.Select(re => new RaceEntry
                    {
                        Id              = re.Id,
                        RaceId          = Guid.Parse(re.RaceId),
                        SeriesType      = re.SeriesType,
                        CarClass        = re.CarClass,
                        CarManufacturer = re.CarManufacturer,
                        CarModel        = re.CarModel,
                        PUManufacturer  = re.PUManufacturer,
                        RaceEntryName   = re.RaceEntryName
                        // Drivers are linked via RaceEntryDrivers below, not set here.
                    }).ToList();

                    _context.RaceEntry.AddRange(raceEntryEntities);
                    _context.SaveChanges();
                    _logger.LogInformation("Seeded {Count} race entries.", raceEntryEntities.Count);
                }

                // ── 5. Race Entry Drivers (junction table) ────────────────────────────────
                // Depends on RaceEntries + Drivers.
                // This replaces the old Driver1/Driver2/Driver3/Driver4/DriverRes columns.
                // JSON format: [{ "raceEntryId": "...", "driverId": "...", "role": 1 }, ...]
                if (!_context.RaceEntryDriver.Any())
                {
                    var entryDrivers = LoadSeedData<RaceEntryDriverDto>(Path.Combine(seedDataPath, "RaceEntryDrivers.json"));
                    var entryDriverEntities = entryDrivers.Select(ed => new RaceEntryDriver
                    {
                        RaceEntryId = Guid.Parse(ed.RaceEntryId),
                        DriverId    = Guid.Parse(ed.DriverId),
                        Role        = (DriverRole)ed.Role
                    }).ToList();

                    _context.RaceEntryDriver.AddRange(entryDriverEntities);
                    _context.SaveChanges();
                    _logger.LogInformation("Seeded {Count} race entry drivers.", entryDriverEntities.Count);
                }

                // ── 6. Past Results ───────────────────────────────────────────────────────
                // Depends on Races (RaceId FK).
                // A PastResult is a historical year's result for a given Race.
                // JSON format: [{ "id": "...", "raceId": "...", "year": 2023 }, ...]
                if (!_context.PastResult.Any())
                {
                    var pastResults = LoadSeedData<PastResultDto>(Path.Combine(seedDataPath, "PastResults.json"));
                    var pastResultEntities = pastResults.Select(pr => new PastResult
                    {
                        Id     = pr.Id,
                        RaceId = Guid.Parse(pr.RaceId),  // Fixed: was int, now Guid
                        Year   = pr.Year
                        // RaceResults are linked below via their PastResultId FK.
                    }).ToList();

                    _context.PastResult.AddRange(pastResultEntities);
                    _context.SaveChanges();
                    _logger.LogInformation("Seeded {Count} past results.", pastResultEntities.Count);
                }

                // ── 7. Race Results ───────────────────────────────────────────────────────
                // Depends on Drivers, RaceEntries, and PastResults.
                // One row = one driver/entry finishing position inside a PastResult.
                // JSON format: see RaceResultDto below.
                if (!_context.RaceResult.Any())
                {
                    var raceResults = LoadSeedData<RaceResultDto>(Path.Combine(seedDataPath, "RaceResults.json"));
                    var raceResultEntities = raceResults.Select(rr => new RaceResult
                    {
                        Id               = rr.Id,
                        DriverId         = Guid.Parse(rr.DriverId),
                        RaceEntryId      = Guid.Parse(rr.RaceEntryId),  // Fixed: explicit FK
                        PastResultId     = Guid.Parse(rr.PastResultId), // Fixed: was missing
                        LapsCompleted    = rr.LapsCompleted,
                        TotalTime        = TimeSpan.Parse(rr.TotalTime),
                        BestLapTime      = TimeSpan.Parse(rr.BestLapTime),
                        Position         = rr.Position,                 // Fixed: was 'position'
                        Points           = rr.Points,
                        StartingPosition = rr.StartingPosition
                    }).ToList();

                    _context.RaceResult.AddRange(raceResultEntities);
                    _context.SaveChanges();
                    _logger.LogInformation("Seeded {Count} race results.", raceResultEntities.Count);
                }

                _logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        // Fixed: was a hardcoded absolute path on your dev machine.
        // AppContext.BaseDirectory resolves to the folder where the compiled DLL lives,
        // which works both locally and inside Docker.
        // Make sure your .csproj copies the SeedData folder to output — see README comment below.
        private string GetSeedDataFolderPath()
        {
            return Path.Combine(AppContext.BaseDirectory, "Database", "SeedData");
        }

        private List<T> LoadSeedData<T>(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Seed data file not found: {filePath}");

            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<T>>(json, options) ?? new List<T>();
        }

        // ── DTOs ──────────────────────────────────────────────────────────────────────────
        // These are plain data shapes used only for deserialising JSON.
        // They are separate from your domain entities on purpose — you control exactly
        // what fields exist in the JSON without polluting your entity classes.

        public class DriverSeedDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int Number { get; set; }
            public string Nationality { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public int RaceWins { get; set; }   // Fixed: was string
            // public int Podiums { get; set; }
            // public int Poles { get; set; }
        }

        public class TrackSeedDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public string Country { get; set; } = string.Empty;
            public string TimeZone { get; set; } = string.Empty;
            public decimal LengthInKm { get; set; }     // Fixed: was int
            public string ImageUrl { get; set; } = string.Empty;
            public string? BestLapTime { get; set; }    // nullable string "00:01:27.456", parsed to TimeSpan?
        }

        public class RaceDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public DateTime StartTimeUTC { get; set; }
            public string TrackId { get; set; } = string.Empty;
            public int Laps { get; set; }
            public int Status { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class RaceEntryDto
        {
            public Guid Id { get; set; }
            public string RaceId { get; set; } = string.Empty;
            public string SeriesType { get; set; } = string.Empty;
            public string CarClass { get; set; } = string.Empty;
            public string CarManufacturer { get; set; } = string.Empty;
            public string CarModel { get; set; } = string.Empty;
            public string PUManufacturer { get; set; } = string.Empty;
            public string RaceEntryName { get; set; } = string.Empty;
            // Note: no driver fields here — drivers are seeded via RaceEntryDrivers.json
        }

        // New DTO for the junction table
        public class RaceEntryDriverDto
        {
            public string RaceEntryId { get; set; } = string.Empty;
            public string DriverId { get; set; } = string.Empty;
            public int Role { get; set; }   // maps to DriverRole enum: 1=Driver1 ... 5=Reserve
        }

        public class PastResultDto
        {
            public Guid Id { get; set; }
            public string RaceId { get; set; } = string.Empty;  // Fixed: was int in entity
            public int Year { get; set; }
        }

        public class RaceResultDto
        {
            public Guid Id { get; set; }
            public string DriverId { get; set; } = string.Empty;
            public string RaceEntryId { get; set; } = string.Empty;    // Fixed: explicit FK
            public string PastResultId { get; set; } = string.Empty;   // Fixed: was missing
            public int LapsCompleted { get; set; }
            public string TotalTime { get; set; } = string.Empty;      // "02:45:30.123" → TimeSpan
            public string BestLapTime { get; set; } = string.Empty;    // "00:01:27.456" → TimeSpan
            public int Position { get; set; }
            public int Points { get; set; }
            public int StartingPosition { get; set; }
        }
    }
}