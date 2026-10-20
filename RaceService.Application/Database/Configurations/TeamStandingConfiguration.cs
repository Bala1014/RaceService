using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Standings;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class TeamStandingConfiguration : IEntityTypeConfiguration<TeamStanding>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TeamStanding> builder)
    {
        builder.ToTable("team_standing");

        builder.HasKey(standing => standing.Id);

        builder.Property(standing => standing.Points).HasPrecision(8, 2);
        builder.Property(standing => standing.PointsBeforeDrops).HasPrecision(8, 2);

        builder.HasOne(standing => standing.Season)
            .WithMany(season => season.TeamStandings)
            .HasForeignKey(standing => standing.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(standing => standing.Team)
            .WithMany(team => team.Standings)
            .HasForeignKey(standing => standing.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(standing => standing.VehicleClass)
            .WithMany(vehicleClass => vehicleClass.TeamStandings)
            .HasForeignKey(standing => standing.VehicleClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(standing => standing.AfterEvent)
            .WithMany()
            .HasForeignKey(standing => standing.AfterEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(standing => new { standing.SeasonId, standing.TeamId, standing.VehicleClassId, standing.AfterEventId })
            .IsUnique()
            .HasDatabaseName("ux_team_standing_season_team_class_event");

        builder.HasIndex(standing => new { standing.SeasonId, standing.Position })
            .HasFilter("after_event_id is null")
            .HasDatabaseName("ix_team_standing_current_by_position");
    }
}
