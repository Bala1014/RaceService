using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Catalog;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class VehicleClassConfiguration : IEntityTypeConfiguration<VehicleClass>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<VehicleClass> builder)
    {
        builder.ToTable("vehicle_class");

        builder.HasKey(vehicleClass => vehicleClass.Id);

        builder.Property(vehicleClass => vehicleClass.Name).HasMaxLength(120).IsRequired();
        builder.Property(vehicleClass => vehicleClass.ShortName).HasMaxLength(30).IsRequired();
        builder.Property(vehicleClass => vehicleClass.Slug).HasMaxLength(120).IsRequired();
        builder.Property(vehicleClass => vehicleClass.Description).HasMaxLength(2000);
        builder.Property(vehicleClass => vehicleClass.ColorHex).HasMaxLength(9);

        builder.HasOne(vehicleClass => vehicleClass.Series)
            .WithMany(series => series.VehicleClasses)
            .HasForeignKey(vehicleClass => vehicleClass.SeriesId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique per series. Two partial indexes rather than one, because in Postgres NULL is
        // never equal to NULL: a plain unique index on (series_id, slug) would happily allow two
        // global classes both slugged "gt3".
        builder.HasIndex(vehicleClass => new { vehicleClass.SeriesId, vehicleClass.Slug })
            .IsUnique()
            .HasFilter("series_id is not null")
            .HasDatabaseName("ux_vehicle_class_series_slug");

        builder.HasIndex(vehicleClass => vehicleClass.Slug)
            .IsUnique()
            .HasFilter("series_id is null")
            .HasDatabaseName("ux_vehicle_class_global_slug");

        builder.HasIndex(vehicleClass => new { vehicleClass.SeriesId, vehicleClass.DisplayOrder })
            .HasDatabaseName("ix_vehicle_class_series_order");
    }
}
