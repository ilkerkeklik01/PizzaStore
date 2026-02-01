using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        
        builder.HasKey(oi => oi.Id);
        
        builder.Property(oi => oi.PizzaNameAtOrder)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(oi => oi.PizzaSizeAtOrder)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(oi => oi.PizzaBasePriceAtOrder)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
            
        builder.Property(oi => oi.SubtotalAtOrder)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
            
        builder.Property(oi => oi.Quantity)
            .IsRequired();
            
        builder.Property(oi => oi.SpecialInstructions)
            .HasMaxLength(500);
            
        builder.HasIndex(oi => oi.OrderId);
        builder.HasIndex(oi => oi.PizzaVariantId);
        
        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(oi => oi.PizzaVariant)
            .WithMany()
            .HasForeignKey(oi => oi.PizzaVariantId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
