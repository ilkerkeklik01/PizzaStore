namespace PizzaStore.Application.Features.Queries.Order;

/// <summary>
/// Represents a topping added to an order item, capturing the topping details at the time of order
/// </summary>
public class OrderItemToppingDto
{
    /// <summary>
    /// Unique identifier for the order item topping
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440003</example>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Identifier of the topping that was added
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440004</example>
    public string ToppingId { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the topping at the time the order was placed (immutable snapshot)
    /// </summary>
    /// <example>Extra Cheese</example>
    public string ToppingNameAtOrder { get; set; } = string.Empty;
    
    /// <summary>
    /// Price of the topping at the time the order was placed (immutable snapshot)
    /// </summary>
    /// <example>1.50</example>
    public decimal ToppingPriceAtOrder { get; set; }

    public static OrderItemToppingDto FromEntity(Domain.Entities.OrderItemTopping topping)
    {
        return new OrderItemToppingDto
        {
            Id = topping.Id,
            ToppingId = topping.ToppingId ?? string.Empty,
            ToppingNameAtOrder = topping.ToppingNameAtOrder,
            ToppingPriceAtOrder = topping.ToppingPriceAtOrder
        };
    }
}
