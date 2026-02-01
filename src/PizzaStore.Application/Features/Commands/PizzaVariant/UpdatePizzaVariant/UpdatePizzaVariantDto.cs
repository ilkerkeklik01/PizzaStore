namespace PizzaStore.Application.Features.Commands.PizzaVariant.UpdatePizzaVariant;

/// <summary>
/// Data transfer object for updating an existing pizza variant
/// </summary>
public class UpdatePizzaVariantDto
{
    /// <summary>
    /// Updated price for the pizza variant
    /// </summary>
    /// <example>15.99</example>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Indicates whether the pizza variant should be available for ordering
    /// </summary>
    public bool IsAvailable { get; set; } = true;
}
