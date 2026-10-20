using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Database.Conventions;
using RaceService.Application.Domain.Entities.Results;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class SessionResultConfiguration : IEntityTypeConfiguration<SessionResult>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SessionResult> builder)
    {
        builder.ToTable("session_result");

        builder.HasKey(result => result.Id);

        builder.Property(result => result.StatusDetail).HasMaxLength(200);
        builder.Property(result => result.PointsScored).HasPrecision(7, 2);
        builder.Property(result => result.AverageSpeedKph).HasPrecision(6, 2);
        builder.Property(result => result.TopSpeedKph).HasPrecision(6, 2);

        builder.Property(result => result.Version).AsXminConcurrencyToken();

        builder.HasOne(result => result.Session)
            .WithMany(session => session.Results)
            .HasForeignKey(result => result.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(result => result.EventEntry)
            .WithMany(eventEntry => eventEntry.SessionResults)
            .HasForeignKey(result => result.EventEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        // One classification per car per session. This single constraint is what the old
        // three-foreign-key result row could never express.
        builder.HasIndex(result => new { result.SessionId, result.EventEntryId })
            .IsUnique()
            .HasDatabaseName("ux_session_result_session_entry");

        // No two cars share a finishing position. Partial, because any number of cars can have
        // no position at all — every DNS and unclassified retirement is null here.
        builder.HasIndex(result => new { result.SessionId, result.Position })
            .IsUnique()
            .HasFilter("position is not null")
            .HasDatabaseName("ux_session_result_session_position");

        // The results table itself, read in classification order. Covering the common read path
        // in one index scan is the point of putting position second.
        builder.HasIndex(result => new { result.SessionId, result.Position, result.ClassPosition })
            .HasDatabaseName("ix_session_result_session_order");

        // At most one fastest lap and one pole per session. Both are partial unique indexes over
        // the same column, which needs the named overload: EF identifies an index by its property
        // set, so two calls to HasIndex(result => result.SessionId) would define the same index
        // twice and the second would silently replace the first.
        builder.HasIndex([nameof(SessionResult.SessionId)], "ux_session_result_single_fastest_lap")
            .IsUnique()
            .HasFilter("is_fastest_lap = true");

        builder.HasIndex([nameof(SessionResult.SessionId)], "ux_session_result_single_pole")
            .IsUnique()
            .HasFilter("is_pole_position = true");

        // "Every result for this car this season", for the entry's form guide.
        builder.HasIndex(result => result.EventEntryId)
            .HasDatabaseName("ix_session_result_event_entry");

        builder.ToTable(table => table.HasCheckConstraint(
            "ck_session_result_position_positive",
            "position is null or position >= 1"));

        builder.ToTable(table => table.HasCheckConstraint(
            "ck_session_result_laps_not_negative",
            "laps_completed >= 0 and laps_led >= 0"));

        // A car cannot lead more laps than it completed.
        builder.ToTable(table => table.HasCheckConstraint(
            "ck_session_result_laps_led_within_completed",
            "laps_led <= laps_completed"));
    }
}
