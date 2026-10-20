using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Database.Conventions;
using RaceService.Application.Domain.Entities.Events;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class RaceEventConfiguration : IEntityTypeConfiguration<RaceEvent>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RaceEvent> builder)
    {
        builder.ToTable("race_event");

        builder.HasKey(raceEvent => raceEvent.Id);

        builder.Property(raceEvent => raceEvent.Name).HasMaxLength(200).IsRequired();
        builder.Property(raceEvent => raceEvent.OfficialName).HasMaxLength(300);
        builder.Property(raceEvent => raceEvent.ShortName).HasMaxLength(60);
        builder.Property(raceEvent => raceEvent.Slug).HasMaxLength(150).IsRequired();
        builder.Property(raceEvent => raceEvent.TimeZoneId).HasMaxLength(80).IsRequired().HasDefaultValue("UTC");
        builder.Property(raceEvent => raceEvent.ShortDescription).HasMaxLength(500);
        builder.Property(raceEvent => raceEvent.Description).HasMaxLength(8000);
        builder.Property(raceEvent => raceEvent.PosterImageUrl).HasMaxLength(1000);
        builder.Property(raceEvent => raceEvent.BannerImageUrl).HasMaxLength(1000);

        // Maps to the Postgres xmin system column: optimistic concurrency for free, with no
        // extra column and no chance of an admin's edit being silently overwritten.
        builder.Property(raceEvent => raceEvent.Version).AsXminConcurrencyToken();

        builder.HasOne(raceEvent => raceEvent.Season)
            .WithMany(season => season.Events)
            .HasForeignKey(raceEvent => raceEvent.SeasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(raceEvent => raceEvent.TrackLayout)
            .WithMany(layout => layout.Events)
            .HasForeignKey(raceEvent => raceEvent.TrackLayoutId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(raceEvent => raceEvent.PreviousEdition)
            .WithMany()
            .HasForeignKey(raceEvent => raceEvent.PreviousEditionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(raceEvent => new { raceEvent.SeasonId, raceEvent.Slug })
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ux_race_event_season_slug");

        // A round number can only belong to one event in a season.
        builder.HasIndex(raceEvent => new { raceEvent.SeasonId, raceEvent.RoundNumber })
            .IsUnique()
            .HasFilter("is_deleted = false and round_number is not null")
            .HasDatabaseName("ux_race_event_season_round");

        // The workhorse index. Every calendar query filters on published status and orders by
        // start time, so the column order here matters: equality predicates first, then the
        // range/sort column, which lets Postgres satisfy the whole query from one index scan
        // without a sort step.
        builder.HasIndex(raceEvent => new { raceEvent.IsPublished, raceEvent.StartTimeUtc })
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_race_event_published_start");

        // "Next race" and the upcoming strip: narrowed further by status.
        builder.HasIndex(raceEvent => new { raceEvent.Status, raceEvent.StartTimeUtc })
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_race_event_status_start");

        // The season calendar page, already in round order.
        builder.HasIndex(raceEvent => new { raceEvent.SeasonId, raceEvent.StartTimeUtc })
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_race_event_season_start");

        // "All races at this circuit", for the venue page's history table.
        builder.HasIndex(raceEvent => new { raceEvent.TrackLayoutId, raceEvent.StartTimeUtc })
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_race_event_layout_start");

        builder.ToTable(table => table.HasCheckConstraint(
            "ck_race_event_time_order",
            "end_time_utc >= start_time_utc"));

        builder.ToTable(table => table.HasCheckConstraint(
            "ck_race_event_round_positive",
            "round_number is null or round_number >= 1"));
    }
}
