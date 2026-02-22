namespace RaceService.Application.Domain.Entities
{
    public class Race
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartTimeUTC { get; set; }
        public Track Track { get; set; } 
        public int TrackId { get; set; }
        public int Laps { get; set; }
        public RaceStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<RaceEntry> Entries { get; private set; } = [];
        public List<PastResult> PastResults { get; private set; } = [];
    }
    public class  PastResult
    {
        public Guid Id { get; set; }
        public int RaceId { get; set; }
        public int Year { get; set; }
        public List<RaceResult> RaceResults { get; private set; } = [];
    }

    public class RaceResult
    {
        public Guid Id { get; set; }
        public Guid DriverId { get; set; }
        public RaceEntry RaceEntry { get; set; }
        public int LapsCompleted { get; set; }
        public TimeSpan TotalTime { get; set; }
        public TimeSpan BestLapTime { get; set; }
        public int position { get; set; }
        public int Points { get; set; }
        public int StartingPosition { get; set; }
    }
    public class Driver 
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Number { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public string Category { get; set; }  // pro, am, pro-am, semi-pro, gold
        public string RaceWins { get; set; } = string.Empty; // TODO add other stats like podiums, poles, etc
    }

    public class RaceEntry
    {
        public Guid Id { get;  set; }
        public Guid RaceId { get; set; }
        public string SeriesType { get; set; } = string.Empty;
        public string CarClass { get; private set; } = string.Empty;
        public Driver Driver1 { get; set;  } 
        public Driver Driver2 { get; set; } 
        public Driver Driver4 { get; set; } 
        public Driver Driver3 { get; set; } 
        public Driver DriverRes { get; set; } 
        public string CarManufacturer { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public string PUManufacturer { get; set; } = string.Empty;
        public string RaceEntryName { get; set; } = string.Empty;

    }

    public enum  RaceStatus
    {
        Live = 1,
        Upcoming = 2,
        Completed = 3
    }

    public class Track
    {
        public Guid Id  { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
        public int LengthInKm { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public TimeOnly BestLapTime { get; set; }

    }
}
