using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities;

namespace RaceService.Application.Database.Configurations
{
    public class TrackConfiguration : IEntityTypeConfiguration<Track>
    {
        public void Configure(EntityTypeBuilder<Track> builder)
        {
            builder.ToTable("Track");

            builder.HasKey(track => track.Id);

            builder.Property(track => track.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(track => track.Location)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(track => track.Country)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(track => track.TimeZone)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(track => track.ImageUrl)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(track => track.LengthInKm)
                .HasPrecision(6, 3);
        }
    }
}
