namespace PizzaStore.Application.Features.Commands.PizzaVariant.UpdatePizzaVariant;

public class UpdatePizzaVariantResponse
{
    public string Id { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public string Message { get; set; } = string.Empty;
}
