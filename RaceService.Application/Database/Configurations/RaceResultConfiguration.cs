using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities;

namespace RaceService.Application.Database.Configurations
{
    public class RaceResultConfiguration : IEntityTypeConfiguration<RaceResult>
    {
        public void Configure(EntityTypeBuilder<RaceResult> builder)
        {
            builder.ToTable("RaceResult");

            builder.HasKey(result => result.Id);

            builder.HasIndex(result => result.DriverId);
            builder.HasIndex(result => result.RaceEntryId);
            builder.HasIndex(result => new { result.PastResultId, result.Position });

            builder.HasOne(result => result.Driver)
                .WithMany()
                .HasForeignKey(result => result.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(result => result.RaceEntry)
                .WithMany(entry => entry.Results)
                .HasForeignKey(result => result.RaceEntryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(result => result.PastResult)
                .WithMany(pastResult => pastResult.RaceResults)
                .HasForeignKey(result => result.PastResultId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
