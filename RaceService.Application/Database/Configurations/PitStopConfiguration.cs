using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Results;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class PitStopConfiguration : IEntityTypeConfiguration<PitStop>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PitStop> builder)
    {
        builder.ToTable("pit_stop");

        builder.HasKey(stop => stop.Id);

        builder.Property(stop => stop.TyreCompoundFitted).HasMaxLength(30);
        builder.Property(stop => stop.Note).HasMaxLength(300);

        builder.HasOne(stop => stop.Session)
            .WithMany(session => session.PitStops)
            .HasForeignKey(stop => stop.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(stop => stop.EventEntry)
            .WithMany()
            .HasForeignKey(stop => stop.EventEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(stop => stop.DriverIn)
            .WithMany()
            .HasForeignKey(stop => stop.DriverInId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(stop => stop.DriverOut)
            .WithMany()
            .HasForeignKey(stop => stop.DriverOutId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(stop => new { stop.SessionId, stop.EventEntryId, stop.StopNumber })
            .IsUnique()
            .HasDatabaseName("ux_pit_stop_session_entry_number");

        // The strategy timeline for the whole session, in the order stops happened.
        builder.HasIndex(stop => new { stop.SessionId, stop.LapNumber })
            .HasDatabaseName("ix_pit_stop_session_lap");

        // "Fastest stop of the race", a standard broadcast statistic.
        builder.HasIndex(stop => new { stop.SessionId, stop.StationaryTimeMs })
            .HasFilter("stationary_time_ms is not null")
            .HasDatabaseName("ix_pit_stop_session_stationary_time");

        builder.ToTable(table => table.HasCheckConstraint(
            "ck_pit_stop_number_positive",
            "stop_number >= 1"));

        // The pit lane loss always includes the stationary time, never less.
        builder.ToTable(table => table.HasCheckConstraint(
            "ck_pit_stop_total_covers_stationary",
            "total_pit_lane_time_ms is null or stationary_time_ms is null or total_pit_lane_time_ms >= stationary_time_ms"));
    }
}
