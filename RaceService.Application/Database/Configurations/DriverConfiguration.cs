using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Competitors;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("driver");

        builder.HasKey(driver => driver.Id);

        builder.Property(driver => driver.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(driver => driver.LastName).HasMaxLength(100).IsRequired();
        builder.Property(driver => driver.FullName).HasMaxLength(200).IsRequired();
        builder.Property(driver => driver.Abbreviation).HasMaxLength(5).IsFixedLength(false);
        builder.Property(driver => driver.Slug).HasMaxLength(150).IsRequired();
        builder.Property(driver => driver.CountryCode).HasMaxLength(3).IsFixedLength();
        builder.Property(driver => driver.Nationality).HasMaxLength(100);
        builder.Property(driver => driver.PlaceOfBirth).HasMaxLength(150);
        builder.Property(driver => driver.ImageUrl).HasMaxLength(1000);
        builder.Property(driver => driver.HeadshotUrl).HasMaxLength(1000);
        builder.Property(driver => driver.WebsiteUrl).HasMaxLength(1000);
        builder.Property(driver => driver.Biography).HasMaxLength(8000);

        builder.HasIndex(driver => driver.Slug)
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ux_driver_slug");

        // Name search from the admin UI and the site's search box.
        builder.HasIndex(driver => driver.FullName)
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_driver_full_name");

        builder.HasIndex(driver => new { driver.IsActive, driver.LastName })
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_driver_active_last_name");

        // Not unique: permanent numbers are reused across series and eras.
        builder.HasIndex(driver => driver.PermanentNumber)
            .HasFilter("permanent_number is not null and is_deleted = false")
            .HasDatabaseName("ix_driver_permanent_number");
    }
}
