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
                // Check if data already exists
                if (_context.Driver.Any() || _context.Track.Any() || _context.Race.Any())
                {
                    _logger.LogInformation("Database already contains data. Skipping seed.");
                    return;
                }

                var seedDataPath = GetSeedDataFolderPath();

                // Seed Drivers
                if (!_context.Driver.Any())
                {
                    var drivers = LoadSeedData<Driver>(Path.Combine(seedDataPath, "Drivers.json"));
                    _context.Driver.AddRange(drivers);
                    _context.SaveChanges();
                    _logger.LogInformation("Seeded {Count} drivers.", drivers.Count);
                }

                // Seed Tracks
                if (!_context.Track.Any())
                {
                    var tracks = LoadSeedData<Track>(Path.Combine(seedDataPath, "Tracks.json"));
                    _context.Track.AddRange(tracks);
                    _context.SaveChanges();
                    _logger.LogInformation("Seeded {Count} tracks.", tracks.Count);
                }

                // Seed Races
                if (!_context.Race.Any())
                {
                    var races = LoadSeedData<RaceDto>(Path.Combine(seedDataPath, "Races.json"));
                    var raceEntities = races.Select(r => new Race
                    {
                        Id = r.Id,
                        Name = r.Name,
                        StartTimeUTC = r.StartTimeUTC,
                        Laps = r.Laps,
                        Status = (RaceStatus)r.Status,
                        CreatedAt = r.CreatedAt,
                        TrackId = Guid.Parse(r.TrackId)
                    }).ToList();
                    _context.Race.AddRange(raceEntities);
                    _context.SaveChanges();
                    _logger.LogInformation("Seeded {Count} races.", raceEntities.Count);
                }

                // Seed Race Entries
                if (!_context.RaceEntry.Any())
                {
                    var raceEntries = LoadSeedData<RaceEntryDto>(Path.Combine(seedDataPath, "RaceEntries.json"));
                    var raceEntryEntities = raceEntries.Select(re => new RaceEntry
                    {
                        Id = re.Id,
                        RaceId = Guid.Parse(re.RaceId),
                        SeriesType = re.SeriesType,
                        CarClass = re.CarClass,
                        CarManufacturer = re.CarManufacturer,
                        CarModel = re.CarModel,
                        PUManufacturer = re.PUManufacturer,
                        RaceEntryName = re.RaceEntryName
                    }).ToList();
                    _context.RaceEntry.AddRange(raceEntryEntities);
                    _context.SaveChanges();
                    _logger.LogInformation("Seeded {Count} race entries.", raceEntryEntities.Count);
                }

                // Seed Past Results
                if (!_context.PastResult.Any())
                {
                    var pastResults = LoadSeedData<PastResult>(Path.Combine(seedDataPath, "PastResults.json"));
                    _context.PastResult.AddRange(pastResults);
                    _context.SaveChanges();
                    _logger.LogInformation("Seeded {Count} past results.", pastResults.Count);
                }

                // Seed Race Results
                if (!_context.RaceResult.Any())
                {
                    var raceResults = LoadSeedData<RaceResultDto>(Path.Combine(seedDataPath, "RaceResults.json"));
                    var raceResultEntities = raceResults.Select(rr => new RaceResult
                    {
                        Id = rr.Id,
                        DriverId = Guid.Parse(rr.DriverId),
                        RaceEntry = rr.RaceEntry,
                        LapsCompleted = rr.LapsCompleted,
                        TotalTime = TimeSpan.Parse(rr.TotalTime),
                        BestLapTime = TimeSpan.Parse(rr.BestLapTime),
                        position = rr.Position,
                        Points = rr.Points,
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
                throw;
            }
        }
        private string GetSeedDataFolderPath()
        {
            // TODO: fix this path and make it such that it is dynamic not static
            return "/home/balagod99/Dev/RaceService/RaceService/RaceService.Application/Database/SeedData";
        }

        private List<T> LoadSeedData<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Seed data file not found: {filePath}");
            }

            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<T>>(json, options) ?? new List<T>();
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
        }

        public class RaceResultDto
        {
            public Guid Id { get; set; }
            public string DriverId { get; set; } = string.Empty;
            public RaceEntry RaceEntry { get; set; } 
            public int LapsCompleted { get; set; }
            public string TotalTime { get; set; } = string.Empty;
            public string BestLapTime { get; set; } = string.Empty;
            public int Position { get; set; }
            public int Points { get; set; }
            public int StartingPosition { get; set; }
        }

    }
}