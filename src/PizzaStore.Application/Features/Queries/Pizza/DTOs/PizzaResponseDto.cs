using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Features.Queries.Pizza.DTOs;

/// <summary>
/// Represents a pizza menu item with all its available variants and details
/// </summary>
public class PizzaResponseDto
{
    /// <summary>
    /// Unique identifier for the pizza
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440005</example>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the pizza
    /// </summary>
    /// <example>Margherita</example>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description of the pizza and its ingredients
    /// </summary>
    /// <example>Classic Italian pizza with tomato sauce, fresh mozzarella, and basil</example>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Category or specialty type of the pizza
    /// </summary>
    public PizzaType Type { get; set; }
    
    /// <summary>
    /// URL to the pizza's image, null if no image is available
    /// </summary>
    /// <example>https://example.com/images/margherita.jpg</example>
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// Indicates whether the pizza is currently available for ordering
    /// </summary>
    public bool IsAvailable { get; set; }
    
    /// <summary>
    /// List of available size variants for this pizza with their respective prices
    /// </summary>
    public List<VariantResponseDto> Variants { get; set; } = new();
}
