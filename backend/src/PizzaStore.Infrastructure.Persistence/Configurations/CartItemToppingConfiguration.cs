using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Infrastructure.Persistence.Configurations;

public class CartItemToppingConfiguration : IEntityTypeConfiguration<CartItemTopping>
{
    public void Configure(EntityTypeBuilder<CartItemTopping> builder)
    {
        builder.ToTable("CartItemToppings");
        
        // Use surrogate ID as primary key (consistent with OrderItemTopping)
        builder.HasKey(cit => cit.Id);
        
        // Add unique index on CartItemId + ToppingId (prevent duplicates)
        builder.HasIndex(cit => new { cit.CartItemId, cit.ToppingId })
            .IsUnique();
        
        builder.HasOne(cit => cit.CartItem)
            .WithMany(ci => ci.CartItemToppings)
            .HasForeignKey(cit => cit.CartItemId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(cit => cit.Topping)
            .WithMany()
            .HasForeignKey(cit => cit.ToppingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
