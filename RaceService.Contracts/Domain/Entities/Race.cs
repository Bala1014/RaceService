namespace RaceService.Contracts.Domain.Entities
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
        public DateTime CreatedAtUTC { get; set; }
        public ICollection<RaceEntry> Entries { get; set; } = [];    // not stored in the db
        public ICollection<PastResult> PastResults { get; set; } = [];  // not stored in the db
    }
}