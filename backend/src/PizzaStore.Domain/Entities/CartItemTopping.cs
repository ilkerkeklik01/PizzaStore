namespace PizzaStore.Domain.Entities;

/// <summary>
/// Join entity representing toppings selected for a cart item
/// Uses surrogate ID for consistency with OrderItemTopping
/// </summary>
public class CartItemTopping : BaseEntity
{
    public string CartItemId { get; set; } = string.Empty;
    public CartItem CartItem { get; set; } = null!;
    
    public string ToppingId { get; set; } = string.Empty;
    public Topping Topping { get; set; } = null!;
}
