namespace PizzaStore.Application.Features.Commands.Cart.DecreaseCartItemQuantity;

public class DecreaseCartItemQuantityResponse
{
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }
    public bool ItemRemoved { get; set; }
}
