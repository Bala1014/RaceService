using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Venues;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class LapRecordConfiguration : IEntityTypeConfiguration<LapRecord>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LapRecord> builder)
    {
        builder.ToTable("lap_record");

        builder.HasKey(record => record.Id);

        builder.Property(record => record.VehicleDescription).HasMaxLength(200);

        builder.HasOne(record => record.TrackLayout)
            .WithMany(layout => layout.LapRecords)
            .HasForeignKey(record => record.TrackLayoutId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(record => record.Series)
            .WithMany()
            .HasForeignKey(record => record.SeriesId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(record => record.VehicleClass)
            .WithMany()
            .HasForeignKey(record => record.VehicleClassId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(record => record.Driver)
            .WithMany()
            .HasForeignKey(record => record.DriverId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(record => record.Team)
            .WithMany()
            .HasForeignKey(record => record.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(record => record.Session)
            .WithMany()
            .HasForeignKey(record => record.SessionId)
            .OnDelete(DeleteBehavior.SetNull);

        // The circuit page asks exactly this: current records for this layout.
        builder.HasIndex(record => new { record.TrackLayoutId, record.RecordType, record.IsCurrent })
            .HasDatabaseName("ix_lap_record_layout_type_current");

        // The historical progression list for one record category, fastest first.
        builder.HasIndex(record => new { record.TrackLayoutId, record.SeriesId, record.RecordType, record.LapTimeMs })
            .HasDatabaseName("ix_lap_record_layout_series_type_time");

        builder.ToTable(table => table.HasCheckConstraint(
            "ck_lap_record_time_positive",
            "lap_time_ms > 0"));
    }
}
