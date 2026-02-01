namespace PizzaStore.Application.Features.PizzaVariant.Commands.AddPizzaVariant;

public class AddPizzaVariantResponse
{
    public string Id { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Message { get; set; } = string.Empty;
}
