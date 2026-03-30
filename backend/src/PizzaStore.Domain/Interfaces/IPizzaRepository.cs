using PizzaStore.Domain.Entities;

namespace PizzaStore.Domain.Interfaces;

public interface IPizzaRepository : IRepository<Pizza>
{
    /// <summary>
    /// Gets a pizza by ID including all its variants
    /// </summary>
    Task<Pizza?> GetByIdWithVariantsAsync(string id);
    
    /// <summary>
    /// Gets all pizzas including their variants (including unavailable)
    /// </summary>
    Task<IEnumerable<Pizza>> GetAllWithVariantsAsync();
    
    /// <summary>
    /// Gets only available pizzas with their available variants
    /// </summary>
    Task<IEnumerable<Pizza>> GetAvailablePizzasWithVariantsAsync();
}
