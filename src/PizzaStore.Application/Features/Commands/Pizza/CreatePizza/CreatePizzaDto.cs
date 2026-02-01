using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Features.Commands.Pizza.CreatePizza;

/// <summary>
/// Data transfer object for creating a new pizza with its variants
/// </summary>
public class CreatePizzaDto
{
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
    /// URL to the pizza's image
    /// </summary>
    /// <example>https://example.com/images/margherita.jpg</example>
    public string ImageUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// List of size variants to create for this pizza with their respective prices
    /// </summary>
    public List<PizzaVariantDto> Variants { get; set; } = new();
}

/// <summary>
/// Represents a pizza size variant with its price during pizza creation
/// </summary>
public class PizzaVariantDto
{
    /// <summary>
    /// Size of the pizza variant
    /// </summary>
    public PizzaSize Size { get; set; }
    
    /// <summary>
    /// Price for this specific size variant
    /// </summary>
    /// <example>12.99</example>
    public decimal Price { get; set; }
}
