namespace RaceService.Application.Domain.Entities
{
    public class Track : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
        public decimal LengthInKm { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public TimeSpan? BestLapTime { get; set; }
        public ICollection<Race> Races { get; set; } = [];
    }
}
