using System.ComponentModel.DataAnnotations;

namespace PizzaStore.Domain.Entities;

public class OrderItem : BaseEntity
{
    public string OrderId { get; set; } = string.Empty;
    public Order Order { get; set; } = null!;
    
    public string? PizzaVariantId { get; set; }
    public PizzaVariant? PizzaVariant { get; set; }
    
    public string PizzaNameAtOrder { get; set; } = string.Empty;
    public string PizzaSizeAtOrder { get; set; } = string.Empty;
    public decimal PizzaBasePriceAtOrder { get; set; }
    
    [Range(1, 50, ErrorMessage = "Quantity must be between 1 and 50")]
    public int Quantity { get; set; }

    [MaxLength(500)]
    public string? SpecialInstructions { get; set; } = string.Empty;

    public decimal SubtotalAtOrder { get; set; }
    
    public ICollection<OrderItemTopping> OrderItemToppings { get; set; } = new List<OrderItemTopping>();
}
