using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Venues;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class TrackLayoutConfiguration : IEntityTypeConfiguration<TrackLayout>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TrackLayout> builder)
    {
        builder.ToTable("track_layout");

        builder.HasKey(layout => layout.Id);

        builder.Property(layout => layout.Name).HasMaxLength(150).IsRequired();
        builder.Property(layout => layout.Slug).HasMaxLength(150).IsRequired();
        builder.Property(layout => layout.MapImageUrl).HasMaxLength(1000);

        builder.HasOne(layout => layout.Track)
            .WithMany(track => track.Layouts)
            .HasForeignKey(layout => layout.TrackId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(layout => new { layout.TrackId, layout.Slug })
            .IsUnique()
            .HasDatabaseName("ux_track_layout_track_slug");

        // At most one current configuration per venue, enforced by a partial unique index rather
        // than by application code that could be bypassed.
        builder.HasIndex(layout => layout.TrackId)
            .IsUnique()
            .HasFilter("is_current = true")
            .HasDatabaseName("ux_track_layout_single_current");

        builder.ToTable(table => table.HasCheckConstraint(
            "ck_track_layout_length_positive",
            "length_meters > 0"));
    }
}
