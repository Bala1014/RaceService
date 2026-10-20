using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Competitors;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class DriverCareerStatConfiguration : IEntityTypeConfiguration<DriverCareerStat>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DriverCareerStat> builder)
    {
        builder.ToTable("driver_career_stat");

        builder.HasKey(stat => stat.Id);

        builder.Property(stat => stat.CareerPoints).HasPrecision(10, 2);

        builder.HasOne(stat => stat.Driver)
            .WithMany(driver => driver.CareerStats)
            .HasForeignKey(stat => stat.DriverId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(stat => stat.Series)
            .WithMany(series => series.DriverCareerStats)
            .HasForeignKey(stat => stat.SeriesId)
            .OnDelete(DeleteBehavior.Cascade);

        // One row per driver per series, plus one all-series row. Split into two partial unique
        // indexes for the same NULL-is-not-equal-to-NULL reason as vehicle classes.
        builder.HasIndex(stat => new { stat.DriverId, stat.SeriesId })
            .IsUnique()
            .HasFilter("series_id is not null")
            .HasDatabaseName("ux_driver_career_stat_driver_series");

        builder.HasIndex(stat => stat.DriverId)
            .IsUnique()
            .HasFilter("series_id is null")
            .HasDatabaseName("ux_driver_career_stat_driver_overall");

        // "Most wins in this series" leaderboards.
        builder.HasIndex(stat => new { stat.SeriesId, stat.Wins })
            .IsDescending(false, true)
            .HasDatabaseName("ix_driver_career_stat_series_wins");
    }
}
