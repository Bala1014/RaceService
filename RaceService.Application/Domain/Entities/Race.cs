namespace RaceService.Application.Domain.Entities
{
    public class Race
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartTimeUTC { get; set; }
        public Guid TrackId { get; set; }
        public Track Track { get; set; } = null!;
        public int Laps { get; set; }
        public RaceStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<RaceEntry> Entries { get; set; } = [];
        public ICollection<PastResult> PastResults { get; set; } = [];
    }

    public class PastResult
    {
        public Guid Id { get; set; }
        public Guid RaceId { get; set; }            // Fixed: was int, must be Guid to match Race.Id
        public Race Race { get; set; } = null!;
        public int Year { get; set; }
        public ICollection<RaceResult> RaceResults { get; set; } = [];
    }

    public class RaceResult
    {
        public Guid Id { get; set; }
        public Guid DriverId { get; set; }
        public Driver Driver { get; set; } = null!;
        public Guid RaceEntryId { get; set; }       // Fixed: explicit FK instead of nav prop only
        public RaceEntry RaceEntry { get; set; } = null!;
        public Guid PastResultId { get; set; }      // Fixed: was missing entirely
        public PastResult PastResult { get; set; } = null!;
        public int LapsCompleted { get; set; }
        public TimeSpan TotalTime { get; set; }
        public TimeSpan BestLapTime { get; set; }
        public int Position { get; set; }           // Fixed: was lowercase 'position'
        public int Points { get; set; }
        public int StartingPosition { get; set; }
    }

    public class Driver
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Number { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;   // pro, am, pro-am, semi-pro, gold
        public int RaceWins { get; set; }           // Fixed: was string
        // public int Podiums { get; set; }
        // public int Poles { get; set; }
        public ICollection<RaceEntryDriver> RaceEntryDrivers { get; set; } = [];
    }

    // Fixed: replaces Driver1/Driver2/Driver3/Driver4/DriverRes columns on RaceEntry
    // This is a junction (join) table — one row per driver per entry
    public class RaceEntryDriver
    {
        public Guid RaceEntryId { get; set; }
        public RaceEntry RaceEntry { get; set; } = null!;
        public Guid DriverId { get; set; }
        public Driver Driver { get; set; } = null!;
        public DriverRole Role { get; set; }
    }

    public enum DriverRole
    {
        Driver1 = 1,
        Driver2 = 2,
        Driver3 = 3,
        Driver4 = 4,
        Reserve = 5
    }

    public class RaceEntry
    {
        public Guid Id { get; set; }
        public Guid RaceId { get; set; }
        public Race Race { get; set; } = null!;
        public string SeriesType { get; set; } = string.Empty;
        public string CarClass { get; set; } = string.Empty;
        public string CarManufacturer { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public string PUManufacturer { get; set; } = string.Empty;
        public string RaceEntryName { get; set; } = string.Empty;
        public ICollection<RaceEntryDriver> Drivers { get; set; } = [];    // replaces Driver1–4 + DriverRes
        public ICollection<RaceResult> Results { get; set; } = [];
    }

    public enum RaceStatus
    {
        Live = 1,
        Upcoming = 2,
        Completed = 3
    }

    public class Track
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
        public decimal LengthInKm { get; set; }     // Fixed: was int, tracks have decimal lengths e.g. 7.004
        public string ImageUrl { get; set; } = string.Empty;
        public TimeSpan? BestLapTime { get; set; }  // Fixed: was TimeOnly — TimeSpan is correct for durations
        public ICollection<Race> Races { get; set; } = [];
    }
}