using Microsoft.EntityFrameworkCore;
using RaceService.Application.Domain.Entities;

namespace RaceService.Application.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Race> Race { get; set; } = null!;
        public DbSet<RaceEntry> RaceEntry { get; set; } = null!;
        public DbSet<Driver> Driver { get; set; } = null!;
        public DbSet<PastResult> PastResult { get; set; } = null!;
        public DbSet<RaceResult> RaceResult { get; set; } = null!;
        public DbSet<Track> Track { get; set; } = null!;
        public DbSet<RaceEntryDriver> RaceEntryDriver { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
