using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Catalog;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class SeriesConfiguration : IEntityTypeConfiguration<Series>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Series> builder)
    {
        builder.ToTable("series");

        builder.HasKey(series => series.Id);

        builder.Property(series => series.Name).HasMaxLength(200).IsRequired();
        builder.Property(series => series.ShortName).HasMaxLength(30).IsRequired();
        builder.Property(series => series.Slug).HasMaxLength(120).IsRequired();
        builder.Property(series => series.SanctioningBody).HasMaxLength(100);
        builder.Property(series => series.Description).HasMaxLength(4000);
        builder.Property(series => series.LogoUrl).HasMaxLength(1000);
        builder.Property(series => series.WebsiteUrl).HasMaxLength(1000);
        builder.Property(series => series.PrimaryColorHex).HasMaxLength(9);

        // Slugs are the public URL key, so uniqueness is a correctness requirement, not a hint.
        // Filtered on is_deleted so archiving a series frees its slug for reuse.
        builder.HasIndex(series => series.Slug)
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ux_series_slug");

        // Serves the series picker and the "browse by discipline" navigation.
        builder.HasIndex(series => new { series.IsActive, series.Discipline, series.DisplayOrder })
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_series_active_discipline_order");
    }
}
