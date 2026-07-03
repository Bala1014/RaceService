namespace RaceService.Contracts.Domain.Entities
{
    public class PastResult : Entity
    {
        public Guid RaceId { get; set; }
        public Race Race { get; set; } = null!;   // not stored in the db
        public int Year { get; set; }
        public ICollection<RaceResult> RaceResults { get; set; } = [];  // not stored in the db
    }
}
