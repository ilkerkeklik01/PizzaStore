using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Infrastructure.Persistence.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");
        
        builder.HasKey(ci => ci.Id);
        
        builder.Property(ci => ci.Quantity)
            .IsRequired();
            
        builder.Property(ci => ci.SpecialInstructions)
            .HasMaxLength(500);
            
        builder.HasIndex(ci => ci.CartId);
        builder.HasIndex(ci => ci.PizzaVariantId);
        
        builder.HasOne(ci => ci.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(ci => ci.PizzaVariant)
            .WithMany()
            .HasForeignKey(ci => ci.PizzaVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
