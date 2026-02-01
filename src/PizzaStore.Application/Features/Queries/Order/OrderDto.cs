using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Features.Queries.Order;

/// <summary>
/// Represents a complete pizza order with all items, pricing, and status information
/// </summary>
public class OrderDto
{
    /// <summary>
    /// Unique identifier for the order
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Identifier of the user who placed the order
    /// </summary>
    /// <example>auth0|123456789</example>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Total price of the order including all items and toppings
    /// </summary>
    /// <example>45.99</example>
    public decimal TotalPrice { get; set; }
    
    /// <summary>
    /// Current status of the order in the fulfillment lifecycle
    /// </summary>
    public OrderStatus Status { get; set; }
    
    /// <summary>
    /// Date and time when the order was created
    /// </summary>
    /// <example>2024-02-01T10:30:00Z</example>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Date and time when the order was confirmed, null if not yet confirmed
    /// </summary>
    /// <example>2024-02-01T10:32:00Z</example>
    public DateTime? ConfirmedAt { get; set; }
    
    /// <summary>
    /// Date and time when the order was completed/delivered, null if not yet completed
    /// </summary>
    /// <example>2024-02-01T11:15:00Z</example>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Date and time when the order was cancelled, null if not cancelled
    /// </summary>
    /// <example>2024-02-01T10:35:00Z</example>
    public DateTime? CancelledAt { get; set; }
    
    /// <summary>
    /// List of pizza items included in this order
    /// </summary>
    public List<OrderItemDto> Items { get; set; } = new();

    public static OrderDto FromEntity(Domain.Entities.Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            TotalPrice = order.TotalPrice,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            ConfirmedAt = order.ConfirmedAt,
            CompletedAt = order.CompletedAt,
            CancelledAt = order.CancelledAt,
            Items = order.OrderItems
                .Select(item => OrderItemDto.FromEntity(item))
                .ToList()
        };
    }
}
