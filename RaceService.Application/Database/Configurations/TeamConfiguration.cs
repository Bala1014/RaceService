using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Competitors;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("team");

        builder.HasKey(team => team.Id);

        builder.Property(team => team.Name).HasMaxLength(200).IsRequired();
        builder.Property(team => team.OfficialName).HasMaxLength(300);
        builder.Property(team => team.ShortName).HasMaxLength(50).IsRequired();
        builder.Property(team => team.Slug).HasMaxLength(150).IsRequired();
        builder.Property(team => team.CountryCode).HasMaxLength(3).IsFixedLength();
        builder.Property(team => team.BaseCity).HasMaxLength(120);
        builder.Property(team => team.BaseCountry).HasMaxLength(100);
        builder.Property(team => team.LogoUrl).HasMaxLength(1000);
        builder.Property(team => team.WebsiteUrl).HasMaxLength(1000);
        builder.Property(team => team.PrimaryColorHex).HasMaxLength(9);
        builder.Property(team => team.Description).HasMaxLength(4000);

        builder.HasIndex(team => team.Slug)
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ux_team_slug");

        builder.HasIndex(team => team.Name)
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_team_name");
    }
}
