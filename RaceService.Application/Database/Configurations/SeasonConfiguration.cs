using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Catalog;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class SeasonConfiguration : IEntityTypeConfiguration<Season>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Season> builder)
    {
        builder.ToTable("season");

        builder.HasKey(season => season.Id);

        builder.Property(season => season.Name).HasMaxLength(250).IsRequired();
        builder.Property(season => season.Slug).HasMaxLength(120).IsRequired();
        builder.Property(season => season.Description).HasMaxLength(4000);
        builder.Property(season => season.BannerImageUrl).HasMaxLength(1000);

        builder.HasOne(season => season.Series)
            .WithMany(series => series.Seasons)
            .HasForeignKey(season => season.SeriesId)
            // Restrict, not Cascade: deleting a series must never silently take a decade of
            // race history with it. The soft-delete interceptor makes this a non-event anyway.
            .OnDelete(DeleteBehavior.Restrict);

        // One season per series per year is a domain invariant, so the database enforces it.
        builder.HasIndex(season => new { season.SeriesId, season.Year })
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ux_season_series_year");

        // Backs /series/{slug}/seasons/{slug} lookups.
        builder.HasIndex(season => new { season.SeriesId, season.Slug })
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ux_season_series_slug");

        // "Which seasons are running now" across all series, for the home page.
        builder.HasIndex(season => new { season.Status, season.Year })
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_season_status_year");

        builder.ToTable(table => table.HasCheckConstraint(
            "ck_season_year_range",
            "year between 1894 and 2200"));

        builder.ToTable(table => table.HasCheckConstraint(
            "ck_season_date_order",
            @"start_date is null or end_date is null or start_date <= end_date"));
    }
}
