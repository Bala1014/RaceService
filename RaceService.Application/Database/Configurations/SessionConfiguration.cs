using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Database.Conventions;
using RaceService.Application.Domain.Entities.Events;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("session");

        builder.HasKey(session => session.Id);

        builder.Property(session => session.Name).HasMaxLength(120).IsRequired();
        builder.Property(session => session.WeatherSummary).HasMaxLength(300);
        builder.Property(session => session.StatusNote).HasMaxLength(1000);
        builder.Property(session => session.AirTemperatureC).HasPrecision(4, 1);
        builder.Property(session => session.TrackTemperatureC).HasPrecision(4, 1);
        builder.Property(session => session.HumidityPercent).HasPrecision(5, 2);
        builder.Property(session => session.SequenceNumber).HasDefaultValue(1);

        builder.Property(session => session.Version).AsXminConcurrencyToken();

        builder.HasOne(session => session.RaceEvent)
            .WithMany(raceEvent => raceEvent.Sessions)
            .HasForeignKey(session => session.RaceEventId)
            // A session cannot outlive its event, so this one really is a cascade.
            .OnDelete(DeleteBehavior.Cascade);

        // No two "Practice 2" rows in one weekend.
        builder.HasIndex(session => new { session.RaceEventId, session.Type, session.SequenceNumber })
            .IsUnique()
            .HasDatabaseName("ux_session_event_type_sequence");

        // The weekend schedule, already in running order.
        builder.HasIndex(session => new { session.RaceEventId, session.WeekendOrder })
            .HasDatabaseName("ix_session_event_order");

        // "What is on next, anywhere" — the live ticker across every series. Ordering by start
        // time within a status is exactly what this index is shaped for.
        builder.HasIndex(session => new { session.Status, session.ScheduledStartUtc })
            .HasDatabaseName("ix_session_status_scheduled_start");

        // Feeds the status-transition job, which sweeps sessions whose scheduled time has passed.
        builder.HasIndex(session => session.ScheduledStartUtc)
            .HasDatabaseName("ix_session_scheduled_start");

        // "Latest sessions with official results", for the results landing page.
        builder.HasIndex(session => new { session.ResultsStatus, session.ScheduledStartUtc })
            .HasDatabaseName("ix_session_results_status_start");

        builder.ToTable(table => table.HasCheckConstraint(
            "ck_session_sequence_positive",
            "sequence_number >= 1"));

        builder.ToTable(table => table.HasCheckConstraint(
            "ck_session_scheduled_time_order",
            "scheduled_end_utc is null or scheduled_end_utc >= scheduled_start_utc"));

        builder.ToTable(table => table.HasCheckConstraint(
            "ck_session_actual_time_order",
            "actual_end_utc is null or actual_start_utc is null or actual_end_utc >= actual_start_utc"));
    }
}
