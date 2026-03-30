using PizzaStore.Domain.Entities;

namespace PizzaStore.Domain.Interfaces;

public interface ICartRepository : IRepository<Cart>
{
    /// <summary>
    /// Gets a user's cart including all cart items and their details
    /// </summary>
    Task<Cart?> GetCartWithItemsByUserIdAsync(string userId);
    
    /// <summary>
    /// Gets or creates a cart for the specified user
    /// </summary>
    Task<Cart> GetOrCreateCartForUserAsync(string userId);
    
    /// <summary>
    /// Gets a specific cart item by ID
    /// </summary>
    Task<CartItem?> GetCartItemByIdAsync(string cartItemId);
    
    /// <summary>
    /// Checks if a cart item belongs to a specific user
    /// </summary>
    Task<bool> IsCartItemOwnedByUserAsync(string cartItemId, string userId);
}
