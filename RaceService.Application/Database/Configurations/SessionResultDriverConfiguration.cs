using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Results;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class SessionResultDriverConfiguration : IEntityTypeConfiguration<SessionResultDriver>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SessionResultDriver> builder)
    {
        builder.ToTable("session_result_driver");

        builder.HasKey(resultDriver => new { resultDriver.SessionResultId, resultDriver.DriverId });

        builder.Property(resultDriver => resultDriver.PointsScored).HasPrecision(7, 2);

        builder.HasOne(resultDriver => resultDriver.SessionResult)
            .WithMany(result => result.Drivers)
            .HasForeignKey(resultDriver => resultDriver.SessionResultId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(resultDriver => resultDriver.Driver)
            .WithMany(driver => driver.SessionResults)
            .HasForeignKey(resultDriver => resultDriver.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        // The index that makes driver standings cheap: gather one driver's scoring rows without
        // touching the car-side results at all.
        builder.HasIndex(resultDriver => new { resultDriver.DriverId, resultDriver.PointsScored })
            .HasDatabaseName("ix_session_result_driver_driver_points");

        builder.ToTable(table => table.HasCheckConstraint(
            "ck_session_result_driver_laps_not_negative",
            "laps_driven >= 0"));
    }
}
