using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Catalog;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class PointsSchemeEntryConfiguration : IEntityTypeConfiguration<PointsSchemeEntry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PointsSchemeEntry> builder)
    {
        builder.ToTable("points_scheme_entry");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Points).HasPrecision(6, 2);

        builder.HasOne(entry => entry.PointsScheme)
            .WithMany(scheme => scheme.Entries)
            .HasForeignKey(entry => entry.PointsSchemeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Also the lookup path: given a scheme and a finishing position, find the points.
        builder.HasIndex(entry => new { entry.PointsSchemeId, entry.Position })
            .IsUnique()
            .HasDatabaseName("ux_points_scheme_entry_scheme_position");

        builder.ToTable(table => table.HasCheckConstraint(
            "ck_points_scheme_entry_position_positive",
            "position >= 1"));
    }
}
