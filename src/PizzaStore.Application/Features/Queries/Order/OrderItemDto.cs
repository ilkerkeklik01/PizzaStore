namespace PizzaStore.Application.Features.Queries.Order;

/// <summary>
/// Represents a single pizza item within an order, capturing the pizza details and customizations at the time of order
/// </summary>
public class OrderItemDto
{
    /// <summary>
    /// Unique identifier for the order item
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440001</example>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Identifier of the parent order
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    public string OrderId { get; set; } = string.Empty;
    
    /// <summary>
    /// Identifier of the pizza variant (size-specific) that was ordered
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440002</example>
    public string PizzaVariantId { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the pizza at the time the order was placed (immutable snapshot)
    /// </summary>
    /// <example>Margherita</example>
    public string PizzaNameAtOrder { get; set; } = string.Empty;
    
    /// <summary>
    /// Size of the pizza at the time the order was placed (immutable snapshot)
    /// </summary>
    /// <example>Large</example>
    public string PizzaSizeAtOrder { get; set; } = string.Empty;
    
    /// <summary>
    /// Base price of the pizza at the time the order was placed (immutable snapshot)
    /// </summary>
    /// <example>12.99</example>
    public decimal PizzaBasePriceAtOrder { get; set; }
    
    /// <summary>
    /// Number of pizzas of this type in the order
    /// </summary>
    /// <example>2</example>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Optional special instructions or customization notes from the customer
    /// </summary>
    /// <example>Extra cheese, well done</example>
    public string SpecialInstructions { get; set; } = string.Empty;
    
    /// <summary>
    /// Total price for this item including base price, toppings, and quantity (immutable snapshot)
    /// </summary>
    /// <example>28.98</example>
    public decimal SubtotalAtOrder { get; set; }
    
    /// <summary>
    /// List of additional toppings selected for this pizza
    /// </summary>
    public List<OrderItemToppingDto> Toppings { get; set; } = new();

    public static OrderItemDto FromEntity(Domain.Entities.OrderItem orderItem)
    {
        return new OrderItemDto
        {
            Id = orderItem.Id,
            OrderId = orderItem.OrderId,
            PizzaVariantId = orderItem.PizzaVariantId ?? string.Empty,
            PizzaNameAtOrder = orderItem.PizzaNameAtOrder,
            PizzaSizeAtOrder = orderItem.PizzaSizeAtOrder,
            PizzaBasePriceAtOrder = orderItem.PizzaBasePriceAtOrder,
            Quantity = orderItem.Quantity,
            SpecialInstructions = orderItem.SpecialInstructions,
            SubtotalAtOrder = orderItem.SubtotalAtOrder,
            Toppings = orderItem.OrderItemToppings
                .Select(t => OrderItemToppingDto.FromEntity(t))
                .ToList()
        };
    }
}
