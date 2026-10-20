using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Catalog;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class ManufacturerConfiguration : IEntityTypeConfiguration<Manufacturer>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Manufacturer> builder)
    {
        builder.ToTable("manufacturer");

        builder.HasKey(manufacturer => manufacturer.Id);

        builder.Property(manufacturer => manufacturer.Name).HasMaxLength(150).IsRequired();
        builder.Property(manufacturer => manufacturer.Slug).HasMaxLength(150).IsRequired();
        builder.Property(manufacturer => manufacturer.CountryCode).HasMaxLength(3).IsFixedLength();
        builder.Property(manufacturer => manufacturer.LogoUrl).HasMaxLength(1000);

        builder.HasIndex(manufacturer => manufacturer.Slug)
            .IsUnique()
            .HasDatabaseName("ux_manufacturer_slug");

        // Powers the filtered pickers on the entry admin form.
        builder.HasIndex(manufacturer => new { manufacturer.SuppliesChassis, manufacturer.IsActive })
            .HasDatabaseName("ix_manufacturer_chassis_active");

        builder.HasIndex(manufacturer => new { manufacturer.SuppliesEngines, manufacturer.IsActive })
            .HasDatabaseName("ix_manufacturer_engines_active");
    }
}
