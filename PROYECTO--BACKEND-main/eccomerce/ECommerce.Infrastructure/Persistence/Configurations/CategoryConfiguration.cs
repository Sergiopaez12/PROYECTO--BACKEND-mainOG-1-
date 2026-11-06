using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public const int ElectronicaId = 1;
    public const int RopaId = 2;
    public const int HogarId = 3;

    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);

        builder.HasData(
            new Category(ElectronicaId, "Electrónica"),
            new Category(RopaId, "Ropa"),
            new Category(HogarId, "Hogar")
        );
    }
}
