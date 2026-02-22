using Microsoft.EntityFrameworkCore;
using RaceService.Application.Domain.Entities;

namespace RaceService.Application.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Race> Race { get; set; } 
        public DbSet<RaceEntry> RaceEntry { get; set; }
        public DbSet<Driver> Driver { get; set; }
        public DbSet<PastResult> PastResult { get; set; }
        public DbSet<RaceResult> RaceResult { get; set; }
        public DbSet<Track> Track { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure relationships and constraints here if needed
            //modelBuilder.Entity<RaceEntry>()
            //    .HasOne(re => re.Driver1)
            //    .WithMany()
            //    .HasForeignKey("Driver1Id")
            //    .OnDelete(DeleteBehavior.Restrict);
            //modelBuilder.Entity<RaceEntry>()
            //    .HasOne(re => re.Driver2)
            //    .WithMany()
            //    .HasForeignKey("Driver2Id")
            //    .OnDelete(DeleteBehavior.Restrict);
            //modelBuilder.Entity<RaceEntry>()
            //    .HasOne(re => re.Driver3)
            //    .WithMany()
            //    .HasForeignKey("Driver3Id")
            //    .OnDelete(DeleteBehavior.Restrict);
            //modelBuilder.Entity<RaceEntry>()
            //    .HasOne(re => re.Driver4)
            //    .WithMany()
            //    .HasForeignKey("Driver4Id")
            //    .OnDelete(DeleteBehavior.Restrict);
            //modelBuilder.Entity<RaceEntry>()
            //    .HasOne(re => re.DriverRes)
            //    .WithMany()
            //    .HasForeignKey("DriverResId")
            //    .OnDelete(DeleteBehavior.Restrict);
        }


    }
}
