using PizzaStore.Domain.Entities;

namespace PizzaStore.Domain.Interfaces;

public interface IToppingRepository : IRepository<Topping>
{
    /// <summary>
    /// Gets all toppings that are currently available
    /// </summary>
    Task<IEnumerable<Topping>> GetAvailableToppingsAsync();
    
    /// <summary>
    /// Gets multiple toppings by their IDs
    /// </summary>
    Task<IEnumerable<Topping>> GetToppingsByIdsAsync(IEnumerable<string> ids);
}
