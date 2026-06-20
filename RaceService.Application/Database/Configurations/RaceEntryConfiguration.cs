using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities;

namespace RaceService.Application.Database.Configurations
{
    public class RaceEntryConfiguration : IEntityTypeConfiguration<RaceEntry>
    {
        public void Configure(EntityTypeBuilder<RaceEntry> builder)
        {
            builder.ToTable("RaceEntry");

            builder.HasKey(entry => entry.Id);

            builder.Property(entry => entry.SeriesType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(entry => entry.CarClass)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(entry => entry.CarManufacturer)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(entry => entry.CarModel)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(entry => entry.PUManufacturer)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(entry => entry.RaceEntryName)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex(entry => entry.RaceId);

            builder.HasOne(entry => entry.Race)
                .WithMany(race => race.Entries)
                .HasForeignKey(entry => entry.RaceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
