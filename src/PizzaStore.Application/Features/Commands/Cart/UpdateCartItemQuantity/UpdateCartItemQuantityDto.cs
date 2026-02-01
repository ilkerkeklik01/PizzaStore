namespace PizzaStore.Application.Features.Commands.Cart.UpdateCartItemQuantity;

/// <summary>
/// Data transfer object for updating the quantity and instructions of a cart item
/// </summary>
public class UpdateCartItemQuantityDto
{
    /// <summary>
    /// Identifier of the cart item to update
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440008</example>
    public string CartItemId { get; set; } = string.Empty;
    
    /// <summary>
    /// New quantity for the cart item
    /// </summary>
    /// <example>3</example>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Optional updated special instructions or customization notes
    /// </summary>
    /// <example>Light sauce, crispy crust</example>
    public string? SpecialInstructions { get; set; }
}
