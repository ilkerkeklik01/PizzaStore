namespace PizzaStore.Application.Features.Commands.Cart.AddPizzaToCart;

/// <summary>
/// Data transfer object for adding a pizza to the shopping cart
/// </summary>
public class AddPizzaToCartDto
{
    /// <summary>
    /// Identifier of the pizza variant (size-specific) to add to cart
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440006</example>
    public string PizzaVariantId { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of pizzas to add to the cart
    /// </summary>
    /// <example>2</example>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Optional special instructions or customization notes from the customer
    /// </summary>
    /// <example>Extra cheese, well done</example>
    public string? SpecialInstructions { get; set; }
    
    /// <summary>
    /// List of topping identifiers to add to the pizza
    /// </summary>
    public List<string> ToppingIds { get; set; } = new();
}
