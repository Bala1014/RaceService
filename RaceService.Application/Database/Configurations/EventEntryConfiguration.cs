using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Database.Conventions;
using RaceService.Application.Domain.Entities.Competitors;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class EventEntryConfiguration : IEntityTypeConfiguration<EventEntry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EventEntry> builder)
    {
        builder.ToTable("event_entry");

        builder.HasKey(eventEntry => eventEntry.Id);

        builder.Property(eventEntry => eventEntry.CarNumberOverride).HasMaxLength(10);
        builder.Property(eventEntry => eventEntry.StatusNote).HasMaxLength(500);

        builder.Property(eventEntry => eventEntry.Version).AsXminConcurrencyToken();

        builder.HasOne(eventEntry => eventEntry.RaceEvent)
            .WithMany(raceEvent => raceEvent.Entries)
            .HasForeignKey(eventEntry => eventEntry.RaceEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(eventEntry => eventEntry.Entry)
            .WithMany(entry => entry.EventEntries)
            .HasForeignKey(eventEntry => eventEntry.EntryId)
            .OnDelete(DeleteBehavior.Restrict);

        // A car appears at most once on an event's entry list.
        builder.HasIndex(eventEntry => new { eventEntry.RaceEventId, eventEntry.EntryId })
            .IsUnique()
            .HasDatabaseName("ux_event_entry_event_entry");

        // The entry list for one round, filtered by who actually confirmed.
        builder.HasIndex(eventEntry => new { eventEntry.RaceEventId, eventEntry.Status })
            .HasDatabaseName("ix_event_entry_event_status");

        // "Which rounds did this car contest", for the car/team history table.
        builder.HasIndex(eventEntry => eventEntry.EntryId)
            .HasDatabaseName("ix_event_entry_entry");

        // The starting grid, in grid order.
        builder.HasIndex(eventEntry => new { eventEntry.RaceEventId, eventEntry.StartingGridPosition })
            .HasFilter("starting_grid_position is not null")
            .HasDatabaseName("ix_event_entry_event_grid");

        builder.ToTable(table => table.HasCheckConstraint(
            "ck_event_entry_grid_positive",
            "starting_grid_position is null or starting_grid_position >= 1"));
    }
}
