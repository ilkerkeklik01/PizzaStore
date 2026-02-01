namespace PizzaStore.Application.Features.Commands.Topping.CreateTopping;

public class CreateToppingResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Message { get; set; } = string.Empty;
}
