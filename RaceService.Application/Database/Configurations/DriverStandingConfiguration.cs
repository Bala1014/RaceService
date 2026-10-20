using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Standings;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class DriverStandingConfiguration : IEntityTypeConfiguration<DriverStanding>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DriverStanding> builder)
    {
        builder.ToTable("driver_standing");

        builder.HasKey(standing => standing.Id);

        builder.Property(standing => standing.Points).HasPrecision(8, 2);
        builder.Property(standing => standing.PointsBeforeDrops).HasPrecision(8, 2);

        builder.HasOne(standing => standing.Season)
            .WithMany(season => season.DriverStandings)
            .HasForeignKey(standing => standing.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(standing => standing.Driver)
            .WithMany()
            .HasForeignKey(standing => standing.DriverId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(standing => standing.VehicleClass)
            .WithMany(vehicleClass => vehicleClass.DriverStandings)
            .HasForeignKey(standing => standing.VehicleClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(standing => standing.AfterEvent)
            .WithMany()
            .HasForeignKey(standing => standing.AfterEventId)
            .OnDelete(DeleteBehavior.Cascade);

        // One row per driver per class per snapshot point, so a rebuild cannot double up.
        builder.HasIndex(standing => new { standing.SeasonId, standing.DriverId, standing.VehicleClassId, standing.AfterEventId })
            .IsUnique()
            .HasDatabaseName("ux_driver_standing_season_driver_class_event");

        // The championship table as it stands right now, already in order. The partial filter on
        // a null after_event_id keeps this index small even after a full season of snapshots.
        builder.HasIndex(standing => new { standing.SeasonId, standing.Position })
            .HasFilter("after_event_id is null")
            .HasDatabaseName("ix_driver_standing_current_by_position");

        // The title-race graph: one driver's points across every round.
        builder.HasIndex(standing => new { standing.DriverId, standing.SeasonId })
            .HasDatabaseName("ix_driver_standing_driver_season");
    }
}
