using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities;

namespace RaceService.Application.Database.Configurations
{
    public class RaceEntryDriverConfiguration : IEntityTypeConfiguration<RaceEntryDriver>
    {
        public void Configure(EntityTypeBuilder<RaceEntryDriver> builder)
        {
            builder.ToTable("RaceEntryDriver");

            builder.HasKey(entryDriver => new { entryDriver.RaceEntryId, entryDriver.DriverId });

            builder.Property(entryDriver => entryDriver.Role)
                .HasConversion<int>()
                .IsRequired();

            builder.HasIndex(entryDriver => entryDriver.DriverId);
            builder.HasIndex(entryDriver => new { entryDriver.RaceEntryId, entryDriver.Role })
                .IsUnique();

            builder.HasOne(entryDriver => entryDriver.RaceEntry)
                .WithMany(entry => entry.Drivers)
                .HasForeignKey(entryDriver => entryDriver.RaceEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(entryDriver => entryDriver.Driver)
                .WithMany(driver => driver.RaceEntryDrivers)
                .HasForeignKey(entryDriver => entryDriver.DriverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
