using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities;

namespace RaceService.Application.Database.Configurations
{
    public class RaceConfiguration : IEntityTypeConfiguration<Race>
    {
        public void Configure(EntityTypeBuilder<Race> builder)
        {
            builder.ToTable("Race");

            builder.HasKey(race => race.Id);

            builder.Property(race => race.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(race => race.StartTimeUTC)
                .HasColumnName("StartTimeUTC")
                .IsRequired();

            builder.Property(race => race.CreatedAtUTC)
                .HasColumnName("CreatedAt")
                .IsRequired();

            builder.Property(race => race.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.HasIndex(race => race.StartTimeUTC);
            builder.HasIndex(race => new { race.Status, race.StartTimeUTC });

            builder.HasOne(race => race.Track)
                .WithMany(track => track.Races)
                .HasForeignKey(race => race.TrackId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
