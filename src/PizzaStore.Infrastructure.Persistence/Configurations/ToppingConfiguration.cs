using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Infrastructure.Persistence.Configurations;

public class ToppingConfiguration : IEntityTypeConfiguration<Topping>
{
    public void Configure(EntityTypeBuilder<Topping> builder)
    {
        builder.ToTable("Toppings");
        
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(t => t.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
            
        builder.Property(t => t.IsAvailable)
            .IsRequired();
            
        builder.HasIndex(t => t.IsAvailable);
    }
}
