namespace PizzaStore.Application.Features.Commands.Topping.UpdateTopping;

public class UpdateToppingResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public string Message { get; set; } = string.Empty;
}
