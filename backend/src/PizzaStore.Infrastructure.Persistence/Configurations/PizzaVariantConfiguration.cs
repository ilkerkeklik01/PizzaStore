using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PizzaStore.Domain.Entities;

namespace PizzaStore.Infrastructure.Persistence.Configurations;

public class PizzaVariantConfiguration : IEntityTypeConfiguration<PizzaVariant>
{
    public void Configure(EntityTypeBuilder<PizzaVariant> builder)
    {
        builder.ToTable("PizzaVariants");
        
        builder.HasKey(pv => pv.Id);
        
        builder.Property(pv => pv.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
            
        builder.Property(pv => pv.Size)
            .IsRequired();
            
        builder.Property(pv => pv.IsAvailable)
            .IsRequired();
            
        builder.HasIndex(pv => new { pv.PizzaId, pv.Size })
            .IsUnique()
            .HasDatabaseName("IX_PizzaVariants_PizzaId_Size_Unique");
            
        builder.HasIndex(pv => pv.IsAvailable);
        
        builder.HasOne(pv => pv.Pizza)
            .WithMany(p => p.Variants)
            .HasForeignKey(pv => pv.PizzaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
