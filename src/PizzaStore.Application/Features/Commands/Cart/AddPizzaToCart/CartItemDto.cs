using PizzaStore.Application.Extensions;

namespace PizzaStore.Application.Features.Commands.Cart.AddPizzaToCart;

/// <summary>
/// Represents a pizza item in the shopping cart with pricing and customization details
/// </summary>
public class CartItemDto
{
    /// <summary>
    /// Unique identifier for the cart item
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440008</example>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Identifier of the cart this item belongs to
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440009</example>
    public string CartId { get; set; } = string.Empty;
    
    /// <summary>
    /// Identifier of the pizza variant (size-specific) in the cart
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440006</example>
    public string PizzaVariantId { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the pizza variant size
    /// </summary>
    /// <example>Large</example>
    public string PizzaVariantName { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the pizza
    /// </summary>
    /// <example>Margherita</example>
    public string PizzaName { get; set; } = string.Empty;
    
    /// <summary>
    /// Base price of the pizza variant without toppings
    /// </summary>
    /// <example>12.99</example>
    public decimal BasePrice { get; set; }
    
    /// <summary>
    /// Number of pizzas in the cart item
    /// </summary>
    /// <example>2</example>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Optional special instructions or customization notes from the customer
    /// </summary>
    /// <example>Extra cheese, well done</example>
    public string SpecialInstructions { get; set; } = string.Empty;
    
    /// <summary>
    /// List of toppings added to this cart item
    /// </summary>
    public List<CartItemToppingDto> Toppings { get; set; } = new();
    
    /// <summary>
    /// Total cost of all toppings for a single pizza
    /// </summary>
    /// <example>3.00</example>
    public decimal ToppingsTotal { get; set; }
    
    /// <summary>
    /// Total price for a single pizza including base price and toppings
    /// </summary>
    /// <example>15.99</example>
    public decimal ItemPrice { get; set; }
    
    /// <summary>
    /// Total price for all pizzas in this cart item (ItemPrice * Quantity)
    /// </summary>
    /// <example>31.98</example>
    public decimal SubTotal { get; set; }

    public static CartItemDto FromEntity(Domain.Entities.CartItem cartItem)
    {
        var toppingsTotal = cartItem.CartItemToppings.Sum(t => t.Topping.Price);
        var itemPrice = cartItem.CalculateItemPrice();
        var subTotal = cartItem.CalculateSubtotal();

        return new CartItemDto
        {
            Id = cartItem.Id,
            CartId = cartItem.CartId,
            PizzaVariantId = cartItem.PizzaVariantId,
            PizzaVariantName = $"{cartItem.PizzaVariant.Size}",
            PizzaName = cartItem.PizzaVariant.Pizza.Name,
            BasePrice = cartItem.PizzaVariant.Price,
            Quantity = cartItem.Quantity,
            SpecialInstructions = cartItem.SpecialInstructions,
            Toppings = cartItem.CartItemToppings
                .Select(t => new CartItemToppingDto
                {
                    ToppingId = t.ToppingId,
                    ToppingName = t.Topping.Name,
                    Price = t.Topping.Price
                })
                .ToList(),
            ToppingsTotal = toppingsTotal,
            ItemPrice = itemPrice,
            SubTotal = subTotal
        };
    }
}

/// <summary>
/// Represents a topping added to a cart item
/// </summary>
public class CartItemToppingDto
{
    /// <summary>
    /// Unique identifier for the topping
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440007</example>
    public string ToppingId { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the topping
    /// </summary>
    /// <example>Extra Cheese</example>
    public string ToppingName { get; set; } = string.Empty;
    
    /// <summary>
    /// Price of the topping
    /// </summary>
    /// <example>1.50</example>
    public decimal Price { get; set; }
}
