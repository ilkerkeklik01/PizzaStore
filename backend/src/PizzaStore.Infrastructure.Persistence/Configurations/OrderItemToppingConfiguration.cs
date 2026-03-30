using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Infrastructure.Persistence.Configurations;

public class OrderItemToppingConfiguration : IEntityTypeConfiguration<OrderItemTopping>
{
    public void Configure(EntityTypeBuilder<OrderItemTopping> builder)
    {
        builder.ToTable("OrderItemToppings");
        
        builder.HasKey(oit => oit.Id);
        
        builder.Property(oit => oit.ToppingNameAtOrder)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(oit => oit.ToppingPriceAtOrder)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
            
        builder.HasIndex(oit => oit.OrderItemId);
        
        builder.HasOne(oit => oit.OrderItem)
            .WithMany(oi => oi.OrderItemToppings)
            .HasForeignKey(oit => oit.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(oit => oit.Topping)
            .WithMany()
            .HasForeignKey(oit => oit.ToppingId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
