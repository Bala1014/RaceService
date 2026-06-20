using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities;

namespace RaceService.Application.Database.Configurations
{
    public class PastResultConfiguration : IEntityTypeConfiguration<PastResult>
    {
        public void Configure(EntityTypeBuilder<PastResult> builder)
        {
            builder.ToTable("PastResult");

            builder.HasKey(pastResult => pastResult.Id);

            builder.HasIndex(pastResult => new { pastResult.RaceId, pastResult.Year });

            builder.HasOne(pastResult => pastResult.Race)
                .WithMany(race => race.PastResults)
                .HasForeignKey(pastResult => pastResult.RaceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
