namespace PizzaStore.Application.Features.Cart.Commands.ClearCart;

public class ClearCartResponse
{
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int ItemsRemoved { get; set; }
}
