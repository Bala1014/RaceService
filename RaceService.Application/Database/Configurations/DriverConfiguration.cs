using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceService.Application.Domain.Entities;

namespace RaceService.Application.Database.Configurations
{
    public class DriverConfiguration : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {
            builder.ToTable("Driver");

            builder.HasKey(driver => driver.Id);

            builder.Property(driver => driver.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(driver => driver.Nationality)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(driver => driver.Category)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(driver => driver.Number);
        }
    }
}
