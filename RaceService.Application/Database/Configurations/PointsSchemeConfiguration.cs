using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Catalog;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class PointsSchemeConfiguration : IEntityTypeConfiguration<PointsScheme>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PointsScheme> builder)
    {
        builder.ToTable("points_scheme");

        builder.HasKey(scheme => scheme.Id);

        builder.Property(scheme => scheme.Name).HasMaxLength(150).IsRequired();
        builder.Property(scheme => scheme.FastestLapPoints).HasPrecision(6, 2);
        builder.Property(scheme => scheme.PolePositionPoints).HasPrecision(6, 2);
        builder.Property(scheme => scheme.PointsMultiplier).HasPrecision(6, 2).HasDefaultValue(1m);
        builder.Property(scheme => scheme.MinimumDistancePercent).HasPrecision(5, 2);
        builder.Property(scheme => scheme.MinimumDriverDistancePercent).HasPrecision(5, 2);

        builder.HasOne(scheme => scheme.Season)
            .WithMany(season => season.PointsSchemes)
            .HasForeignKey(scheme => scheme.SeasonId)
            // A scheme is meaningless without its season, so it is genuinely owned.
            .OnDelete(DeleteBehavior.Cascade);

        // One scheme per session type per championship per season; a second would make the
        // points calculation ambiguous.
        builder.HasIndex(scheme => new { scheme.SeasonId, scheme.AppliesToSessionType, scheme.ChampionshipType })
            .IsUnique()
            .HasDatabaseName("ux_points_scheme_season_session_championship");
    }
}
