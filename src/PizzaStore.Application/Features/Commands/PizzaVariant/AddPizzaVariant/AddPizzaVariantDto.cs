using PizzaStore.Domain.Entities;

namespace PizzaStore.Application.Features.Commands.PizzaVariant.AddPizzaVariant;

/// <summary>
/// Data transfer object for adding a new size variant to an existing pizza
/// </summary>
public class AddPizzaVariantDto
{
    /// <summary>
    /// Identifier of the pizza to add the variant to
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440005</example>
    public string PizzaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Size of the new pizza variant
    /// </summary>
    public PizzaSize Size { get; set; }
    
    /// <summary>
    /// Price for this size variant
    /// </summary>
    /// <example>14.99</example>
    public decimal Price { get; set; }
}
