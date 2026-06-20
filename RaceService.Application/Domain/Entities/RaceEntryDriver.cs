namespace RaceService.Application.Domain.Entities
{
    public class RaceEntryDriver
    {
        public Guid RaceEntryId { get; set; }
        public RaceEntry RaceEntry { get; set; } = null!;
        public Guid DriverId { get; set; }
        public Driver Driver { get; set; } = null!;
        public DriverRole Role { get; set; } = DriverRole.Driver1;
    }
}
