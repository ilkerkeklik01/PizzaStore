using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Features.Queries.Pizza.DTOs;

/// <summary>
/// Represents a specific size variant of a pizza with its price and availability
/// </summary>
public class VariantResponseDto
{
    /// <summary>
    /// Unique identifier for the pizza variant
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440006</example>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Size of the pizza variant
    /// </summary>
    public PizzaSize Size { get; set; }
    
    /// <summary>
    /// Price for this specific size variant
    /// </summary>
    /// <example>12.99</example>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Indicates whether this variant is currently available for ordering
    /// </summary>
    public bool IsAvailable { get; set; }
}
