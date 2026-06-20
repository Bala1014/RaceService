namespace RaceService.Application.Domain.Entities
{
    public class RaceResult : Entity
    {
        public Guid DriverId { get; set; }
        public Driver Driver { get; set; } = null!;
        public Guid RaceEntryId { get; set; }
        public RaceEntry RaceEntry { get; set; } = null!;
        public Guid PastResultId { get; set; }
        public PastResult PastResult { get; set; } = null!;
        public int LapsCompleted { get; set; }
        public TimeSpan TotalTime { get; set; }
        public TimeSpan BestLapTime { get; set; }
        public int Position { get; set; }
        public int Points { get; set; }
        public int StartingPosition { get; set; }
    }
}
