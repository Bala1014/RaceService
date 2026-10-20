using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Competitors;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class EventEntryDriverConfiguration : IEntityTypeConfiguration<EventEntryDriver>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EventEntryDriver> builder)
    {
        builder.ToTable("event_entry_driver");

        // A composite key on the pair, because the pair is the identity: this driver, in this
        // car, at this round. A surrogate key would add nothing and would permit duplicates.
        builder.HasKey(entryDriver => new { entryDriver.EventEntryId, entryDriver.DriverId });

        builder.Property(entryDriver => entryDriver.LineupPosition).HasDefaultValue(1);

        builder.HasOne(entryDriver => entryDriver.EventEntry)
            .WithMany(eventEntry => eventEntry.Drivers)
            .HasForeignKey(entryDriver => entryDriver.EventEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entryDriver => entryDriver.Driver)
            .WithMany(driver => driver.EventEntries)
            .HasForeignKey(entryDriver => entryDriver.DriverId)
            // Restrict: a driver who has appeared in a lineup cannot be deleted out from under it.
            .OnDelete(DeleteBehavior.Restrict);

        // The reverse lookup — "every car this driver has been in" — which the composite primary
        // key cannot serve because the driver is its second column.
        builder.HasIndex(entryDriver => entryDriver.DriverId)
            .HasDatabaseName("ix_event_entry_driver_driver");

        // Two drivers cannot share a slot in the same crew.
        builder.HasIndex(entryDriver => new { entryDriver.EventEntryId, entryDriver.LineupPosition })
            .IsUnique()
            .HasDatabaseName("ux_event_entry_driver_lineup_position");

        // At most one driver takes the start.
        builder.HasIndex(entryDriver => entryDriver.EventEntryId)
            .IsUnique()
            .HasFilter("is_starting_driver = true")
            .HasDatabaseName("ux_event_entry_driver_single_starter");

        builder.ToTable(table => table.HasCheckConstraint(
            "ck_event_entry_driver_lineup_positive",
            "lineup_position >= 1"));
    }
}
