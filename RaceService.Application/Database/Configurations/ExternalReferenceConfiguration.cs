using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Integration;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class ExternalReferenceConfiguration : IEntityTypeConfiguration<ExternalReference>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ExternalReference> builder)
    {
        builder.ToTable("external_reference");

        builder.HasKey(reference => reference.Id);

        builder.Property(reference => reference.Provider).HasMaxLength(60).IsRequired();
        builder.Property(reference => reference.EntityType).HasMaxLength(60).IsRequired();
        builder.Property(reference => reference.ExternalId).HasMaxLength(200).IsRequired();
        builder.Property(reference => reference.PayloadHash).HasMaxLength(64);
        builder.Property(reference => reference.LastPayloadJson).HasColumnType("jsonb");

        // The constraint that makes imports idempotent: a provider record maps to exactly one
        // internal row, so re-running a feed updates rather than duplicates.
        builder.HasIndex(reference => new { reference.Provider, reference.EntityType, reference.ExternalId })
            .IsUnique()
            .HasDatabaseName("ux_external_reference_provider_type_external");

        // The reverse direction: given an internal row, what does each provider call it.
        builder.HasIndex(reference => new { reference.EntityType, reference.InternalId })
            .HasDatabaseName("ix_external_reference_type_internal");
    }
}
