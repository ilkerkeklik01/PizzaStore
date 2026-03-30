using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Infrastructure.Persistence.Configurations;

public class PizzaConfiguration : IEntityTypeConfiguration<Pizza>
{
    public void Configure(EntityTypeBuilder<Pizza> builder)
    {
        builder.ToTable("Pizzas");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(1000);
            
        builder.Property(p => p.ImageUrl)
            .HasMaxLength(500);
            
        builder.Property(p => p.Type)
            .IsRequired();
            
        builder.Property(p => p.IsAvailable)
            .IsRequired();
            
        builder.HasIndex(p => p.IsAvailable);
        
        builder.HasMany(p => p.Variants)
            .WithOne(v => v.Pizza)
            .HasForeignKey(v => v.PizzaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
