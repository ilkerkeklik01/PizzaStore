namespace PizzaStore.Domain.Entities;

/// <summary>
/// Represents the lifecycle status of a pizza order from creation to completion
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order has been received but not yet confirmed by the system
    /// </summary>
    Pending,
    
    /// <summary>
    /// Order has been confirmed and accepted for preparation
    /// </summary>
    Confirmed,
    
    /// <summary>
    /// Pizza is currently being prepared in the kitchen
    /// </summary>
    Preparing,
    
    /// <summary>
    /// Order has been dispatched and is on its way to the customer
    /// </summary>
    OutForDelivery,
    
    /// <summary>
    /// Order has been successfully delivered to the customer
    /// </summary>
    Delivered,
    
    /// <summary>
    /// Order has been cancelled and will not be fulfilled
    /// </summary>
    Cancelled
}
