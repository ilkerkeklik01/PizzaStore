namespace PizzaStore.Application.Features.Commands.Topping.UpdateTopping;

/// <summary>
/// Data transfer object for updating an existing topping
/// </summary>
public class UpdateToppingDto
{
    /// <summary>
    /// Updated name of the topping
    /// </summary>
    /// <example>Premium Extra Cheese</example>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Updated price for the topping
    /// </summary>
    /// <example>2.00</example>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Indicates whether the topping should be available for ordering
    /// </summary>
    public bool IsAvailable { get; set; } = true;
}
