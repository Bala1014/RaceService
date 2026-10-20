using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Results;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class PenaltyConfiguration : IEntityTypeConfiguration<Penalty>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Penalty> builder)
    {
        builder.ToTable("penalty");

        builder.HasKey(penalty => penalty.Id);

        builder.Property(penalty => penalty.Reason).HasMaxLength(1000);
        builder.Property(penalty => penalty.FineAmount).HasPrecision(12, 2);
        builder.Property(penalty => penalty.FineCurrency).HasMaxLength(3).IsFixedLength();

        builder.HasOne(penalty => penalty.Session)
            .WithMany(session => session.Penalties)
            .HasForeignKey(penalty => penalty.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(penalty => penalty.AppliedToSession)
            .WithMany()
            // No cascade on this second path to the same table: two cascading routes into one
            // row is exactly the multiple-cascade-paths error, and a later session being removed
            // should not delete the qualifying penalty that earned the grid drop.
            .HasForeignKey(penalty => penalty.AppliedToSessionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(penalty => penalty.EventEntry)
            .WithMany()
            .HasForeignKey(penalty => penalty.EventEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(penalty => penalty.Driver)
            .WithMany()
            .HasForeignKey(penalty => penalty.DriverId)
            .OnDelete(DeleteBehavior.SetNull);

        // Penalties shown on a session report.
        builder.HasIndex(penalty => new { penalty.SessionId, penalty.EventEntryId })
            .HasDatabaseName("ix_penalty_session_entry");

        // Outstanding grid penalties to apply to an upcoming session.
        builder.HasIndex(penalty => new { penalty.AppliedToSessionId, penalty.IsServed })
            .HasFilter("applied_to_session_id is not null")
            .HasDatabaseName("ix_penalty_applied_session_served");

        // A driver's licence points tally, which is what a race ban is counted from.
        builder.HasIndex(penalty => new { penalty.DriverId, penalty.IssuedAtUtc })
            .HasFilter("driver_id is not null and is_rescinded = false")
            .HasDatabaseName("ix_penalty_driver_issued");
    }
}
