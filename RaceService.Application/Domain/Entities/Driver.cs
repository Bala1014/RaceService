namespace RaceService.Application.Domain.Entities
{
    public class Driver : Entity
    {
        public string Name { get; set; } = string.Empty;
        public int Number { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int RaceWins { get; set; }

        // TODO : Add other driver stats
        public ICollection<RaceEntryDriver> RaceEntryDrivers { get; set; } = [];
    }
}
