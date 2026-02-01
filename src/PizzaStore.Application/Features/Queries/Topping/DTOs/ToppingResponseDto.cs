namespace PizzaStore.Application.Features.Queries.Topping.DTOs;

/// <summary>
/// Represents a topping that can be added to pizzas
/// </summary>
public class ToppingResponseDto
{
    /// <summary>
    /// Unique identifier for the topping
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440007</example>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the topping
    /// </summary>
    /// <example>Extra Cheese</example>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional cost for adding this topping
    /// </summary>
    /// <example>1.50</example>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Indicates whether the topping is currently available
    /// </summary>
    public bool IsAvailable { get; set; }
}
