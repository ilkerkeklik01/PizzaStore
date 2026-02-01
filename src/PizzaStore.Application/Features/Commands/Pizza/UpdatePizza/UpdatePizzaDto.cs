using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Features.Commands.Pizza.UpdatePizza;

/// <summary>
/// Data transfer object for updating an existing pizza's details
/// </summary>
public class UpdatePizzaDto
{
    /// <summary>
    /// Updated name of the pizza
    /// </summary>
    /// <example>Margherita Supreme</example>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Updated description of the pizza
    /// </summary>
    /// <example>Classic Italian pizza with extra toppings</example>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Updated category or specialty type of the pizza
    /// </summary>
    public PizzaType Type { get; set; }
    
    /// <summary>
    /// Updated URL to the pizza's image
    /// </summary>
    /// <example>https://example.com/images/margherita-supreme.jpg</example>
    public string ImageUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Indicates whether the pizza should be available for ordering
    /// </summary>
    public bool IsAvailable { get; set; } = true;
}
