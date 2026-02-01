namespace PizzaStore.Domain.Entities;

public class OrderItemTopping : BaseEntity
{
    public string OrderItemId { get; set; } = string.Empty;
    public OrderItem OrderItem { get; set; } = null!;
    
    public string? ToppingId { get; set; }
    public Topping? Topping { get; set; }
    
    public string ToppingNameAtOrder { get; set; } = string.Empty;
    public decimal ToppingPriceAtOrder { get; set; }
}
