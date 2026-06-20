namespace RaceService.Application.Domain.Entities
{
    public class PastResult : Entity
    {
        public Guid RaceId { get; set; }
        public Race Race { get; set; } = null!;
        public int Year { get; set; }
        public ICollection<RaceResult> RaceResults { get; set; } = [];
    }
}
