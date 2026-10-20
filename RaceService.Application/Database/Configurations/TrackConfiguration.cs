using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Venues;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class TrackConfiguration : IEntityTypeConfiguration<Track>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Track> builder)
    {
        builder.ToTable("track");

        builder.HasKey(track => track.Id);

        builder.Property(track => track.Name).HasMaxLength(200).IsRequired();
        builder.Property(track => track.OfficialName).HasMaxLength(250);
        builder.Property(track => track.Slug).HasMaxLength(150).IsRequired();
        builder.Property(track => track.City).HasMaxLength(120);
        builder.Property(track => track.Country).HasMaxLength(100).IsRequired();
        builder.Property(track => track.CountryCode).HasMaxLength(3).IsFixedLength();
        builder.Property(track => track.TimeZoneId).HasMaxLength(80).IsRequired().HasDefaultValue("UTC");
        builder.Property(track => track.Description).HasMaxLength(4000);
        builder.Property(track => track.ImageUrl).HasMaxLength(1000);
        builder.Property(track => track.WebsiteUrl).HasMaxLength(1000);

        builder.HasIndex(track => track.Slug)
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ux_track_slug");

        // Backs the "circuits by country" browse page.
        builder.HasIndex(track => new { track.Country, track.Name })
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_track_country_name");

        builder.ToTable(table => table.HasCheckConstraint(
            "ck_track_latitude_range",
            "latitude is null or (latitude between -90 and 90)"));

        builder.ToTable(table => table.HasCheckConstraint(
            "ck_track_longitude_range",
            "longitude is null or (longitude between -180 and 180)"));
    }
}
