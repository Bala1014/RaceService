using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities.Integration;

namespace RaceService.Application.Database.Configurations;

/// <inheritdoc />
public sealed class ImportRunConfiguration : IEntityTypeConfiguration<ImportRun>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ImportRun> builder)
    {
        builder.ToTable("import_run");

        builder.HasKey(run => run.Id);

        builder.Property(run => run.Provider).HasMaxLength(60).IsRequired();
        builder.Property(run => run.Resource).HasMaxLength(60).IsRequired();
        builder.Property(run => run.ErrorMessage).HasMaxLength(4000);
        builder.Property(run => run.CorrelationId).HasMaxLength(100);

        // "How did the last few runs of this import go", newest first.
        builder.HasIndex(run => new { run.Provider, run.Resource, run.StartedAtUtc })
            .IsDescending(false, false, true)
            .HasDatabaseName("ix_import_run_provider_resource_started");

        // Finds a run that is still marked running, which is how a crashed import is detected.
        builder.HasIndex(run => run.Status)
            .HasFilter("status = 'Running'")
            .HasDatabaseName("ix_import_run_in_progress");
    }
}
