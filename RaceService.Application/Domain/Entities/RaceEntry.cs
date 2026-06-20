namespace RaceService.Application.Domain.Entities
{
    public class RaceEntry : Entity
    {
        public Guid RaceId { get; set; }
        public Race Race { get; set; } = null!;
        public string SeriesType { get; set; } = string.Empty;
        public string CarClass { get; set; } = string.Empty;
        public string CarManufacturer { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public string PUManufacturer { get; set; } = string.Empty;
        public string RaceEntryName { get; set; } = string.Empty;
        public ICollection<RaceEntryDriver> Drivers { get; set; } = [];
        public ICollection<RaceResult> Results { get; set; } = [];
    }
}
