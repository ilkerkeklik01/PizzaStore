namespace PizzaStore.Application.Features.Commands.Topping.CreateTopping;

/// <summary>
/// Data transfer object for creating a new topping
/// </summary>
public class CreateToppingDto
{
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
}
