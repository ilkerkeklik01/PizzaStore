using PizzaStore.Domain.Entities;

namespace PizzaStore.Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    /// <summary>
    /// Gets all orders for a specific user
    /// </summary>
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
    
    /// <summary>
    /// Gets an order by ID with all related details (items, toppings)
    /// </summary>
    Task<Order?> GetOrderByIdWithDetailsAsync(string orderId);
    
    /// <summary>
    /// Gets all orders with optional filtering by status and user
    /// </summary>
    Task<IEnumerable<Order>> GetAllOrdersWithDetailsAsync(OrderStatus? status = null, string? userId = null);
    
    /// <summary>
    /// Checks if an order belongs to a specific user
    /// </summary>
    Task<bool> IsOrderOwnedByUserAsync(string orderId, string userId);
}
