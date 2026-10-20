using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Database.Conventions;
using RaceService.Application.Domain.Entities.Competitors;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class EntryConfiguration : IEntityTypeConfiguration<Entry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Entry> builder)
    {
        builder.ToTable("entry");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.CarNumber).HasMaxLength(10).IsRequired();
        builder.Property(entry => entry.EntrantName).HasMaxLength(250).IsRequired();
        builder.Property(entry => entry.ChassisModel).HasMaxLength(120);
        builder.Property(entry => entry.EngineModel).HasMaxLength(120);
        builder.Property(entry => entry.LiveryImageUrl).HasMaxLength(1000);
        builder.Property(entry => entry.PrimaryColorHex).HasMaxLength(9);

        builder.Property(entry => entry.Version).AsXminConcurrencyToken();

        builder.HasOne(entry => entry.Season)
            .WithMany(season => season.Entries)
            .HasForeignKey(entry => entry.SeasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entry => entry.Team)
            .WithMany(team => team.Entries)
            .HasForeignKey(entry => entry.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entry => entry.VehicleClass)
            .WithMany(vehicleClass => vehicleClass.Entries)
            .HasForeignKey(entry => entry.VehicleClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entry => entry.ChassisManufacturer)
            .WithMany()
            .HasForeignKey(entry => entry.ChassisManufacturerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entry => entry.EngineManufacturer)
            .WithMany()
            .HasForeignKey(entry => entry.EngineManufacturerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entry => entry.TyreSupplier)
            .WithMany()
            .HasForeignKey(entry => entry.TyreSupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        // A car number identifies a car within its season — this is the constraint that makes
        // "car 51" a meaningful thing to say. Withdrawn entries are excluded so a number freed
        // mid-season can be reissued.
        builder.HasIndex(entry => new { entry.SeasonId, entry.CarNumber })
            .IsUnique()
            .HasFilter("is_deleted = false and status <> 'Withdrawn'")
            .HasDatabaseName("ux_entry_season_car_number");

        // The season entry list, grouped by class the way the published list is.
        builder.HasIndex(entry => new { entry.SeasonId, entry.VehicleClassId, entry.CarNumber })
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_entry_season_class_number");

        // "Every car this team has run", for the team page.
        builder.HasIndex(entry => new { entry.TeamId, entry.SeasonId })
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_entry_team_season");

        // Lets a user find their own submitted entries without scanning the table.
        builder.HasIndex(entry => entry.SubmittedByUserId)
            .HasFilter("submitted_by_user_id is not null")
            .HasDatabaseName("ix_entry_submitted_by_user");
    }
}
