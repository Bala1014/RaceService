using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Results;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class SessionLapConfiguration : IEntityTypeConfiguration<SessionLap>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SessionLap> builder)
    {
        builder.ToTable("session_lap");

        builder.HasKey(lap => lap.Id);

        builder.Property(lap => lap.TyreCompound).HasMaxLength(30);
        builder.Property(lap => lap.SpeedTrapKph).HasPrecision(6, 2);

        builder.HasOne(lap => lap.Session)
            .WithMany(session => session.Laps)
            .HasForeignKey(lap => lap.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(lap => lap.EventEntry)
            .WithMany()
            .HasForeignKey(lap => lap.EventEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(lap => lap.Driver)
            .WithMany()
            .HasForeignKey(lap => lap.DriverId)
            .OnDelete(DeleteBehavior.SetNull);

        // A car runs each lap number once.
        builder.HasIndex(lap => new { lap.SessionId, lap.EventEntryId, lap.LapNumber })
            .IsUnique()
            .HasDatabaseName("ux_session_lap_session_entry_lap");

        // The lap chart: every car's position, lap by lap.
        builder.HasIndex(lap => new { lap.SessionId, lap.LapNumber })
            .HasDatabaseName("ix_session_lap_session_lap_number");

        // "Fastest laps of the session", ascending so the best is first. Partial because a lap
        // with no time — an in-lap, an aborted run — is noise in this ordering.
        builder.HasIndex(lap => new { lap.SessionId, lap.LapTimeMs })
            .HasFilter("lap_time_ms is not null and is_deleted_by_stewards = false")
            .HasDatabaseName("ix_session_lap_session_time");

        // A driver's own laps, for stint analysis in endurance racing.
        builder.HasIndex(lap => new { lap.DriverId, lap.SessionId })
            .HasFilter("driver_id is not null")
            .HasDatabaseName("ix_session_lap_driver_session");

        builder.ToTable(table => table.HasCheckConstraint(
            "ck_session_lap_number_positive",
            "lap_number >= 1"));

        builder.ToTable(table => table.HasCheckConstraint(
            "ck_session_lap_time_positive",
            "lap_time_ms is null or lap_time_ms > 0"));
    }
}
