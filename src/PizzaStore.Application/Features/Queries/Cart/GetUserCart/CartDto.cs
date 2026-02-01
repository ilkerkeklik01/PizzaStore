using PizzaStore.Application.Features.Commands.Cart.AddPizzaToCart;

namespace PizzaStore.Application.Features.Queries.Cart.GetUserCart;

/// <summary>
/// Represents a user's shopping cart with all items and pricing totals
/// </summary>
public class CartDto
{
    /// <summary>
    /// Unique identifier for the cart
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440009</example>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Identifier of the user who owns the cart
    /// </summary>
    /// <example>auth0|123456789</example>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// List of pizza items in the cart
    /// </summary>
    public List<CartItemDto> Items { get; set; } = new();
    
    /// <summary>
    /// Subtotal of all items in the cart before taxes and fees
    /// </summary>
    /// <example>45.99</example>
    public decimal SubTotal => Items.Sum(item => item.SubTotal);
    
    /// <summary>
    /// Total price of the cart (can be extended for taxes, discounts, etc.)
    /// </summary>
    /// <example>45.99</example>
    public decimal Total => SubTotal;
    
    /// <summary>
    /// Number of distinct items in the cart
    /// </summary>
    /// <example>3</example>
    public int ItemCount => Items.Count;
    
    /// <summary>
    /// Total quantity of all pizzas in the cart
    /// </summary>
    /// <example>5</example>
    public int TotalQuantity => Items.Sum(item => item.Quantity);
}
